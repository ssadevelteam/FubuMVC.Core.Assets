using FubuMVC.Core.Registration;
using FubuMVC.Core.View;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuMVC.Core.Assets.Testing
{
    [TestFixture]
    public class Assets_namespace_should_be_registered_as_a_global_namespace
    {
        [Test]
        public void namespace_is_registered()
        {
            var graph = BehaviorGraph.BuildFrom(x => {
                x.Import<AssetBottleRegistration>();
            });

            graph.Settings.Get<CommonViewNamespaces>().Namespaces.ShouldContain(typeof(AssetBottleRegistration).Namespace);
        }
    }
}