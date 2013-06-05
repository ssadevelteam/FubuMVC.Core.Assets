using System.Net;
using FubuMVC.Core.UI;
using HtmlTags;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuMVC.Core.Assets.IntegrationTesting
{
    [TestFixture]
    public class default_assets_are_included_in_the_bottle
    {
        [Test]
        public void fetch_jquery()
        {
            SelfHostHarness.Endpoints.Get<AssetEndpoint>(x => x.get_jquery())
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ScriptNames().ShouldHaveTheSameElementsAs("_content/scripts/jquery-1.8.2.min.js");
        }

        [Test]
        public void fetch_underscore()
        {
            SelfHostHarness.Endpoints.Get<AssetEndpoint>(x => x.get_underscore())
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ScriptNames().ShouldHaveTheSameElementsAs("_content/scripts/underscore-min.js");
        }

        [Test]
        public void fetch_jquery_form()
        {
            SelfHostHarness.Endpoints.Get<AssetEndpoint>(x => x.get_jquery_form())
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ScriptNames().ShouldHaveTheSameElementsAs("_content/scripts/jquery-1.8.2.min.js", "_content/scripts/jquery.form.js");
        }
    }

    public class AssetEndpoint
    {
        private readonly FubuHtmlDocument _document;

        public AssetEndpoint(FubuHtmlDocument document)
        {
            _document = document;
        }

        public HtmlDocument get_jquery()
        {
            _document.Asset("jquery");
            _document.WriteAssetsToHead();

            return _document;
        }

        public HtmlDocument get_underscore()
        {
            _document.Asset("underscore");
            _document.WriteAssetsToHead();

            return _document;
        }

        public HtmlDocument get_jquery_form()
        {
            _document.Asset("jquery.form.js");
            _document.WriteAssetsToHead();

            return _document;
        }
    }
}