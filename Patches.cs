using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;

namespace VisualOutfits;

internal sealed record AppearanceState(Hat? Hat, Clothing? Shirt, Clothing? Pants, string? RealShoeColor);

[HarmonyPatch]
public static class Patches
{
    // ── Button position ──────────────────────────────────────────────────────
    private const int ToggleSize    = 32;
    private const int ToggleInset   = 0;
    private const int ToggleOffsetX = -18;
    private const int ToggleOffsetY = -50;
    // ────────────────────────────────────────────────────────────────────────

    // Pixel offset of the icon inside the button bounds.
    private const int IconOffsetX = 0;
    private const int IconOffsetY = 0;
    // Scale applied to the PNG. At 1f a 32×32 PNG fills the inner button area.
    private const float IconScale = 2f;
    // ────────────────────────────────────────────────────────────────────────

    private static readonly Rectangle ToggleSourceRect = new(128, 256, 64, 64);

    private static readonly BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static void Dbg(string message)
    {
        if (!ModEntry.DebugEnabled)
            return;

        ModEntry.Log?.Log(message, LogLevel.Debug);
    }

    private static void BeginVisualSwap(out GearState realGear)
    {
        Farmer player = Game1.player;
        realGear = GearUtil.Capture(player);

        VisualSwapContext.Begin(realGear);
        GearUtil.Apply(player, GearUtil.LoadVisual());
    }

