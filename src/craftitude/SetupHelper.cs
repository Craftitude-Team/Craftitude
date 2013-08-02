using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craftitude
{
    public class SetupHelper
    {
        public virtual void Run(string[] arguments) { }

        internal Profile _profile;
        internal Package _package;

        public Profile Profile { get { return _profile; } }
        public Package Package { get { return _package; } }
    }
}
