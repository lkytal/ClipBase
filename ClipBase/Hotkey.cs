using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Collections;

namespace ClipBase
{
	///<summary>
	/// ֱ�ӹ�����ʵ������ע��
	/// �Զ����ע��
	/// ע��ע��ʱ���׳��쳣
	/// ע��ϵͳ�ȼ���
	///</summary>
	public class HotKey
	{
		#region Member

		private int KeyId;         //�ȼ����
		private IntPtr Handle;     //������
		private Window Window;     //�ȼ����ڴ���
		private uint ControlKey;   //�ȼ����Ƽ�
		private uint Key;          //�ȼ�����

		public delegate void OnHotKeyEventHandler();
		public event OnHotKeyEventHandler OnHotKey = null;

		private static Hashtable KeyPair = new Hashtable();         //�ȼ���ϣ��
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
		/// ���캯��
		///</summary>
		///<param name="win">ע�ᴰ��</param>
		///<param name="control">���Ƽ�</param>
		///<param name="key">����</param>
		public HotKey(Window win, KeyFlags control, Keys key)
		{
			Handle = new WindowInteropHelper(win).Handle;
			Window = win;
			ControlKey = (uint)control;
			Key = (uint)key;
			KeyId = (int)ControlKey + (int)Key * 10;

			if (KeyPair.ContainsKey(KeyId))
			{
				throw new Exception("�ȼ��Ѿ���ע��!");
			}

			if (false == RegisterHotKey(Handle, KeyId, ControlKey, Key))
			{
				throw new Exception("�ȼ�ע��ʧ��!");
			}

			//��Ϣ�ҹ�ֻ������һ��!!
			if (KeyPair.Count == 0)
			{
				if (false == InstallHotKeyHook(this))
				{
					throw new Exception("��Ϣ�ҹ�����ʧ��!");
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

			//�����ϢԴ
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
