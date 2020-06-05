using System;

namespace NesLib.Devices.CartridgeEntities.Mappers
{
    [Serializable]
    public class Mapper003 : Mapper
    {
        private int chrBank;

        public Mapper003(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
        {
            Reset();
        }

        public override bool MapCpuRead(UInt16 address, ref UInt16 mappedAddress)
        {
            if(address >= 0x8000 && address <= 0xFFFF)
            {
                // prgBanks is either 1 (16Kb ROM) or 2 (32Kb ROM)
                mappedAddress = (UInt16)(address & ((prgBanks == 1) ? 0x3FFF : 0x7FFF));
                return true;
            }

            return false;
        }

        public override bool MapCpuWrite(UInt16 address, ref UInt16 mappedAddress, byte data)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                chrBank = data & 0x03;
                mappedAddress = address;
            }

            return false;
        }

        public override bool MapPpuRead(UInt16 address, ref UInt16 mappedAddress)
        {
            if(address <= 0x1FFF)
            {
                mappedAddress = (UInt16)(chrBank * 0x2000 + address);
                return true;
            }

            return false;
        }

        public override bool MapPpuWrite(UInt16 address, ref UInt16 mappedAddress)
        {
            return false;
        }

        public override void Reset()
        {
            chrBank = 0;
        }
    }
}
