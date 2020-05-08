using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices
{
    public abstract class AbstractRegister
    {
        protected byte _register;
        public byte Register 
        {
            get
            {
                return _register;
            }

            set
            {
                _register = value;
            }
        }

        public AbstractRegister()
        {
            Register = 0x00;
        }
    }
}
