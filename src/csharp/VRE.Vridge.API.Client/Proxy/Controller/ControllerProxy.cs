using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NetMQ;
using VRE.Vridge.API.Client.Messages.OpenVR;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using VRE.Vridge.API.Client.Messages.v3.Controller.Requests;
using VRE.Vridge.API.Client.Messages.v3.Controller.Responses;
using VRController = VRE.Vridge.API.Client.Messages.v3.Controller.VRController;
using VRE.Vridge.API.Client.Remotes;


namespace VRE.Vridge.API.Client.Proxy.Controller
{
    /// <summary>
    /// Lower level access method for sending controller data to VRidge. Consider using <see cref="VridgeRemote"/> 
    /// with <see cref="VridgeRemote.Controller"/> for easier operation.
    /// </summary>
    public class ControllerProxy : ClientProxyBasePB
    {    
        private ControllerStateRequest controller;        

        /// <summary>
        /// Creates controller proxy and establishes connection. 
        /// </summary>
        /// <param name="endpointAddress">
        /// Endpoint address (ip:port). <see cref="APIClient.ConnectToControllerProxy">Should be requested from control connection</see>. 
        /// </param>
        /// <param name="keepAlive">
        /// True if automatic pings should keep connection alive even if caller doesn't send data.
        /// </param>
        public ControllerProxy(string endpointAddress, bool keepAlive = false)
            : base(endpointAddress, keepAlive)
        {
            controller = new ControllerStateRequest()
            {
                TaskType = (byte) ControllerTask.SendFullState,    
                Version = ControllerStateRequest.CurrentVersion
            };
        }

        /// <summary>
        /// Send full single VR controller state to VR. 
        /// </summary>        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendControllerData(VRController state)
        {
            controller.ControllerState = state;
            controller.Version = ControllerStateRequest.CurrentVersion;
            SendMessage(controller);
        }

        /// <summary>
        /// Recenter head tracking. Works the same as pressing recenter hotkey as configured in VRidge settings.
        /// </summary>
        public void RecenterHead()
        {
            SendMessage(new ControllerStateRequest()
            {
                ControllerState = default(VRController),
                TaskType = (byte) ControllerTask.RecenterHead,
                Version = ControllerStateRequest.CurrentVersion
            });
        }

        /// <summary>
        /// Disconnected from controller API and frees the API for other clients.
        /// </summary>
        public void Disconnect()
        {            
            var disconnectRequest = new ControllerStateRequest()
            {                
                TaskType = (byte)ControllerTask.Disconnect
            };

            try
            {
                SendMessage(disconnectRequest);
            }
            catch (TimeoutException)
            {
                // Connection probably dropped another way, ignoring
            }
            catch (FiniteStateMachineException)
            {
                // Connection state invalid, close anyway
            }
            CloseSocket();
        }        

        private ControllerStateResponse SendMessage(ControllerStateRequest obj) => SendMessage<ControllerStateResponse>(obj);
    }
}
