using NesLib;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        public MainWindow()
        {
            InitializeComponent();
            nes = new Nes();

            RenderOptions.SetBitmapScalingMode(NesScreen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(NesScreen, EdgeMode.Aliased);

            CompositionTarget.Rendering += RenderNesScreen;
        }

        protected void RenderNesScreen(object sender, EventArgs e)
        {
            nes.controllers[0].Left = Keyboard.IsKeyDown(Key.Left);
            nes.controllers[0].Right = Keyboard.IsKeyDown(Key.Right);
            nes.controllers[0].Up = Keyboard.IsKeyDown(Key.Up);
            nes.controllers[0].Down = Keyboard.IsKeyDown(Key.Down);

            nes.controllers[0].Start = Keyboard.IsKeyDown(Key.Q);
            nes.controllers[0].Select = Keyboard.IsKeyDown(Key.W);
            nes.controllers[0].A = Keyboard.IsKeyDown(Key.A);
            nes.controllers[0].B = Keyboard.IsKeyDown(Key.S);

            //Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                nes.Clock();
            }
            while (!nes.FrameComplete);
            //stopwatch.Stop();
            NesScreen.Source = nes.Screen;
            nes.FrameComplete = false;
        }
    }
}
