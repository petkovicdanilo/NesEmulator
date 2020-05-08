using NesLib.Devices.CartridgeEntities;
using NesLib.Devices.CpuEntities;
using NesLib.Devices.PpuEntities;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace NesLib
{
    public class Nes
    {
        private Cpu cpu;
        private Ppu ppu;
        private Cartridge cartridge;

        private const UInt16 CPU_RAM_LEFT = 0x0000, CPU_RAM_RIGHT = 0x1FFF;
        private const UInt16 PPU_LEFT = 0x2000, PPU_RIGHT = 0x3FFF;

        private const UInt16 CPU_RAM_MASK = 0x07FF;
        private const UInt16 PPU_MASK = 0x0007;

        private int clockCounter = 0;

        private byte[] cpuRam = new byte[2048];

        public bool FrameComplete
        {
            get
            {
                return ppu.FrameComplete;
            }
            set
            {
                ppu.FrameComplete = value;
            }
        }

        public Nes()
        {
            this.cpu = new Cpu(this);
            this.ppu = new Ppu(this);

            InsertCartridge(@"C:\Users\Danilo\Desktop\NES games\nestest.nes");
            Reset();

        }

        public void InsertCartridge(string filePath)
        {
            cartridge = new Cartridge(filePath);
            ppu.ConnectCartridge(cartridge);
        }

        public byte CpuRead(UInt16 address, bool debugMode)
        {
            if (InCpuRamRange(address))
            {
                return cpuRam[MaskCpuRam(address)];
            }
            else if(InPpuRange(address))
            {
                return ppu.CpuRead(MaskPpu(address), debugMode);
            }
            else if (address == 0x4014)
            {

            }
            else if (address >= 0x4016 && address <= 0x4017)
            {

            }
            else if(address >= 0x4020 && address <= 0xFFFF)
            {
                return cartridge.CpuRead(address);
            }

            return 0x00;
        }

        public void CpuWrite(UInt16 address, byte data)
        {
            if (InCpuRamRange(address))
            {
                cpuRam[MaskCpuRam(address)] = data;
            }
            else if (InPpuRange(address))
            {
                ppu.CpuWrite(MaskPpu(address), data);
            }
            else if (address == 0x4014)
            {

            }
            else if (address >= 0x4016 && address <= 0x4017)
            {

            }
            else if (address >= 0x4020 && address <= 0xFFFF)
            {
                cartridge.CpuWrite(address, data);
            }
        }

        // global system clock
        public void Clock()
        {
            ppu.Clock();

            if(clockCounter % 3 == 0)
            {
                cpu.Clock();
            }

            if(ppu.EmitNmi)
            {
                cpu.NMI();
            }

            clockCounter++;
            if(clockCounter == 3)
            {
                clockCounter = 0;
            }
        }

        public void Reset()
        {
            cpu.Reset();
            ppu.Reset();
            clockCounter = 0;
        }

        private bool InCpuRamRange(UInt16 address)
        {
            return (address >= CPU_RAM_LEFT && address <= CPU_RAM_RIGHT);
        }

        private bool InPpuRange(UInt16 address)
        {
            return (address >= PPU_LEFT && address <= PPU_RIGHT);
        }

        private UInt16 MaskCpuRam(UInt16 address)
        {
            return (UInt16)(address & CPU_RAM_MASK);
        }

        private UInt16 MaskPpu(UInt16 address)
        {
            return (UInt16)(address & PPU_MASK);
        }

        public WriteableBitmap Screen()
        {
            return ppu.Screen;
        }
     }
}
