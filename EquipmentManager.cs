using System;
using System.IO;
using System.Xml.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace VisualOutfits;

public enum VisualSlot
{
    Hat,
    Shirt,
    Pants,
    Boots,
    LeftRing,
    RightRing,
}

public static class EquipmentManager
{
    public const string ModDataKeyPrefix = "jessia.visualoutfits/";

    private const string ActiveSetKey = ModDataKeyPrefix + "activeSet";

    public const string SetVanilla = "vanilla";
    public const string SetVisual = "visual";

    public static void EnsureInitialized()
    {
        InvalidateCache();
        if (!Game1.player.modData.ContainsKey(ActiveSetKey))
            Game1.player.modData[ActiveSetKey] = SetVanilla;
    }

    public static string GetActiveSet()
    {
        return Game1.player.modData.TryGetValue(ActiveSetKey, out var set) ? set : SetVanilla;
    }

    public static void ToggleActiveSet()
    {
        string current = GetActiveSet();
        Game1.player.modData[ActiveSetKey] = current == SetVanilla ? SetVisual : SetVanilla;
    }

    private static string VisualKey(VisualSlot slot)
    {
        return slot switch
        {
            VisualSlot.Hat => ModDataKeyPrefix + "visual_hat",
            VisualSlot.Shirt => ModDataKeyPrefix + "visual_shirt",
            VisualSlot.Pants => ModDataKeyPrefix + "visual_pants",
            VisualSlot.Boots => ModDataKeyPrefix + "visual_boots",
            VisualSlot.LeftRing => ModDataKeyPrefix + "visual_leftRing",
            VisualSlot.RightRing => ModDataKeyPrefix + "visual_rightRing",
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
        };
    }

    // Per-frame cache: eliminates XML deserialization on every FarmerRenderer.draw call.
    private static readonly Item?[] _cache = new Item?[6]; // indexed by (int)VisualSlot
    private static bool _cacheValid;

    private static void InvalidateCache()
    {
        _cacheValid = false;
        Array.Clear(_cache, 0, _cache.Length);
    }

    private static void EnsureCache()
    {
        if (_cacheValid) return;
        foreach (VisualSlot slot in Enum.GetValues<VisualSlot>())
            _cache[(int)slot] = GetVisualItemFromData(slot);
        _cacheValid = true;
    }

    private static Item? GetVisualItemFromData(VisualSlot slot)
    {
        string key = VisualKey(slot);
        if (!Game1.player.modData.TryGetValue(key, out string? xml) || string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            return slot switch
            {
                VisualSlot.Hat => XmlTools.FromXml<Hat>(xml),
                VisualSlot.Shirt => XmlTools.FromXml<Clothing>(xml),
                VisualSlot.Pants => XmlTools.FromXml<Clothing>(xml),
                VisualSlot.Boots => XmlTools.FromXml<Boots>(xml),
                VisualSlot.LeftRing => XmlTools.FromXml<Ring>(xml),
                VisualSlot.RightRing => XmlTools.FromXml<Ring>(xml),
                _ => null,
            };
        }
        catch (Exception ex)
        {
            ModEntry.Log?.Log($"Failed to deserialize visual item for {slot}: {ex.Message}", LogLevel.Warn);
            return null;
        }
    }

    public static Item? GetVisualItem(VisualSlot slot)
    {
        EnsureCache();
        return _cache[(int)slot];
    }

    public static void SetVisualItem(VisualSlot slot, Item? item)
    {
        if (_cacheValid && ReferenceEquals(_cache[(int)slot], item))
            return;

        string key = VisualKey(slot);

        if (item == null)
        {
            Game1.player.modData.Remove(key);
            InvalidateCache();
            return;
        }

        if (!IsValidItemForSlot(slot, item))
            throw new InvalidOperationException($"Item '{item.DisplayName}' can't go in {slot}.");

        string xml = slot switch
        {
            VisualSlot.Hat => XmlTools.ToXmlString((Hat)item),
            VisualSlot.Shirt => XmlTools.ToXmlString((Clothing)item),
            VisualSlot.Pants => XmlTools.ToXmlString((Clothing)item),
            VisualSlot.Boots => XmlTools.ToXmlString((Boots)item),
            VisualSlot.LeftRing => XmlTools.ToXmlString((Ring)item),
            VisualSlot.RightRing => XmlTools.ToXmlString((Ring)item),
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
        };

        Game1.player.modData[key] = xml;
        InvalidateCache();
    }

    public static bool IsValidItemForSlot(VisualSlot slot, Item item)
    {
        return slot switch
        {
            VisualSlot.Hat => item is Hat,
            VisualSlot.Boots => item is Boots,
            VisualSlot.LeftRing => item is Ring,
            VisualSlot.RightRing => item is Ring,
            VisualSlot.Shirt => item is Clothing c && (int)c.clothesType.Value == 0,
            VisualSlot.Pants => item is Clothing c && (int)c.clothesType.Value == 1,
            _ => false,
        };
    }

    public static bool HasAnyVisualAppearance()
    {
        return GetVisualItem(VisualSlot.Hat) != null
            || GetVisualItem(VisualSlot.Shirt) != null
            || GetVisualItem(VisualSlot.Pants) != null;
    }

    public static Hat? GetVisualHat() => GetVisualItem(VisualSlot.Hat) as Hat;
    public static Clothing? GetVisualShirt() => GetVisualItem(VisualSlot.Shirt) as Clothing;
    public static Clothing? GetVisualPants() => GetVisualItem(VisualSlot.Pants) as Clothing;
    public static Boots? GetVisualBoots() => GetVisualItem(VisualSlot.Boots) as Boots;
}

internal static class XmlTools
{
    public static string ToXmlString<T>(T input)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(T)).Serialize(writer, input);
        return writer.ToString();
    }

    public static T FromXml<T>(string value)
    {
        using TextReader reader = new StringReader(value);
        return (T)new XmlSerializer(typeof(T)).Deserialize(reader)!;
    }
}

