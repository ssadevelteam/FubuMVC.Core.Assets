using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FubuMVC.Core;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Assets.Caching;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Assets.Http;
using FubuMVC.Core.Http.Headers;
using FubuMVC.Core.Resources.Etags;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace FubuMVC.Tests.Assets.Http
{
    [TestFixture]
    public class when_writing_an_asset_that_cannot_be_found : InteractionContext<AssetWriter>
    {
        private AssetPath theAssetPath;

        protected override void beforeEach()
        {

            theAssetPath = new AssetPath("scripts/something")
            {
                ResourceHash = Guid.NewGuid().ToString()
            };

            MockFor<IContentWriter>().Expect(x => x.Write(theAssetPath, null))
                .Constraints(Is.Equal(theAssetPath), Is.NotNull())
                .Return(false);


            FubuMode.Reset();
            FubuMode.InDevelopment().ShouldBeFalse();

            ClassUnderTest.Write(theAssetPath);
        }

        [Test]
        public void should_write_the_404_status_code()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.WriteResponseCode(HttpStatusCode.NotFound));
        }

        [Test]
        public void writes_a_brief_message_so_that_Kayak_does_not_go_pear_shaped_from_an_empty_body()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.Write("anything"), x => x.IgnoreArguments());
        }
    }

    public class StubContentWriter : IContentWriter
    {
        private readonly AssetPath _path;
        private readonly IEnumerable<AssetFile> _files;

        public StubContentWriter(AssetPath path, IEnumerable<AssetFile> files)
        {
            _path = path;
            _files = files;
        }

        public bool Write(AssetPath asset, Action<IEnumerable<AssetFile>> writeHeaders)
        {
            asset.ShouldEqual(_path);
            writeHeaders(_files);

            return true;
        }
    }

    [TestFixture]
    public class when_writing_the_asset_in_production_mode : InteractionContext<AssetWriter>
    {
        private string theEtag;
        private AssetFile[] theFiles;
        private AssetPath theAssetPath;
        private Header[] theHeaders;

        protected override void beforeEach()
        {
            theEtag = "12345";

            theFiles = new[]{new AssetFile("a"), new AssetFile("b")};

            theAssetPath = new AssetPath("scripts/something"){
                ResourceHash = Guid.NewGuid().ToString()
            };

            var writer = new StubContentWriter(theAssetPath, theFiles);
            Services.Inject<IContentWriter>(writer);

            theHeaders = new Header[]{
                new Header("a", "1"), 
                new Header("b", "2"), 
                new Header("c", "3"), 
            };

            MockFor<IAssetCacheHeaders>().Stub(x => x.HeadersFor(theFiles)).Return(theHeaders);

            MockFor<IETagGenerator<IEnumerable<AssetFile>>>()
                .Stub(x => x.Create(theFiles))
                .Return(theEtag);

            FubuMode.Reset();
            FubuMode.InDevelopment().ShouldBeFalse();

            ClassUnderTest.Write(theAssetPath);
        }

        [Test]
        public void when_not_in_dev_mode_write_the_headers_for_asset_content_caching()
        {
            var output = MockFor<IOutputWriter>();
            output.AssertWasCalled(x => x.AppendHeader("a", "1"));
            output.AssertWasCalled(x => x.AppendHeader("b", "2"));
            output.AssertWasCalled(x => x.AppendHeader("c", "3"));

        }

        [Test]
        public void should_apply_the_etag_from_all_the_files_to_the_returned_value()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.AppendHeader(HttpResponseHeader.ETag, "12345"));
        }

        [Test]
        public void should_have_linked_all_the_files_to_a_resource_hash()
        {
            MockFor<IAssetContentCache>().AssertWasCalled(
                x => x.LinkFilesToResource(theAssetPath.ResourceHash, theFiles));
        }

    }



    public class when_writing_the_asset_in_DEVELOPMENT_mode : InteractionContext<AssetWriter>
    {
        private string theEtag;
        private AssetFile[] theFiles;
        private AssetPath theAssetPath;
        private Header[] theHeaders;

        protected override void beforeEach()
        {
            theEtag = "12345";

            theFiles = new[] { new AssetFile("a"), new AssetFile("b") };

            theAssetPath = new AssetPath("scripts/something")
            {
                ResourceHash = Guid.NewGuid().ToString()
            };

            theHeaders = new Header[]{
                new Header("a", "1"), 
                new Header("b", "2"), 
                new Header("c", "3"), 
            };

            MockFor<IAssetCacheHeaders>().Stub(x => x.HeadersFor(theFiles)).Return(theHeaders);

            var writer = new StubContentWriter(theAssetPath, theFiles);
            Services.Inject<IContentWriter>(writer);


            MockFor<IETagGenerator<IEnumerable<AssetFile>>>()
                .Stub(x => x.Create(theFiles))
                .Return(theEtag);

            FubuMode.Mode(FubuMode.Development);
            FubuMode.InDevelopment().ShouldBeTrue();

            ClassUnderTest.Write(theAssetPath);
        }

        [Test]
        public void when_in_dev_mode_should_NOT_write_the_headers_for_asset_content_caching()
        {
            var output = MockFor<IOutputWriter>();
            output.AssertWasNotCalled(x => x.AppendHeader("a", "1"));
            output.AssertWasNotCalled(x => x.AppendHeader("b", "2"));
            output.AssertWasNotCalled(x => x.AppendHeader("c", "3"));

        }

        [Test]
        public void should_apply_the_etag_from_all_the_files_to_the_returned_value()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.AppendHeader(HttpResponseHeader.ETag, "12345"));
        }

        [Test]
        public void should_have_linked_all_the_files_to_a_resource_hash()
        {
            MockFor<IAssetContentCache>().AssertWasCalled(
                x => x.LinkFilesToResource(theAssetPath.ResourceHash, theFiles));
        }

    }
}