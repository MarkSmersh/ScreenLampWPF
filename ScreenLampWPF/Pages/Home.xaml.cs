using ScreenLampWPF.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using LastTryTapo;
using LastTryTapo.Models;
using ScreenLampWPF.Helper;

namespace ScreenLampWPF.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private string oldColor;
        bool IsOn = false;

        private BackgroundWorker Worker = new BackgroundWorker();
        private Alerts _alerts = new Alerts();

        private TapoConnection _tapoConnection;
        private DeviceInfo _tapoToken;

        public event EventHandler ShowSettings;
        public event EventHandler ShowSetUp;

        public Home()
        {
            InitializeComponent();

            Worker.DoWork += ColorListening_DoWork;
            Worker.ProgressChanged += ColorListening_ProgressChanged;
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
        }

        private async void TurnButton_Click(object sender, RoutedEventArgs e)
        {

            if (TurnButton.IsChecked == true)
            {
                if (Settings.Default.IS_LOGINED == false)
                {
                    _alerts.ThrowWarningWindow("You are not logined now. Go to settings - and login, using your Tapo e-mail, password and device IP. If you need help, go to the FAQ.");
                    TurnButton.IsChecked = false;
                    return;
                }

                _tapoConnection = new TapoConnection(
                    Settings.Default.EMAIL,
                    Settings.Default.PASSWORD,
                    Settings.Default.DEVICE_IP);

                try
                {
                    _tapoToken = await _tapoConnection.LoginWithIP();
                }
                catch (Exception ex)
                {
                    _alerts.ThrowErrorWindow(ex.Message, ex.StackTrace, "Try stop spamming toggle button to fix this problem.");
                    TurnButton.IsChecked = false;
                    return;
                }

                IsOn = true;

                try
                {

                    Worker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    _alerts.ThrowErrorWindow(ex.Message, ex.StackTrace);
                    TurnButton.IsChecked = false;
                    return;
                }
            }
            else
            {
                Worker.CancelAsync();
                IsOn = false;
                Thread.Sleep(Settings.Default.COLOR_TIME_UPDATE);
            }
        }

        private void ToSettings_Click(object sender, RoutedEventArgs e)
        {
            if (ShowSettings != null)
            {
                ShowSettings(this, e);
            }
        }
        private void ToSetUpDevice_Click(object sender, RoutedEventArgs e)
        {
            if (ShowSetUp != null)
            {
                ShowSetUp(this, e);
            }
        }

        private void ColorListening_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (IsOn)
                {
                    Worker.ReportProgress(0);
                    Thread.Sleep(Settings.Default.COLOR_TIME_UPDATE);
                }
                else
                {
                    break;
                }
            }
        }

        private async void ColorListening_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string color = ColorTaker.Take(Settings.Default.X_PIXEL, Settings.Default.Y_PIXEL); // 960, 540 center
            if (oldColor != color)
            {
                DeviceIdBlock.Text = color;
                var bc = new BrushConverter();
                DeviceIdBlock.Foreground = (Brush)bc.ConvertFrom(color);
                Console.WriteLine(color);

                await _tapoConnection.setColour(color, _tapoToken);
                oldColor = color;
            }
        }
    }
}
