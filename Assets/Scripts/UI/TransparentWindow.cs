using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindow : MonoBehaviour
{
    [SerializeField]
    private Material m_Material;

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    private struct RGB
    {
        public int r;
        public int g;
        public int b;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    
    /*
    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    */
    const int GWL_STYLE = -20;
    const uint WS_POPUP = 0x00080000;
    const uint WS_VISIBLE = 0x00000020;
    
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint LWA_COLORKEY = 0x00000001;
    void Start()
    {
#if !UNITY_EDITOR   // Can't set transparency in the editor
        var margins = new MARGINS() { cxLeftWidth = -1 };
        var hwnd = GetActiveWindow();
        
        //Sets a transparent background
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        //Makes things in the background clickable
        //SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        
        //Makes anything not completely black with 0 alpha clickable
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP);
        
        int red = 30;
        int green = 30;
        int blue = 30;

        // Combine RGB values into a COLORREF (0x00bbggrr format)
        uint colorKey = (uint)((blue << 16) | (green << 8) | red);
        SetLayeredWindowAttributes(hwnd, colorKey, 0, LWA_COLORKEY);
    
        //Application stays on top of everything
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, 0);
        
        Application.runInBackground = true;
#endif
    }
    
    /*
    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        Graphics.Blit(from, to, m_Material);
    }
    */
}