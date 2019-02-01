using System;
using System.Collections.Generic;
using System.Text;

namespace VRE.Vridge.API.Client.Remotes
{
    public enum VridgeRemoteConnectionStatus
    {
        /// <summary>
        /// Connected or will be auto-connected on method call.
        /// </summary>
        Okay,

        Unreachable,
        InUse,
        UnexpectedError
    }
}
