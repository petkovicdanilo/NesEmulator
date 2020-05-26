using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices
{
    [Serializable]
    public class Controller
    {
        private byte _state = 0x00;
        public byte State
        {
            get
            {
                return _state;
            }
            private set
            {
                _state = value;
            }
        }

        public bool Right
        {
            get
            {
                return BitMagic.IsBitSet(_state, 0);
            }
            set
            {
                BitMagic.SetBit(ref _state, 0, value);
            }
        }
        public bool Left
        {
            get
            {
                return BitMagic.IsBitSet(_state, 1);
            }
            set
            {
                BitMagic.SetBit(ref _state, 1, value);
            }
        }
        public bool Down
        {
            get
            {
                return BitMagic.IsBitSet(_state, 2);
            }
            set
            {
                BitMagic.SetBit(ref _state, 2, value);
            }
        }
        public bool Up
        {
            get
            {
                return BitMagic.IsBitSet(_state, 3);
            }
            set
            {
                BitMagic.SetBit(ref _state, 3, value);
            }
        }
        public bool Start
        {
            get
            {
                return BitMagic.IsBitSet(_state, 4);
            }
            set
            {
                BitMagic.SetBit(ref _state, 4, value);
            }
        }
        public bool Select
        {
            get
            {
                return BitMagic.IsBitSet(_state, 5);
            }
            set
            {
                BitMagic.SetBit(ref _state, 5, value);
            }
        }
        public bool B
        {
            get
            {
                return BitMagic.IsBitSet(_state, 6);
            }
            set
            {
                BitMagic.SetBit(ref _state, 6, value);
            }
        }
        public bool A
        {
            get
            {
                return BitMagic.IsBitSet(_state, 7);
            }
            set
            {
                BitMagic.SetBit(ref _state, 7, value);
            }
        }
    }
}
