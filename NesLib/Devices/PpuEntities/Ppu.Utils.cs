using NesLib.Devices.CartridgeEntities;
using NesLib.Devices.PpuEntities.OAM;
using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities
{
    public partial class Ppu
    {
        public void Reset()
        {
            fineX = 0;
            toggle = false;
            internalBuffer = 0;
            scanLine = 0;
            cycle = 0;
            FrameComplete = false;

            bgNextTileId = 0;
            bgNextTileAttribute = 0;
            bgNextTileLsb = 0;
            bgNextTileMsb = 0;

            bgPatternLow.Register = 0;
            bgPatternHigh.Register = 0;
            bgAttributeLow.Register = 0;
            bgAttributeHigh.Register = 0;

            vRam.Register = 0;
            tRam.Register = 0;
            controlRegister.Register = 0;
            maskRegister.Register = 0;
            statusRegister.Register = 0;

            for (int i = 0; i < 8; ++i)
            {
                currentScanlineSprites[i] = new ObjectAttributeEntry();
            }

            spriteZeroSelected = false;
            spriteZeroRendered = false;
        }

        public void ConnectCartridge(Cartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        private void NextCycle()
        {
            cycle++;

            if (cycle >= 341)
            {
                cycle = 0;
                scanLine++;

                if (scanLine > 260)
                {
                    scanLine = -1;
                    FrameComplete = true;
                }
            }
        }

        private void DrawPixel()
        {
            Pixel bgPixel = new Pixel();

            if (maskRegister.ShowBackground)
            {
                UInt16 bitmask = (UInt16)(0x8000 >> fineX);

                bgPixel.Value = (byte)
                (
                    (((bgPatternHigh.Register & bitmask) != 0 ? 1 : 0) << 1) |
                    ((bgPatternLow.Register & bitmask) != 0 ? 1 : 0)
                );

                bgPixel.Palette = (byte)
                (
                    (((bgAttributeHigh.Register & bitmask) != 0 ? 1 : 0) << 1) |
                    ((bgAttributeLow.Register & bitmask) != 0 ? 1 : 0)
                );
            }

            Pixel fgPixel = new Pixel();
            bool fgPriority = false;
            if (maskRegister.ShowSprites)
            {
                spriteZeroRendered = false;

                for (int i = 0; i < spritesNumber; ++i)
                {
                    var sprite = currentScanlineSprites[i];

                    if (sprite.X == 0)
                    {
                        // we've hit the sprite
                        byte fgPixelLow = (byte)(BitMagic.IsBitSet(spriteShiftLow[i], 7) ? 0x01 : 0x00);
                        byte fgPixelHigh = (byte)(BitMagic.IsBitSet(spriteShiftHigh[i], 7) ? 0x01 : 0x00);

                        fgPixel.Value = (byte)((fgPixelHigh << 1) | fgPixelLow);
                        fgPixel.Palette = sprite.Attributes.Palette;
                        fgPriority = sprite.Attributes.Priority;

                        if (fgPixel.Value != 0)
                        {
                            if (i == 0)
                            {
                                spriteZeroRendered = true;
                            }

                            break;
                        }
                    }
                }
            }

            Pixel pixel = Pixel.Pick(bgPixel, fgPixel, fgPriority);

            if (spriteZeroSelected && spriteZeroRendered && bgPixel.Value > 0 && fgPixel.Value > 0)
            {
                if (maskRegister.ShowBackground && maskRegister.ShowSprites)
                {
                    if (!(maskRegister.ShowBackgroundLeft || maskRegister.ShowSpritesLeft))
                    {
                        if (cycle >= 9 && cycle <= 256)
                        {
                            statusRegister.SpriteZeroHit = true;
                        }
                    }
                    else
                    {
                        if (cycle >= 1 && cycle <= 256)
                        {
                            statusRegister.SpriteZeroHit = true;
                        }
                    }
                }
            }

            if (cycle >= 1 && cycle <= 256 && scanLine >= 0 && scanLine < 240)
            {
                var color = paletteRam.PixelColor(pixel);

                int index = 256 * scanLine + cycle - 1;

                int finalColor = 0;
                finalColor |= (color.R << 16);
                finalColor |= (color.G << 8);
                finalColor |= (color.B << 0);

                PixelBuffer[index] = finalColor;
            }
        }

        private void UpdateShiftRegisters()
        {
            UpdateBackgroundShiftRegisters();
            UpdateSpriteShiftRegisters();
        }

        private bool RenderingEnabled()
        {
            return maskRegister.ShowBackground || maskRegister.ShowSprites;
        }
    }
}
