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
using ScreenLampWPF;
using ScreenLampWPF.Properties;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using ScreenLampWPF.Windows;
using ScreenLampWPF.Helper;

namespace ScreenLampWPF.Pages
{
    /// <summary>
    /// Interaction logic for AppSettings.xaml
    /// </summary>
    public partial class AppSettings : Page
    {
        public event EventHandler BackToHome;
        public event EventHandler Update;

        private bool _isEditColorTimeUpdateActive;
        private bool _isMousePositionPicked = true;

        private Point _mousePosition;
        private Alerts _alerts = new Alerts();
        private ColorPalette _cp = new ColorPalette();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point 
        {
            public Int32 X;
            public Int32 Y;
        };

        private ColorPickerContainer _colorPickerContainer;

        BackgroundWorker Worker = new BackgroundWorker();

        public AppSettings()
        {
            InitializeComponent();

            changeColorTime(Settings.Default.COLOR_TIME_UPDATE);
            changePinAppState(Settings.Default.PIN_APP);
            changePixelCoordinates(Settings.Default.X_PIXEL, Settings.Default.Y_PIXEL);

            /*_colorPickerContainer.SendCursorPosition += new EventHandler(ColorPickerContainer_SendCursorPosition);*/

            Worker.DoWork += MousePicker_DoWork;
            Worker.ProgressChanged += MousePicker_ProgressChanged;
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
        }

        private void MousePicker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!_isMousePositionPicked)
                {
                    Thread.Sleep(50);
                } else
                {
                    Worker.ReportProgress(0);
                    break;
                }
            }
        }

        private void MousePicker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            changePixelCoordinates(int.Parse(_mousePosition.X.ToString()), int.Parse(_mousePosition.Y.ToString()));
            Worker.CancelAsync();
            _colorPickerContainer.Hide();

            Settings.Default.X_PIXEL = int.Parse(_mousePosition.X.ToString());
            Settings.Default.Y_PIXEL = int.Parse(_mousePosition.Y.ToString());

            Settings.Default.Save();
        }

        void ColorPickerContainer_SendCursorPosition(object sender, EventArgs e)
        {
            /*_colorPickerContainer.Close();*/
            _mousePosition = _colorPickerContainer._currentMousePosition;
            _isMousePositionPicked = true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (BackToHome != null)
            {
                BackToHome(this, e);
            }
        }

        private void changeColorTime(int milliseconds)
        {
            EditColorTimeUpdateData.Text = milliseconds.ToString() + " ms";
        }

        private void changePinAppState(bool toPin)
        {
            var bc = new BrushConverter();

            ChangePinAppData.Text = (toPin).ToString();
            if (toPin)
            {
                ChangePinAppData.Foreground = _cp.OK;
            } else
            {
                ChangePinAppData.Foreground = _cp.FALSE;
            }

            if (Update != null)
            {
                Update(this, new RoutedEventArgs());
            }
        }

        private void changePixelCoordinates(int xPixel, int yPixel)
        {
            PickPixelCoordinatesData.Text = $"X: {xPixel} - Y: {yPixel}";
        }

        private void EditColorTimeUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditColorTimeUpdateActive)
            {
                var gottenTime = EditColorTimeUpdateData.Text;

                var isInt = int.TryParse(gottenTime, out int gottenTimeInt);

                if (isInt)
                {
                    if (gottenTimeInt < 75 || gottenTimeInt > 5000)
                    {
                        _alerts.ThrowWarningWindow("Choose number between 75 and 5000. Try again.");

                        changeColorTime(Settings.Default.COLOR_TIME_UPDATE);

                        EditColorTimeUpdateData.IsReadOnly = true;
                        _isEditColorTimeUpdateActive = false;
                    }
                    else
                    {
                        changeColorTime(gottenTimeInt);

                        Settings.Default.COLOR_TIME_UPDATE = gottenTimeInt;
                        Settings.Default.Save();

                        EditColorTimeUpdateData.IsReadOnly = true;
                        _isEditColorTimeUpdateActive = false;
                    }
                } else
                {
                    _alerts.ThrowWarningWindow("Input data is not a number or is incorrect. Try again.");

                    changeColorTime(Settings.Default.COLOR_TIME_UPDATE);

                    EditColorTimeUpdateData.IsReadOnly = true;
                    _isEditColorTimeUpdateActive = false;
                }
            } else
            {
                EditColorTimeUpdateData.Text = "";
                EditColorTimeUpdateData.IsReadOnly = false;
                EditColorTimeUpdateData.Focus();

                _isEditColorTimeUpdateActive = true;
            };


        }

        private void ChangePinApp_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.PIN_APP = !Settings.Default.PIN_APP;
            changePinAppState(Settings.Default.PIN_APP);

            Settings.Default.Save();
        }

        private void PickPixelCoordinates_Click(object sender, RoutedEventArgs e)
        {
            if (_isMousePositionPicked)
            {
                Worker.RunWorkerAsync();
                _isMousePositionPicked = false;


                _colorPickerContainer = new ColorPickerContainer();
                _colorPickerContainer.SendCursorPosition += new EventHandler(ColorPickerContainer_SendCursorPosition);
                _colorPickerContainer.Show();
            }
        }
    }
}
