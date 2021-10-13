using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleBA
{
    class ProgressViewModel : PropertyNotifyBase
    {
        private RootViewModel root;

        public ProgressViewModel(RootViewModel root)
        {
            this.root = root;
        }
        private void WireEventsHandlers()
        {

        }
    }
}
