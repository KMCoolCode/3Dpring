using CypressLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDLP.PPC
{
    public class CypressGpioWithId:CypressGpio
    {
        public CypressGpioWithId(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
