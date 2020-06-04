using NesLib.Devices.CartridgeEntities;
using NesLib.Devices.PpuEntities.OAM;
using NesLib.Devices.PpuEntities.Registers;
using System;

namespace NesLib.Devices.PpuEntities
{
    [Serializable]
    public partial class Ppu : IClockDevice, ICpuBusDevice, IPpuBusDevice
    {
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

        public int[] PixelBuffer = new int[256 * 240];

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

        private bool spriteZeroSelected = false;
        private bool spriteZeroRendered = false;

        public bool FrameComplete { get; set; } = false;

        public Ppu()
        {
            for(int i = 0; i < 8; ++i)
            {
                currentScanlineSprites[i] = new ObjectAttributeEntry();
            }
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

                    statusRegister.SpriteZeroHit = false;
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
    }
}
