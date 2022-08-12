using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenLampWPF.Helper;
using LastTryTapo;
using LastTryTapo.Models;
using System.Threading.Tasks;
using ScreenLampWPF.Properties;
using ScreenLampWPF.Pages;

namespace ScreenLampWPF
{
    public partial class MainWindow : Window
    {
        private Home _homePage = new Home();
        private AppSettings _appSettings = new AppSettings();
        private SetUp _setUpPage = new SetUp();
        
        public MainWindow()
        {
            InitializeComponent();
            
            Topmost = Settings.Default.PIN_APP;

            _appSettings.BackToHome += new EventHandler(Window_BackToHome);
            _setUpPage.BackToHome += new EventHandler(Window_BackToHome);

            _appSettings.Update += new EventHandler(AppSettings_Update);

            _homePage.ShowSettings += new EventHandler(HomePage_ShowSettings);
            _homePage.ShowSetUp += new EventHandler(HomePage_SetUpPage);

            MainFrame.Content = _homePage;

            Console.WriteLine("App has started");
        }

        #region WindowEvents

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hWnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hWnd).AddHook(WindowProc);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Keyboard.ClearFocus();
                DragMove();
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        #endregion

        #region WindowControl

        private void WinMinimize_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(hWnd, ApiCodes.WM_SYSCOMMAND, new IntPtr(ApiCodes.SC_MINIMIZE), IntPtr.Zero);
        }

        private void WinClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region WindowMinimize

        internal class ApiCodes
        {
            public const int SC_RESTORE = 0xF120;
            public const int SC_MINIMIZE = 0xF020;
            public const int WM_SYSCOMMAND = 0x0112;
        }

        private IntPtr hWnd;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == ApiCodes.WM_SYSCOMMAND)
            {
                if (wParam.ToInt32() == ApiCodes.SC_MINIMIZE)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Minimized;
                    handled = true;
                }
                else if (wParam.ToInt32() == ApiCodes.SC_RESTORE)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.None;
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
        #endregion

        #region PagesEvents

        void Window_BackToHome(object sender, EventArgs e)
        {
            MainFrame.NavigationService.RemoveBackEntry();
            MainFrame.Content = _homePage;
        }

        void AppSettings_Update(object sender, EventArgs e)
        {
            Topmost = Settings.Default.PIN_APP;
        }

        void HomePage_ShowSettings(object sender, EventArgs e)
        {
            MainFrame.NavigationService.RemoveBackEntry();
            MainFrame.Content = _appSettings;
        }

        void HomePage_SetUpPage(object sender, EventArgs e)
        {
            MainFrame.NavigationService.RemoveBackEntry();
            MainFrame.Content = _setUpPage;
        }

        #endregion
    }
}
