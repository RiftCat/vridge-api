using System;
using System.Runtime.InteropServices;
using ProtoBuf;
using VRE.Vridge.API.Client.Messages.v3.HeadTracking.Responses;

namespace VRE.Vridge.API.Client.Messages.v3.HeadTracking.Requests
{
    /// <summary>
    /// Request to a VRidge API server that contains head tracking data or queries.
    /// <see cref="HeadTrackingResponse"/> will be returned as a response.   
    /// </summary>
    [ProtoContract]
    public struct HeadTrackingRequest
    {
        private const int CurrentVersion = 3;

        /// <summary>
        /// Should always be 3 for v3
        /// </summary>
        [ProtoMember(1)]
        public int Version;

        /// <summary>
        /// Describes how API should handle the incoming data, see <see cref="Task"/>.
        /// </summary>        
        [ProtoMember(2)]
        public byte TaskType;

        /// <summary>
        /// Tracking data itself.
        /// </summary>        
        [ProtoMember(3)]
        public byte[] Data;
        
        /// <summary>
        /// Task stored in <see cref="HeadTrackingRequest.TaskType"/> defines what kind of data the packet is carrying.
        /// <remarks>
        ///  * Matrices use column-major order byte layout            
        ///                          
        ///  * Rotational send tasks (0-4) override VRidge's rotational tracking data. You need to send continous stream of data. 
        ///    Vridge will fall back to its own data if it doesn't receive your data after some time (few seconds). 
        ///    Same timeout applies to RequestSyncOffset.  */
        /// </remarks>
        /// </summary>
        public enum Task
        {
            /// <summary>
            /// Packet closes your head tracking API connection and lets other clients use it.
            /// </summary>
            Disconnect = 255,

            /// <summary>
            /// Changes state of tracked device. Data[0] contains a byte representing TrackedDeviceStatus.
            /// </summary>
            ChangeState = 254,

            /// <summary>
            /// <see cref="Data"/> contains 4x4 float matrix but only rotation component is used 
            /// float[16] - 64B
            /// </summary>
            SendPoseMatrixRotationOnly = 0,

            /// <summary>
            /// <see cref="Data"/> contains 4x4 float matrix, both rotation and position is used.
            /// float[16] - 64B
            /// </summary>            
            SendPoseMatrixFull = 6,

            /// <summary>
            /// <see cref="Data"/> contains 3x3 float rotation matrix .
            /// float[9] - 36B
            /// </summary>                        
            SendRotationMatrix = 1,

            // [NYI] - float[3] gyro, float[3] accel, float[3] magnet, long timestamp(ms) N
            //SendRawData = 2,

            /// <summary>
            /// <see cref="Data"/> contains radian rotation and position data.
            /// [pitch(+up), yaw (+to left), roll(+left), X, Y, Z]
            /// float[6] - 24B
            /// </summary>
            SendRadRotationAndPosition = 3,

            /// <summary>
            /// <see cref="Data"/> contains quaternion rotation and position data.
            /// [X, Y, Z, W, X, Y, Z] (quaternion, then abs. position)
            /// float[7] - 28B
            /// </summary>            
            SendQuatRotationAndPosition = 4,

            /// <summary>
            /// <see cref="Data"/> contains absolute position only. Phone rotation will be used.
            /// [X, Y, Z]
            /// float[3] - 12B
            /// </summary>               
            SendPositionOnly = 5,

            /// <summary>
            /// Sets current orientation as absolute center.
            /// </summary>
            Recenter = 50,

            /// <summary>
            /// Requests a response with current pose data so the client can modify it. 
            /// This will prevent mobile data from reaching VR so you need to use any 
            /// Send* (0-6) packet as next req to keep the tracking data flowing.
            /// </summary>            
            RequestSyncOffset = 100,

            /// <summary>
            /// Requests a response with current data but doesn't alter VRidge internal state in any way. 
            /// You can use it to find a reference for <see cref="SetYawOffset"/>
            /// HeadTrackingResponse.TrackedPose will contain result response.
            /// </summary>
            RequestReadOnlyPose = 199,
               

