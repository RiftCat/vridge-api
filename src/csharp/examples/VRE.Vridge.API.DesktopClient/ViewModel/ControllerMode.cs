using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRE.Vridge.API.DesktopTester.ViewModel
{
    public enum ControllerMode
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
        /// HeadRelation = SticksToHead, Position = defined
        /// </summary>
        SticksToHead = 2,        

        /// <summary>
        /// Equivalent to setting position to NULL from API perspective.
        /// It won't auto-follow head when controller data is not being sent.
        /// HeadRelation = Unrelated, Position = null
        /// </summary>
        ThreeDof = 3,

        /// <summary>
        /// Equivalent to setting position to NULL from API perspective.
        /// Also instructs API server to keep updating the controller pose whenever head pose changes.
        /// HeadRelation = SticksToHead, Position = null
        /// </summary>
        StickyThreeDof = 4
    }
}
