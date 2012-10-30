using FubuMVC.Core.View;

namespace FubuMVC.Core.Assets
{
    public class AssetBottleRegistration : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Services<AssetServicesRegistry>();
            registry.Policies.Add<AssetContentEndpoint>();

            registry.AlterSettings<CommonViewNamespaces>(x => {
                x.AddForType<AssetBottleRegistration>();
            });
        }
    }
}