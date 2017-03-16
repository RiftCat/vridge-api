using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using VRE.Vridge.API.Client.Messages;
using VRE.Vridge.API.Client.Proxy;
using VRE.Vridge.API.DesktopTester.Service.Controller;
using VRE.Vridge.API.DesktopTester.Service.HeadTracking;

namespace VRE.Vridge.API.DesktopTester.ViewModel
{
    public class ControlViewModel : ViewModelBase
    {
        private Size canvasSize;

        // Canvas marker position
        private double markerLeftOffset;
        private double markerTopOffset;
        
        // Position currently set by XYZ sliders
        private double positionX;
        private double positionY;
        private double positionZ;

        // Rotation currently set by rotational sliders
        private double yaw;
        private double pitch;
        private double roll;

        // Analog states currently as set by sliders in controller panel
        private double analogX;
        private double analogY;
        private double analogTrigger;

        // State of OpenVR buttons as stored by ToggleButtons in controller panel
        private bool isMenuPressed;
        private bool isSystemPressed;

        // Which thing is controller by UI (head or controllers)
        private ControlTarget selectedControlTarget;

        // What head tracking mode is in effect
        private TrackingType selectedHeadTrackingMode;
        
        // API client         
        private readonly APIClient apiClient = new APIClient();

        // Helper services
        private TrackingService headTrackingService;
        private ControllerService controllerService;

        public ControlViewModel()
        {
            
            HeadTrackingModes = new  Dictionary<TrackingType, string>()
            {
                {TrackingType.Position, "Send position only"},
                {TrackingType.PositionAndRotation, "Send position and rotation"},
                {TrackingType.SyncOffset, "Send synchronous offset"},
                {TrackingType.AsyncOffset, "Send asynchronous offset"},
            };

            ControlTargets = new Dictionary<ControlTarget, string>()
            {
                {ControlTarget.Head, "Control head tracking"},
                {ControlTarget.Controller1, "Control VR controller #1"},
                {ControlTarget.Controller2, "Control VR controller #2"},
                {ControlTarget.Controller3, "Control VR controller #3"},
                {ControlTarget.Controller4, "Control VR controller #4"},
            };

            Connect = new RelayCommand(ConnectOrReconnect);
            Status = new RelayCommand(CheckStatus);
            ResetAsync = new RelayCommand(ResetAsyncRotation);

            // Try connecting by default
            ConnectOrReconnect();
        }

        public double MarkerSize => 20;

        #region UI Binding boilerplate code

        public RelayCommand Connect { get; private set; }

        public RelayCommand Status { get; private set; }

        public RelayCommand ResetAsync { get; private set; }

        public Dictionary<ControlTarget, string> ControlTargets { get; }

        public Dictionary<TrackingType, string> HeadTrackingModes { get; }        

        public ControlTarget SelectedControlTarget
        {
            get { return selectedControlTarget; }
            set
            {                
                selectedControlTarget = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => IsControllingHeadTracking);
                RaisePropertyChanged(() => IsControllingControllers);

            }
        }

        public bool IsControllingHeadTracking => SelectedControlTarget == ControlTarget.Head;

        public bool IsControllingControllers => SelectedControlTarget != ControlTarget.Head;

        public TrackingType SelectedHeadTrackingMode
        {
            get { return selectedHeadTrackingMode; }
            set
            {
                // If switching off the sync offset mode
                if (selectedHeadTrackingMode == TrackingType.SyncOffset && value != TrackingType.SyncOffset)
                {
                    headTrackingService.StopSyncOffsetMode();
                }

                // If switching on sync offset mode
                if (value == TrackingType.SyncOffset && selectedHeadTrackingMode != TrackingType.SyncOffset)
                {
                    headTrackingService?.BeginSyncOffsetMode();
                }

                selectedHeadTrackingMode = value;
                RaisePropertyChanged();

                // Send current slider states to VR
                OnRotationChanged();
                OnPositionChanged();
            }
        }

        public double PositionX
        {
            get { return positionX; }
            set
            {
                positionX = value;
                RaisePropertyChanged();
                OnPositionChanged();
            }
        }

        public double PositionY
        {
            get { return positionY; }
            set
            {
                positionY = value;
                RaisePropertyChanged();
                OnPositionChanged();
            }
        }

        public double PositionZ
        {
            get { return positionZ; }
            set
            {
                positionZ = value;
                RaisePropertyChanged();
                OnPositionChanged();
            }
        }

        public double MarkerLeftOffset
        {
            get { return markerLeftOffset; }
            set
            {
                markerLeftOffset = value;
                RaisePropertyChanged();
            }
        }

        public double MarkerTopOffset
        {
            get { return markerTopOffset; }
            set
            {
                markerTopOffset = value;
                RaisePropertyChanged();
            }
        }

