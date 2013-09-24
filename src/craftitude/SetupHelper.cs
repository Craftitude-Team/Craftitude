using Craftitude.Profile;

namespace Craftitude
{
    public class SetupHelper
    {
        public virtual void Run(string[] arguments) { }

        public CraftitudeProfile Profile { get; internal set; }
        public Package Package { get; internal set; }
    }
}
