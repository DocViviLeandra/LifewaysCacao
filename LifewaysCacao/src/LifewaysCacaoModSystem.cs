using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace LifewaysCacao
{
    public class LifewaysCacaoModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            RegisterBehaviors(api);
            Mod.Logger.Event("started '{0}' mod", Mod.Info.Name);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            
        }

        public void RegisterBehaviors(ICoreAPI api) {
            api.RegisterCollectibleBehaviorClass("LifewaysCacao.HandProcessable", typeof(HandProcessable));
        }

    }
}
