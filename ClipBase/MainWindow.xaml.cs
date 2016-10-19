using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ClipBase
{
	/// <summary>
	/// MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int MAX_COUNT = 100;
		/// Next clipboard viewer window
		private IntPtr HWndNextViewer;
		private HwndSource HWndSource;

		private int Index;
		public ObservableCollection<string> Data = new ObservableCollection<string>();

		private bool IsViewing;

		public MainWindow()
		{
			InitializeComponent();

			pnlContent.BorderThickness = new Thickness(0, 0, 0, 0);

			pnlContent.ItemsSource = Data;
		}

		private void InitCBViewer()
		{
			WindowInteropHelper wih = new WindowInteropHelper(this);
			HWndSource = HwndSource.FromHwnd(wih.Handle);

			if (HWndSource != null)
			{
				HWndSource.AddHook(WinProc);   // start processing window messages
				HWndNextViewer = Win32.SetClipboardViewer(HWndSource.Handle);   // set this window as a viewer
			}
			IsViewing = true;
		}

		private void CloseCBViewer()
		{
			// remove this window from the clipboard viewer chain
			Win32.ChangeClipboardChain(HWndSource.Handle, HWndNextViewer);

			HWndNextViewer = IntPtr.Zero;
			HWndSource.RemoveHook(WinProc);
			IsViewing = false;

			Window.Title = IsViewing.ToString();
		}

		private void DrawContent()
		{
			if (Clipboard.ContainsText())
			{
				string text = Clipboard.GetText();

				int pos = Data.IndexOf(text);
				if (pos != -1)
				{
					Data.RemoveAt(pos);
				}

				Data.Insert(0, text);
				Index = 0;

				if (Data.Count > MAX_COUNT)
				{
					Data.RemoveAt(Data.Count - 1);
				}
			}
		}

		private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case Win32.WM_CHANGECBCHAIN:
					if (wParam == HWndNextViewer)
					{
						// clipboard viewer chain changed, need to fix it.
						HWndNextViewer = lParam;
					}
					else if (HWndNextViewer != IntPtr.Zero)
					{
						// pass the message to the next viewer.
						Win32.SendMessage(HWndNextViewer, msg, wParam, lParam);
					}
					break;

				case Win32.WM_DRAWCLIPBOARD:
					// clipboard content changed
					DrawContent();
					// pass the message to the next viewer.
					Win32.SendMessage(HWndNextViewer, msg, wParam, lParam);
					break;
			}

			return IntPtr.Zero;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			CloseCBViewer();
		}

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			WindowInteropHelper wndHelper = new WindowInteropHelper(this);
			IntPtr HWND = wndHelper.Handle;
			int GWL_EXSTYLE = -20;
			SetWindowLong(HWND, GWL_EXSTYLE, (IntPtr)(0x8000000));

			Hide();

			InitCBViewer();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			HotKey hotKeyWindow = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, Keys.T);
			hotKeyWindow.OnHotKey += OnHotKeyShowWindow;
			HotKey hotKeyPaste = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, Keys.W);
			hotKeyPaste.OnHotKey += OnHotKeyPaste;
		}

		private void OnHotKeyPaste()
		{
			if (Index > 0)
			{
				if (Index > Data.Count - 1)
				{
					Index = 0;
				}

				InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Z);
			}

			InputSimulator.SimulateTextEntry(Data[Index]);

			Index += 1;
		}

		private void OnHotKeyShowWindow()
		{
			Show();
		}

		private void PnlContentKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				Clipboard.SetText(pnlContent.SelectedItem.ToString());
				Hide();
			}
		}

		private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				Clipboard.SetText(pnlContent.SelectedItem.ToString());
				Hide();
			}
		}
	}
}
