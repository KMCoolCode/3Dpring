using CypressLibrary;
using Ddp442xLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDLP.PPC
{
    public class Ddp442xWithId: Ddp442x
    {
        public byte Id { get; }
        private readonly CypressI2c _cypressI2C;
        private readonly CypressSpi _cypressSpi;
        private readonly CypressGpio _cypressGpio;

        internal Ddp442xWithId(CypressI2c cypressI2c, CypressSpi cypressSpi, CypressGpio cypressGpio) :base(cypressI2c,cypressSpi, cypressGpio)
        {
            _cypressI2C = cypressI2c;
            _cypressSpi = cypressSpi;
            _cypressGpio = cypressGpio;
        }
        public Ddp442xWithId(byte id) : this(new CypressI2cWithId(id), new CypressSpiWithId(id), new CypressGpioWithId(id))
        {
            Id = id;
        }
        public int OpenI2C()
        {
            return _cypressI2C.Open(Id);
        }
        public int OpenGpio()
        {
            return _cypressGpio.Open(Id);
        }
        public int CloseGpio()
        {
            return _cypressGpio.Close();
        }
        public int OpenSpi()
        {
            return _cypressSpi.Open(Id);
        }
        public int CloseSpi()
        {
            return _cypressSpi.Close();
        }

        public int CloseI2C()
        {
            return _cypressI2C.Close();
        }
    }
}
