using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;

namespace ScreenLampWPF.Windows
{
    /// <summary>
    /// Interaction logic for ColorPickerContainer.xaml
    /// </summary>
    public partial class ColorPickerContainer : Window
    {
        public event EventHandler SendCursorPosition;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        BackgroundWorker Worker = new BackgroundWorker();

        private bool _isEditColorPickerActive;
        public Point _currentMousePosition;

        public ColorPickerContainer()
        {
            InitializeComponent();

            Worker.DoWork += MouseListening_DoWork;
            Worker.ProgressChanged += MouseListening_ProgressChanged;
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            _isEditColorPickerActive = true;
            Worker.RunWorkerAsync();
        }

        /*public void RunMouseListening ()
        {
            _isEditColorPickerActive = true;
            Worker.RunWorkerAsync();
        }*/

        private void MouseListening_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (_isEditColorPickerActive)
                {
                    Worker.ReportProgress(0);
                    Thread.Sleep(50);
                }
                else
                {
                    if (SendCursorPosition != null)
                    {
                        SendCursorPosition(this, e);
                    }
                    break;
                }
            }
        }

        private void MouseListening_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GetMousePosition();
        }

        public void GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            _currentMousePosition = new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(_currentMousePosition);
            _isEditColorPickerActive = false;
            Worker.CancelAsync();
        }
    }
}
