using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;

public static class DesktopScreenshot
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern int BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(IntPtr hdc, IntPtr hBitmap, uint start, uint cLines, byte[] lpvBits, ref BITMAPINFO lpbmi, uint usage);

    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    private const int SRCCOPY = 0x00CC0020;

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public uint[] bmiColors;
    }

    public static void CaptureDesktop(string savePath)
    {
        IntPtr desktopHandle = GetDesktopWindow();
        IntPtr desktopDC = GetWindowDC(desktopHandle);
        IntPtr memoryDC = CreateCompatibleDC(desktopDC);

        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;

        IntPtr bitmapHandle = CreateCompatibleBitmap(desktopDC, screenWidth, screenHeight);
        IntPtr oldBitmap = SelectObject(memoryDC, bitmapHandle);

        // Capture the screen into the memory DC
        BitBlt(memoryDC, 0, 0, screenWidth, screenHeight, desktopDC, 0, 0, SRCCOPY);

        // Prepare to copy the bitmap data
        BITMAPINFO bmi = new BITMAPINFO();
        bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
        bmi.bmiHeader.biWidth = screenWidth;
        bmi.bmiHeader.biHeight = screenHeight; // Positive height for bottom-up DIB
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = 0; // BI_RGB

        int dataSize = screenWidth * screenHeight * 4; // 4 bytes per pixel
        byte[] pixelData = new byte[dataSize];

        // Get the bitmap data without flipping
        GetDIBits(memoryDC, bitmapHandle, 0, (uint)screenHeight, pixelData, ref bmi, 0);

        // Create a Texture2D from the pixel data
        Texture2D texture = new Texture2D(screenWidth, screenHeight, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(pixelData);
        texture.Apply();

        // Save the texture as a PNG
        File.WriteAllBytes(savePath, texture.EncodeToPNG());

        // Clean up
        SelectObject(memoryDC, oldBitmap);
        DeleteObject(bitmapHandle);
        DeleteDC(memoryDC);
        ReleaseDC(desktopHandle, desktopDC);

        Debug.Log($"Screenshot saved to: {savePath}");
    }
}
