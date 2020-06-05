namespace NesLib.Devices.PpuEntities
{
    public class Pixel
    {
        public byte Value { get; set; }
        public byte Palette { get; set; }

        public Pixel(byte value, byte palette)
        {
            Value = value;
            Palette = palette;
        }

        public Pixel() : this(0, 0)
        {
        }

        public static Pixel Pick(Pixel background, Pixel foreground, bool foregroundPriority)
        {
            if(background.Value == 0 && foreground.Value == 0)
            {
                return new Pixel();
            }

            if (background.Value == 0 || (foreground.Value > 0 && foregroundPriority))
            {
                return foreground;
            }

            return background;
        }
    }
}
