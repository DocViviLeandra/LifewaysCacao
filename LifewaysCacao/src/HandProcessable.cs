using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace LifewaysCacao {
    internal class HandProcessable : CollectibleBehavior {

        public ICoreAPI api;

        float processingTime;

        JsonItemStack[] processingResultStacks;
        ItemStack[] processingResultStacksResolved;

        public HandProcessable(CollectibleObject collObj) : base(collObj) { }

        public override void Initialize(JsonObject properties) {
            base.Initialize(properties);

            if (properties != null) {
                processingTime = properties["processingTime"].AsFloat(defaultValue: 2.0f);
                processingResultStacks = properties["processingResults"].AsObject<JsonItemStack[]>();
            }
        }

        public override void OnLoaded(ICoreAPI api) {
            base.OnLoaded(api);
            this.api = api;

            processingResultStacksResolved = new ItemStack[processingResultStacks.Length];

            for (int i = 0; i < processingResultStacks.Length; i++) {
                JsonItemStack itemStack = processingResultStacks[i];
                itemStack.Resolve(api.World, "Processing result");
                if (itemStack.ResolvedItemstack != null) {
                    processingResultStacksResolved[i] = itemStack.ResolvedItemstack;
                } else api.Logger.Warning($"Unable to resolve itemstack {itemStack.Code}");
            }
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling) {
            api.Logger.Event("Started processing");
            handling = EnumHandling.PreventDefault;
            handHandling = EnumHandHandling.PreventDefault;
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling) {
            handling = EnumHandling.PreventDefault;

            if (byEntity.World is IClientWorldAccessor) {
                return secondsUsed < processingTime;
            }

            return true;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handled) {
            api.Logger.Event("Processing Canceled");
            handled = EnumHandling.PreventDefault;
            return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason, ref handled);
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling) {
            api.Logger.Event("Finished processing");
            handling = EnumHandling.PreventDefault;

            if (secondsUsed > processingTime) {
                if (api.Side == EnumAppSide.Server) {
                    // Handle processing results
                    for (int i = 0; i < processingResultStacksResolved.Length; i++) {
                        ItemStack result = processingResultStacksResolved[i].Clone();
                        if (!byEntity.TryGiveItemStack(result)) {
                            byEntity.World.SpawnItemEntity(result, byEntity.Pos.XYZ.Add(0, 0.5, 0));
                        }
                    }
                }

                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling) {
            handling = EnumHandling.PreventDefault;
            return new WorldInteraction[] {
                new WorldInteraction {
                    ActionLangCode = "heldhelp-handprocess",
                    MouseButton = EnumMouseButton.Right,
                }
            };
        }
    }
}
