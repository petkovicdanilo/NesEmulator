﻿using System;
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
                int entryIndex = ind / 4;
                int innerIndex = ind % 4;

                return entries[entryIndex][innerIndex];
            }

            set
            {
                int entryIndex = ind / 4;
                int innerIndex = ind % 4;

                entries[entryIndex][innerIndex] = value;
            }
        }

        public ObjectAttributeEntry EntryAt(int ind)
        {
            return entries[ind];
        }
    }
}
