using NesLib.Devices.PpuEntities.OAM;
using NesLib.Utils;
using System;

namespace NesLib.Devices.PpuEntities
{
    public partial class Ppu
    {
        private void EvaluateSprites()
        {
            for (int i = 0; i < spritesNumber; ++i)
            {
                currentScanlineSprites[i].Reset();
            }
            spritesNumber = 0;

            spriteZeroSelected = false;
            statusRegister.SpriteOverflow = false;
            // decide which sprites are visible on current scanline
            for (int i = 0; i < 64; ++i)
            {
                ObjectAttributeEntry currentSprite = Oam.EntryAt(i);
                int diff = scanLine - currentSprite.Y;

                if (diff >= 0 && diff < (controlRegister.SpriteSize ? 16 : 8))
                {
                    if (spritesNumber == 8)
                    {
                        statusRegister.SpriteOverflow = true;
                        break;
                    }

                    // clone because we are modifying X value in each cycle
                    currentScanlineSprites[spritesNumber++] = currentSprite.Clone() as ObjectAttributeEntry;

                    // sprite zero is in current scanline
                    if (i == 0)
                    {
                        spriteZeroSelected = true;
                    }
                }
            }
        }

        private void PopulateSpriteShifters()
        {
            for (int i = 0; i < 8; ++i)
            {
                spriteShiftLow[i] = 0;
                spriteShiftHigh[i] = 0;
            }

            for (int i = 0; i < spritesNumber; ++i)
            {
                var sprite = currentScanlineSprites[i];
                UInt16 addrLow = 0x0000, addrHigh = 0x0000;
                byte patternTable = 0x00, spriteRow = 0x00;
                UInt16 tilePosition = 0x0000;

                if (!controlRegister.SpriteSize)
                {
                    // 8x8
                    patternTable = (byte)((controlRegister.PatternSprite ? 0x01 : 0x00) << 12);
                    tilePosition = (UInt16)(sprite.Id << 4);

                    if (!sprite.Attributes.FlipVertically)
                    {
                        spriteRow = (byte)(scanLine - sprite.Y);
                    }
                    else
                    {
                        spriteRow = (byte)(7 - (scanLine - sprite.Y));
                    }
                }
                else
                {
                    // 8x16
                    patternTable = (byte)((BitMagic.IsBitSet(sprite.Id, 0) ? 0x01 : 0x00) << 12);

                    if (!sprite.Attributes.FlipVertically)
                    {
                        if (scanLine - sprite.Y < 8)
                        {
                            // top half
                            tilePosition = (UInt16)((sprite.Id & 0xFE) << 4);
                        }
                        else
                        {
                            // bottom half
                            tilePosition = (UInt16)(((sprite.Id & 0xFE) + 1) << 4);
                        }

                        spriteRow = (byte)((scanLine - sprite.Y) & 0x07);
                    }
                    else
                    {
                        if (scanLine - sprite.Y < 8)
                        {
                            // top half
                            tilePosition = (UInt16)(((sprite.Id & 0xFE) + 1) << 4);
                        }
                        else
                        {
                            // bottom half
                            tilePosition = (UInt16)((sprite.Id & 0xFE) << 4);
                        }

                        spriteRow = (byte)((7 - (scanLine - sprite.Y)) & 0x07);
                    }
                }

                addrLow = (UInt16)(patternTable | tilePosition | spriteRow);
                addrHigh = (UInt16)(addrLow + 8);

                byte spriteLow = PpuRead(addrLow);
                byte spriteHigh = PpuRead(addrHigh);

                if (sprite.Attributes.FlipHorizontally)
                {
                    spriteLow = BitMagic.Flip(spriteLow);
                    spriteHigh = BitMagic.Flip(spriteHigh);
                }

                spriteShiftLow[i] = spriteLow;
                spriteShiftHigh[i] = spriteHigh;
            }
        }

        private void UpdateSpriteShiftRegisters()
        {
            if (maskRegister.ShowSprites && cycle >= 1 && cycle <= 256)
            {
                for (int i = 0; i < spritesNumber; ++i)
                {
                    var sprite = currentScanlineSprites[i];

                    if (sprite.X > 0)
                    {
                        sprite.X--;
                    }
                    else
                    {
                        spriteShiftLow[i] <<= 1;
                        spriteShiftHigh[i] <<= 1;
                    }
                }
            }
        }
    }
}
