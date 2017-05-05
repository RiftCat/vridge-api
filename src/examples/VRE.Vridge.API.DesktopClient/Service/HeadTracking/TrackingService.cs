using System;
using System.Windows.Media.Media3D;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages.v1;
using VRE.Vridge.API.Client.Proxy.HeadTracking;

namespace VRE.Vridge.API.DesktopTester.Service.HeadTracking
{
    public class TrackingService
    {               
        private readonly HeadTrackingProxy proxy;

        private Vector3D position;
        private Matrix3D offsetMatrix;

        public TrackingService(HeadTrackingProxy proxy)
        {
            this.proxy = proxy;
        }

        /// <summary>
        /// Sets current orientation as new center.
        /// </summary>
        public void RecenterView()
        {
            proxy.RecenterView();
        }

        /// <summary>
        /// Toggles HMD tracking state between "actively tracked" and "out of tracking range".
        /// </summary>        
        public void ChangeStatus(bool isCurrentlyBeingTracked)
        {
            proxy.ChangeTrackingState(isCurrentlyBeingTracked);
        }

        /// <summary>
        /// Sends absolute position to VR without touching rotation.
        /// </summary>        
        public void SendPositionOnly(double x, double y, double z)
        {
            position = new Vector3D(x, y, z);
            proxy.SetPosition((float)x, (float)y, (float)z);
        }
               
        /// <summary>
        /// Sends absolute position and rotation. VRidge will expect a steady stream of rotational data
        /// after using this call. It will timeout and revert back to mobile tracking data if no rotational data 
        /// is sent for a while.
        /// </summary>        
        public void SendRotationAndPosition(double yaw, double pitch, double roll, double x, double y, double z)
        {
            position = new Vector3D(x, y, z);

            // Invert yaw so sliding to the right rotates to the right
            // Makes more sense with horizontal slider

            proxy.SetRotationAndPosition(
                (float)MathHelpers.DegToRad(-yaw),
                (float)MathHelpers.DegToRad(pitch),
                (float)MathHelpers.DegToRad(roll),
                (float)x, (float)y, (float)z);
        }

        /// <summary>
        /// Return latest mobile pose. Can be used by async offset user to find
        /// a reference point for drift correction.
        /// </summary>
        /// <returns>Column-major 4x4 pose matrix.</returns>
        public float[] GetCurrentPhonepose()
        {
            return proxy.GetCurrentPhonePose();
        }

        /// <summary>
        /// Store rotational offset matrix on VRidge side which will be combined with 
        /// every incoming mobile pose matrix. Effectively: lagless offset.
        /// </summary>
        public bool SendAsyncOffset(double yaw, double pitch, double roll)
        {
            // Invert yaw so sliding to the right rotates to the right
            // Makes more sense with horizontal slider

            return proxy.SetAsyncOffset(
                (float)MathHelpers.DegToRad(-yaw),
                (float)MathHelpers.DegToRad(pitch),
                (float)MathHelpers.DegToRad(roll));
        }

        /// <summary>
        /// Resets offset stored with <see cref="SendAsyncOffset"/>
        /// </summary>        
        public bool ResetAsyncOffset()
        {
            if (proxy == null) return false;

            return proxy.ResetAsyncOffset();
        }

        /// <summary>
        /// Begin listening to mobile tracking data. <see cref="OnNewSyncPose"/>
        /// is called whenever new data is available and modifiable.
        /// </summary>
        public void BeginSyncOffsetMode()
        {
            if (proxy == null) return;

            proxy.BeginSyncOffset(OnNewSyncPose);
            proxy.SyncModeDisconnected += OnSyncDisconnect;
        }

        /// <summary>
        /// Update local matrix that is combined with each mobile matrix for sync offset mode.
        /// </summary> 
        public void UpdateOffsetMatrix(double yaw, double pitch, double roll, double x, double y, double z)
        {
            // Create 4x4 transformation matrix with help of WPF built-in methods
            offsetMatrix = Matrix3D.Identity;

            position = new Vector3D(x, y, z);

            offsetMatrix.Rotate(Helpers.QuaternionFromYawPitchRoll((float)MathHelpers.DegToRad(yaw), (float)MathHelpers.DegToRad(pitch), (float)MathHelpers.DegToRad(roll)));

        }

        /// <summary>
        /// Stops listening to mobile tracking data.
        /// </summary>
        public void StopSyncOffsetMode()
        {
            if (proxy == null) return;

            proxy.StopSyncOffset();
            proxy.SyncModeDisconnected -= OnSyncDisconnect;
        }

        /// <summary>
        /// Close the connection and let other API clients use head tracking service.
        /// </summary>
        public void Disconnect()
        {
            proxy?.Disconnect();
        }

        /// <summary>
        /// Called whenever new mobile pose is sent to VRidge.
        /// </summary>        
        private void OnNewSyncPose(float[] poseMatrix)
        {
            /* Android's Matrix is column-major while .NET's Matrix3D uses row-major layout 
             * therefore matrix transposition is required */
            Matrix3D currentData = new Matrix3D(
                poseMatrix[0], poseMatrix[4], poseMatrix[8], poseMatrix[12],
                poseMatrix[1], poseMatrix[5], poseMatrix[9], poseMatrix[13],
                poseMatrix[2], poseMatrix[6], poseMatrix[10], poseMatrix[14],
                poseMatrix[3], poseMatrix[8], poseMatrix[11], poseMatrix[15]
                );

            // Now we combine our offset with phone pose (only rotations are used by both matrices)
            var combinedMatrix = Matrix3D.Multiply(currentData, offsetMatrix);

            // Override position with absolutely placed position
            combinedMatrix.OffsetX = position.X;
            combinedMatrix.OffsetY = position.Y;
            combinedMatrix.OffsetZ = position.Z;                        

            // Override original matrix data with our modified matrix
            Array.Copy(combinedMatrix.FlattenAsColumnMajor(), poseMatrix, 16);

            // Arrays are passed by reference so calling method will have modified data
        }

        private void OnSyncDisconnect(object sender, Exception e)
        {
            StopSyncOffsetMode();
        }
    }
}
