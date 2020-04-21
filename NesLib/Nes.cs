using NesLib.Devices;
using System;

namespace NesLib
{
    public class Nes
    {
        private Cpu cpu;

        // temp
        private byte[] ram = new byte[64 * 1024];

        public Nes()
        {
            this.cpu = new Cpu(this);

            for (int i = 0; i < 64 * 1024; ++i)
            {
                ram[i] = 0x00;
            }

            //byte[] program = { 0xa9, 0x01, 0x8d, 0x00, 0x02, 0xa9, 0x05, 0x8d, 
            //    0x01, 0x02, 0xa9, 0x08, 0x8d, 0x02, 0x02 };

            byte[] program =
            {
                0xA2, 0x0A, 0x8E, 0x00, 0x00, 0xA2, 0x03, 0x8E, 0x01, 
                0x00, 0xAC, 0x00, 0x00, 0xA9, 0x00, 0x18, 0x6D, 0x01, 
                0x00, 0x88, 0xD0, 0xFA, 0x8D, 0x02, 0x00, 0xEA, 0xEA, 
                0xEA
            };

            UInt16 offSet = 0x8000;

            foreach(byte code in program)
            {
                ram[offSet++] = code;
            }

            ram[0xFFFC] = 0x00;
            ram[0xFFFD] = 0x80;

            cpu.Reset();

            //while(true)
            //{
            //    Console.ReadLine();
            //    PrintCpu();
            //    cpu.Clock();
            //}
        }

        public string PrintCpu()
        {
            return $"A={cpu.A:X}, X={cpu.X:X}, Y={cpu.Y:X}, PC={cpu.PC:X}, StackPointer={cpu.StackPointer:X} " +
                $"Status={cpu.Status.Register:X}";
        }

        public void Connect(IDevice device, UInt16 left, UInt16 right)
        {

        }

        public byte Read(UInt16 address)
        {
            if (address >= 0x0000 && address <= 0xFFFF)
            {
                return ram[address];
            }

            return 0x00;
        }

        public void Write(UInt16 address, byte data)
        {
            if (address >= 0x0000 && address <= 0xFFFF)
            {
                ram[address] = data;
            }
        }

        public void Clock()
        {
            cpu.Clock();
        }
    }
}
