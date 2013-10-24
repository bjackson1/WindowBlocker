using System;
using System.Runtime.InteropServices;

namespace NHS.NeoIdentityAgent
{
    internal static class NativeMethods
    {
        internal const int MF_BYPOSITION = 0x400;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, String lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern int GetMenuItemCount(IntPtr hWnd);
    }
}
