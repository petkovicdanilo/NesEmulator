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

namespace NesEmulatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Nes nes;
        private int counter = 0;

        public MainWindow()
        {
            InitializeComponent();
            nes = new Nes();

            RenderOptions.SetBitmapScalingMode(NesScreen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(NesScreen, EdgeMode.Aliased);

            CompositionTarget.Rendering += RenderNesScreen;
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

            if (counter == 15)
            {
                using (FileStream stream5 = new FileStream("screen.bmp", FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(nes.Screen()));
                    encoder5.Save(stream5);
                }
            }
            counter++;

            // Determine frame rate in fps (frames per second).
            long frameRate = (long)(_frameCounter / this._stopwatch.Elapsed.TotalSeconds);
            if (frameRate > 0)
            {
                // Update elapsed time, number of frames, and frame rate.

            }
        }
    }
}
