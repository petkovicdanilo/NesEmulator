using NesLib.Devices;
using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities.Registers
{
    [Serializable]
    public class MaskRegister : AbstractRegister
    {
        public bool GreyScale
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

        public bool ShowBackgroundLeft
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

        public bool ShowSpritesLeft
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

        public bool ShowBackground
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

        public bool ShowSprites
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

        public bool EmphasizeRed
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

        public bool EmphasizeGreen
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

        public bool EmphasizeBlue
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
    }
}
