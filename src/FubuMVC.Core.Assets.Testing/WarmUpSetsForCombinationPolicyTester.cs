using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuMVC.Tests.Assets
{
    [TestFixture]
    public class WarmUpSetsForCombinationPolicyTester : InteractionContext<WarmUpSetsForCombinationPolicy>
    {
        private AssetGraph _graph;
        private IEnumerable<string> _assetsForSetA;
        private IEnumerable<string> _assetsForSetB;
        private IEnumerable<string> _assetsForSetC;
        protected override void beforeEach()
        {
            _graph = new AssetGraph();

            _assetsForSetA = new[] { "a-1.js", "a-2.js" };
            _assetsForSetB = new[] { "b-1.css", "b-2.css" };

            _assetsForSetC = new[] {"c-1.js", "c-2.js", "c-3.css", "c-4.css"};

            _assetsForSetA.Each(x => _graph.AddToSet("setA", x));
            _assetsForSetB.Each(x => _graph.AddToSet("setB", x));
            _assetsForSetC.Each(x => _graph.AddToSet("setC", x));

            _graph.CompileDependencies(null);

            MockFor<IAssetDependencyFinder>()
                .Stub(x => x.CompileDependenciesAndOrder(new[] { "setA" }))
                .Return(_assetsForSetA);

            MockFor<IAssetDependencyFinder>()
                .Stub(x => x.CompileDependenciesAndOrder(new[] { "setB" }))
                .Return(_assetsForSetB);

            MockFor<IAssetDependencyFinder>()
                .Stub(x => x.CompileDependenciesAndOrder(new[] { "setC" }))
                .Return(_assetsForSetC);

            ClassUnderTest.Apply(null, null, _graph);
        }

        [Test]
        public void plans_for_sets_are_generated()
        {
            MockFor<IAssetTagPlanCache>().AssertWasCalled(x => x.PlanFor(Arg<MimeType>.Is.Equal(MimeType.Javascript), Arg<IEnumerable<string>>.List.Equal(_assetsForSetA)));
            MockFor<IAssetTagPlanCache>().AssertWasCalled(x => x.PlanFor(Arg<MimeType>.Is.Equal(MimeType.Css), Arg<IEnumerable<string>>.List.Equal(_assetsForSetB)));
            MockFor<IAssetTagPlanCache>().AssertWasCalled(x => x.PlanFor(Arg<MimeType>.Is.Equal(MimeType.Javascript), Arg<IEnumerable<string>>.List.Equal(_assetsForSetC.Where(f => f.EndsWith(".js")))));
            MockFor<IAssetTagPlanCache>().AssertWasCalled(x => x.PlanFor(Arg<MimeType>.Is.Equal(MimeType.Css), Arg<IEnumerable<string>>.List.Equal(_assetsForSetC.Where(f => f.EndsWith(".css")))));

        }
    }
}