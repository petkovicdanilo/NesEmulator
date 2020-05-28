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
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NesEmulatorGUI.Windows;

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
            // **
            nes.InsertCartridge(@"C:\Users\Danilo\Desktop\NES games\Super Mario Bros. (World).nes");
            nesResetEvent.Set();
            nesRunning = true;
            UpdateWindowTitle();
            // **

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
            nes.controllers[0] = ControllerManager.controllers[0];
            nes.controllers[1] = ControllerManager.controllers[1];

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
                
                nes.DoOneFrame();

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
      
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ControllerManager.SetKeyPressed(e.Key, true);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            ControllerManager.SetKeyPressed(e.Key, false);
        }
        #endregion

        #region Menu items

        #region File
        private void LoadGameCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NesPause();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".nes";
            dialog.Filter = "NES game images (*.nes)|*.nes";

            if(dialog.ShowDialog() == true)
            {
                nes.InsertCartridge(dialog.FileName);
                UpdateWindowTitle();
            }

            NesResume();
        }

        private void LoadStateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NesPause();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".state";
            dialog.Filter = "NES save state (*.state)|*.state";

            if(dialog.ShowDialog() == true)
            {
                IFormatter formatter = new BinaryFormatter();
                using (var fileStream = new FileStream(dialog.FileName, FileMode.Open))
                {
                    try
                    {
                        nes = formatter.Deserialize(fileStream) as Nes;

                        // Reattach controllers
                        nes.controllers[0] = ControllerManager.controllers[0];
                        nes.controllers[1] = ControllerManager.controllers[1];

                        UpdateWindowTitle();
                    }
                    catch(Exception)
                    {
                        MessageBox.Show("Failed to load state file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }

            NesResume();
        }

        private void SaveStateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NesPause();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.OverwritePrompt = true;
            dialog.ValidateNames = true;
            dialog.DefaultExt = ".state";
            dialog.Filter = "NES save state (*.state)|*.state";
            dialog.Title = "Save emulator state";
            dialog.FileName = $"{nes.GameName} {DateTime.Now.ToString("yyyy-MM-dd HH-MM-ss")}.state";

            if(dialog.ShowDialog() == true)
            {
                using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    IFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(fileStream, nes);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to save state file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            NesResume();
        }

        private void ExitCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Game
        private void ResumeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NesResume();
        }

        private void ResumeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !nesRunning;
        }

        private void PauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NesPause();
        }
        private void PauseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = nesRunning;
        }

        private void ResetCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            nes.Reset();
            NesResume();
        }

        private void ScreenshotCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.OverwritePrompt = true;
            dialog.ValidateNames = true;
            dialog.DefaultExt = ".png";
            dialog.Filter = "NES screenshot (*.png)|*.png";
            dialog.Title = "Save screenshot as";
            dialog.FileName = $"{nes.GameName} {DateTime.Now.ToString("yyyy-MM-dd HH-MM-ss")}.png";

            if (dialog.ShowDialog() == true)
            {
                NesPause();

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap
                (
                    (int) NesScreen.Width, 
                    (int) NesScreen.Height, 
                    96, 
                    96,
                    PixelFormats.Pbgra32
                );

                renderTargetBitmap.Render(NesScreen);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (var fileStream = new FileStream(dialog.FileName, FileMode.CreateNew))
                {
                    encoder.Save(fileStream);
                }

                NesResume();
            }

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
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;

            NesPause();
            aboutWindow.ShowDialog();
            NesResume();
        }
        #endregion

        #endregion

        private void NesPause()
        {
            nesResetEvent.Reset();
            nesRunning = false;
        }

        private void NesResume()
        {
            nesResetEvent.Set();
            nesRunning = true;
        }

        // TODO solve with binding
        private void UpdateWindowTitle()
        {
            Title = nes.GameName;
        }
    }
}
