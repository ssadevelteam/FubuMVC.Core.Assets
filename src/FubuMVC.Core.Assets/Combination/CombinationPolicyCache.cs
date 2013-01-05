using System.Collections;
using System.Collections.Generic;

namespace FubuMVC.Core.Assets.Combination
{
    public interface ICombinationPolicyCache : IEnumerable<ICombinationPolicy>
    {
        void Add(ICombinationPolicy policy);
    }

    public class CombinationPolicyCache : ICombinationPolicyCache
    {
        private readonly IList<ICombinationPolicy> _policies = new List<ICombinationPolicy>();

        public CombinationPolicyCache(IEnumerable<ICombinationPolicy> policies)
        {
            _policies.AddRange(policies);
        }

        public void Add(ICombinationPolicy policy)
        {
            _policies.Add(policy);
        }

        public IEnumerator<ICombinationPolicy> GetEnumerator()
        {
            return _policies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}