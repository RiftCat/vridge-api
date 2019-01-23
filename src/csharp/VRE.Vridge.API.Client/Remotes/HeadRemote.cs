using System;
using System.Collections.Generic;
using System.Text;
using VRE.Vridge.API.Client.Proxy.HeadTracking;

namespace VRE.Vridge.API.Client.Remotes
{
    public class HeadRemote : RemoteBase<HeadTrackingProxy>
    {
        internal HeadRemote(HeadTrackingProxy proxy) : base(proxy)
        {
            
        }

        /// <summary>
        /// Reorients tracking system and sets new center to current head direction.
        /// </summary>
        public void Recenter()
        {
            WrapTimeouts(() => Proxy.RecenterView());
        }

        public void SetPosition(float x, float y, float z)
        {
            WrapTimeouts(() => Proxy.SetPosition(x, y, z));
        }

        public void SetRotationAndPosition(float yaw, float pitch, float roll, float x, float y, float z)
        {
            WrapTimeouts(() => Proxy.SetRotationAndPosition(yaw, pitch, roll, x, y, z));
        }
        
        public void SetAsyncOffset(float yaw)
        {
            WrapTimeouts(() => Proxy.SetAsyncOffset(yaw));
        }

        public float[] GetCurrentPose()
        {
            return WrapTimeouts(() => Proxy.GetCurrentPhonePose());
        }

        public void SetStatus(bool isInTrackingRange)
        {
            WrapTimeouts(() => Proxy.ChangeTrackingState(isInTrackingRange));
        }

        // Type-cast-methods
        public void SetAsyncOffset(double yaw) => 
            SetAsyncOffset((float)yaw);

        public void SetRotationAndPosition(double yaw, double pitch, double roll, double x, double y, double z) =>
            SetRotationAndPosition((float) yaw, (float) pitch, (float) roll, (float) x, (float) y, (float) z);

        public void SetPosition(double x, double y, double z) =>
            SetPosition((float) x, (float) y, (float) z);

        internal override void Dispose()
        {
            Proxy?.Disconnect();
            base.Dispose();
        }        
    }
}
