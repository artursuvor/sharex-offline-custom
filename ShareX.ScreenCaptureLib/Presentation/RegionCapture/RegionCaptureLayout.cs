using Avalonia;
using System;

namespace ShareX.ScreenCaptureLib.Presentation.RegionCapture;

/// <summary>Places the capture tools beside the selection within the current monitor.</summary>
public static class RegionCaptureLayout
{
    public static Point PlaceToolbar(Rect selection, Rect viewport, Size toolbar)
    {
        const double gap = 8;
        double left = Math.Clamp(selection.Right - toolbar.Width, viewport.Left + gap,
            Math.Max(viewport.Left + gap, viewport.Right - toolbar.Width - gap));
        double top = selection.Bottom + gap;
        if (top + toolbar.Height > viewport.Bottom - gap)
            top = selection.Top - toolbar.Height - gap;
        top = Math.Clamp(top, viewport.Top + gap,
            Math.Max(viewport.Top + gap, viewport.Bottom - toolbar.Height - gap));
        return new Point(left, top);
    }
}
