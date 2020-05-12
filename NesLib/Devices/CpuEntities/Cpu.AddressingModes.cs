using NesLib.Utils;
using System;

namespace NesLib.Devices.CpuEntities
{
    public partial class Cpu
    {
        [AddressingModeMethod(AddressingMode = AddressingMode.ACC)]
        public bool ACC()
        {
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IMP)]
        public bool IMP()
        {
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IMM)]
        public bool IMM()
        {
            address = PC++;
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ZP0)]
        public bool ZP0()
        {
            byte low = CpuRead(PC++);

            address = (UInt16)(0x00FF & low);
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ZPX)]
        public bool ZPX()
        {
            byte low = (byte)(CpuRead(PC) + X);
            PC++;

            address = (UInt16)(0x00FF & low);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ZPY)]
        public bool ZPY()
        {
            byte low = (byte)(CpuRead(PC) + Y);
            PC++;

            address = (UInt16)(0x00FF & low);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.REL)]
        public bool REL()
        {
            jumpOffset = CpuRead(PC++);

            if ((jumpOffset & 0x80) != 0)
            {
                jumpOffset = (Int16)(jumpOffset | 0xFF00);
            }

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ABS)]
        public bool ABS()
        {
            byte low = CpuRead(PC++);
            byte high = CpuRead(PC++);

            address = BitMagic.Combine(high, low);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ABX)]
        public bool ABX()
        {
            ABS();

            UInt16 baseAddress = address;
            address += X;

            // if address crossed the page when X is added
            // additional cycle is needed
            if (Page(address) != Page(baseAddress))
            {
                return true;
            }

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ABY)]
        public bool ABY()
        {
            ABS();

            UInt16 baseAddress = address;
            address += Y;

            // if address crossed the page when Y is added
            // additional cycle is needed
            if (Page(address) != Page(baseAddress))
            {
                return true;
            }

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IND)]
        public bool IND()
        {
            byte low = CpuRead(PC++);
            byte high = CpuRead(PC++);

            UInt16 ptr = BitMagic.Combine(high, low);

            byte addrLow, addrHigh;

            // hardware bug
            if (low == 0x00FF)
            {
                addrLow = CpuRead((ptr));
                addrHigh = CpuRead((UInt16)(ptr & 0xFF00));
            }
            else
            {
                addrLow = CpuRead(ptr);
                addrHigh = CpuRead((UInt16)(ptr + 1));
            }

            address = BitMagic.Combine(addrHigh, addrLow);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IZX)]
        public bool IZX()
        {
            byte t = CpuRead(PC++);

            // address is in zero page
            byte addrLow = CpuRead((UInt16)((t + X) & 0x00FF));
            byte addrHigh = CpuRead((UInt16)((t + X + 1) & 0x00FF));

            address = BitMagic.Combine(addrHigh, addrLow);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IZY)]
        public bool IZY()
        {
            byte t = CpuRead(PC++);

            // address is in zero page
            byte addrLow = CpuRead((UInt16)(t & 0x00FF));
            byte addrHigh = CpuRead((UInt16)((t + 1) & 0x00FF));

            address = BitMagic.Combine(addrHigh, addrLow);
            address += Y;

            // if page is crossed
            if (Page(address) != Page((UInt16)(addrHigh << 8)))
            {
                return true;
            }

            return false;
        }

    }
}