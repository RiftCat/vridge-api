using System.Windows.Media.Media3D;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages.v1.Controller.OpenVR;
using VRE.Vridge.API.Client.Proxy.Controller;

namespace VRE.Vridge.API.DesktopTester.Service.Controller
{
    class ControllerService
    {
        private readonly ControllerProxy proxy;

        private uint packetNum = 0;        

        public ControllerService(ControllerProxy proxy)
        {
            this.proxy = proxy;
        }

        /// <summary>
        /// Sends full controller state as defined by arguments.
        /// </summary>        
        public void SendControllerState(
            // Controller ID
            int controllerId, 

            // Absolute position in meters
            double positionX, 
            double positionY,
            double positionZ, 

            // Rotation in degrees
            double yaw, 
            double pitch, 
            double roll, 

            // Trackpad state [-1,1]
            double analogX, 
            double analogY, 

            // Trigger state
            double analogTrigger, 

            // Button states
            bool isMenuPressed,
            bool isSystemPressed)
        {

            // See openvr.h in OpenVR SDK for mappings and masks
            // https://github.com/ValveSoftware/openvr/blob/master/headers/openvr.h

            var buttons = new VRControllerState_t()
            {
                rAxis0 = new VRControllerAxis_t((float) analogX, (float) analogY),      // Touchpad
                rAxis1 = new VRControllerAxis_t((float) analogTrigger, 0),              // Trigger
                rAxis2 = new VRControllerAxis_t(0, 0),
                rAxis3 = new VRControllerAxis_t(0, 0),
                rAxis4 = new VRControllerAxis_t(0, 0),
                ulButtonPressed = BuildButtonPressedMask(isMenuPressed, isSystemPressed, analogX),
                ulButtonTouched = BuildButtonTouchedMask(true, true), 
                unPacketNum = ++packetNum

            };

            var controller = new VRController()
            {
                ButtonState = buttons,
                Status = 0,
                ControllerId = controllerId,
                OrientationMatrix = BuildControllerMatrix(positionX, positionY, positionZ, yaw, pitch, roll).FlattenAsColumnMajor()
            };
            
            proxy.SendControllerData(controller);
        }

        /// <summary>
        /// Maps button pressed state into OpenVR packed ulong.
        /// </summary>                
        private ulong BuildButtonPressedMask(bool isMenuPressed, bool isSystemPressed, double analogX)
        {            
            bool isTriggerPressed = analogX > 0.9f;

            ulong mask = 0;

            if (isMenuPressed) mask |= ButtonMask.ApplicationMenu;
            if (isSystemPressed) mask |= ButtonMask.System;
            if (isTriggerPressed) mask |= ButtonMask.Trigger;

            return mask;
        }


        /// <summary>
        /// Maps button touched state into OpenVR packed ulong.
        /// </summary>         
        private ulong BuildButtonTouchedMask(bool isTouchpadTouched, bool isTriggerTouched)
        {
            ulong mask = 0;
            
            if (isTouchpadTouched) mask |= ButtonMask.Touchpad;
            if (isTriggerTouched)  mask |= ButtonMask.Trigger;            

            return mask;
        }

        /// <summary>
        /// Builds 4x4 combined matrix for given translation and rotation
        /// </summary>        
        private Matrix3D BuildControllerMatrix(double x, double y, double z, double yaw, double pitch, double roll)
        {
            Matrix3D m = new Matrix3D();
             
            m.Rotate(Helpers.QuaternionFromYawPitchRoll(
                (float) MathHelpers.DegToRad(yaw),
                (float) MathHelpers.DegToRad(pitch),
                (float) MathHelpers.DegToRad(roll)));

            m.Translate(new Vector3D(x, y, z));

            return m;
        }
    }
}