        public double Yaw
        {
            get { return yaw; }
            set
            {
                yaw = value;
                RaisePropertyChanged();
                OnRotationChanged();
            }
        }

        public double Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                RaisePropertyChanged();
                OnRotationChanged();
            }
        }

        public double Roll
        {
            get { return roll; }
            set
            {
                roll = value;
                RaisePropertyChanged();
                OnRotationChanged();
            }
        }

        // Controller specific
        public double AnalogX
        {
            get { return analogX; }
            set
            {
                analogX = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public double AnalogY
        {
            get { return analogY; }
            set
            {
                analogY = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public double AnalogTrigger
        {
            get { return analogTrigger; }
            set
            {
                analogTrigger = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public bool IsMenuPressed
        {
            get {return isMenuPressed; }
            set
            {
                isMenuPressed = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public bool IsSystemPressed
        {
            get { return isSystemPressed; }
            set
            {
                isSystemPressed = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        #endregion

        #region Canvas related drawing calculations

        public void UpdateDrawingBounds(SizeChangedEventArgs e)
        {
            canvasSize = e.NewSize;
            OnPositionChanged();
        }

        public void NotifyCanvasDrag(Vector delta)
        {
            PositionX += delta.X / canvasSize.Width * 6;
            PositionZ += delta.Y / canvasSize.Height * 6;
        }


        // 6 == XYZ range [-3,3]
        private double NormXToCanvasX(double normX) => (normX + 3) / 6 * canvasSize.Width - MarkerSize / 2;
        private double NormYToCanvasY(double normY) => (normY + 3) / 6 * canvasSize.Height - MarkerSize / 2;

        #endregion

        private void ConnectOrReconnect()
        {
            try
            {
                // Close active connections (if restarting)
                apiClient.DisconnectHeadTrackingProxy();    
                apiClient.DisconnectControllerProxy();

                // Give it some time to clean up
                Thread.Sleep(10);

                // Connect to the services
                headTrackingService = new TrackingService(apiClient.ConnectHeadTrackingProxy());
                controllerService = new ControllerService(apiClient.ConnectToControllerProxy());
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckStatus()
        {
            var status = apiClient.GetStatus();
            if (status == null)
            {
                MessageBox.Show("Can't reach API.");
                return;
            }

            var msg = "API endpoint status: ";
            status.Endpoints.ForEach(e => msg += $"{e.Name} is {(ControlResponseCode)e.Code}, ");
            MessageBox.Show(msg);
        }

        private void ResetAsyncRotation()
        {
            headTrackingService?.ResetAsyncOffset();
        }

        private void OnPositionChanged()
        {
            // Notify UI
            MarkerLeftOffset = NormXToCanvasX(PositionX);
            MarkerTopOffset = NormYToCanvasY(PositionZ);

            SendCurrentTrackingDataToVridge();            
        }
        
        private void OnRotationChanged()
        {            
            SendCurrentTrackingDataToVridge();            
        }

        private void OnControllerStateChanged()
        {
            SendControllerStateToVridge();
        }

        private void SendCurrentTrackingDataToVridge()
        {
            try
            {
                if (SelectedControlTarget == ControlTarget.Head)
                {
                    SendHeadTrackingDataToVridge();
                }
                else
                {
                    SendControllerStateToVridge();
                }
                
            }
            catch (TimeoutException x)
            {
                MessageBox.Show("Reconnecting because: \n\n" + x);
                ConnectOrReconnect();
            }
        }

        private void SendHeadTrackingDataToVridge()
        {
            switch (SelectedHeadTrackingMode)
            {
                case TrackingType.Position:
                    headTrackingService?.SendPositionOnly(PositionX, PositionY, PositionZ);
                    break;
                case TrackingType.PositionAndRotation:
                    headTrackingService?.SendRotationAndPosition(Yaw, Pitch, Roll, PositionX, PositionY, PositionZ);
                    break;
                case TrackingType.SyncOffset:
                    headTrackingService?.UpdateOffsetMatrix(Yaw, Pitch, Roll, PositionX, PositionY, PositionZ);
                    break;
                case TrackingType.AsyncOffset:
                    headTrackingService?.SendAsyncOffset(Yaw, Pitch, Roll);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SendControllerStateToVridge()
        {
            try
            {
                // Enum members use int as underlying type so each controller has unique ID [1-4]
                int controllerId = (int) SelectedControlTarget;

                controllerService?.SendControllerState(controllerId,
                    PositionX, PositionY, PositionZ,
                    Yaw, Pitch, Roll,
                    AnalogX, AnalogY, AnalogTrigger, 
                    IsMenuPressed, IsSystemPressed);
            }
            catch (TimeoutException x)
            {
                MessageBox.Show("Reconnecting because: \n\n" + x);
                ConnectOrReconnect();
            }
        }

    }
}