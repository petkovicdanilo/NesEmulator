using System;

namespace NesLib.Devices.PpuEntities.OAM
{
    [Serializable]
    public class ObjectAttributeEntry : ICloneable
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
                
                return 0;
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

        public ObjectAttributeEntry()
        {

        }

        public ObjectAttributeEntry(ObjectAttributeEntry other)
        {
            Attributes = other.Attributes.Clone() as Attributes;
            X = other.X;
            Y = other.Y;
            Id = other.Id;
        }

        public object Clone()
        {
            return new ObjectAttributeEntry(this);
        }

        public void Reset()
        {
            Attributes.Register = 0xFF;
            X = 0xFF;
            Y = 0xFF;
            Id = 0xFF;
        }
    }
}
