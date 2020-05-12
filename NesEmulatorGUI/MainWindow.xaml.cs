using NesLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NesLib.Utils;
using NesLib.Devices.PpuEntities.Registers;
using System.ComponentModel;
using System.Threading;

namespace NesEmulatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Nes nes;

        private Thread nesThread;

        private static BackgroundWorker backgroundWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public MainWindow()
        {
            InitializeComponent();
            nes = new Nes();

            RenderOptions.SetBitmapScalingMode(NesScreen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(NesScreen, EdgeMode.Aliased);

            //backgroundWorker.DoWork += BackgroundWorker_DoWork;
            //backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            //backgroundWorker.RunWorkerAsync();

            CompositionTarget.Rendering += RenderNesScreen;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            NesScreen.Source = nes.Screen();
            Console.Write("bla");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_frameCounter++ == 0)
            {
                // Starting timing.
                _stopwatch.Start();
            }

            nes.controllers[0].Left = Keyboard.IsKeyDown(Key.Left);
            nes.controllers[0].Right = Keyboard.IsKeyDown(Key.Right);
            nes.controllers[0].Up = Keyboard.IsKeyDown(Key.Up);
            nes.controllers[0].Down = Keyboard.IsKeyDown(Key.Down);

            nes.controllers[0].Start = Keyboard.IsKeyDown(Key.Q);
            nes.controllers[0].Select = Keyboard.IsKeyDown(Key.W);
            nes.controllers[0].A = Keyboard.IsKeyDown(Key.A);
            nes.controllers[0].B = Keyboard.IsKeyDown(Key.S);

            do
            //{
            //for (int i = 0; i < 100000; ++i)
            {
                nes.Clock();
            }
            //}
            while (!nes.FrameComplete);
            //NesScreen.Source = nes.Screen();
            backgroundWorker.ReportProgress(5);
            nes.FrameComplete = false;

            // Determine frame rate in fps (frames per second).
            long frameRate = (long)(_frameCounter / this._stopwatch.Elapsed.TotalSeconds);
            if (frameRate > 0)
            {
                // Update elapsed time, number of frames, and frame rate.

            }
        }

        private int _frameCounter = 0;
        private Stopwatch _stopwatch = new Stopwatch();

        protected void RenderNesScreen(object sender, EventArgs e)
        {
            if (_frameCounter++ == 0)
            {
                // Starting timing.
                _stopwatch.Start();
            }

            nes.controllers[0].Left = Keyboard.IsKeyDown(Key.Left);
            nes.controllers[0].Right = Keyboard.IsKeyDown(Key.Right);
            nes.controllers[0].Up = Keyboard.IsKeyDown(Key.Up);
            nes.controllers[0].Down = Keyboard.IsKeyDown(Key.Down);

            nes.controllers[0].Start = Keyboard.IsKeyDown(Key.Q);
            nes.controllers[0].Select = Keyboard.IsKeyDown(Key.W);
            nes.controllers[0].A = Keyboard.IsKeyDown(Key.A);
            nes.controllers[0].B = Keyboard.IsKeyDown(Key.S);

            do
            //{
            //for (int i = 0; i < 100000; ++i)
            {
                nes.Clock();
            }
            //}
            while (!nes.FrameComplete);
            NesScreen.Source = nes.Screen();
            nes.FrameComplete = false;

            //if (counter == 15)
            //{
            //    using (FileStream stream5 = new FileStream("screen.bmp", FileMode.Create))
            //    {
            //        PngBitmapEncoder encoder5 = new PngBitmapEncoder();
            //        encoder5.Frames.Add(BitmapFrame.Create(nes.Screen()));
            //        encoder5.Save(stream5);
            //    }
            //}
            //counter++;

            // Determine frame rate in fps (frames per second).
            long frameRate = (long)(_frameCounter / this._stopwatch.Elapsed.TotalSeconds);
            if (frameRate > 0)
            {
                // Update elapsed time, number of frames, and frame rate.

            }
        }
    }
}
