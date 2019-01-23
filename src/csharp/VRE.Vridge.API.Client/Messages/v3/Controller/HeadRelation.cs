using System;
using System.Collections.Generic;
using System.Text;

namespace VRE.Vridge.API.Client.Messages.v3.Controller
{
    public enum HeadRelation : byte
    {
        /// <summary>
        /// Pose is unrelated to current head pose.        
        /// </summary>
        Unrelated = 0,

        /// <summary>
        /// Pose already is in head space.
        /// Will be auto-adjusted when head is recentered.
        /// </summary>
        IsInHeadSpace = 1,

        /// <summary>
        /// Pose is unrelated but is to be remapped in a way that assumes that pose forward
        /// should always be aligned to head's forward. Effectively the given pose is relative angle from current's head forward.
        /// </summary>
        SticksToHead = 2
    }
}
