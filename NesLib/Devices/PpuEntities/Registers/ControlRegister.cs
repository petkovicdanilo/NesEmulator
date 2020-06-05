using NesLib.Utils;
using System;

namespace NesLib.Devices.PpuEntities.Registers
{
    [Serializable]
    public class ControlRegister : AbstractRegister
    {
        public bool EnableNmi
        {
            get
            {
                return BitMagic.IsBitSet(Register, 7);
            }
            set
            {
                BitMagic.SetBit(ref _register, 7, value);
            }
        }

        // unused
        public bool SlaveMode
        {
            get
            {
                return BitMagic.IsBitSet(Register, 6);
            }
            set
            {
                BitMagic.SetBit(ref _register, 6, value);
            }
        }

        public bool SpriteSize
        {
            get
            {
                return BitMagic.IsBitSet(Register, 5);
            }
            set
            {
                BitMagic.SetBit(ref _register, 5, value);
            }
        }

        public bool PatternBackground
        {
            get
            {
                return BitMagic.IsBitSet(Register, 4);
            }
            set
            {
                BitMagic.SetBit(ref _register, 4, value);
            }
        }

        public bool PatternSprite
        {
            get
            {
                return BitMagic.IsBitSet(Register, 3);
            }
            set
            {
                BitMagic.SetBit(ref _register, 3, value);
            }
        }

        public bool IncrementMode
        {
            get
            {
                return BitMagic.IsBitSet(Register, 2);
            }
            set
            {
                BitMagic.SetBit(ref _register, 2, value);
            }
        }

        public bool NametableY
        {
            get
            {
                return BitMagic.IsBitSet(Register, 1);
            }
            set
            {
                BitMagic.SetBit(ref _register, 1, value);
            }
        }

        public bool NametableX
        {
            get
            {
                return BitMagic.IsBitSet(Register, 0);
            }
            set
            {
                BitMagic.SetBit(ref _register, 0, value);
            }
        }
    }
}
