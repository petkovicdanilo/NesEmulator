using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.CartridgeEntities.Exceptions
{
    public class MapperNotSupportedException : Exception
    {
        public MapperNotSupportedException(int i) : base($"Mapper {i} is not supported")
        {

        }
    }
}
