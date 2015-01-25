using System;
using System.Runtime.InteropServices;

namespace Scratch.Uploader
{
	internal static class Win32
	{
		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool GetWindowRect( IntPtr hWnd, ref RECT lpRect );

		[StructLayout( LayoutKind.Sequential )]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
	}
}
