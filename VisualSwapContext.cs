namespace VisualOutfits;

internal static class VisualSwapContext
{
    internal static int Depth { get; private set; }
    internal static GearState? RealGear { get; private set; }

    internal static bool Active => Depth > 0;

    internal static void Begin(GearState realGear)
    {
        Depth++;
        if (Depth == 1)
            RealGear = realGear;
    }

    internal static void End()
    {
        if (Depth > 0)
            Depth--;

        if (Depth <= 0)
        {
            Depth = 0;
            RealGear = null;
        }
    }
}
