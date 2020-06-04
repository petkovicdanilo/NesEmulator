using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;

namespace NesLib.Devices.PpuEntities
{
    [Serializable]
    public class PaletteRam
    {
        private byte[] palette = new byte[32];

        private static Color[] colors = new Color[]
        {
            Color.FromRgb(84, 84, 84),
            Color.FromRgb(0, 30, 116),
            Color.FromRgb(8, 16, 144),
            Color.FromRgb(48, 0, 136),
            Color.FromRgb(68, 0, 100),
            Color.FromRgb(92, 0, 48),
            Color.FromRgb(84, 4, 0),
            Color.FromRgb(60, 24, 0),
            Color.FromRgb(32, 42, 0),
            Color.FromRgb(8, 58, 0),
            Color.FromRgb(0, 64, 0),
            Color.FromRgb(0, 60, 0),
            Color.FromRgb(0, 50, 60),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),

            Color.FromRgb(152, 150, 152),
            Color.FromRgb(8, 76, 196),
            Color.FromRgb(48, 50, 236),
            Color.FromRgb(92, 30, 228),
            Color.FromRgb(136, 20, 176),
            Color.FromRgb(160, 20, 100),
            Color.FromRgb(152, 34, 32),
            Color.FromRgb(120, 60, 0),
            Color.FromRgb(84, 90, 0),
            Color.FromRgb(40, 114, 0),
            Color.FromRgb(8, 124, 0),
            Color.FromRgb(0, 118, 40),
            Color.FromRgb(0, 102, 120),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),

            Color.FromRgb(236, 238, 236),
            Color.FromRgb(76, 154, 236),
            Color.FromRgb(120, 124, 236),
            Color.FromRgb(176, 98, 236),
            Color.FromRgb(228, 84, 236),
            Color.FromRgb(236, 88, 180),
            Color.FromRgb(236, 106, 100),
            Color.FromRgb(212, 136, 32),
            Color.FromRgb(160, 170, 0),
            Color.FromRgb(116, 196, 0),
            Color.FromRgb(76, 208, 32),
            Color.FromRgb(56, 204, 108),
            Color.FromRgb(56, 180, 204),
            Color.FromRgb(60, 60, 60),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),

            Color.FromRgb(236, 238, 236),
            Color.FromRgb(168, 204, 236),
            Color.FromRgb(188, 188, 236),
            Color.FromRgb(212, 178, 236),
            Color.FromRgb(236, 174, 212),
            Color.FromRgb(236, 174, 236),
            Color.FromRgb(236, 180, 176),
            Color.FromRgb(228, 196, 144),
            Color.FromRgb(204, 210, 120),
            Color.FromRgb(180, 222, 120),
            Color.FromRgb(168, 226, 144),
            Color.FromRgb(152, 226, 180),
            Color.FromRgb(160, 214, 228),
            Color.FromRgb(160, 162, 160),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0)
        };

        public Color PixelColor(Pixel pixel)
        {
            UInt16 address = (UInt16)((0x3F00 + (pixel.Palette << 2) + pixel.Value));

            return colors[Read(address) & 0x3F];
        }

        private UInt16 MaskAddress(UInt16 address)
        {
            address &= 0x001F;

            if (address == 0x0010 || address == 0x0014 || address == 0x0018 || address == 0x001C)
            {
                address &= 0x000F;
            }

            return address;
        }

        public byte Read(UInt16 address)
        {
            address = MaskAddress(address);
            return palette[address];
        }

        public void Write(UInt16 address, byte data)
        {
            address = MaskAddress(address);
            palette[address] = data;
        }
    }
}
