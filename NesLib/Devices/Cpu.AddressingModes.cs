using System;

namespace NesLib.Devices
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
            byte low = Read(PC++);

            address = (UInt16)(0x00FF | low);
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ZPX)]
        public bool ZPX()
        {
            UInt16 addressToRead = (UInt16)(PC + X);

            byte low = Read(addressToRead);
            PC++;

            address = (UInt16)(0x00FF | low);
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ZPY)]
        public bool ZPY()
        {
            UInt16 addressToRead = (UInt16)(PC + Y);

            byte low = Read(addressToRead);
            PC++;

            address = (UInt16)(0x00FF | low);
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.REL)]
        public bool REL()
        {
            jumpOffset = Read(PC++);

            if ((jumpOffset & 0x80) != 0)
            {
                jumpOffset = (Int16)(jumpOffset | 0xFF00);
            }
            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.ABS)]
        public bool ABS()
        {
            byte low = Read(PC++);
            byte high = Read(PC++);

            address = (UInt16)((high << 8) | low);
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
            byte low = Read(PC++);
            byte high = Read(PC++);

            UInt16 ptr = (UInt16)((high << 8) | low);

            byte addrLow, addrHigh;

            // hardware bug
            if (low == 0x00FF)
            {
                addrLow = Read((UInt16)(ptr & 0xFF00));
                addrHigh = Read((UInt16)(ptr));
            }
            else
            {
                addrLow = Read(ptr);
                addrHigh = Read((UInt16)(ptr + 1));
            }

            address = (UInt16)((addrHigh << 8) | addrLow);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IZX)]
        public bool IZX()
        {
            byte low = Read(PC++);
            byte high = Read(PC++);

            UInt16 ptr = (UInt16)((high << 8) | low);

            // address is in zero page
            byte addrLow = Read((UInt16)((ptr + X) & 0x00FF));
            byte addrHigh = Read((UInt16)((ptr + X + 1) & 0x00FF));

            address = (UInt16)((addrHigh << 8) | addrLow);

            return false;
        }

        [AddressingModeMethod(AddressingMode = AddressingMode.IZY)]
        public bool IZY()
        {
            byte low = Read(PC++);
            byte high = Read(PC++);

            UInt16 ptr = (UInt16)((high << 8) | low);

            // address is in zero page
            byte addrLow = Read((UInt16)((ptr + Y) & 0x00FF));
            byte addrHigh = Read((UInt16)((ptr + Y + 1) & 0x00FF));

            address = (UInt16)((addrHigh << 8) | addrLow);

            // if page is crossed
            if ((address & 0xFF00) != (high << 8))
            {
                return true;
            }

            return false;
        }

    }
}