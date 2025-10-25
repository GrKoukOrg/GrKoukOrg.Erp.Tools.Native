using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Microsoft.Maui.Storage;
using Window = Microsoft.UI.Xaml.Window;

//using WinPlatforms = Platforms.Windows;

namespace Platforms.Windows
{
    static class WindowPositionHelper
    {
        const string KeyLeftDip = "WindowLeftDip";
        const string KeyTopDip = "WindowTopDip";
        const string KeyWidthDip = "WindowWidthDip";
        const string KeyHeightDip = "WindowHeightDip";
        const string KeyShowCmd = "WindowShowCmd";
        const string KeySavedMonitorDpi = "WindowSavedMonitorDpi";

        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;

        const uint MONITOR_DEFAULTTONULL = 0x00000000;
        const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        static readonly string LogFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WindowPositionHelper.log");

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(IntPtr hwnd); // Windows 10+

        [StructLayout(LayoutKind.Sequential)]
        struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        static void Log(string message)
        {
            try
            {
                var line = $"[{DateTime.Now:O}] {message}";
                Debug.WriteLine(line);
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
            catch
            {
                // best-effort logging, swallow exceptions
            }
        }

        static double GetScaleForWindow(IntPtr hwnd)
        {
            try
            {
                uint dpi = GetDpiForWindow(hwnd);
                if (dpi == 0) return 1.0;
                return dpi / 96.0;
            }
            catch
            {
                return 1.0;
            }
        }

        public static void SaveWindow(Window nativeWindow)
        {
            try
            {
                var hwnd = WindowNative.GetWindowHandle(nativeWindow);
                if (hwnd == IntPtr.Zero) return;

                if (!GetWindowPlacement(hwnd, out WINDOWPLACEMENT placement)) return;

                var rect = placement.rcNormalPosition;

                double scale = GetScaleForWindow(hwnd);
                // convert physical pixels -> DIPs
                double leftDip = rect.Left / scale;
                double topDip = rect.Top / scale;
                double widthDip = (rect.Right - rect.Left) / scale;
                double heightDip = (rect.Bottom - rect.Top) / scale;

                Preferences.Set(KeyShowCmd, placement.showCmd);
                Preferences.Set(KeyLeftDip, leftDip);
                Preferences.Set(KeyTopDip, topDip);
                Preferences.Set(KeyWidthDip, widthDip);
                Preferences.Set(KeyHeightDip, heightDip);
                Preferences.Set(KeySavedMonitorDpi, (int)Math.Round(scale * 96));

                Log($"Saved window. showCmd={placement.showCmd}, leftDip={leftDip:F1}, topDip={topDip:F1}, widthDip={widthDip:F1}, heightDip={heightDip:F1}, savedDpi={(int)Math.Round(scale*96)}");
            }
            catch (Exception ex)
            {
                Log($"SaveWindow failed: {ex}");
            }
        }

        public static void RestoreWindow(Window nativeWindow)
        {
            try
            {
                var hwnd = WindowNative.GetWindowHandle(nativeWindow);
                if (hwnd == IntPtr.Zero) return;

                if (!Preferences.ContainsKey(KeyLeftDip)) return; // nothing to restore

                int savedShowCmd = Preferences.Get(KeyShowCmd, SW_SHOWNORMAL);
                double leftDip = Preferences.Get(KeyLeftDip, 100.0);
                double topDip = Preferences.Get(KeyTopDip, 100.0);
                double widthDip = Preferences.Get(KeyWidthDip, 800.0);
                double heightDip = Preferences.Get(KeyHeightDip, 600.0);
                int savedMonitorDpi = Preferences.Get(KeySavedMonitorDpi, 96);

                // sanitize DIPs
                if (widthDip <= 0) widthDip = 800;
                if (heightDip <= 0) heightDip = 600;

                // Convert desired bounds (DIPs) into physical pixels for the monitor we will use.
                RECT desiredRect = new RECT
                {
                    Left = (int)Math.Round(leftDip * (savedMonitorDpi / 96.0)),
                    Top = (int)Math.Round(topDip * (savedMonitorDpi / 96.0)),
                    Right = (int)Math.Round((leftDip + widthDip) * (savedMonitorDpi / 96.0)),
                    Bottom = (int)Math.Round((topDip + heightDip) * (savedMonitorDpi / 96.0))
                };

                // Find nearest monitor for the desired rectangle
                IntPtr hMonitor = MonitorFromRect(ref desiredRect, MONITOR_DEFAULTTONEAREST);

                // If no monitor, fallback to nearest to window
                if (hMonitor == IntPtr.Zero)
                {
                    RECT currentRect = new RECT { Left = 0, Top = 0, Right = 0, Bottom = 0 };
                    hMonitor = MonitorFromRect(ref currentRect, MONITOR_DEFAULTTONEAREST);
                }

                double targetScale = GetScaleForWindow(hwnd);
                if (targetScale <= 0) targetScale = 1.0;

                if (hMonitor != IntPtr.Zero)
                {
                    var mi = new MONITORINFO();
                    mi.cbSize = Marshal.SizeOf<MONITORINFO>();
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        int workLeft = mi.rcWork.Left;
                        int workTop = mi.rcWork.Top;
                        int workRight = mi.rcWork.Right;
                        int workBottom = mi.rcWork.Bottom;
                        int workWidth = Math.Max(1, workRight - workLeft);
                        int workHeight = Math.Max(1, workBottom - workTop);

                        // Convert desired DIPs to physical pixels using current monitor DPI (targetScale)
                        int leftPx = (int)Math.Round(leftDip * targetScale);
                        int topPx = (int)Math.Round(topDip * targetScale);
                        int widthPx = (int)Math.Round(widthDip * targetScale);
                        int heightPx = (int)Math.Round(heightDip * targetScale);

                        // Clamp size to work area
                        if (widthPx > workWidth) widthPx = workWidth;
                        if (heightPx > workHeight) heightPx = workHeight;

                        // Adjust position to ensure window is fully visible within work area
                        if (leftPx < workLeft) leftPx = workLeft;
                        if (topPx < workTop) topPx = workTop;
                        if (leftPx + widthPx > workRight) leftPx = workRight - widthPx;
                        if (topPx + heightPx > workBottom) topPx = workBottom - heightPx;

                        // final sanitize again
                        if (leftPx < workLeft) leftPx = workLeft;
                        if (topPx < workTop) topPx = workTop;

                        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                        placement.length = Marshal.SizeOf<WINDOWPLACEMENT>();
                        GetWindowPlacement(hwnd, out placement); // best-effort fill

                        // Avoid starting minimized
                        placement.showCmd = savedShowCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : savedShowCmd;

                        placement.rcNormalPosition.Left = leftPx;
                        placement.rcNormalPosition.Top = topPx;
                        placement.rcNormalPosition.Right = leftPx + widthPx;
                        placement.rcNormalPosition.Bottom = topPx + heightPx;

                        SetWindowPlacement(hwnd, ref placement);

                        Log($"Restored window. showCmd={placement.showCmd}, leftPx={leftPx}, topPx={topPx}, widthPx={widthPx}, heightPx={heightPx}, targetDpi={(int)Math.Round(targetScale*96)}");
                        return;
                    }
                }

                // If reach here: no monitor info -> fallback simple set using current window DPI
                int fallbackLeft = (int)Math.Round(leftDip * targetScale);
                int fallbackTop = (int)Math.Round(topDip * targetScale);
                int fallbackRight = fallbackLeft + (int)Math.Round(widthDip * targetScale);
                int fallbackBottom = fallbackTop + (int)Math.Round(heightDip * targetScale);

                WINDOWPLACEMENT fallbackPlacement = new WINDOWPLACEMENT();
                fallbackPlacement.length = Marshal.SizeOf<WINDOWPLACEMENT>();
                GetWindowPlacement(hwnd, out fallbackPlacement);

                fallbackPlacement.showCmd = savedShowCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : savedShowCmd;
                fallbackPlacement.rcNormalPosition.Left = fallbackLeft;
                fallbackPlacement.rcNormalPosition.Top = fallbackTop;
                fallbackPlacement.rcNormalPosition.Right = fallbackRight;
                fallbackPlacement.rcNormalPosition.Bottom = fallbackBottom;

                SetWindowPlacement(hwnd, ref fallbackPlacement);

                Log($"Restored window (fallback). showCmd={fallbackPlacement.showCmd}, left={fallbackLeft}, top={fallbackTop}, right={fallbackRight}, bottom={fallbackBottom}, targetDpi={(int)Math.Round(targetScale*96)}");
            }
            catch (Exception ex)
            {
                Log($"RestoreWindow failed: {ex}");
            }
        }
    }
}