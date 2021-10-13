using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleBA
{
    internal class Hresult
    {
        public static bool Succeeded(int status)
        {
            return status >= 0;
        }
    }
}
