using NesLib.Utils;
using System;

namespace NesLib.Devices.PpuEntities.Registers
{
    [Serializable]
    public class LoopyRegister
    {
        private UInt16 _register;
        public UInt16 Register 
        {
            get
            {
                return _register;
            }

            set
            {
                _register = value;
            }
        }

        public byte FineY
        {
            get
            {
                
                // extract first 3 bits of register
                // and shift it to fit into byte
                return (byte)((Register & 0x7000) >> 12);
            }
            set
            {
                value &= 0x07; // we need only 3 bits

                UInt16 fineYMask = 0x0FFF;

                // unset all fine y bits with fineYMask
                // then add bits from value
                Register &= (UInt16)(fineYMask);
                Register |= (UInt16)(value << 12);
            }
        }

        public bool NametableX
        {
            get
            {
                return BitMagic.IsBitSet(Register, 10);
            }
            set
            {
                BitMagic.SetBit(ref _register, 10, value);
            }
        }
        public bool NametableY
        {
            get
            {
                return BitMagic.IsBitSet(Register, 11);
            }
            set
            {
                BitMagic.SetBit(ref _register, 11, value);
            }
        }

        public byte CoarseY
        {
            get
            {
                return (byte)((Register & 0x03E0) >> 5);
            }
            set
            {
                value &= 0x1F; // we need only 5 bits

                UInt16 coarseYMask = 0x7C1F;

                // unset all coarse y bits with coarseYMask
                // then add bits from value
                Register &= (UInt16)(coarseYMask);
                Register |= (UInt16)(value << 5);
            }
        }

        public byte CoarseX
        {
            get
            {
                return (byte)(Register & 0x001F);
            }
            set
            {
                value &= 0x1F; // we need only 5 bits

                UInt16 coarseXMask = 0x7FE0;

                // unset all coarse x bits with coarseXMask
                // then add bits from value
                Register &= (UInt16)(coarseXMask);
                Register |= (UInt16)(value);
            }
        }

        public void Increment(bool horizontalMode = true)
        {
            Register += (UInt16)(horizontalMode ? 32 : 1);
        }

        public void IncrementX()
        {
            if(CoarseX == 31)
            {
                CoarseX = 0;
                NametableX = !NametableX;
            }
            else
            {
                CoarseX++;
            }
        }

        public void IncrementY()
        {
            if (FineY < 7)
            {
                FineY++;
            }
            else
            {
                FineY = 0;

                if(CoarseY == 29)
                {
                    CoarseY = 0;

                    NametableY = !NametableY;
                }
                else if(CoarseY == 31)
                {
                    CoarseY = 0;
                }
                else
                {
                    CoarseY++;
                }
            }
        }

        public void TransferX(LoopyRegister source)
        {
            this.NametableX = source.NametableX;
            this.CoarseX = source.CoarseX;
        }

        public void TransferY(LoopyRegister source)
        {
            this.FineY = source.FineY;
            this.NametableY = source.NametableY;
            this.CoarseY = source.CoarseY;
        }
    }
}
