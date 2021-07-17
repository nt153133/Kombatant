using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ff14bot;

namespace Kombatant.Helpers
{
	internal class SendKeysHelper
	{
		internal static class NativeMethods
		{
			[DllImport("user32.dll", SetLastError = true)]
			internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll", SetLastError = true)]
			internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

			internal const int WM_KEYDOWN = 0x0100;
			internal const int WM_KEYUP = 0x0101;
			internal const int WM_SYSKEYDOWN = 0x104;
			internal const int WM_SYSKEYUP = 0x105;
		}
	}
}
