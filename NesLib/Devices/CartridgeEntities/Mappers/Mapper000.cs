using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.CartridgeEntities.Mappers
{
    [Serializable]
    public class Mapper000 : Mapper
    {
        public Mapper000(int prgBanks, int chrBanks) :  base(prgBanks, chrBanks)
        {

        }

        public override bool MapCpuRead(UInt16 address, ref UInt16 mappedAddress)
        {
            return MapCpu(address, ref mappedAddress);
        }

        public override bool MapCpuWrite(UInt16 address, ref UInt16 mappedAddress, byte data)
        {
            return MapCpu(address, ref mappedAddress);
        }

        public override bool MapPpuRead(UInt16 address, ref UInt16 mappedAddress)
        {
            if (InChrRange(address))
            {
                // no mapping
                mappedAddress = address;
                return true;
            }

            return false;
        }

        public override bool MapPpuWrite(UInt16 address, ref UInt16 mappedAddress)
        {
            // cannot write to ROM
            if(InChrRange(address) && chrBanks == 0)
            {
                mappedAddress = address;
                return true;
            }

            return false;
        }

        private bool InChrRange(UInt16 address)
        {
            return (address >= 0x0000 && address <= 0x1FFF);
        }

        private bool MapCpu(UInt16 address, ref UInt16 mappedAddress)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                // if 32K mirror
                // if 62K mask to begin from 0x0000 for ROM file
                mappedAddress = (UInt16)(address & (prgBanks > 1 ? 0x7FFF : 0x3FFF));
                return true;
            }

            return false;
        }

        public override void Reset()
        {
            
        }
    }
}
