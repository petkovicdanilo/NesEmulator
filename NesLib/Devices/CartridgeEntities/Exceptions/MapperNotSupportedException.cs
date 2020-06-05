using System;

namespace NesLib.Devices.CartridgeEntities.Exceptions
{
    public class MapperNotSupportedException : Exception
    {
        public MapperNotSupportedException(int i) : base($"Mapper {i} is not supported")
        {

        }
    }
}
