using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleBA
{
    public class UpdateViewModel : PropertyNotifyBase
    {
        private RootViewModel root;

        public UpdateViewModel(RootViewModel root)
        {
            this.root = root;
        }
        private void WireEventsHandlers()
        {

        }
    }
}