    private static void EndVisualSwap(GearState realGear)
    {
        Farmer player = Game1.player;
        VisualSwapContext.End();

        // Re-equip vanilla items properly to restore their effects.
        if (realGear.Hat != null)
            player.Equip(realGear.Hat, player.hat);
        else
            player.hat.Value = null;

        if (realGear.Shirt != null)
            player.Equip(realGear.Shirt, player.shirtItem);
        else
            player.shirtItem.Value = null;

        if (realGear.Pants != null)
            player.Equip(realGear.Pants, player.pantsItem);
        else
            player.pantsItem.Value = null;

        if (realGear.Boots != null)
            player.Equip(realGear.Boots, player.boots);
        else
            player.boots.Value = null;

        if (realGear.LeftRing != null)
            player.Equip(realGear.LeftRing, player.leftRing);
        else
            player.leftRing.Value = null;

        if (realGear.RightRing != null)
            player.Equip(realGear.RightRing, player.rightRing);
        else
            player.rightRing.Value = null;
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), new[] { typeof(SpriteBatch) })]
    [HarmonyPrefix]
    private static void InventoryPage_draw_Prefix(ref GearState? __state)
    {
        if (EquipmentManager.GetActiveSet() != EquipmentManager.SetVisual)
            return;

        BeginVisualSwap(out GearState realGear);
        __state = realGear;
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), new[] { typeof(SpriteBatch) })]
    [HarmonyPostfix]
    private static void InventoryPage_draw_Postfix(InventoryPage __instance, SpriteBatch b, GearState? __state)
    {
        // Draw button on top.
        DrawToggleButton(__instance, b);

        if (__state != null)
        {
            VisualSwapContext.End();
            GearUtil.Apply(Game1.player, __state);
        }
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    private static bool InventoryPage_receiveLeftClick_Prefix(InventoryPage __instance, int x, int y, bool playSound, ref GearState? __state)
    {
        Rectangle toggleBounds = GetToggleBounds(__instance);
        if (toggleBounds.Contains(x, y))
        {
            EquipmentManager.ToggleActiveSet();
            if (playSound)
                Game1.playSound("button1");

            Dbg($"Toggle click: now {(EquipmentManager.GetActiveSet() == EquipmentManager.SetVisual ? "visual" : "vanilla")}");
            return false;
        }

        if (EquipmentManager.GetActiveSet() != EquipmentManager.SetVisual)
            return true;

        BeginVisualSwap(out GearState realGear);
        __state = realGear;
        return true;
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    private static void InventoryPage_receiveLeftClick_Postfix(GearState? __state)
    {
        if (__state == null)
            return;

        try
        {
            GearUtil.SaveVisualFromPlayer(Game1.player);
        }
        catch (Exception ex)
        {
            ModEntry.Log?.Log($"Failed to save visual gear after left click: {ex}", LogLevel.Warn);
        }
        finally
        {
            EndVisualSwap(__state);
        }
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveRightClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    private static void InventoryPage_receiveRightClick_Prefix(ref GearState? __state)
    {
        if (EquipmentManager.GetActiveSet() != EquipmentManager.SetVisual)
            return;

        BeginVisualSwap(out GearState realGear);
        __state = realGear;
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveRightClick))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    private static void InventoryPage_receiveRightClick_Postfix(GearState? __state)
    {
        if (__state == null)
            return;

        try
        {
            GearUtil.SaveVisualFromPlayer(Game1.player);
        }
        catch (Exception ex)
        {
            ModEntry.Log?.Log($"Failed to save visual gear after right click: {ex}", LogLevel.Warn);
        }
        finally
        {
            EndVisualSwap(__state);
        }
    }

    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.releaseLeftClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    private static void IClickableMenu_releaseLeftClick_Prefix(IClickableMenu __instance, ref GearState? __state)
    {
        if (EquipmentManager.GetActiveSet() != EquipmentManager.SetVisual)
            return;

        if (__instance is not InventoryPage)
            return;

        BeginVisualSwap(out GearState realGear);
        __state = realGear;
    }

    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.releaseLeftClick))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    private static void IClickableMenu_releaseLeftClick_Postfix(IClickableMenu __instance, GearState? __state)
    {
        if (__state == null)
            return;

        try
        {
            if (__instance is InventoryPage)
                GearUtil.SaveVisualFromPlayer(Game1.player);
        }
        catch (Exception ex)
        {
            ModEntry.Log?.Log($"Failed to save visual gear after release click: {ex}", LogLevel.Warn);
        }
        finally
        {
            EndVisualSwap(__state);
        }
    }

    private static void DrawToggleButton(InventoryPage page, SpriteBatch b)
    {
        bool isVisual = EquipmentManager.GetActiveSet() == EquipmentManager.SetVisual;
        Rectangle bounds = GetToggleBounds(page);

        Texture2D? icon = isVisual ? ModEntry.IconVisual : ModEntry.IconVanilla;
        if (icon != null)
        {
            b.Draw(icon, new Vector2(bounds.X + IconOffsetX, bounds.Y + IconOffsetY),
                null, Color.White, 0f, Vector2.Zero, IconScale, SpriteEffects.None, 0.87f);
        }
    }

    private static Rectangle GetToggleBounds(InventoryPage page)
    {
        Rectangle? preview = TryFindPlayerPreviewBounds(page);
        if (preview != null)
        {
            int x = preview.Value.X + ToggleInset + ToggleOffsetX;
            int y = preview.Value.Y + ToggleInset + ToggleOffsetY;
            return new Rectangle(x, y, ToggleSize, ToggleSize);
        }

        return new Rectangle(page.xPositionOnScreen + 100, page.yPositionOnScreen + 50, ToggleSize, ToggleSize);
    }

    private static Rectangle? TryFindPlayerPreviewBounds(InventoryPage page)
    {
        Rectangle? byName = TryGetBounds(page, ["portrait", "Portrait", "playerPortrait", "characterPortrait", "farmerBox", "characterBox", "playerBox"]);
        if (byName != null)
            return byName;

        Rectangle? best = null;
        int bestScore = int.MinValue;

        foreach (FieldInfo field in page.GetType().GetFields(AnyInstance))
        {
            object? obj;
            try
            {
                obj = field.GetValue(page);
            }
            catch
            {
                continue;
            }

            Rectangle? bounds = obj switch
            {
                ClickableTextureComponent ctc => ctc.bounds,
                ClickableComponent cc => cc.bounds,
                _ => null
            };

            if (bounds == null)
                continue;

            Rectangle r = bounds.Value;
            if (r.Width < 96 || r.Height < 96)
                continue;

            int area = r.Width * r.Height;
            int dx = Math.Abs(r.X - page.xPositionOnScreen);
            int dy = Math.Abs(r.Y - page.yPositionOnScreen);
            int score = area - (dx * 5) - (dy * 5);

            if (score > bestScore)
            {
                best = r;
                bestScore = score;
            }
        }

        return best;
    }

    private static Rectangle? TryGetBounds(InventoryPage page, string[] candidateNames)
    {
        foreach (string name in candidateNames)
        {
            FieldInfo? field = page.GetType().GetField(name, AnyInstance);
            if (field == null)
                continue;

            object? obj = field.GetValue(page);
            if (obj is ClickableTextureComponent ctc)
                return ctc.bounds;
            if (obj is ClickableComponent cc)
                return cc.bounds;
        }

        return null;
    }
}

[HarmonyPatch]
internal static class FarmerRendererDrawPatch
{
    private static bool _loggedOnce;

