using System;
using System.Threading;
using NetMQ;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages.v1.HeadTracking.Requests;
using VRE.Vridge.API.Client.Messages.v1.HeadTracking.Responses;

namespace VRE.Vridge.API.Client.Proxy.HeadTracking
{
    public class HeadTrackingProxy : ClientProxyBase
    {        
        public Action<float[]> NewSyncDataAvailable;
        public event EventHandler<Exception> SyncModeDisconnected;

        private bool isProvidingOffset;
        private Thread offsetThread;

        /// <summary>
        /// Creates head tracking proxy and establishes connection. 
        /// </summary>
        /// <param name="endpointAddress">
        /// Endpoint address (ip:port). <see cref="APIClient.ConnectHeadTrackingProxy">Should be requested from control connection</see>.
        /// </param>
        /// <param name="keepAlive">
        /// True if automatic pings should keep connection alive even if caller doesn't send data.
        /// </param>
        public HeadTrackingProxy(string endpointAddress, bool keepAlive = false) : base(endpointAddress, keepAlive) { }

        /// <summary>
        /// Sets position only and returns true if the value was accepted.
        /// </summary>        
        public bool SetPosition(float x, float y, float z)
        {
            var request = HeadTrackingRequest.CreatePositionPacket(x, y, z);
            var reply = SendMessage(request);

            return reply.ReplyCode == (int) HeadTrackingResponse.Response.AcceptedYourData;
        }

        /// <summary>
        /// Sets position and rotation and returns true if the value was accepted. 
        /// </summary>        
        public bool SetRotationAndPosition(float yaw, float pitch, float roll, float x, float y, float z)
        {
            var request = HeadTrackingRequest.CreateRotationPositionVectorPacket(yaw, pitch, roll, x, y, z);
            var reply = SendMessage(request);

            return reply.ReplyCode == (int)HeadTrackingResponse.Response.AcceptedYourData;
        }        

        /// <summary>
        /// Sets rotational offset that will be applied to each mobile pose. Use radians.
        /// </summary>
        public bool SetAsyncOffset(float pitch, float yaw, float roll)
        {
            var request = HeadTrackingRequest.CreateAsyncOffsetPacket(yaw, pitch, roll);
            var reply = SendMessage(request);

            return reply.ReplyCode == (byte)HeadTrackingResponse.Response.AcceptedYourData;
        }

        /// <summary>
        /// Clear async offset that was set with <see cref="SetAsyncOffset"/>
        /// </summary>
        /// <returns></returns>
        public bool ResetAsyncOffset()
        {
            var request = HeadTrackingRequest.CreateEmptyPacketByType(HeadTrackingRequest.Task.ResetAsyncOffset);
            var reply = SendMessage(request);

            return reply.ReplyCode == (byte) HeadTrackingResponse.Response.AcceptedYourData;
        }

        /// <summary>
        /// Request latest phone pose matrix. You can use it for <see cref="SetAsyncOffset"/>. 
        /// This method will block until fresh data is received from mobile phone.
        /// </summary>
        /// <returns>
        /// 4x4 transformation matrix flattened as column-major array.
        /// </returns>
        public float[] GetCurrentPhonePose()
        {
            var request = HeadTrackingRequest.CreateEmptyPacketByType(HeadTrackingRequest.Task.RequestReadOnlyPhonePose);
            var reply = SendMessage(request);

            var replyData = new float[16];

            Array.Copy(reply.Data, 0, replyData, 0, 16);
            return replyData;
        }

        /// <summary>
        /// Begins listening to mobile phone tracking data. You callback method will be called with modifiable tracking data.
        /// For example, you can correct drift by mixing phone tracking data with yours in real time.
        /// </summary>
        /// <param name="callback">
        /// Method that will be called with a reference to a column-major phone 4x4 pose matrix.
        /// You can modify the array but you need to do it fast to keep latency low. 
        /// </param>
        public void BeginSyncOffset(Action<float[]> callback)
        {
            NewSyncDataAvailable = callback;
        
            isProvidingOffset = true;
            offsetThread = new Thread(SyncOffsetLoop)
            {
                IsBackground = true
            };
            offsetThread.Start();
        }

        /// <summary>
        /// Stops listening to mobile phone tracking data. Use it after you no longer 
        /// need sync offset mode started by <see cref="BeginSyncOffset"/>
        /// </summary>
        public void StopSyncOffset()
        {
            NewSyncDataAvailable = null;

            isProvidingOffset = false;
            if(offsetThread != null && offsetThread.IsAlive)
                offsetThread.Join();            
        }

        /// <summary>
        /// Disconnected from head tracking API and frees the API for other clients.
        /// </summary>
        public void Disconnect()
        {
            isProvidingOffset = false;

            var disconnectRequest = new HeadTrackingRequest()
            {
                Version = 1,
                TaskType = (byte) HeadTrackingRequest.Task.Disconnect,                
            };

            try
            {
                SendMessage(disconnectRequest);
            }
            catch (TimeoutException x)
            {
                // Connection probably dropped another way, ignoring
            }
            catch (FiniteStateMachineException x)
            {
                // Connection state invalid, close anyway
            }
            CloseSocket();
        }

        private void SyncOffsetLoop()
        {
            HeadTrackingRequest reqModifiable = new HeadTrackingRequest()
            {
                Version = 1,
                Data = new byte[64],
                DataLength = 0,
                TaskType = (byte)HeadTrackingRequest.Task.RequestSyncOffset
            };

            try
            {
                var reqFrame = SerializationHelpers.StructureToByteArray(reqModifiable);

                while (isProvidingOffset)
                {
                    // Get current data
                    var response = SendRawFrame<HeadTrackingResponse>(reqFrame);

                    if (response.ReplyCode == (int) HeadTrackingResponse.Response.PhoneDataTimeout)
                    {                        
                        throw new TimeoutException("Phone is not sending new data.");
                    }

                    var currentPhonePose = new float[16];
                    Array.Copy(response.Data, currentPhonePose, 16);

                    // Notify through callback so listeners can modify the array
                    NewSyncDataAvailable?.Invoke(currentPhonePose);

                    // Send updated data
                    SendMessage(HeadTrackingRequest.CreateFullPoseMatrixPacket(currentPhonePose));
                }
            }
            catch (Exception x)
            {
                // Signal it callers if anyone is listening
                SyncModeDisconnected?.Invoke(this, x);
                Disconnect();
            }            
        }

        private HeadTrackingResponse SendMessage(object obj) => SendMessage<HeadTrackingResponse>(obj);

        
    }
}
