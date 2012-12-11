using System.Linq;
using Bottles.Diagnostics;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Core.Assets
{
    public class WarmUpSetsForCombinationPolicy : IAssetPolicy
    {
        private readonly IAssetTagPlanCache _planCache;
        private readonly IAssetDependencyFinder _finder;

        public WarmUpSetsForCombinationPolicy(IAssetTagPlanCache planCache, IAssetDependencyFinder finder)
        {
            _planCache = planCache;
            _finder = finder;
        }

        public void Apply(IPackageLog log, IAssetFileGraph fileGraph, AssetGraph graph)
        {
            graph.ForEachSetName(WarmUpSet);
        }

        public void WarmUpSet(string name)
        {
            var dependencies = _finder.CompileDependenciesAndOrder(new[] { name }).ToList();
            if (dependencies.Count == 0)
            {
                return;
            }

            var mimeTypes = dependencies.GroupBy(MimeType.MimeTypeByFileName);

            foreach (var mimeType in mimeTypes)
            {
                _planCache.PlanFor(mimeType.Key, mimeType);
            }
        }
    }
}