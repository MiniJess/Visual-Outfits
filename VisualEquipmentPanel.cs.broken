using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;

namespace VisualOutfits;

/// <summary>
/// Custom UI panel that displays visual equipment slots separately from vanilla equipment.
/// </summary>
internal sealed class VisualEquipmentPanel
{
    private const int SlotSize = 64;
    private const int Padding = 8;
    private const int TitleHeight = 24;

    // Layout: title + 4 slots arranged vertically
    private readonly int PanelWidth = SlotSize + (Padding * 2);
    private readonly int PanelHeight = TitleHeight + (SlotSize * 2) + (Padding * 3); // 2 rows of 2 slots

    public int X { get; set; }
    public int Y { get; set; }

    private Rectangle HatSlotBounds => new(X + Padding, Y + TitleHeight + Padding, SlotSize, SlotSize);
    private Rectangle ShirtSlotBounds => new(X + Padding + SlotSize + Padding, Y + TitleHeight + Padding, SlotSize, SlotSize);
    private Rectangle PantsSlotBounds => new(X + Padding, Y + TitleHeight + Padding + SlotSize + Padding, SlotSize, SlotSize);
    private Rectangle BootsSlotBounds => new(X + Padding + SlotSize + Padding, Y + TitleHeight + Padding + SlotSize + Padding, SlotSize, SlotSize);

    public Rectangle Bounds => new(X, Y, PanelWidth * 2 + Padding, PanelHeight);

    public VisualEquipmentPanel(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Draw(SpriteBatch b)
    {
        // Draw panel background
        IClickableMenu.drawTextureBox(b, X, Y, PanelWidth * 2 + Padding, PanelHeight, Color.White);

        // Draw title
        SpriteText.drawString(b, "Visual", X + Padding, Y + Padding);

        // Draw slots with current visual items
        DrawSlot(b, HatSlotBounds, VisualSlot.Hat, EquipmentManager.GetVisualHat());
        DrawSlot(b, ShirtSlotBounds, VisualSlot.Shirt, EquipmentManager.GetVisualShirt());
        DrawSlot(b, PantsSlotBounds, VisualSlot.Pants, EquipmentManager.GetVisualPants());
        DrawSlot(b, BootsSlotBounds, VisualSlot.Boots, EquipmentManager.GetVisualItem(VisualSlot.Boots) as Boots);
    }

    private static void DrawSlot(SpriteBatch b, Rectangle bounds, VisualSlot slot, Item? item)
    {
        // Draw slot background
        b.Draw(Game1.menuTexture, bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1), Color.White);

        if (item != null)
        {
            // Draw item icon
            item.drawInMenu(b, new Vector2(bounds.X, bounds.Y), 1f, 1f, 1f, StackDrawType.Hide, Color.White, false);
        }
    }

    public bool TryClickSlot(int x, int y, Item? heldItem)
    {
        if (heldItem == null)
            return false;

        if (HatSlotBounds.Contains(x, y) && EquipmentManager.IsValidItemForSlot(VisualSlot.Hat, heldItem))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Hat, heldItem.getOne());
            return true;
        }

        if (ShirtSlotBounds.Contains(x, y) && EquipmentManager.IsValidItemForSlot(VisualSlot.Shirt, heldItem))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Shirt, heldItem.getOne());
            return true;
        }

        if (PantsSlotBounds.Contains(x, y) && EquipmentManager.IsValidItemForSlot(VisualSlot.Pants, heldItem))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Pants, heldItem.getOne());
            return true;
        }

        if (BootsSlotBounds.Contains(x, y) && EquipmentManager.IsValidItemForSlot(VisualSlot.Boots, heldItem))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Boots, heldItem.getOne());
            return true;
        }

        return false;
    }

    public bool TryRightClickSlot(int x, int y)
    {
        if (HatSlotBounds.Contains(x, y))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Hat, null);
            return true;
        }

        if (ShirtSlotBounds.Contains(x, y))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Shirt, null);
            return true;
        }

        if (PantsSlotBounds.Contains(x, y))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Pants, null);
            return true;
        }

        if (BootsSlotBounds.Contains(x, y))
        {
            EquipmentManager.SetVisualItem(VisualSlot.Boots, null);
            return true;
        }

        return false;
    }
}
