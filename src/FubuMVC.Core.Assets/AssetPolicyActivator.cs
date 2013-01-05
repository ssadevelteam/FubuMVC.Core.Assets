using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuMVC.Core.Assets.Combination;
using FubuMVC.Core.Assets.Files;

namespace FubuMVC.Core.Assets
{
    public interface IAssetPolicy
    {
        void Apply(IPackageLog log, IAssetFileGraph fileGraph, AssetGraph graph);
    }

    public class AssetPolicyActivator : IActivator
    {
        private readonly IEnumerable<IAssetPolicy> _policies;
        private readonly IAssetFileGraph _fileGraph;
        private readonly AssetGraph _graph;

        public AssetPolicyActivator(IEnumerable<IAssetPolicy> policies, IAssetFileGraph fileGraph, AssetGraph graph)
        {
            _policies = policies;
            _fileGraph = fileGraph;
            _graph = graph;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _policies.Each(p =>
            {
                log.Trace("Running " + p);
                p.Apply(log, _fileGraph, _graph);
            });

            _graph.PolicyTypes.Each(type =>
            {
                if (type.CanBeCastTo<IAssetPolicy>() && type.IsConcreteWithDefaultCtor())
                {
                    var policy = Activator.CreateInstance(type).As<IAssetPolicy>();
                    log.Trace("Running " + policy);
                    policy.Apply(log, _fileGraph, _graph);
                }
            });
        }
    }
}