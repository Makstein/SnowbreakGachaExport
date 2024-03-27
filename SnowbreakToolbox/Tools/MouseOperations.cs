using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace SnowbreakToolbox.Tools;

public class MouseOperations
{
    public static void MoveMouseTo(double absoluteX, double absoluteY)
    {
        int x = (int)Math.Truncate(absoluteX);
        int y = (int)Math.Truncate(absoluteY);

        // Convert px to user32 api coordinate
        x = x * 65536 / User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
        y = y * 65536 / User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);

        User32.INPUT input = new()
        {
            type = User32.INPUTTYPE.INPUT_MOUSE,
            mi = new User32.MOUSEINPUT()
            {
                dx = x,
                dy = y,
                dwFlags = User32.MOUSEEVENTF.MOUSEEVENTF_MOVE | User32.MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE
            }
        };

        User32.INPUT[] inputs = [input];

        uint resNum = User32.SendInput(1, inputs, Marshal.SizeOf(typeof(User32.INPUT)));
        if (resNum != 1)
        {
            throw new Exception("Send mouse move input failed");
        }
    }

    public static void LeftMouseClick()
    {
        User32.INPUT[] inputs
            = [MouseBtnEvent(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN), MouseBtnEvent(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTUP)];

        uint resNum = User32.SendInput(2, inputs, Marshal.SizeOf(typeof(User32.INPUT)));
        if (resNum != 2)
        {
            throw new Exception("Send mouse btn input failed");
        }
    }

    public static void LeftMouseClick(double x, double y)
    {
        MoveMouseTo(x, y);
        LeftMouseClick();
    }

    private static User32.INPUT MouseBtnEvent(User32.MOUSEEVENTF buttonEvent)
    {
        User32.INPUT input = new()
        {
            type = User32.INPUTTYPE.INPUT_MOUSE,
            mi = new User32.MOUSEINPUT()
            {
                dwFlags = buttonEvent
            }
        };

        return input;
    }
}