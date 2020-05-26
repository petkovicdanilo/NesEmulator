using NesLib;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using NesLib.Devices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Win32;

namespace NesEmulatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Nes nes;
        private Thread nesThread;
        private WriteableBitmap nesScreenBitmap;
        private ManualResetEvent nesResetEvent = new ManualResetEvent(false);
        private bool nesRunning = false;

        private Controller controller1 = new Controller();
        private Controller controller2 = new Controller();

        public MainWindow()
        {
            InitializeComponent();
            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            nes = new Nes();
            // DEBUG ONLY
            nes.InsertCartridge(@"C:\Users\Danilo\Desktop\NES games\Super Mario Bros. (World).nes");
            nesResetEvent.Set();
            nesRunning = true;

            RenderOptions.SetBitmapScalingMode(NesScreen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(NesScreen, EdgeMode.Aliased);

            nesScreenBitmap = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgr32, null);
            NesScreen.Source = nesScreenBitmap;

            nesThread = new Thread(new ThreadStart(RunNes));
            nesThread.IsBackground = true;
            nesThread.Priority = ThreadPriority.Highest;
            nesThread.Start();
        }

        //private static List<long> timesTaken = new List<long>();
        //static void OnProcessExit(object sender, EventArgs e)
        //{
        //    using(StreamWriter sw = new StreamWriter("frameTimes.txt"))
        //    {
        //        foreach(var time in timesTaken)
        //        {
        //            sw.WriteLine(time);
        //        }

        //        sw.WriteLine("===========");
        //        sw.WriteLine(timesTaken.FindAll(time => time != 12).Count);
        //    }
        //}

        private void RunNes()
        {
            nes.controllers[0] = controller1;
            nes.controllers[1] = controller2;

            Stopwatch stopwatch = Stopwatch.StartNew();
            long previousFrameTime = 0;
            while (true)
            {
                nesResetEvent.WaitOne();
                stopwatch.Stop();
                long currentTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Start();

                if(currentTime - previousFrameTime < 16)
                {
                    continue;
                }
                //timesTaken.Add(currentTime - previousFrameTime);

                previousFrameTime = currentTime;

                do
                {
                    nes.Clock();
                }
                while (!nes.FrameComplete);
                nes.FrameComplete = false;

                DrawNesScreen();  
            }
        }

        private void DrawNesScreen()
        {
            long backBufferStride = 4 * 256;
            long pBackBuffer = 0;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Lock bitmap in ui thread
                nesScreenBitmap.Lock();
                pBackBuffer = (long)nesScreenBitmap.BackBuffer;
            });

            // Back to the worker thread
            unsafe
            {
                for (int row = 0; row < 240; ++row)
                {
                    for (int col = 0; col < 256; ++col)
                    {
                        int colorData = nes.PixelBuffer[256 * row + col];

                        *((int*)(pBackBuffer + row * backBufferStride + 4 * col)) = colorData;
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // UI thread does post update operations
                nesScreenBitmap.AddDirtyRect(new Int32Rect(0, 0, 256, 240));
                nesScreenBitmap.Unlock();
            });
        }

        #region Controller input
        private void UpdateControllers(Key key, bool value)
        {
            switch (key)
            {
                case Key.Left:
                    controller1.Left = value;
                    break;
                case Key.Right:
                    controller1.Right = value;
                    break;
                case Key.Up:
                    controller1.Up = value;
                    break;
                case Key.Down:
                    controller1.Down = value;
                    break;

                case Key.Q:
                    controller1.Start = value;
                    break;
                case Key.W:
                    controller1.Select = value;
                    break;
                case Key.A:
                    controller1.A = value;
                    break;
                case Key.B:
                    controller1.B = value;
                    break;

                case Key.D4:
                    controller2.Left = value;
                    break;
                case Key.D6:
                    controller2.Right = value;
                    break;
                case Key.D8:
                    controller2.Up = value;
                    break;
                case Key.D5:
                    controller2.Down = value;
                    break;

                case Key.D7:
                    controller2.Start = value;
                    break;
                case Key.D9:
                    controller2.Select = value;
                    break;
                case Key.D1:
                    controller2.A = value;
                    break;
                case Key.D2:
                    controller2.B = value;
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateControllers(e.Key, true);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateControllers(e.Key, false);
        }
        #endregion

        #region Menu items

        #region File
        private void LoadGameCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Pause game
            nesResetEvent.Reset();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".nes";
            dialog.Filter = "NES game images (*.nes)|*.nes";

            if(dialog.ShowDialog() == true)
            {
                nes.InsertCartridge(dialog.FileName);
            }

            // Unpause game
            nesResetEvent.Set();
        }

        private void LoadStateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SaveStateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void ExitCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Game
        private void ResumeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            nesResetEvent.Set();
            nesRunning = true;

            CommandManager.InvalidateRequerySuggested();
        }

        private void ResumeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !nesRunning;
        }

        private void PauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            nesResetEvent.Reset();
            nesRunning = false;
        }
        private void PauseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = nesRunning;
        }

        private void ResetCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void ScreenshotCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Settings
        private void InputSettingsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void WindowSettingsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Help
        private void AboutCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #endregion
    }
}
