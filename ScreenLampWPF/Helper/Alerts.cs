using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenLampWPF.Helper
{
    public class Alerts
    {
        public void ThrowErrorWindow(string messageError, string stackTrace, string suggestMessage = "", string header = "ScreenLamp")
        {

            string messageText = messageError + " " + suggestMessage;

            messageText += "\n\nShow full message error?";

            var result = MessageBox.Show(
                messageText,
                header,
                MessageBoxButton.YesNo,
                MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    messageError + stackTrace,
                    header + " - " + "Stack trace of error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        public void ThrowWarningWindow(string suggestMessage, string header = "ScreenLamp")
        {
            MessageBox.Show(
                suggestMessage,
                header,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
