using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuMVC.Core.Assets.Combination;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Bootstrapping;
using FubuCore;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Runtime;
using System.Linq;

namespace FubuMVC.Core.Assets
{
    public class AssetCombinationBuildingActivator : IActivator
    {
        private readonly AssetGraph _graph;
        private readonly IAssetCombinationCache _cache;
        private readonly IAssetFileGraph _fileGraph;
        private readonly ICombinationPolicyCache _combinations;

        public AssetCombinationBuildingActivator(AssetGraph graph, IAssetCombinationCache cache, IAssetFileGraph fileGraph, ICombinationPolicyCache combinations)
        {
            _graph = graph;
            _cache = cache;
            _fileGraph = fileGraph;
            _combinations = combinations;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _graph.PolicyTypes.Each(type =>
            {
                if (type.CanBeCastTo<ICombinationPolicy>() && type.IsConcreteWithDefaultCtor())
                {
                    log.Trace("Registering {0} as an ICombinationPolicy", type.FullName);
                    var policy = Activator.CreateInstance(type).As<ICombinationPolicy>();

                    _combinations.Add(policy);
                }
            });

            _graph.ForCombinations((name, assetNames) =>
            {
                var mimeType = MimeType.MimeTypeByFileName(assetNames.First());
                _cache.AddFilesToCandidate(mimeType, name, assetNames.Select(x => _fileGraph.Find(x)));
            });

        }
    }
}