    // Last values baked into FarmerRenderer's baseTexture for each visual slot.
    // Null when no visual item is active. Compared each frame to detect changes and
    // trigger exactly one GPU recolor per change rather than one per frame.
    private static string? _lastBakedVisualShoeColor;
    private static string? _lastBakedVisualShirt;
    private static string? _lastBakedVisualPants;

    // Reflection handles — cached once, looked up lazily.
    private static FieldInfo? _netStringValueField;   // NetFieldBase<string,NetString>.value
    private static FieldInfo? _shoesDirtyField;       // FarmerRenderer._shoesDirty
    private static FieldInfo? _spriteDirtyField;      // FarmerRenderer._spriteDirty
    private static FieldInfo? _shirtDirtyField;       // FarmerRenderer._shirtDirty
    private static FieldInfo? _pantsDirtyField;       // FarmerRenderer._pantsDirty
    // Backing fields for Farmer's net-ref slots (bypass Set() → no events, no MarkDirty()).
    private static FieldInfo? _hatNetValueField;
    private static FieldInfo? _clothingNetValueField;

    private static FieldInfo? NetStringValueField
    {
        get
        {
            if (_netStringValueField != null) return _netStringValueField;
            Type? t = typeof(NetString);
            while (t != null)
            {
                FieldInfo? f = t.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
                if (f?.FieldType == typeof(string))
                {
                    _netStringValueField = f;
                    return f;
                }
                t = t.BaseType;
            }
            return null;
        }
    }

