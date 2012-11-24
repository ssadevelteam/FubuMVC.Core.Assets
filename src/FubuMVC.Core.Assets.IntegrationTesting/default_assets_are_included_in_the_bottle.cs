using System.Diagnostics;
using System.Net;
using System.Reflection;
using Bottles.PackageLoaders.Assemblies;
using FubuMVC.Core.UI;
using HtmlTags;
using NUnit.Framework;
using FubuTestingSupport;
using System.Collections.Generic;

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
    }

    
}