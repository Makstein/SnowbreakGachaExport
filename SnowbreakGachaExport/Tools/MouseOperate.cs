using System;
using System.Runtime.InteropServices;

namespace SnowbreakGachaExport.Tools;

public static class MouseOperate
{
    private struct INPUT
    {
        public SendInputEventType  Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    private struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
    }

    private struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public int MouseData;
        public MouseEventFlags Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
    
    [Flags]
    enum MouseEventFlags : uint
    {
        MOUSEEVENTF_MOVE = 0x0001,
        MOUSEEVENTF_LEFTDOWN = 0x0002,
        MOUSEEVENTF_LEFTUP = 0x0004,
        MOUSEEVENTF_ABSOLUTE = 0x8000
    }
    enum SendInputEventType
    {
        InputMouse,
    }
    
    enum SystemMetric
    {
        SM_CXSCREEN = 0,
        SM_CYSCREEN = 1,
    }

    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(SystemMetric smIndex);

    private static int CalculateAbsoluteCoordinateX(int x)
    {
        return (x * 65536) / GetSystemMetrics(SystemMetric.SM_CXSCREEN);
    }

    private static int CalculateAbsoluteCoordinateY(int y)
    {
        return (y * 65536) / GetSystemMetrics(SystemMetric.SM_CYSCREEN);
    }
    
    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);
    
    public static void DoMouseClick(int x, int y)
    {
        var mouseInput = new INPUT
        {
            Type = SendInputEventType.InputMouse
        };
        mouseInput.Data.Mouse.X = CalculateAbsoluteCoordinateX(x);
        mouseInput.Data.Mouse.Y = CalculateAbsoluteCoordinateY(y);
        mouseInput.Data.Mouse.MouseData = 0;
        
        mouseInput.Data.Mouse.Flags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
        SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
        
        mouseInput.Data.Mouse.Flags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
        SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
        
        mouseInput.Data.Mouse.Flags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
        SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
    }
}