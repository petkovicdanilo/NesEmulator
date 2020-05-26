using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.CartridgeEntities.Mappers
{
    [Serializable]
    public class Mapper000 : Mapper
    {
        private const UInt16 PRG_LEFT = 0x8000;
        private const UInt16 PRG_RIGHT = 0xFFFF;

        private const UInt16 CHR_LEFT = 0x0000;
        private const UInt16 CHR_RIGHT = 0x1FFF;

        public Mapper000(int prgBanks, int chrBanks) :  base(prgBanks, chrBanks)
        {

        }

        // TODO: handle invalid addresses
        public override UInt16 MapCpuRead(UInt16 address)
        {
            return MapCpu(address);
        }

        public override UInt16 MapCpuWrite(UInt16 address)
        {
            return MapCpu(address);
        }

        public override UInt16 MapPpuRead(UInt16 address)
        {
            if (InChrRange(address))
            {
                // no mapping
                return address;
            }

            return 0;
        }

        public override UInt16 MapPpuWrite(UInt16 address)
        {
            // cannot write to ROM
            if(InChrRange(address) && chrBanks == 0)
            {
                return address;
            }
            return 0;
        }

        private bool InPrgRange(UInt16 address)
        {
            return (address >= PRG_LEFT && address <= PRG_RIGHT);
        }

        private bool InChrRange(UInt16 address)
        {
            return (address >= CHR_LEFT && address <= CHR_RIGHT);
        }

        private UInt16 MapCpu(UInt16 address)
        {
            if (InPrgRange(address))
            {
                // if 32K mirror
                // if 62K mask to begin from 0x0000 for ROM file
                return (UInt16)(address & (prgBanks > 1 ? 0x7FFF : 0x3FFF));
            }

            return 0;
        }
    }
}
