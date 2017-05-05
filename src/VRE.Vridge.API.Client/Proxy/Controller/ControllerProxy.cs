using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NetMQ;
using VRE.Vridge.API.Client.Messages.OpenVR;
using VRE.Vridge.API.Client.Messages.v2.Controller;
using VRE.Vridge.API.Client.Messages.v2.Controller.Requests;
using VRE.Vridge.API.Client.Messages.v2.Controller.Responses;


namespace VRE.Vridge.API.Client.Proxy.Controller
{
    public class ControllerProxy : ClientProxyBase
    {
        private ControllerRequest controller;

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
            controller = new ControllerRequest()
            {
                Version = 2,
                Origin = (byte) ControllerOrigin.Zero,
                TaskType = (byte) ControllerTask.SendFullState          
            };
        }

        /// <summary>
        /// Send full single VR controller state to VR. 
        /// </summary>        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendControllerData(VRController state)
        {
            controller.ControllerState = state;
            SendMessage(controller);
        }

        /// <summary>
        /// Disconnected from controller API and frees the API for other clients.
        /// </summary>
        public void Disconnect()
        {            
            var disconnectRequest = new ControllerRequest()
            {
                Version = 2,
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

        private ControllerStateResponse SendMessage(object obj) => SendMessage<ControllerStateResponse>(obj);
    }
}