    // Walk the inheritance chain of a NetField type to find the protected 'value' backing field.
    private static FieldInfo? FindNetValueField(Type netFieldType, Type expectedValueType)
    {
        Type? t = netFieldType;
        while (t != null)
        {
            FieldInfo? f = t.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f?.FieldType == expectedValueType)
                return f;
            t = t.BaseType;
        }
        return null;
    }

    private static void EnsureRendererFields(FarmerRenderer renderer)
    {
        if (_spriteDirtyField == null)
            _spriteDirtyField = typeof(FarmerRenderer).GetField("_spriteDirty", BindingFlags.NonPublic | BindingFlags.Instance);
        if (_shoesDirtyField == null)
            _shoesDirtyField = typeof(FarmerRenderer).GetField("_shoesDirty", BindingFlags.NonPublic | BindingFlags.Instance);
        if (_shirtDirtyField == null)
            _shirtDirtyField = typeof(FarmerRenderer).GetField("_shirtDirty", BindingFlags.NonPublic | BindingFlags.Instance);
        if (_pantsDirtyField == null)
            _pantsDirtyField = typeof(FarmerRenderer).GetField("_pantsDirty", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static string? GetNetStringRaw(NetString ns) =>
        NetStringValueField?.GetValue(ns) as string;

    private static void SetNetStringRaw(NetString ns, string? value) =>
        NetStringValueField?.SetValue(ns, value ?? "");

    static IEnumerable<MethodBase> TargetMethods() =>
        AccessTools.GetDeclaredMethods(typeof(FarmerRenderer)).Where(m => m.Name == "draw");

    private static Farmer? FindFarmer(object[] args)
    {
        foreach (object? arg in args)
            if (arg is Farmer f) return f;
        return null;
    }

    [HarmonyPrefix]
    private static void Prefix(MethodBase __originalMethod, object[] __args, ref AppearanceState? __state)
    {
        Farmer? who = FindFarmer(__args);
        if (who == null || !ReferenceEquals(who, Game1.player))
            return;

        Hat? hat = EquipmentManager.GetVisualHat();
        Clothing? shirt = EquipmentManager.GetVisualShirt();
        Clothing? pants = EquipmentManager.GetVisualPants();
        Boots? boots = EquipmentManager.GetVisualBoots();

        FarmerRenderer renderer = who.FarmerRenderer;

        // Detect deactivation of each visual slot. When a slot goes from active → null,
        // schedule a one-time recolor so the real item's appearance is restored.
        bool needsShirtRebake = false;
        bool needsPantsRebake = false;
        bool needsShoeRebake = false;

        if (shirt == null && _lastBakedVisualShirt != null) { _lastBakedVisualShirt = null; needsShirtRebake = true; }
        if (pants == null && _lastBakedVisualPants != null) { _lastBakedVisualPants = null; needsPantsRebake = true; }
        if (boots == null && _lastBakedVisualShoeColor != null) { _lastBakedVisualShoeColor = null; needsShoeRebake = true; }

        if (hat == null && shirt == null && pants == null && boots == null)
        {
            // Nothing active. If any slot just deactivated, fire one recolor to restore real item.
            if (needsShirtRebake || needsPantsRebake || needsShoeRebake)
            {
                EnsureRendererFields(renderer);
                _spriteDirtyField?.SetValue(renderer, true);
                if (needsShirtRebake) _shirtDirtyField?.SetValue(renderer, true);
                if (needsPantsRebake) _pantsDirtyField?.SetValue(renderer, true);
                if (needsShoeRebake) _shoesDirtyField?.SetValue(renderer, true);
            }
            return;
        }

        if (!_loggedOnce && ModEntry.DebugEnabled)
        {
            _loggedOnce = true;
            ModEntry.Log?.Log($"[Renderer] Visual outfit active via {__originalMethod.Name}({string.Join(", ", __originalMethod.GetParameters().Select(p => p.ParameterType.Name))})", LogLevel.Debug);
        }

        // Lazy-init net backing field handles from the actual runtime types.
        if (_hatNetValueField == null) _hatNetValueField = FindNetValueField(who.hat.GetType(), typeof(Hat));
        if (_clothingNetValueField == null) _clothingNetValueField = FindNetValueField(who.shirtItem.GetType(), typeof(Clothing));

        // Detect visual shirt/pants activation or item change → schedule one GPU recolor.
        if (shirt != null && _lastBakedVisualShirt != shirt.QualifiedItemId) { _lastBakedVisualShirt = shirt.QualifiedItemId; needsShirtRebake = true; }
        if (pants != null && _lastBakedVisualPants != pants.QualifiedItemId) { _lastBakedVisualPants = pants.QualifiedItemId; needsPantsRebake = true; }

        // Handle shoe color via backing field — avoids NetString.Set() → MarkDirty() →
        // networking loopback → fieldChangeVisibleEvent ~every 20 s → GPU reads.
        string? savedRealShoeColor = null;
        if (boots != null)
        {
            string visualShoeColor = boots.GetBootsColorString();
            savedRealShoeColor = GetNetStringRaw(renderer.shoes);
            SetNetStringRaw(renderer.shoes, visualShoeColor);
            if (_lastBakedVisualShoeColor != visualShoeColor) { _lastBakedVisualShoeColor = visualShoeColor; needsShoeRebake = true; }
        }

        // Schedule a one-time GPU recolor for any changed slot.
        // executeRecolorActions() only runs when _spriteDirty=true, so steady-state frames are free.
        if (needsShirtRebake || needsPantsRebake || needsShoeRebake)
        {
            EnsureRendererFields(renderer);
            _spriteDirtyField?.SetValue(renderer, true);
            if (needsShirtRebake) _shirtDirtyField?.SetValue(renderer, true);
            if (needsPantsRebake) _pantsDirtyField?.SetValue(renderer, true);
            if (needsShoeRebake) _shoesDirtyField?.SetValue(renderer, true);
        }

        // Save real item refs (reading .Value is safe — getter has no side effects).
        __state = new AppearanceState(who.hat.Value, who.shirtItem.Value, who.pantsItem.Value, savedRealShoeColor);

        // Swap to visual items via backing fields — bypasses NetRef.Set(), so no
        // fieldChangeVisibleEvent, no UpdateClothing(), no MarkSpriteDirty(), no MarkDirty().
        // draw() reads who.hat/shirtItem/pantsItem directly, so it sees the swapped values.
        if (hat != null) _hatNetValueField?.SetValue(who.hat, hat);
        if (shirt != null) _clothingNetValueField?.SetValue(who.shirtItem, shirt);
        if (pants != null) _clothingNetValueField?.SetValue(who.pantsItem, pants);
    }

    [HarmonyPostfix]
    private static void Postfix(object[] __args, AppearanceState? __state)
    {
        if (__state == null)
            return;

        Farmer? who = FindFarmer(__args);
        if (who == null)
            return;

        // Restore via backing fields — no events, no MarkDirty(), no loopback.
        _hatNetValueField?.SetValue(who.hat, __state.Hat);
        _clothingNetValueField?.SetValue(who.shirtItem, __state.Shirt);
        _clothingNetValueField?.SetValue(who.pantsItem, __state.Pants);

        if (__state.RealShoeColor != null)
            SetNetStringRaw(who.FarmerRenderer.shoes, __state.RealShoeColor);
    }
}
