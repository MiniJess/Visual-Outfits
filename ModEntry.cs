using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace VisualOutfits;

public sealed class ModEntry : Mod
{
    internal static IMonitor? Log { get; private set; }
    internal static bool DebugEnabled { get; private set; }

    // Loaded from assets/icon_vanilla.png and assets/icon_visual.png.
    // Null if the file is absent — button falls back to the default eye icon.
    internal static Texture2D? IconVanilla { get; private set; }
    internal static Texture2D? IconVisual  { get; private set; }

    public override void Entry(IModHelper helper)
    {
        Log = Monitor;

        IconVanilla = TryLoadTexture(helper, "assets/icon_vanilla.png");
        IconVisual  = TryLoadTexture(helper, "assets/icon_visual.png");

        helper.ConsoleCommands.Add(
            name: "vo_debug",
            documentation: "Toggle Visual Outfits debug logging.\n\nUsage: vo_debug on|off|toggle|status",
            callback: OnDebugCommand
        );

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.PatchAll();

        Monitor.Log("Visual Outfits loaded.", LogLevel.Info);

        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    private static Texture2D? TryLoadTexture(IModHelper helper, string path)
    {
        try
        {
            return helper.ModContent.Load<Texture2D>(path);
        }
        catch
        {
            return null;
        }
    }

    private void OnDebugCommand(string command, string[] args)
    {
        string mode = args.Length > 0 ? args[0] : "toggle";

        if (mode.Equals("on", StringComparison.OrdinalIgnoreCase))
            DebugEnabled = true;
        else if (mode.Equals("off", StringComparison.OrdinalIgnoreCase))
            DebugEnabled = false;
        else if (mode.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            // no change
        }
        else
            DebugEnabled = !DebugEnabled;

        Log?.Log($"VisualOutfits debug logging: {(DebugEnabled ? "ON" : "OFF")}", LogLevel.Info);
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        EquipmentManager.EnsureInitialized();
    }
}
