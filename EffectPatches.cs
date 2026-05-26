using System;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace VisualOutfits;

internal static class EffectPatches
{
    internal static void Apply(Harmony harmony)
    {
        // Patch the Farmer.Equip method to skip onEquip/onUnequip for visual items
        TryPatchFarmerEquip(harmony);
    }

    private static void TryPatchFarmerEquip(Harmony harmony)
    {
        // Patch Farmer.Equip<TItem>(TItem oldItem, TItem newItem, Action<TItem> equip)
        var equipMethod = AccessTools.Method(typeof(Farmer), "Equip", new[] { typeof(Item), typeof(Item), typeof(System.Action<Item>) });
        if (equipMethod == null)
        {
            ModEntry.Log?.Log("[EffectPatches] Could not find Farmer.Equip method", LogLevel.Warn);
            return;
        }

        harmony.Patch(
            equipMethod,
            prefix: new HarmonyMethod(typeof(EffectPatches), nameof(Equip_Prefix))
        );

        ModEntry.Log?.Log("[EffectPatches] Patched Farmer.Equip to skip visual item effects", LogLevel.Info);
    }

    private static bool Equip_Prefix(Farmer __instance, Item oldItem, Item newItem, Action<Item> equip)
    {
        // If newItem is a visual item, we need to equip it without calling onEquip
        if (newItem != null && IsVisualItem(newItem))
        {
            // Handle unequip of old item
            bool raiseEvents = Game1.hasLoadedGame && Game1.dayOfMonth > 0 && __instance.IsLocalPlayer;
            if (raiseEvents)
            {
                oldItem?.onUnequip(__instance);
            }

            // Equip new item WITHOUT calling onEquip
            equip(newItem);
            newItem.HasBeenInInventory = true;
            // Skip: newItem?.onEquip(__instance);

            // Mark buffs as dirty if needed
            if ((oldItem?.HasEquipmentBuffs() ?? false) || !((!(newItem?.HasEquipmentBuffs())) ?? true))
            {
                __instance.buffs.Dirty = true;
            }

            return false; // Skip original method
        }

        // For non-visual items, use the original method
        return true;
    }

    private static bool IsVisualItem(Item item)
    {
        if (item == null)
            return false;

        // Check if this item is stored in visual equipment
        if (EquipmentManager.GetActiveSet() != EquipmentManager.SetVisual)
            return false;

        // Compare by qualified ID with visual items
        Hat? visualHat = EquipmentManager.GetVisualHat();
        if (visualHat != null && item.QualifiedItemId == visualHat.QualifiedItemId)
            return true;

        Clothing? visualShirt = EquipmentManager.GetVisualShirt();
        if (visualShirt != null && item.QualifiedItemId == visualShirt.QualifiedItemId)
            return true;

        Clothing? visualPants = EquipmentManager.GetVisualPants();
        if (visualPants != null && item.QualifiedItemId == visualPants.QualifiedItemId)
            return true;

        Boots? visualBoots = EquipmentManager.GetVisualItem(VisualSlot.Boots) as Boots;
        if (visualBoots != null && item.QualifiedItemId == visualBoots.QualifiedItemId)
            return true;

        return false;
    }
}
