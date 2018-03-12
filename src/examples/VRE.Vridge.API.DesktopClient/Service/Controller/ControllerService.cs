using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Media3D;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages.OpenVR;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using VRE.Vridge.API.Client.Messages.v3.Controller.Requests;
using VRE.Vridge.API.Client.Proxy.Controller;

namespace VRE.Vridge.API.DesktopTester.Service.Controller
{
    class ControllerService : IDisposable
    {
        private readonly ControllerProxy proxy;

        private uint packetNum = 0;
        private VRController controllerState;
        private bool isActive = true;

        private readonly AutoResetEvent controllerStateChangeWaitHandle = new AutoResetEvent(false);

        public ControllerService(ControllerProxy proxy)
        {
            this.proxy = proxy;

            var sendingThread = new Thread(StateSendingLoop)
            {
                IsBackground = true
            };

            // Set initial state
            SetControllerState(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false);

            sendingThread.Start();            
        }

        /// <summary>
        /// Sends full controller state as defined by arguments.
        /// </summary>        
        public void SetControllerState(
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
            bool isSystemPressed,
            bool isTriggerPressed)
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
                ulButtonPressed = BuildButtonPressedMask(isMenuPressed, isSystemPressed, isTriggerPressed),
                ulButtonTouched = BuildButtonTouchedMask(true, true), 
                unPacketNum = ++packetNum

            };

            controllerState = new VRController()
            {
                ButtonState = buttons,
                Status = 0,
                ControllerId = controllerId,
                OrientationMatrix = BuildControllerMatrix(positionX, positionY, positionZ, yaw, pitch, roll).FlattenAsColumnMajor()
            };

            controllerStateChangeWaitHandle.Set();
        }

        private void StateSendingLoop()
        {
            try
            {
                while (isActive)
                {
                    proxy.SendControllerData(controllerState);
                    
                    // Send state when it was updated or at least once very 1000ms to read haptic feedback
                    controllerStateChangeWaitHandle.WaitOne(millisecondsTimeout: 1000);
                }
            }
            catch (Exception x)
            {
                // Possibly connection crashed or state became corrupted in some way
                Debug.WriteLine(x);
                Dispose();
            }
            
        }

        /// <summary>
        /// Maps button pressed state into OpenVR packed ulong.
        /// </summary>                
        private ulong BuildButtonPressedMask(bool isMenuPressed, bool isSystemPressed, bool isTriggerPressed)
        {            
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

        public void Dispose()
        {
            isActive = false;    
            proxy?.Dispose();                    
        }
    }
}
