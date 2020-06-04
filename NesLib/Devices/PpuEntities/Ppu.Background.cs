using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities
{
    public partial class Ppu
    {

        private void PreloadData()
        {
            switch ((cycle - 1) % 8)
            {
                case 0:
                    LoadShifters();
                    PreloadNametableData();
                    break;
                case 2:
                    PreloadAttributeData();
                    break;
                case 4:
                    PreloadBackgroundTileLow();
                    break;
                case 6:
                    PreloadBackgroundTileHigh();
                    break;
                case 7:
                    if (RenderingEnabled())
                    {
                        vRam.IncrementX();
                    }
                    break;
            }
        }

        private void PreloadNametableData()
        {
            UInt16 nametableOffset = 0x2000;

            // extract nametable and coarse data from vRam register
            // offset it to nametable address range
            UInt16 address = (UInt16)(nametableOffset | (vRam.Register & 0x0FFF));

            bgNextTileId = PpuRead(address);
        }

        private void PreloadAttributeData()
        {
            UInt16 attribMemoryOffset = 0x23C0;

            UInt16 address = (UInt16)
            (
                attribMemoryOffset |
                ((vRam.NametableY ? 1 : 0) << 11) |
                ((vRam.NametableX ? 1 : 0) << 10) |
                ((vRam.CoarseY >> 2) << 3) |
                (vRam.CoarseX >> 2)
            );

            bgNextTileAttribute = PpuRead(address);

            if ((vRam.CoarseY & 0x02) != 0)
            {
                bgNextTileAttribute >>= 4;
            }
            if ((vRam.CoarseX & 0x02) != 0)
            {
                bgNextTileAttribute >>= 2;
            }

            // we need only bottom two bits
            bgNextTileAttribute &= 0x03;
        }

        private void PreloadBackgroundTileLow()
        {
            // choose pattern table
            UInt16 patternId = (UInt16)((controlRegister.PatternBackground ? 1 : 0) << 12);

            //
            UInt16 tileId = (UInt16)(bgNextTileId << 4);
            UInt16 offset = (UInt16)(vRam.FineY);

            UInt16 address = (UInt16)(patternId | tileId | offset);

            bgNextTileLsb = PpuRead(address);
        }

        private void PreloadBackgroundTileHigh()
        {
            // choose pattern table
            UInt16 patternId = (UInt16)((controlRegister.PatternBackground ? 1 : 0) << 12);

            //
            UInt16 tileId = (UInt16)(bgNextTileId << 4);
            UInt16 offset = (UInt16)(vRam.FineY + 8);

            UInt16 address = (UInt16)(patternId | tileId | offset);

            bgNextTileMsb = PpuRead(address);
        }

        private void LoadShifters()
        {
            bgPatternLow.Load(bgNextTileLsb);
            bgPatternHigh.Load(bgNextTileMsb);

            byte nextAttributeLowVal = (byte)(BitMagic.IsBitSet(bgNextTileAttribute, 0) ? 0xFF : 0x00);
            bgAttributeLow.Load(nextAttributeLowVal);

            byte nextAttributeHighVal = (byte)(BitMagic.IsBitSet(bgNextTileAttribute, 1) ? 0xFF : 0x00);
            bgAttributeHigh.Load(nextAttributeHighVal);
        }


        private void UpdateBackgroundShiftRegisters()
        {
            if (maskRegister.ShowBackground)
            {
                bgPatternLow.Shift();
                bgPatternHigh.Shift();
                bgAttributeLow.Shift();
                bgAttributeHigh.Shift();
            }
        }

    }
}
