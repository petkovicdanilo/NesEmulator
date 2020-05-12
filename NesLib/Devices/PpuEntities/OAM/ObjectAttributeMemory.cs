using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities.OAM
{
    public class ObjectAttributeMemory
    {
        private ObjectAttributeEntry[] entries = new ObjectAttributeEntry[64];

        public ObjectAttributeMemory()
        {
            for(int i = 0; i < 64; ++i)
            {
                entries[i] = new ObjectAttributeEntry();
            }
        }

        public byte this[int ind]
        {
            get
            {
                int entryIndex = ind / 64;
                int innerIndex = ind % 64;

                return entries[entryIndex][innerIndex];
            }

            set
            {
                int entryIndex = ind / 64;
                int innerIndex = ind % 64;

                entries[entryIndex][innerIndex] = value;
            }
        }
    }
}
