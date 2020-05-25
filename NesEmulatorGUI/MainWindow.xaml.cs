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

        private Controller controller1 = new Controller();
        private Controller controller2 = new Controller();

        public MainWindow()
        {
            InitializeComponent();
            nes = new Nes();

            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

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
                stopwatch.Stop();
                long currentTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Start();

                if(currentTime - previousFrameTime < 12)
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
    }
}
