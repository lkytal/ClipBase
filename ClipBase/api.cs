using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace ClipBase
{
	/// <summary>
	/// This static class holds the Win32 function declarations and constants needed by
	/// this sample application.
	/// </summary>
	internal static class Win32
	{
		/// <summary>
		/// The WM_DRAWCLIPBOARD message notifies a clipboard viewer window that
		/// the content of the clipboard has changed.
		/// </summary>
		internal const int WM_DRAWCLIPBOARD = 0x0308;

		/// <summary>
		/// A clipboard viewer window receives the WM_CHANGECBCHAIN message when
		/// another window is removing itself from the clipboard viewer chain.
		/// </summary>
		internal const int WM_CHANGECBCHAIN = 0x030D;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			/// <summary>
			/// Converts a RECT structure to an equivalent System.Windows.Rect structure. Returns a 0-width rectangle if the calculated width or height is negative.
			/// </summary>
			/// <param name="rect">The RECT to convert.</param>
			/// <returns>The equivalent System.Windows.Rect.</returns>
			public static implicit operator Rect(RECT rect)
			{
				// return a 0-width rectangle if the width or height is negative
				if (rect.right - rect.left < 0 || rect.bottom - rect.top < 0)
					return new Rect(rect.left, rect.top, 0, 0);
				return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
			}

			/// <summary>
			/// Converts a RECT structure equivalent to the specified System.Windows.Rect. Double precision is lost.
			/// </summary>
			/// <param name="rect">The System.Windows.Rect to convert.</param>
			/// <returns>The equivalent RECT structure.</returns>
			public static implicit operator RECT(Rect rect)
			{
				return new RECT()
				{
					left = (int)rect.Left,
					top = (int)rect.Top,
					right = (int)rect.Right,
					bottom = (int)rect.Bottom
				};
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr FindWindow(string strClassName, string strWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		public struct NOTIFYICONIDENTIFIER
		{
			/// <summary>
			/// Size of this structure, in bytes.
			/// </summary>
			public uint cbSize;

			/// <summary>
			/// A handle to the parent window used by the notification's callback function. For more information, see the hWnd member of the NOTIFYICONDATA structure.
			/// </summary>
			public IntPtr hWnd;

			/// <summary>
			/// The application-defined identifier of the notification icon. Multiple icons can be associated with a single hWnd, each with their own uID.
			/// </summary>
			public uint uID;

			/// <summary>
			/// A registered GUID that identifies the icon.
			/// </summary>
			public Guid guidItem;
		}

		/// <summary>
		/// Gets the screen coordinates of the bounding rectangle of a notification icon.
		/// </summary>
		/// <param name="identifier">Pointer to a NOTIFYICONIDENTIFIER structure that identifies the icon.</param>
		/// <param name="iconLocation">Pointer to a RECT structure that, when this function returns successfully, receives the coordinates of the icon.</param>
		/// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
		[DllImport("Shell32", SetLastError = true)]
		public static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);

		public static Rect? GetNotifyIconRectangle(NotifyIcon notifyicon)
		{
			// get notify icon id
			FieldInfo idFieldInfo = notifyicon.GetType().GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
			int iconid = (int)idFieldInfo.GetValue(notifyicon);

			// get notify icon hwnd
			FieldInfo windowFieldInfo = notifyicon.GetType().GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
			System.Windows.Forms.NativeWindow nativeWindow = (System.Windows.Forms.NativeWindow)windowFieldInfo.GetValue(notifyicon);
			IntPtr iconhandle = nativeWindow.Handle;
			if (iconhandle == IntPtr.Zero)
				return null;

			RECT rect = new RECT();
			NOTIFYICONIDENTIFIER nid = new NOTIFYICONIDENTIFIER()
			{
				hWnd = iconhandle,
				uID = (uint)iconid
			};
			nid.cbSize = (uint)Marshal.SizeOf(nid);

			int result = Shell_NotifyIconGetRect(ref nid, out rect);

			// 0 means success, 1 means the notify icon is in the fly-out - either is fine
			if (result != 0 && result != 1)
				return null;

			// convert to System.Rect and return
			return rect;
		}

		public enum ABMsg
		{
			/// <summary>
			/// Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.
			/// </summary>
			ABM_NEW = 0,

			/// <summary>
			/// Unregisters an appbar, removing the bar from the system's internal list.
			/// </summary>
			ABM_REMOVE = 1,

			/// <summary>
			/// Requests a size and screen position for an appbar.
			/// </summary>
			ABM_QUERYPOS = 2,

			/// <summary>
			/// Sets the size and screen position of an appbar.
			/// </summary>
			ABM_SETPOS = 3,

			/// <summary>
			/// Retrieves the autohide and always-on-top states of the Windows taskbar.
			/// </summary>
			ABM_GETSTATE = 4,

			/// <summary>
			/// Retrieves the bounding rectangle of the Windows taskbar.
			/// </summary>
			ABM_GETTASKBARPOS = 5,

			/// <summary>
			/// Notifies the system to activate or deactivate an appbar. The lParam member of the APPBARDATA pointed to by pData is set to TRUE to activate or FALSE to deactivate.
			/// </summary>
			ABM_ACTIVATE = 6,

			/// <summary>
			/// Retrieves the handle to the autohide appbar associated with a particular edge of the screen.
			/// </summary>
			ABM_GETAUTOHIDEBAR = 7,

			/// <summary>
			/// Registers or unregisters an autohide appbar for an edge of the screen.
			/// </summary>
			ABM_SETAUTOHIDEBAR = 8,

			/// <summary>
			/// Notifies the system when an appbar's position has changed.
			/// </summary>
			ABM_WINDOWPOSCHANGED = 9,

			/// <summary>
			/// Windows XP and later: Sets the state of the appbar's autohide and always-on-top attributes.
			/// </summary>
			ABM_SETSTATE = 10
		}

		public enum ABEdge
		{
			/// <summary>
			/// Left edge of screen.
			/// </summary>
			ABE_LEFT = 0,

			/// <summary>
			/// Top edge of screen.
			/// </summary>
			ABE_TOP = 1,

			/// <summary>
			/// Right edge of screen.
			/// </summary>
			ABE_RIGHT = 2,

			/// <summary>
			/// Bottom edge of screen.
			/// </summary>
			ABE_BOTTOM = 3
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct APPBARDATA
		{
			/// <summary>
			/// The size of the structure, in bytes.
			/// </summary>
			public uint cbSize;

			/// <summary>
			/// The handle to the appbar window.
			/// </summary>
			public IntPtr hWnd;

			/// <summary>
			/// An application-defined message identifier. The application uses the specified identifier for notification messages that it sends to the appbar identified by the hWnd member. This member is used when sending the ABM_NEW message.
			/// </summary>
			public uint uCallbackMessage;

			/// <summary>
			/// A value that specifies an edge of the screen. This member is used when sending the ABM_GETAUTOHIDEBAR, ABM_QUERYPOS, ABM_SETAUTOHIDEBAR, and ABM_SETPOS messages. This member can be one of the following values.
			/// </summary>
			public ABEdge uEdge;

			/// <summary>
			/// A RECT structure to contain the bounding rectangle, in screen coordinates, of an appbar or the Windows taskbar. This member is used when sending the ABM_GETTASKBARPOS, ABM_QUERYPOS, and ABM_SETPOS messages.
			/// </summary>
			public RECT rc;

			/// <summary>
			/// A message-dependent value. This member is used with the ABM_SETAUTOHIDEBAR and ABM_SETSTATE messages.
			/// </summary>
			public IntPtr lParam;
		}

		[DllImport("shell32.dll", SetLastError = true)]
		public static extern IntPtr SHAppBarMessage(ABMsg dwMessage, ref APPBARDATA pData);

		public static Rect GetTaskbarPos()
		{
			APPBARDATA abdata = new APPBARDATA() { hWnd = IntPtr.Zero };
			abdata.cbSize = (uint)Marshal.SizeOf(abdata);

			IntPtr result = SHAppBarMessage(ABMsg.ABM_GETTASKBARPOS, ref abdata);

			if (result == IntPtr.Zero)
				throw new Exception("Could not retrieve taskbar information.");

			return abdata.rc;
		}

		public static Point GetWindowPosition(NotifyIcon notifyicon, double windowwidth, double windowheight, double dpi)
		{
			// ReSharper disable once PossibleInvalidOperationException
			Rect niposition = (Rect)GetNotifyIconRectangle(notifyicon);
			Rect taskPos = GetTaskbarPos();

			// determine center of notify icon
			Point iconCenter = new Point((int) (niposition.Left + niposition.Width / 2), (int) (niposition.Top + niposition.Height / 2));

			double edgeoffset = 0 * dpi;

			double windowleft = iconCenter.X - windowwidth / 2;
			double windowtop = taskPos.Top - windowheight - edgeoffset;

			return new Point((int) windowleft, (int) windowtop);
		}
	}
}
