using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using ProtoBuf;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages.Control;
using VRE.Vridge.API.Client.Messages.Control.Responses;
using VRE.Vridge.API.Client.Messages.v3.Broadcast;
using VRE.Vridge.API.Client.Messages.v3.Discovery;
using VRE.Vridge.API.Client.Proxy;
using VRE.Vridge.API.Client.Proxy.Broadcasts;
using VRE.Vridge.API.Client.Proxy.Controller;
using VRE.Vridge.API.Client.Proxy.HeadTracking;
using VRE.Vridge.API.Client.Remotes.Beacons;

namespace VRE.Vridge.API.Client.Remotes
{
    /// <summary>
    /// Handles communication between all available VRidge remote endpoints.
    /// </summary>
    public class VridgeRemote : IDisposable
    {
        private static readonly DiscoveryClient Discovery;

        // User config
        private readonly int reconnectFrequencyMs;
        private readonly int timeoutThresholdMs;
        private readonly Capabilities capabilities;

        // Access objects
        private readonly APIClient api;        
        private ControllerRemote controller;
        private HeadRemote head;
        private BroadcastProxy broadcasts;

        // State
        private DateTime lastConnectionAttempt;
        private bool isDisposed = false;

        static VridgeRemote()
        {
            Discovery = new DiscoveryClient();
        }

        public VridgeRemote(
            string serverIp,
            string appName,
            Capabilities capabilities,
            int reconnectFrequencyMs = 2000,
            int timeoutThresholdMs = 100)
        {
            this.reconnectFrequencyMs = reconnectFrequencyMs;
            this.timeoutThresholdMs = timeoutThresholdMs;
            this.capabilities = capabilities;

            api = new APIClient(serverIp, appName);
        }

        public event EventHandler<HapticPulse> HapticPulse; 

        /// <summary>
        /// Returns list of currently running API servers in reachable networks.
        /// </summary>
        public static List<VridgeServerBeacon> ActiveVridgeServers => Discovery.ActiveVridgeServers;        

        /// <summary>
        /// Returns controller remote if connection seems valid. Will return null if connection is not in healthy state.
        /// Always check for null as a indicator of connection state.
        /// </summary>        
        public ControllerRemote Controller
        {
            get
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException("You already disposed this remote. Create new one.");
                }

                lock (this)
                {
                    if (controller != null && !controller.IsDisposed)
                    {
                        return controller;
                    }

                    if (lastConnectionAttempt.AddMilliseconds(reconnectFrequencyMs) < DateTime.Now)
                    {
                        return TrySettingUpConnection() ? controller : null;
                    }

                    return null;
                }                
            }
        }

        /// <summary>
        /// Returns head remote if connection seems valid. Will return null if connection is not in healthy state.
        /// Always check for null as a indicator of connection state.
        /// </summary>
        public HeadRemote Head
        {
            get
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException("You already disposed this remote. Create new one.");
                }

                lock (this)
                {
                    if (head != null && !head.IsDisposed)
                    {
                        return head;
                    }

                    if (lastConnectionAttempt.AddMilliseconds(reconnectFrequencyMs) < DateTime.Now)
                    {
                        return TrySettingUpConnection() ? head : null;
                    }

                    return null;
                }
            }
        }

        public APIStatus Status
        {
            get
            {
                lock (this)
                {
                    return api.GetStatus();
                }                
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (isDisposed) return;

                isDisposed = true;
                HapticPulse = null;
                DisconnectAllEndpoints();
                Discovery.Dispose();
                
            }
        }

        /// <summary>
        /// Checks if current instance of VRidgeRemote can communicate
        /// with API without establishing permanent connection.
        /// </summary>        
        public VridgeRemoteConnectionStatus PreConnectionCheck(int timeoutMs)
        {
            lock (this)
            {
                try
                {
                    // First, check if we can even query status
                    var status = api.GetStatus(timeoutMs);
                    if (status == null)
                    {
                        return VridgeRemoteConnectionStatus.Unreachable;
                    }

                    // Then verify if headTracking is available if we want it
                    if (capabilities.HasFlag(Capabilities.HeadTracking))
                    {
                        var endpoint = status.Endpoints.FirstOrDefault(x => x.Name == EndpointNames.HeadTracking);
                        
                        if (endpoint == null)
                        {
                            // This shouldn't happen, ever
                            return VridgeRemoteConnectionStatus.UnexpectedError;
                        }

                        if (endpoint.Code == (int)ControlResponseCode.InUse)
                        {
                            return VridgeRemoteConnectionStatus.InUse;
                        }
                    }

                    // And do the same for controllers; this can be refactored (DRY)                    
                    if (capabilities.HasFlag(Capabilities.Controllers))
                    {
                        var endpoint = status.Endpoints.FirstOrDefault(x => x.Name == EndpointNames.Controller);
                        if (endpoint == null)
                        {
                            return VridgeRemoteConnectionStatus.UnexpectedError;
                        }

                        if (endpoint.Code == (int)ControlResponseCode.InUse)
                        {
                            return VridgeRemoteConnectionStatus.InUse;
                        }
                    }

                    return VridgeRemoteConnectionStatus.Okay;
                }
                catch (Exception)
                {
                    // Discard it and return unknown error. 
                    // API clients can use lower level access to read actual errors instead of VRidge remote which is fire-and-forget api.
                    return VridgeRemoteConnectionStatus.UnexpectedError;
                }

            }
        }

        // Sets up connection and returns true if all links are established.
        private bool TrySettingUpConnection()
        {
            lastConnectionAttempt = DateTime.Now;

            // Make sure API server is alive
            var status = api.GetStatus(timeoutThresholdMs);
            if (status == null) return false;

            // Reset current connections, if exist
            DisconnectAllEndpoints();

            try
            {                
                if (capabilities.HasFlag(Capabilities.HeadTracking))
                {
                    var endpointStatus = status.Endpoints.FirstOrDefault(x => x.Name == EndpointNames.HeadTracking);
                    if (endpointStatus == null || endpointStatus.Code != (int) ControlResponseCode.OK)
                    {
                        return false;
                    }

                    head = new HeadRemote(api.CreateProxy<HeadTrackingProxy>());
                }

                if (capabilities.HasFlag(Capabilities.Controllers))
                {
                    var endpointStatus = status.Endpoints.FirstOrDefault(x => x.Name == EndpointNames.Controller);
                    if (endpointStatus == null || endpointStatus.Code != (int) ControlResponseCode.OK)
                    {
                        return false;
                    }

                    controller = new ControllerRemote(api.CreateProxy<ControllerProxy>());                    
                }

                // Subscribe to haptic pulse broadcasts, if controller proxy is in use
                if (controller != null)
                {                    
                    broadcasts = api.CreateProxy<BroadcastProxy>();

                    // Forward the events to long-lived event so API user doesn't
                    // have to resubscribe on reconnect
                    broadcasts.HapticPulseReceived += (s, e) => HapticPulse?.Invoke(this, e);
                }

                return true;
            }
            catch (Exception x)
            {
                Logger.Error("Error during API connection: " + x);

                // Cleanup possibly connected endpoints
                DisconnectAllEndpoints();
                
            }

            return false;
        }

        private void DisconnectAllEndpoints()
        {
            controller?.Dispose();
            head?.Dispose();
            broadcasts?.Dispose();
            controller = null;
            head = null;
            broadcasts = null;
        }
    }
}
