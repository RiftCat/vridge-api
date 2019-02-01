using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages.BasicTypes;
using VRE.Vridge.API.Client.Messages.OpenVR;
using VRE.Vridge.API.Client.Messages.v3.Broadcast;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using VRE.Vridge.API.Client.Proxy.Controller;
using VRController = VRE.Vridge.API.Client.Messages.v3.Controller.VRController;

namespace VRE.Vridge.API.Client.Remotes
{
    public class ControllerRemote : RemoteBase<ControllerProxy>
    {
        private uint packetNum = 0;                    

        internal ControllerRemote(ControllerProxy proxy) : base(proxy)
        {
        }

        /// <summary>
        /// Sends full controller state as defined by arguments.
        /// </summary>        
        public void SetControllerState(
            // Controller ID
            int controllerId,

            // Pose data
            HeadRelation headRelation,
            HandType suggestedHand,
            Quaternion orientation,
            Vector3? position,

            // Touchpad state [-1,1]
            double analogX,
            double analogY,

            // Trigger state
            double analogTrigger,

            // Button states
            bool isMenuPressed,
            bool isSystemPressed,
            bool isTriggerPressed,
            bool isGripPressed,
            bool isTouchpadPressed,
            bool isTouchpadTouched)
        {

            // See openvr.h in OpenVR SDK for mappings and masks
            // https://github.com/ValveSoftware/openvr/blob/master/headers/openvr.h

            var buttons = new VRControllerState_t()
            {
                rAxis0 = new VRControllerAxis_t((float)analogX, (float)analogY),       // Touchpad
                rAxis1 = new VRControllerAxis_t((float)analogTrigger, 0),              // Trigger
                rAxis2 = new VRControllerAxis_t(0, 0),
                rAxis3 = new VRControllerAxis_t(0, 0),
                rAxis4 = new VRControllerAxis_t(0, 0),
                ulButtonPressed = BuildButtonPressedMask(isMenuPressed, isSystemPressed, isTriggerPressed, isGripPressed, isTouchpadPressed),
                ulButtonTouched = BuildButtonTouchedMask(isTouchpadTouched, true),
                unPacketNum = ++packetNum

            };

            var controllerState = new VRController()
            {
                ButtonState = buttons,
                Status = 0,
                ControllerId = controllerId,
                Position = position.HasValue ? new[] { position.Value.X, position.Value.Y, position.Value.Z } : null,
                Orientation = new [] {orientation.X, orientation.Y, orientation.Z, orientation.W },
                HeadRelation = headRelation,
                SuggestedHand = suggestedHand
            };

            WrapTimeouts(() => { Proxy.SendControllerData(controllerState); });
        }

        /// <summary>
        /// Recenter head tracking. Works the same as pressing recenter hotkey as configured in VRidge settings.
        /// </summary>
        public void RecenterHead()
        {

            WrapTimeouts(() => { Proxy?.RecenterHead(); });

        }

        internal override void Dispose()
        {
            Proxy?.Disconnect();
            base.Dispose();
        }

        /// <summary>
        /// Maps button pressed state into OpenVR packed ulong.
        /// </summary>                
        private ulong BuildButtonPressedMask(
            bool isMenuPressed,
            bool isSystemPressed,
            bool isTriggerPressed,
            bool isGripPressed,
            bool isTouchpadPressed)
        {
            ulong mask = 0;            

            if (isMenuPressed) mask |= ButtonMask.ApplicationMenu;
            if (isSystemPressed) mask |= ButtonMask.System;
            if (isTriggerPressed) mask |= ButtonMask.Trigger;
            if (isGripPressed) mask |= ButtonMask.Grip;
            if (isTouchpadPressed) mask |= ButtonMask.Touchpad;

            return mask;
        }


        /// <summary>
        /// Maps button touched state into OpenVR packed ulong.
        /// </summary>         
        private ulong BuildButtonTouchedMask(bool isTouchpadTouched, bool isTriggerTouched)
        {
            ulong mask = 0;

            if (isTouchpadTouched) mask |= ButtonMask.Touchpad;
            if (isTriggerTouched) mask |= ButtonMask.Trigger;

            return mask;
        }

        /// <summary>
        /// Builds 4x4 combined matrix for given translation and rotation.
        /// Not used and not recommended anymore. Send orientation[+position] instead of matrix.
        /// </summary>        
        private Matrix4x4 BuildControllerMatrix(double x, double y, double z, double yaw, double pitch, double roll)
        {
            Matrix4x4 m = Matrix4x4.Identity;

            
            m *= Matrix4x4.CreateFromYawPitchRoll(
                (float)MathHelpers.DegToRad(yaw),
                (float)MathHelpers.DegToRad(pitch),
                (float)MathHelpers.DegToRad(roll));

            m*= Matrix4x4.CreateTranslation((float)x, (float)y, (float)z);            

            return m;
        }
    }
}
