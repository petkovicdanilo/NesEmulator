using NesLib.Devices.CartridgeEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities
{
    public class Nametable
    {
        private byte[,] nameTable = new byte[2, 1024];

        public byte Read(UInt16 address, Cartridge.MirrorMode mirrorMode)
        {
            UInt16 maskedAddress = Nametable.MaskAddress(address);

            int id = Nametable.MirrorNametable(maskedAddress, mirrorMode);
            int offset = Nametable.Offset(maskedAddress);

            return nameTable[id, offset];
        }

        public void Write(UInt16 address, Cartridge.MirrorMode mirrorMode, byte data)
        {
            UInt16 maskedAddress = Nametable.MaskAddress(address);

            int id = Nametable.MirrorNametable(maskedAddress, mirrorMode);
            int offset = Nametable.Offset(maskedAddress);

            nameTable[id, offset] = data;
        }

        private static UInt16 MaskAddress(UInt16 address)
        {
            return (UInt16)(address & 0x0FFF);
        }

        private static UInt16 Offset(UInt16 address)
        {
            return (UInt16)(address & 0x03FF);
        }

        private static int NametableId(UInt16 address)
        {
            // TODO prettier
            if (address >= 0x0000 && address <= 0x03FF)
            {
                return 0;
            }
            if (address >= 0x0400 && address <= 0x07FF)
            {
                return 1;
            }
            if (address >= 0x0800 && address <= 0x0BFF)
            {
                return 2;
            }
            if (address >= 0x0C00 && address <= 0x0FFF)
            {
                return 3;
            }
            return 0;
        }

        private static int MirrorNametable(UInt16 address, Cartridge.MirrorMode mirrorMode)
        {
            int nameTableId = Nametable.NametableId(address);

            switch (mirrorMode)
            {
                case Cartridge.MirrorMode.VERTICAL:
                    if (nameTableId == 0 || nameTableId == 2)
                    {
                        return 0;
                    }
                    else return 1;
                case Cartridge.MirrorMode.HORIZONTAL:
                    if (nameTableId == 0 || nameTableId == 1)
                    {
                        return 0;
                    }
                    else return 1;
            }

            return 0;
        }
    }
}
