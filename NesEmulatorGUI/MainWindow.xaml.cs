﻿using NesLib;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NesEmulatorGUI.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NesLib.Devices.CartridgeEntities.Exceptions;

namespace NesEmulatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Nes nes;
        private Thread nesThread;
        private WriteableBitmap nesScreenBitmap;
        private ManualResetEvent nesResetEvent = new ManualResetEvent(false);
        private bool nesRunning = false;

        private bool gameInserted = false;

        private int _nesHeight = 720;
        public int NesHeight
        {
            get
            {
                return _nesHeight;
            }

            set
            {
                _nesHeight = value;
                OnPropertyChanged();
            }
        }

        private int _nesWidth = 768;
        public int NesWidth
        {
            get
            {
                return _nesWidth;
            }

            set
            {
                _nesWidth = value;
                OnPropertyChanged();
            }
        }


        private int _nesScale = 3;
        public int NesScale
        {
            get
            {
                return _nesScale;
            }

            set
            {
                _nesScale = value;
                NesHeight = value * 240;
                NesWidth = value * 256;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            nes = new Nes();

            RenderOptions.SetBitmapScalingMode(NesScreen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(NesScreen, EdgeMode.Aliased);

            nesScreenBitmap = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgr32, null);
            NesScreen.Source = nesScreenBitmap;

            nesThread = new Thread(new ThreadStart(RunNes));
            nesThread.IsBackground = true;
            nesThread.Priority = ThreadPriority.Highest;
            nesThread.Start();
        }

        private void RunNes()
        {
            nes.controllers[0] = ControllerManager.Instance.Controllers[0];
            nes.controllers[1] = ControllerManager.Instance.Controllers[1];

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
            ControllerManager.Instance.SetKeyPressed(e.Key, true);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            ControllerManager.Instance.SetKeyPressed(e.Key, false);
        }
        #endregion

        #region Menu items

        #region File
        private void LoadGameCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                NesPause();

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.DefaultExt = ".nes";
                dialog.Filter = "NES game images (*.nes)|*.nes";

                if (dialog.ShowDialog() == true)
                {
                    nes.InsertCartridge(dialog.FileName);
                    UpdateWindowTitle();
                    
                    gameInserted = true;
                }
            }
            catch(MapperNotSupportedException ex)
            {
                MessageBox.Show
                (
                    ex.Message, 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );
            }
            catch(Exception)
            {
                MessageBox.Show
                   (
                       "Failed to load game for unknown reason",
                       "Error",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error
                   );
            }
            finally
            {
                NesResume();
            }
        }

        private void LoadStateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
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
                        nes = formatter.Deserialize(fileStream) as Nes;

                        // Reattach controllers
                        nes.controllers[0] = ControllerManager.Instance.Controllers[0];
                        nes.controllers[1] = ControllerManager.Instance.Controllers[1];

                        UpdateWindowTitle();

                        gameInserted = true;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "Failed to load state file",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                NesResume();
            }
        }

        private void SaveStateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                NesPause();

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.OverwritePrompt = true;
                dialog.ValidateNames = true;
                dialog.DefaultExt = ".state";
                dialog.Filter = "NES save state (*.state)|*.state";
                dialog.Title = "Save emulator state";
                dialog.FileName = $"{nes.GameName} {DateTime.Now.ToString("yyyy-MM-dd HH-MM-ss")}.state";

                if (dialog.ShowDialog() == true)
                {
                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(fileStream, nes);
                    }

                }
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "Failed to save state file",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                NesResume();
            }
        }

        private void SaveStateCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gameInserted;
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
            e.CanExecute = gameInserted && !nesRunning;
        }

        private void PauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NesPause();
        }
        private void PauseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gameInserted && nesRunning;
        }

        private void ResetCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            nes.Reset();
            NesResume();
        }

        private void ResetCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gameInserted;
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

            NesPause();

            if (dialog.ShowDialog() == true)
            {
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
            }

            NesResume();
        }

        private void ScreenshotCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gameInserted;
        }

        #endregion

        #region Settings
        private void InputSettingsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var inputSettingsWindow = new InputSettingsWindow();
            inputSettingsWindow.Owner = this;

            NesPause();
            inputSettingsWindow.ShowDialog();
            NesResume();
        }

        private void EmulatorSettingsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var emulatorSettingsWindow = new EmulatorSettingsWindow(this);
            emulatorSettingsWindow.Owner = this;

            NesPause();
            emulatorSettingsWindow.ShowDialog();
            NesResume();
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
            if(gameInserted)
            {
                nesResetEvent.Set();
                nesRunning = true;
            }
        }

        private void UpdateWindowTitle()
        {
            Title = nes.GameName;
        }
    }
}
