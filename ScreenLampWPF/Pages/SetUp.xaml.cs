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
using ScreenLampWPF.Properties;
using LastTryTapo;
using ScreenLampWPF.Helper;

namespace ScreenLampWPF.Pages
{
    /// <summary>
    /// Interaction logic for SetUp.xaml
    /// </summary>
    public partial class SetUp : Page
    {
        public event EventHandler BackToHome;

        private bool _isEditEmailActive;
        private bool _isEditPasswordActive;
        private bool _isEditDeviceIPActive;

        private string _email;
        private string _password;
        private string _deviceIP;

        private ColorPalette _cp = new ColorPalette();
        private Alerts _alerts = new Alerts();

        public SetUp()
        {
            InitializeComponent();

            _password = Settings.Default.PASSWORD;
            _email = Settings.Default.EMAIL;
            _deviceIP = Settings.Default.DEVICE_IP;

            EmailData.Foreground = _cp.OK;
            PasswordData.Foreground = _cp.OK;
            DeviceIPData.Foreground = _cp.OK;

            EmailData.Text = blurEmail(_email);
            PasswordData.Text = blurString(_password);
            DeviceIPData.Text = Settings.Default.DEVICE_IP;
        }

        private void EmailUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditEmailActive)
            {
                var email = EmailData.Text;
                var splittedEmail = email.Split("@", StringSplitOptions.RemoveEmptyEntries);

                if (splittedEmail.Length == 2)
                {
                    if (splittedEmail[0].Length < 6 || splittedEmail[0].Length > 30)
                    {
                        _alerts.ThrowWarningWindow("Invalid username of email. Try again.");

                        EmailData.Text = blurEmail(_email);
                    }
                    else
                    {
                        _email = email;

                        EmailData.Text = blurEmail(email);
                    }
                } else
                {
                    _alerts.ThrowWarningWindow("Invalid format of email. Try again.");

                    EmailData.Text = blurEmail(_email);
                }

                EmailData.Foreground = _cp.EDITED;
                EmailData.IsReadOnly = true;
                _isEditEmailActive = false;
            } else
            {
                EmailData.Text = "";
                EmailData.IsReadOnly = false;
                EmailData.Focus();

                _isEditEmailActive = true;

                EmailData.Foreground = _cp.BASIC;
            }
        }

        private void PasswordUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditPasswordActive)
            {
                var password = PasswordData.Text;

                if (password.Length < 4 || password.Length > 32)
                {
                    _alerts.ThrowWarningWindow("Invalid password format. Password length should be between 4 and 32.\nIf you have password length more than 32 characters, you can make new issue here: https://github.com/MarkSmersh/ScreenLampWPF/issues");

                    PasswordData.Text = blurString(_password);
                } else
                {
                    _password = password;

                    PasswordData.Text = blurString(password);
                }

                PasswordData.Foreground = _cp.EDITED;
                PasswordData.IsReadOnly = true;
                _isEditPasswordActive = false;
            } else
            {
                PasswordData.Text = "";
                PasswordData.IsReadOnly = false;
                PasswordData.Focus();

                _isEditPasswordActive = true;

                PasswordData.Foreground = _cp.BASIC;
            }
        }

        private void DeviceIPUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditDeviceIPActive)
            {
                var deviceIP = DeviceIPData.Text;
                var splittedDeviceIP = deviceIP.Split(".", StringSplitOptions.RemoveEmptyEntries);

                if (splittedDeviceIP.Length != 4 || deviceIP.Length < 7 || deviceIP.Length > 15)
                {
                    _alerts.ThrowWarningWindow("Invalid device ip format. Try again.\nFor example: 192.168.1.255");

                    DeviceIPData.Text = _deviceIP;
                } else
                {
                    _deviceIP = deviceIP;

                    DeviceIPData.Text = deviceIP;
                }

                DeviceIPData.Foreground = _cp.EDITED;
                DeviceIPData.IsReadOnly = true;
                _isEditDeviceIPActive = false;
            } else
            {
                DeviceIPData.Text = "";
                DeviceIPData.IsReadOnly = false;
                DeviceIPData.Focus();

                _isEditDeviceIPActive = true;

                DeviceIPData.Foreground = _cp.BASIC;
            }
        }

        private string blurString (string data)
        {
            string result = "";

            for (int i = 0; i < data.Length; i++)
            {
                result += "*";
            };

            return result;
        }

        private string blurEmail (string email)
        {
            var splittedEmail = email.Split("@", StringSplitOptions.RemoveEmptyEntries);

            if (splittedEmail.Length == 2)
            {
                return blurString(splittedEmail[0]) + "@" + splittedEmail[1];
            } else
            {
                return "";
            }
        }

        private async void EnterData_Click(object sender, RoutedEventArgs e)
        {
            if (_email != "" || _password != "" || _deviceIP != "")
            {
                var _tapoConnection = new TapoConnection(_email, _password, _deviceIP);

                Console.WriteLine(_email);
                Console.WriteLine(_password);
                Console.WriteLine(_deviceIP);

                try
                {
                    var token = await _tapoConnection.LoginWithIP();
                    Console.WriteLine(token);
                } catch (Exception ex)
                {
                    _alerts.ThrowErrorWindow(ex.Message, ex.StackTrace,
                        "Some user data are invalid or device is not on.");

                    EmailData.Foreground = _cp.EDITED;
                    PasswordData.Foreground = _cp.EDITED;
                    DeviceIPData.Foreground = _cp.EDITED;

                    return;
                };

                EmailData.Foreground = _cp.OK;
                PasswordData.Foreground = _cp.OK;
                DeviceIPData.Foreground = _cp.OK;

                Settings.Default.EMAIL = _email;
                Settings.Default.PASSWORD = _password;
                Settings.Default.DEVICE_IP = _deviceIP;
                Settings.Default.IS_LOGINED = true;

                Settings.Default.Save();
            } else
            {
                _alerts.ThrowWarningWindow("Some user data is missing. Try again.");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (BackToHome != null)
            {
                BackToHome(this, e);
            }
        }
    }
}
