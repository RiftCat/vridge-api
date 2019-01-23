using System;
using System.Collections.Generic;
using System.Text;

namespace VRE.Vridge.API.Client.Remotes
{
    [Flags]
    public enum Capabilities
    {
        HeadTracking = 1,
        Controllers = 2
    }
}