            /// <summary>
            /// Stores rotational yaw offset. It will be applied to each phone rotation packet until VRidge quits. 
            /// You can use <see cref="RequestReadOnlyPhonePose"/> to refresh the offset every now and then according to latest mobile data
            /// 
            /// <see cref="Data"/> should contain float yaw (+to left) in 4B-long array            
            /// float - 4B
            /// </summary>
            SetYawOffset = 201,

            /// <summary>
            /// Delete stored async offset that was previously provided with
            /// <see cref="SetYawOffset"/>
            /// </summary>
            ResetYawOffset = 210,


            /// <summary>
            /// DEPRECATED: Will work but using RequestReadOnlyPosePair is recommended.
            /// Requests a response with current data but doesn't alter VRidge internal state in any way. 
            /// You can use it to find a reference for <see cref="SetYawOffset"/>            
            /// </summary>
            [Obsolete("Use RequestReadOnlyPose instead.")]
            RequestReadOnlyPhonePose = 200,
        }

        /// <summary>
        /// Creates a packet that sets VR position at specific location. 
        /// This position is then combined with phone-provided rotation.
        /// </summary>        
        public static HeadTrackingRequest CreatePositionPacket(float x, float y, float z)
        {
            var packet = new HeadTrackingRequest()
            {
                Version = CurrentVersion,
                TaskType = (int)Task.SendPositionOnly,                
                Data = new byte[12],
            };

            Buffer.BlockCopy(new [] { x, y, z }, 0, packet.Data, 0, 12);

            return packet;
        }

        /// <summary>
        /// Creates a packet that overrides VRidge data with full pose (rotation and position).
        /// Rotation uses radians.
        /// </summary>        
        /// <returns></returns>
        public static HeadTrackingRequest CreateRotationPositionVectorPacket(float yaw, float pitch, float roll, float x, float y, float z)
        {
            var packet = new HeadTrackingRequest()
            {
                Version = CurrentVersion,
                TaskType = (int)Task.SendRadRotationAndPosition,                
                Data = new byte[24],                
            };

            Buffer.BlockCopy(new[] { pitch, yaw, roll, x, y, z }, 0, packet.Data, 0, 24);

            return packet;
        }

        /// <summary>
        /// Creates a packet that overrides VRidge data with full pose (rotation and position).
        /// Matrix is stored as column-major float flat array.
        /// </summary>        
        public static HeadTrackingRequest CreateFullPoseMatrixPacket(float[] poseMatrix)
        {
            var packet = new HeadTrackingRequest()
            {
                Version = CurrentVersion,
                TaskType = (int)Task.SendPoseMatrixFull,
                Data = new byte[64],                
            };

            Buffer.BlockCopy(poseMatrix, 0, packet.Data, 0, 64);

            return packet;
        }

        /// <summary>
        /// Creates a packet that will store an offset on VRidge side that will be applied to each phone rotation.
        /// Rotation is stored indefinitely. It can be reset with <see cref="Task.ResetYawOffset"/> packet.
        /// </summary>        
        public static HeadTrackingRequest CreateAsyncOffsetPacket(float yaw)
        {            
            var packet = new HeadTrackingRequest()
            {
                Version = CurrentVersion,
                TaskType = (int)Task.SetYawOffset,
                Data = BitConverter.GetBytes(yaw)                
            };            

            return packet;
        }

        /// <summary>
        /// Creates a packet that will set current view orientation as center.
        /// </summary>
        /// <returns></returns>
        public static HeadTrackingRequest CreateRecenterPacket()
        {
            return CreateEmptyPacketByType(Task.Recenter);
        }

        /// <summary>
        /// Creates a packet that will trigger a state change of HMD. 
        /// VR runtime will handle the state change according to its rules.
        /// </summary>        
        public static HeadTrackingRequest CreateStateChangePacket(TrackedDeviceStatus newStatus)
        {
            var packet = CreateEmptyPacketByType(Task.ChangeState);
            packet.Data = new byte[1];
            packet.Data[0] = (byte) newStatus;
            return packet;
        }        

        /// <summary>
        /// Creates an empty packet with a specific task type.
        /// </summary>        
        public static HeadTrackingRequest CreateEmptyPacketByType(Task type)
        {
            var packet = new HeadTrackingRequest()
            {
                Version = CurrentVersion,
                TaskType = (byte)type,
                Data = null              
            };            

            return packet;
        }
        
    }
}
