using StardewValley;
using StardewValley.Objects;

namespace VisualOutfits;

internal sealed record GearState(Hat? Hat, Clothing? Shirt, Clothing? Pants, Boots? Boots, Ring? LeftRing, Ring? RightRing);

internal static class GearUtil
{
    internal static GearState Capture(Farmer farmer)
    {
        return new GearState(
            farmer.hat.Value,
            farmer.shirtItem.Value,
            farmer.pantsItem.Value,
            farmer.boots.Value,
            farmer.leftRing.Value,
            farmer.rightRing.Value
        );
    }

    internal static void Apply(Farmer farmer, GearState state)
    {
        farmer.hat.Value = state.Hat;
        farmer.shirtItem.Value = state.Shirt;
        farmer.pantsItem.Value = state.Pants;
        farmer.boots.Value = state.Boots;
        farmer.leftRing.Value = state.LeftRing;
        farmer.rightRing.Value = state.RightRing;
    }

    internal static GearState LoadVisual()
    {
        return new GearState(
            EquipmentManager.GetVisualHat(),
            EquipmentManager.GetVisualShirt(),
            EquipmentManager.GetVisualPants(),
            EquipmentManager.GetVisualItem(VisualSlot.Boots) as Boots,
            EquipmentManager.GetVisualItem(VisualSlot.LeftRing) as Ring,
            EquipmentManager.GetVisualItem(VisualSlot.RightRing) as Ring
        );
    }

    internal static void SaveVisualFromPlayer(Farmer farmer)
    {
        EquipmentManager.SetVisualItem(VisualSlot.Hat, farmer.hat.Value);
        EquipmentManager.SetVisualItem(VisualSlot.Shirt, farmer.shirtItem.Value);
        EquipmentManager.SetVisualItem(VisualSlot.Pants, farmer.pantsItem.Value);
        EquipmentManager.SetVisualItem(VisualSlot.Boots, farmer.boots.Value);
        EquipmentManager.SetVisualItem(VisualSlot.LeftRing, farmer.leftRing.Value);
        EquipmentManager.SetVisualItem(VisualSlot.RightRing, farmer.rightRing.Value);
    }
}
