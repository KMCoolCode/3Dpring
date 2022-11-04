using CypressLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDLP.PPC
{
    public class CypressI2cWithId : CypressI2c
    {
        public CypressI2cWithId(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
