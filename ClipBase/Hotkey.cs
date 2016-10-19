using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Collections;

namespace ClipBase
{
	///<summary>
	/// 直接构造类实例即可注册
	/// 自动完成注销
	/// 注意注册时会抛出异常
	/// 注册系统热键类
	///</summary>
	public class HotKey
	{
		#region Member

		private int KeyId;         //热键编号
		private IntPtr Handle;     //窗体句柄
		private Window Window;     //热键所在窗体
		private uint ControlKey;   //热键控制键
		private uint Key;          //热键主键

		public delegate void OnHotKeyEventHandler();
		public event OnHotKeyEventHandler OnHotKey = null;

		private static Hashtable KeyPair = new Hashtable();         //热键哈希表
		private const int WM_HOTKEY = 0x0312; 

		public enum KeyFlags
		{
			MOD_NONE = 0x0,
			MOD_ALT = 0x1,
			MOD_CONTROL = 0x2,
			MOD_SHIFT = 0x4,
			MOD_WIN = 0x8
		}

		#endregion

		///<summary>
		/// 构造函数
		///</summary>
		///<param name="win">注册窗体</param>
		///<param name="control">控制键</param>
		///<param name="key">主键</param>
		public HotKey(Window win, KeyFlags control, Keys key)
		{
			Handle = new WindowInteropHelper(win).Handle;
			Window = win;
			ControlKey = (uint)control;
			Key = (uint)key;
			KeyId = (int)ControlKey + (int)Key * 10;

			if (KeyPair.ContainsKey(KeyId))
			{
				throw new Exception("热键已经被注册!");
			}

			if (false == RegisterHotKey(Handle, KeyId, ControlKey, Key))
			{
				throw new Exception("热键注册失败!");
			}

			//消息挂钩只能连接一次!!
			if (KeyPair.Count == 0)
			{
				if (false == InstallHotKeyHook(this))
				{
					throw new Exception("消息挂钩连接失败!");
				}
			}

			KeyPair.Add(KeyId, this);
		}

		//~HotKey()
		//{
		//    HotKey.UnregisterHotKey(Handle, KeyId);
		//}

		#region Api

		[System.Runtime.InteropServices.DllImport("user32")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

		[System.Runtime.InteropServices.DllImport("user32")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		private static bool InstallHotKeyHook(HotKey hk)
		{
			if (hk.Window == null || hk.Handle == IntPtr.Zero)
			{
				return false;
			}

			//获得消息源
			HwndSource source = HwndSource.FromHwnd(hk.Handle);
			if (source == null)
			{
				return false;
			}

			source.AddHook(HotKeyHook);
			return true;
		}

		private static IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_HOTKEY)
			{
				HotKey hk = (HotKey)KeyPair[(int)wParam];
				if (hk.OnHotKey != null)
				{
					hk.OnHotKey();
				}
			}
			return IntPtr.Zero;
		}

		#endregion
	}
}
