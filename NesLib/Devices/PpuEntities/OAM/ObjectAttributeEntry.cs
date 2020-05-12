using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities.OAM
{
    public class ObjectAttributeEntry
    {
        public byte Y { get; set; }
        public byte Id { get; set; }
        public Attributes Attributes { get; set; } = new Attributes();
        public byte X { get; set; }

        public byte this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return Y;
                    case 1:
                        return Id;
                    case 2:
                        return Attributes.Register;
                    case 3:
                        return X;
                }
                // TODO
                return Y;
            }

            set
            {
                switch(i)
                {
                    case 0:
                        Y = value;
                        break;
                    case 1:
                        Id = value;
                        break;
                    case 2:
                        Attributes.Register = value;
                        break;
                    case 3:
                        X = value;
                        break;
                }
            }
        }
    }
}
