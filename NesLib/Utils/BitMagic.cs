using System;

namespace NesLib.Utils
{
    public static class BitMagic
    {
        public static UInt16 Combine(byte high, byte low)
        {
            return (UInt16)((high << 8) | low);
        }
    }
}
