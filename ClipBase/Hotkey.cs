using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

		int KeyId;         //�ȼ����
		IntPtr Handle;     //������
		Window Window;     //�ȼ����ڴ���
		uint ControlKey;   //�ȼ����Ƽ�
		uint Key;          //�ȼ�����

		public delegate void OnHotKeyEventHandler();     //�ȼ��¼�ί��
		public event OnHotKeyEventHandler OnHotKey = null;  //�ȼ��¼�

		static Hashtable KeyPair = new Hashtable();         //�ȼ���ϣ��
		private const int WM_HOTKEY = 0x0312;       // �ȼ���Ϣ���

		public enum KeyFlags    //���Ƽ�����
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
		public HotKey(Window win, HotKey.KeyFlags control, Keys key)
		{
			Handle = new WindowInteropHelper(win).Handle;
			Window = win;
			ControlKey = (uint)control;
			Key = (uint)key;
			KeyId = (int)ControlKey + (int)Key * 10;

			if (HotKey.KeyPair.ContainsKey(KeyId))
			{
				throw new Exception("�ȼ��Ѿ���ע��!");
			}

			//ע���ȼ�
			if (false == HotKey.RegisterHotKey(Handle, KeyId, ControlKey, Key))
			{
				throw new Exception("�ȼ�ע��ʧ��!");
			}

			//��Ϣ�ҹ�ֻ������һ��!!
			if (HotKey.KeyPair.Count == 0)
			{
				if (false == InstallHotKeyHook(this))
				{
					throw new Exception("��Ϣ�ҹ�����ʧ��!");
				}
			}

			//�������ȼ�����
			HotKey.KeyPair.Add(KeyId, this);
		}

		//��������,����ȼ�
		//~HotKey()
		//{
		//    HotKey.UnregisterHotKey(Handle, KeyId);
		//}

		#region Api

		[System.Runtime.InteropServices.DllImport("user32")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

		[System.Runtime.InteropServices.DllImport("user32")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		//��װ�ȼ�����ҹ�
		static private bool InstallHotKeyHook(HotKey hk)
		{
			if (hk.Window == null || hk.Handle == IntPtr.Zero)
			{
				return false;
			}

			//�����ϢԴ
			System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(hk.Handle);
			if (source == null)
			{
				return false;
			}

			//�ҽ��¼�
			source.AddHook(HotKey.HotKeyHook);
			return true;
		}

		//�ȼ��������
		static private IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_HOTKEY)
			{
				HotKey hk = (HotKey)HotKey.KeyPair[(int)wParam];
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
