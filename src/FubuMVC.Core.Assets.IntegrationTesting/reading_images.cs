using System.Net;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Endpoints;
using FubuMVC.Core.Runtime;
using FubuMVC.OwinHost;
using FubuMVC.TestingHarness;
using NUnit.Framework;
using FubuTestingSupport;
using FubuMVC.StructureMap;
using StructureMap;
using FubuMVC.Katana;

namespace FubuMVC.Core.Assets.IntegrationTesting
{
    [TestFixture]
    public class reading_images
    {
        private static readonly CommandRunner _runner = new CommandRunner();
        private EmbeddedFubuMvcServer _server;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _runner.RunBottles("init src/TestPackage1 pak1");
            _runner.RunBottles("link src/FubuMVC.Core.Assets.IntegrationTesting pak1");

            _server =
                FubuApplication.DefaultPolicies()
                               .StructureMap(new Container())
                               .RunEmbedded(port: PortFinder.FindPort(5505));

        }

        public void TearDown()
        {
            _server.Dispose();
        }

        public EndpointDriver endpoints
        {
            get { return _server.Endpoints; }
        }

        [Test]
        public void can_read_an_image_from_the_main_application_on_the_first_read()
        {
            endpoints.GetAsset(AssetFolder.images, "icon-add.png")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentTypeShouldBe(MimeType.Png)
                .LengthShouldBe(476);
        }

        // 854

        [Test]
        public void read_asset_with_etag_should_return_NotModified_and_no_content()
        {
            // First request without an etag gets the whole thing
            var etag = endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif")
                .LengthShouldBe(854)
                .ContentTypeShouldBe(MimeType.Gif)
                .StatusCodeShouldBe(HttpStatusCode.OK).Etag();
            
            // Subsequent requests with etag
            endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif", etag: etag)
                .StatusCodeShouldBe(HttpStatusCode.NotModified);

            endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif", etag: etag)
                .StatusCodeShouldBe(HttpStatusCode.NotModified)
                .ContentLength().ShouldBeLessThan(200);

            endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif", etag: etag)
                .StatusCodeShouldBe(HttpStatusCode.NotModified)
                .ShouldHaveHeader(HttpResponseHeader.CacheControl)
                .ShouldHaveHeader(HttpResponseHeader.ETag);
        }

        [Test]
        public void image_tags_should_have_cache_headers_set()
        {
            endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif")
                 .ShouldHaveHeader(HttpResponseHeader.CacheControl);
        }

        [Test]
        public void fetch_with_the_wrong_etag_gets_the_entire_asset()
        {
            // First request without an etag gets the whole thing
            var etag = endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif")
                .LengthShouldBe(854)
                .ContentTypeShouldBe(MimeType.Gif)
                .StatusCodeShouldBe(HttpStatusCode.OK).Etag();

            // Subsequent requests with etag
            endpoints.GetAsset(AssetFolder.images, "aaa/ico-close.gif", etag: etag + "-junk")
                .LengthShouldBe(854)
                .ContentTypeShouldBe(MimeType.Gif)
                .StatusCodeShouldBe(HttpStatusCode.OK);
        }

        [Test]
        public void read_image_from_a_package()
        {
            endpoints.GetAsset(AssetFolder.images, "icon-add-alt.png")
                .LengthShouldBe(3517)
                .ContentTypeShouldBe(MimeType.Png)
                .StatusCodeShouldBe(HttpStatusCode.OK);
        }

        /*

    <Comment><![CDATA[This image is in the main application, so should be downloaded directly from the website content]]></Comment>
    <ImageUrlFor isStep="True" name="icon-add.png" url="/fubu-testing/_content/images/icon-add.png" />
    <Comment><![CDATA[These images are in the TestPackage1 package, so should be downloaded from the _images/*** url]]></Comment>
    <ImageUrlFor isStep="True" name="icon-add-alt.png" url="/fubu-testing/_content/images/icon-add-alt.png" />
    <DownloadImage isStep="True" name="icon-add-alt.png" mimeType="image/png" />
  </Packaging>
         */
    }
}