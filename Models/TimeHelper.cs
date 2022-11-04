using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleDLP.Models
{
    internal class TimeHelper
    {
        public static string GetNewTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        }

        public static string GetZ0NewTimeString()
        {
            return DateTime.Now.AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        }
    }
}
