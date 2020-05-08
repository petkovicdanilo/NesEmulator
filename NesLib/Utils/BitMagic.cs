using System;

namespace NesLib.Utils
{
    public static class BitMagic
    {
        public static UInt16 Combine(byte high, byte low)
        {
            return (UInt16)((high << 8) | low);
        }

        public static bool IsBitSet(byte data, int position)
        {
            return (data & (1 << position)) != 0;
        }

        public static bool IsBitSet(UInt16 data, int position)
        {
            return (data & (1 << position)) != 0;
        }

        public static void SetBit(ref byte target, int positon, bool value)
        {
            if(value)
            {
                target |= (byte)(1 << positon);
            }
            else
            {
                target &= (byte) ~(1 << positon);
            }
        }

        public static void SetBit(ref UInt16 target, int positon, bool value)
        {
            if (value)
            {
                target |= (UInt16)(1 << positon);
            }
            else
            {
                target &= (UInt16)~(1 << positon);
            }
        }
    }
}
