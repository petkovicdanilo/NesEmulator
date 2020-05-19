using NesLib.Devices.CartridgeEntities;
using NesLib.Devices.PpuEntities.OAM;
using NesLib.Devices.PpuEntities.Registers;
using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NesLib.Devices.PpuEntities
{
    public class Ppu : IClockDevice, ICpuBusDevice, IPpuBusDevice
    {
        private Nes nes;

        private Cartridge cartridge;

        private StatusRegister statusRegister = new StatusRegister();
        private MaskRegister maskRegister = new MaskRegister();
        private ControlRegister controlRegister = new ControlRegister();
        private LoopyRegister vRam = new LoopyRegister();
        private LoopyRegister tRam = new LoopyRegister();

        private byte fineX;

        // vram
        private Nametable nametable = new Nametable();
        // palettes
        private PaletteRam paletteRam = new PaletteRam();
        // oam - sprites
        public ObjectAttributeMemory Oam = new ObjectAttributeMemory();

        public WriteableBitmap Screen = BitmapFactory.New(256, 240);

        private int cycle = 0;
        private int scanLine = 0;

        private bool toggle = false;
        private byte internalBuffer;

        private bool _emitNmi = false;
        public bool EmitNmi
        {
            get
            {
                bool oldValue = _emitNmi;
                _emitNmi = false;
                return oldValue;
            }

            private set
            {
                _emitNmi = value;
            }

        }

        private byte bgNextTileId = 0x00;
        private byte bgNextTileAttribute = 0x00;
        private byte bgNextTileLsb = 0x00;
        private byte bgNextTileMsb = 0x00;
        private ShiftRegister bgPatternLow = new ShiftRegister();
        private ShiftRegister bgPatternHigh = new ShiftRegister();
        private ShiftRegister bgAttributeLow = new ShiftRegister();
        private ShiftRegister bgAttributeHigh = new ShiftRegister();

        private byte oamAddress = 0x00;

        private ObjectAttributeEntry[] currentScanlineSprites = new ObjectAttributeEntry[8];
        private byte[] spriteShiftLow = new byte[8];
        private byte[] spriteShiftHigh = new byte[8];
        private int spritesNumber = 0;

        public bool FrameComplete { get; set; } = false;

        public Ppu(Nes nes)
        {
            this.nes = nes;

            for(int i = 0; i < 8; ++i)
            {
                currentScanlineSprites[i] = new ObjectAttributeEntry();
            }
        }

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

            for(int i = 0; i < 8; ++i)
            {
                currentScanlineSprites[i] = new ObjectAttributeEntry();
            }
        }

        public void ConnectCartridge(Cartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        public byte CpuRead(UInt16 address, bool debugMode = false)
        {
            switch (address)
            {
                // Control
                case 0x0000:
                    break;

                // Mask
                case 0x0001:
                    break;

                // Status
                case 0x0002:
                    // lower bits are noise
                    byte data = (byte)((statusRegister.Register & 0xE0) | (internalBuffer & 0x1F));
                    if (!debugMode)
                    {
                        statusRegister.VerticalBlank = false;
                    }
                    toggle = false;
                    return data;

                // OAM Address
                case 0x0003: 
                    break;

                // OAM Data
                case 0x0004:
                    return Oam[oamAddress];

                // Scroll
                case 0x0005: 
                    break;

                // PPU Address
                case 0x0006:
                    break;

                // PPU Data
                case 0x0007:
                    data = internalBuffer;
                    internalBuffer = PpuRead(vRam.Register);

                    if(vRam.Register >= 0x3F00)
                    {
                        data = internalBuffer;
                    }

                    vRam.Increment(controlRegister.IncrementMode);
                    return data;
            }
            return 0;
        }

        public void CpuWrite(UInt16 address, byte data)
        {
            switch (address)
            {
                // Control
                case 0x0000:
                    controlRegister.Register = data;
                    tRam.NametableX = controlRegister.NametableX;
                    tRam.NametableY = controlRegister.NametableY;
                    break;

                // Mask
                case 0x0001:
                    maskRegister.Register = data;
                    break;

                // Status
                case 0x0002:
                    break;

                // OAM Address
                case 0x0003:
                    oamAddress = data;
                    break;

                // OAM Data
                case 0x0004:
                    Oam[oamAddress] = data;
                    break;

                // Scroll
                case 0x0005:
                    if (!toggle)
                    {
                        toggle = true;
                        fineX = (byte)(data & 0x07);
                        tRam.CoarseX = (byte)(data >> 3);
                    }
                    else
                    {
                        toggle = false;
                        tRam.FineY = (byte)(data & 0x07);
                        tRam.CoarseY = (byte)(data >> 3);
                    }
                    break;

                // PPU Address
                case 0x0006:
                    if (!toggle)
                    {
                        toggle = true;
                        // we only need 6 lower bytes
                        data &= 0x3F;
                        tRam.Register = (UInt16)((tRam.Register & 0x00FF) | (data << 8));
                    }
                    else
                    {
                        toggle = false;
                        tRam.Register = (UInt16)((tRam.Register & 0xFF00) | data);
                        vRam.Register = tRam.Register;
                    }
                    break;

                // PPU Data
                case 0x0007:
                    PpuWrite(vRam.Register, data);
                    vRam.Increment(controlRegister.IncrementMode);
                    break;
            }
        }

        public byte PpuRead(UInt16 address)
        {
            address &= 0x3FFF;

            if(address >= 0x0000 && address <= 0x1FFF)
            {
                return cartridge.PpuRead(address);
            }
            else if(address >= 0x2000 && address <= 0x3EFF)
            {
                return nametable.Read(address, cartridge.Mode);
            }
            else if(address >= 0x3F00 && address <= 0x3FFF)
            {
                return (byte)(paletteRam.Read(address) & (maskRegister.GreyScale ? 0x30 : 0x3F));
            }
            return 0;
        }

        public void PpuWrite(UInt16 address, byte data)
        {
            address &= 0x3FFF;

            if (address >= 0x0000 && address <= 0x1FFF)
            {
                // usually ROM but for some games it is RAM
                cartridge.PpuWrite(address, data);
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {
                nametable.Write(address, cartridge.Mode, data);
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                paletteRam.Write(address, data);
            }
        }

        public void Clock()
        {
            // invisible scanline -1
            if (scanLine == -1)
            {
                if (cycle == 1)
                {
                    statusRegister.VerticalBlank = false;

                    statusRegister.SpriteOverflow = false;

                }
            }

            // visible scanlines, except scanline -1
            if (scanLine >= -1 && scanLine < 240)
            {
                // cycle is skipped
                if (scanLine == 0 && cycle == 0)
                {
                    cycle = 1;
                }

                if ((cycle >= 1 && cycle <= 256) || (cycle >= 321 && cycle <= 336))
                {
                    UpdateShiftRegisters();
                    PreloadData();
                }

                // end of line, go to next
                if (cycle == 256 && RenderingEnabled())
                {
                    vRam.IncrementY();
                }

                // reset x position
                if (cycle == 257)
                {
                    LoadShifters();
                    if (RenderingEnabled())
                    {
                        vRam.TransferX(tRam);
                    }
                }

                if (cycle == 338 || cycle == 340)
                {
                    UInt16 address = (UInt16)(0x2000 | (vRam.Register & 0x0FFF));
                    bgNextTileId = PpuRead(address);
                }

                if (scanLine == -1 && cycle >= 280 && cycle < 305)
                {
                    if (RenderingEnabled())
                    {
                        vRam.TransferY(tRam);
                    }
                }

                // foreground

                if (cycle == 257 && scanLine >= 0)
                {
                    EvaluateSprites();
                }

                if (cycle == 340)
                {
                    PopulateSpriteShifters();
                }

            }

            // nothing happens on scanline 240
            if (scanLine == 240)
            {

            }

            // end of visible scanlines
            if (scanLine == 241 && cycle == 1)
            {
                statusRegister.VerticalBlank = true;

                EmitNmi = controlRegister.EnableNmi;
            }

            DrawPixel();

            NextCycle();
        }

        private void DrawPixel()
        {
            byte bgPixel = 0x00, bgPalette = 0x00;
            if (maskRegister.ShowBackground)
            {
                UInt16 bitmask = (UInt16)(0x8000 >> fineX);

                bgPixel = (byte)
                (
                    (((bgPatternHigh.Register & bitmask) > 0 ? 1 : 0) << 1) |
                    ((bgPatternLow.Register & bitmask) > 0 ? 1 : 0)
                );

                bgPalette = (byte)
                (
                    (((bgAttributeHigh.Register & bitmask) > 0 ? 1 : 0) << 1) |
                    ((bgAttributeLow.Register & bitmask) > 0 ? 1 : 0)
                );
            }

            byte fgPixel = 0x00, fgPalette = 0x00;
            bool fgPriority = false;
            if (maskRegister.ShowSprites)
            {
                for (int i = 0; i < currentScanlineSprites.Length; ++i)
                {
                    var sprite = currentScanlineSprites[i];

                    if (sprite.X == 0)
                    {
                        // we've hit the sprite
                        // TODO out of range
                        byte fgPixelLow = (byte)(BitMagic.IsBitSet(spriteShiftLow[i], 7) ? 0x01 : 0x00);
                        byte fgPixelHigh = (byte)(BitMagic.IsBitSet(spriteShiftHigh[i], 7) ? 0x01 : 0x00);

                        fgPixel = (byte)((fgPixelHigh << 1) | fgPixelLow);
                        fgPalette = sprite.Attributes.Palette;
                        fgPriority = sprite.Attributes.Priority; // TODO da li treba invertovano

                        if (fgPixel != 0)
                        {
                            break;
                        }
                    }
                }
            }

            byte pixel, palette;
            if (bgPixel == 0 || (fgPixel > 0 && fgPriority))
            {
                pixel = fgPixel;
                palette = fgPalette;
            }
            else
            {
                pixel = bgPixel;
                palette = bgPalette;
            }

            if (cycle >= 1 && cycle <= 256 && scanLine >= 0 && scanLine < 240)
            {
                Screen.SetPixel(cycle - 1, scanLine, paletteRam.PixelColor(palette, pixel));
            }
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

        private void PreloadData()
        {
            switch((cycle - 1) % 8)
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
                    if(RenderingEnabled())
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

            if((vRam.CoarseY & 0x02) != 0)
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

            UInt16 address = (UInt16)(patternId + tileId + offset);

            bgNextTileLsb = PpuRead(address);
        }

        private void PreloadBackgroundTileHigh()
        {
            // choose pattern table
            UInt16 patternId = (UInt16)((controlRegister.PatternBackground ? 1 : 0) << 12);

            //
            UInt16 tileId = (UInt16)(bgNextTileId << 4);
            UInt16 offset = (UInt16)(vRam.FineY + 8);

            UInt16 address = (UInt16)(patternId + tileId + offset);

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

        private void UpdateShiftRegisters()
        {
            UpdateBackgroundShiftRegisters();
            UpdateSpriteShiftRegisters();
        }

        private void UpdateBackgroundShiftRegisters()
        {
            if(maskRegister.ShowBackground)
            {
                bgPatternLow.Shift();
                bgPatternHigh.Shift();
                bgAttributeLow.Shift();
                bgAttributeHigh.Shift();
            }
        }

        private void EvaluateSprites()
        {
            for(int i = 0; i < spritesNumber; ++i)
            {
                currentScanlineSprites[i].Reset();
            }
            spritesNumber = 0;

            statusRegister.SpriteOverflow = false;
            // decide which sprites are visible on current scanline
            for (int i = 0; i < 64; ++i)
            {
                ObjectAttributeEntry currentSprite = Oam.EntryAt(i);
                int diff = scanLine - currentSprite.Y;

                if(diff >= 0 && diff < (controlRegister.SpriteSize ? 16 : 8))
                {
                    if(spritesNumber == 8)
                    {
                        statusRegister.SpriteOverflow = true;
                        break;
                    }

                    // clone because we are modifying X value in each cycle
                    currentScanlineSprites[spritesNumber++] = currentSprite.Clone() as ObjectAttributeEntry;
                }
            }
        }

        private void PopulateSpriteShifters()
        {
            for(int i = 0; i < 8; ++i)
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

                    if(!sprite.Attributes.FlipVertically)
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

                if(sprite.Attributes.FlipHorizontally)
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
            if(maskRegister.ShowSprites && cycle >= 1 && cycle <= 256)
            {
                for(int i = 0; i < spritesNumber; ++i)
                {
                    var sprite = currentScanlineSprites[i];

                    if(sprite.X > 0)
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

        private bool RenderingEnabled()
        {
            return maskRegister.ShowBackground || maskRegister.ShowSprites;
        }
    }
}
