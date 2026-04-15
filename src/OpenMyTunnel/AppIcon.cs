using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace OpenMyTunnel;

// Generates the application window / taskbar icon at runtime.
// No image file required. Drawn pixel by pixel so AOT compilation stays clean.
internal static class AppIcon
{
    private const int Size = 64;

    // Accent purple (#7C6AF7)
    private const byte AccR = 0x7C, AccG = 0x6A, AccB = 0xF7;

    public static WindowIcon Build()
    {
        var bmp = new WriteableBitmap(
            new PixelSize(Size, Size),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var locked = bmp.Lock();

        unsafe
        {
            var ptr = (byte*)locked.Address;
            int rb = locked.RowBytes;

            // Transparent fill
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int i = y * rb + x * 4;
                    ptr[i] = ptr[i + 1] = ptr[i + 2] = ptr[i + 3] = 0;
                }

            // Purple rounded-square background (corner radius 12)
            FillRoundRect(ptr, rb, 0, 0, Size, Size, 12, AccR, AccG, AccB, 255);

            // Key bow: white outer circle
            FillCircle(ptr, rb, 21, 32, 13, 255, 255, 255, 255);
            // Key bow: punch hole back to accent color
            FillCircle(ptr, rb, 21, 32, 5, AccR, AccG, AccB, 255);

            // Key blade (horizontal bar connecting bow to teeth)
            FillRect(ptr, rb, 34, 27, 22, 10, 255, 255, 255, 255);

            // Tooth 1
            FillRect(ptr, rb, 40, 37, 6, 7, 255, 255, 255, 255);
            // Tooth 2
            FillRect(ptr, rb, 49, 37, 6, 7, 255, 255, 255, 255);
        }

        return new WindowIcon(bmp);
    }

    private static unsafe void FillCircle(
        byte* ptr, int rb,
        int cx, int cy, int r,
        byte R, byte G, byte B, byte A)
    {
        byte pr = (byte)(R * A / 255);
        byte pg = (byte)(G * A / 255);
        byte pb = (byte)(B * A / 255);
        float r2 = (r + 0.5f) * (r + 0.5f);

        for (int y = Math.Max(0, cy - r); y <= Math.Min(Size - 1, cy + r); y++)
            for (int x = Math.Max(0, cx - r); x <= Math.Min(Size - 1, cx + r); x++)
            {
                float dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy > r2) continue;
                int i = y * rb + x * 4;
                ptr[i] = pb; ptr[i + 1] = pg; ptr[i + 2] = pr; ptr[i + 3] = A;
            }
    }

    private static unsafe void FillRect(
        byte* ptr, int rb,
        int x, int y, int w, int h,
        byte R, byte G, byte B, byte A)
    {
        byte pr = (byte)(R * A / 255);
        byte pg = (byte)(G * A / 255);
        byte pb = (byte)(B * A / 255);

        for (int py = Math.Max(0, y); py < Math.Min(Size, y + h); py++)
            for (int px = Math.Max(0, x); px < Math.Min(Size, x + w); px++)
            {
                int i = py * rb + px * 4;
                ptr[i] = pb; ptr[i + 1] = pg; ptr[i + 2] = pr; ptr[i + 3] = A;
            }
    }

    private static unsafe void FillRoundRect(
        byte* ptr, int rb,
        int x, int y, int w, int h, int r,
        byte R, byte G, byte B, byte A)
    {
        byte pr = (byte)(R * A / 255);
        byte pg = (byte)(G * A / 255);
        byte pb = (byte)(B * A / 255);
        float r2 = (float)r * r;

        for (int py = y; py < y + h && py < Size; py++)
            for (int px = x; px < x + w && px < Size; px++)
            {
                bool inH = py >= y + r && py < y + h - r;
                bool inV = px >= x + r && px < x + w - r;

                if (!inH && !inV)
                {
                    // Corner zone: check distance to nearest corner center
                    int cornerX = px < x + r ? x + r : x + w - r;
                    int cornerY = py < y + r ? y + r : y + h - r;
                    float dx = px - cornerX, dy = py - cornerY;
                    if (dx * dx + dy * dy > r2) continue;
                }

                int i = py * rb + px * 4;
                ptr[i] = pb; ptr[i + 1] = pg; ptr[i + 2] = pr; ptr[i + 3] = A;
            }
    }
}
