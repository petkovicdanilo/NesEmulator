using NesLib.Devices;
using System;

namespace NesLib
{
    public class Nes
    {
        private Cpu cpu;
        private Ppu ppu;
        private Cartridge cartridge;

        private const UInt16 CPU_RAM_LEFT = 0x0000, CPU_RAM_RIGHT = 0x1FFF;
        private const UInt16 PPU_LEFT = 0x2000, PPU_RIGHT = 0x3FFF;

        private const UInt16 CPU_RAM_MASK = 0x7FFF;
        private const UInt16 PPU_MASK = 0x0007;

        private int clockCounter = 0;

        private byte[] cpuRam = new byte[2048];

        public Nes()
        {
            this.cpu = new Cpu(this);
            this.ppu = new Ppu();

            //byte[] program = { 0xa9, 0x01, 0x8d, 0x00, 0x02, 0xa9, 0x05, 0x8d, 
            //    0x01, 0x02, 0xa9, 0x08, 0x8d, 0x02, 0x02 };

            //byte[] program =
            //{
            //    0xA2, 0x0A, 0x8E, 0x00, 0x00, 0xA2, 0x03, 0x8E, 0x01, 
            //    0x00, 0xAC, 0x00, 0x00, 0xA9, 0x00, 0x18, 0x6D, 0x01, 
            //    0x00, 0x88, 0xD0, 0xFA, 0x8D, 0x02, 0x00, 0xEA, 0xEA, 
            //    0xEA
            //};

            //UInt16 offSet = 0x8000;

            //foreach(byte code in program)
            //{
            //    cpuRam[offSet++] = code;
            //}

            //cpuRam[0xFFFC] = 0x00;
            //cpuRam[0xFFFD] = 0x80;

            //cpu.Reset();

            InsertCartridge(@"C:\Users\Danilo\Desktop\NES games\Super Mario Bros. (World).nes");
        }

        public string PrintCpu()
        {
            return $"A={cpu.A:X}, X={cpu.X:X}, Y={cpu.Y:X}, PC={cpu.PC:X}, StackPointer={cpu.StackPointer:X} " +
                $"Status={cpu.Status.Register:X}";
        }

        public void InsertCartridge(string filePath)
        {
            cartridge = new Cartridge(filePath);
        }

        public byte CpuRead(UInt16 address)
        {
            if (InCpuRamRange(address))
            {
                return cpuRam[MaskCpuRam(address)];
            }
            else if(InPpuRange(address))
            {
                return ppu.CpuRead(MaskPpu(address));
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
        }

        // global system clock
        public void Clock()
        {
            ppu.Clock();

            if(clockCounter % 3 == 0)
            {
                cpu.Clock();
            }

            clockCounter++;
        }

        public void Reset()
        {

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
    }
}
