using NesLib.Utils;
using System;

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

        public enum ControllerKeys
        {
            Up,
            Down,
            Left,
            Right,
            Select,
            Start,
            B,
            A,
        }

        public bool GetKeyStatus(ControllerKeys key)
        {
            switch(key)
            {
                case ControllerKeys.Up:
                    return Up;
                case ControllerKeys.Down:
                    return Down;
                case ControllerKeys.Left:
                    return Left;
                case ControllerKeys.Right:
                    return Right;
                case ControllerKeys.Select:
                    return Select;
                case ControllerKeys.Start:
                    return Start;
                case ControllerKeys.A:
                    return A;
                case ControllerKeys.B:
                    return B;
            }
            return false;
        }

        public void SetKeyStatus(ControllerKeys key, bool value)
        {
            switch (key)
            {
                case ControllerKeys.Up:
                    Up = value;
                    break;
                case ControllerKeys.Down:
                    Down = value;
                    break;
                case ControllerKeys.Left:
                    Left = value;
                    break;
                case ControllerKeys.Right:
                    Right = value;
                    break;
                case ControllerKeys.Select:
                    Select = value;
                    break;
                case ControllerKeys.Start:
                    Start = value;
                    break;
                case ControllerKeys.A:
                    A = value;
                    break;
                case ControllerKeys.B:
                    B = value;
                    break;
            }
        }
    }
}
