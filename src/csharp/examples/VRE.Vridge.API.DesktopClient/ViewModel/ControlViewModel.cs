using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages;
using VRE.Vridge.API.Client.Messages.BasicTypes;
using VRE.Vridge.API.Client.Messages.Control;
using VRE.Vridge.API.Client.Messages.v3.Broadcast;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using VRE.Vridge.API.Client.Proxy;
using VRE.Vridge.API.Client.Proxy.Broadcasts;
using VRE.Vridge.API.Client.Proxy.Controller;
using VRE.Vridge.API.Client.Proxy.HeadTracking;
using VRE.Vridge.API.Client.Remotes;
using Vector = System.Windows.Vector;

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
        private bool isTriggerPressed;
        private bool isGripPressed;
        private bool isTouchpadPressed;
        private bool isTouchpadTouched;

        // Haptic pulse info
        private DateTime? lastHapticPulseTime = null;
        private uint lastHapticPulseLengthUs = 0;

        // Which thing is controller by UI (head or controllers)
        private ControlTarget selectedControlTarget;

        // What head tracking mode is in effect
        private TrackingType selectedHeadTrackingMode;

        // How controller poses relate to the head
        private ControllerMode selectedHeadRelation;
        
        private bool wasHMDTracked = true;

        private readonly VridgeRemote vridge;

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

            HeadRelations = new Dictionary<ControllerMode, string>()
            {
                {ControllerMode.Unrelated, "Unrelated to head pose"},
                {ControllerMode.SticksToHead, "Attach poses to head pose"},
                {ControllerMode.ThreeDof, "Remap 3->6 DOF"},
                {ControllerMode.StickyThreeDof, "Remap 3->6 DOF (sticky)"},
                {ControllerMode.IsInHeadSpace, "Treat poses as in head-space"},                
            };

            //Status = new RelayCommand(CheckStatus);
            ResetAsync = new RelayCommand(ResetAsyncRotation);
            Recenter = new RelayCommand(RecenterView);
            Discover = new RelayCommand(DiscoverExecute);
            CheckStatus = new RelayCommand(CheckStatusExecute);

            // Kill all active listeners, otherwise process won't quit
            Application.Current.Exit += (sender, args) => vridge?.Dispose();

            // Try connecting by default
            vridge = new VridgeRemote(
                "localhost",
                "Desktop-Tester",
                Capabilities.Controllers | Capabilities.HeadTracking);

            vridge.HapticPulse += OnHapticFeedbackReceived;
        }

        public double MarkerSize => 20;

        #region UI Binding boilerplate code

        public RelayCommand Connect { get; private set; }

        public RelayCommand CheckStatus { get; private set; }

        public RelayCommand Discover { get; private set; }

        public RelayCommand ResetAsync { get; private set; }

        public RelayCommand Recenter { get; private set; }

        public Dictionary<ControlTarget, string> ControlTargets { get; }

        public Dictionary<TrackingType, string> HeadTrackingModes { get; }

        public Dictionary<ControllerMode, string> HeadRelations { get; }

        public ControlTarget SelectedControlTarget
        {
            get { return selectedControlTarget; }
            set
            {              
                selectedControlTarget = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => IsControllingHeadTracking);
                RaisePropertyChanged(() => IsControllingControllers);
                RaisePropertyChanged(() => IsRotationalControlVisible);
                RaisePropertyChanged(() => IsPositionalControlVisible);
                RaisePropertyChanged(() => IsPitchAndRollControlVisible);
            }
        }

        public TrackingType SelectedHeadTrackingMode
        {
            get { return selectedHeadTrackingMode; }
            set
            {
                selectedHeadTrackingMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => IsRotationalControlVisible);
                RaisePropertyChanged(() => IsPositionalControlVisible);
                RaisePropertyChanged(() => IsPitchAndRollControlVisible);

                // Send current slider states to VR
                OnRotationChanged();
                OnPositionChanged();
            }
        }

        public ControllerMode SelectedHeadRelation
        {
            get { return selectedHeadRelation; }
            set
            {
                selectedHeadRelation = value;
                RaisePropertyChanged();
            }
        }

        public bool IsControllingHeadTracking => SelectedControlTarget == ControlTarget.Head;

        public bool IsControllingControllers => SelectedControlTarget != ControlTarget.Head;

        public bool IsRotationalControlVisible => SelectedHeadTrackingMode != TrackingType.SyncOffset;

        public bool IsPositionalControlVisible => SelectedControlTarget != ControlTarget.Head || SelectedHeadTrackingMode != TrackingType.AsyncOffset;

        public bool IsPitchAndRollControlVisible => SelectedControlTarget != ControlTarget.Head ||
            (IsRotationalControlVisible && SelectedHeadTrackingMode != TrackingType.AsyncOffset);



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

        public bool IsTriggerPressed
        {
            get { return isTriggerPressed; }
            set
            {
                isTriggerPressed = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public bool IsGripPressed
        {
            get { return isGripPressed; }
            set
            {
                isGripPressed = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public bool IsTouchpadPressed
        {
            get { return isTouchpadPressed; }
            set
            {
                isTouchpadPressed = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public bool IsTouchpadTouched
        {
            get { return isTouchpadTouched; }
            set
            {
                isTouchpadTouched = value;
                RaisePropertyChanged();
                OnControllerStateChanged();
            }
        }

        public string HapticPulseInfo
        {
            get
            {
                if (!lastHapticPulseTime.HasValue)
                {
                    return "No haptic pulses received.";
                }
                else
                {
                    return $"Last haptic pulse:\n" +
                           $"{lastHapticPulseTime.Value.ToLocalTime()}\n" +
                           $"({lastHapticPulseLengthUs} us)";
                }
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

        private void DiscoverExecute()
        {
            var activeVridgeServers = VridgeRemote.ActiveVridgeServers;
            if (activeVridgeServers.Count == 0)
            {
                MessageBox.Show("No active servers found.");
            }
            else
            {
                MessageBox.Show("Active servers: " + Environment.NewLine + Environment.NewLine +
                                string.Join(Environment.NewLine, activeVridgeServers.Select(x => x.ToString())));
            }
        }

        private void CheckStatusExecute()
        {
            var currentStatus = vridge.Status;
            MessageBox.Show(string.Join("\n", currentStatus.Endpoints));
        }

        private void ResetAsyncRotation()
        {
            vridge.Head?.SetAsyncOffset(0);
        }

        private void RecenterView()
        {
            vridge.Head?.Recenter();
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
            UpdateControllerState();
        }

        private void OnHapticFeedbackReceived(object sender, HapticPulse hapticPulse)
        {
            lastHapticPulseLengthUs = hapticPulse.LengthUs;
            lastHapticPulseTime = DateTime.Now;

            RaisePropertyChanged(() => HapticPulseInfo);           
        }

        private void SendCurrentTrackingDataToVridge()
        {
            if (SelectedControlTarget == ControlTarget.Head)
            {
                SendHeadTrackingDataToVridge();
            }
            else
            {
                UpdateControllerState();
            }
        }

        private void SendHeadTrackingDataToVridge()
        {
            switch (SelectedHeadTrackingMode)
            {
                case TrackingType.Position:
                    vridge.Head?.SetPosition(PositionX, PositionY, PositionZ);
                    break;
                case TrackingType.PositionAndRotation:
                    vridge.Head?.SetRotationAndPosition((float)MathHelpers.DegToRad(-yaw),
                        (float)MathHelpers.DegToRad(pitch),
                        (float)MathHelpers.DegToRad(roll),
                        (float)PositionX, (float)PositionY, (float)PositionZ);
                    break;                
                case TrackingType.AsyncOffset:
                    vridge.Head?.SetAsyncOffset(MathHelpers.DegToRad(Yaw));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Mark HMD as in/out of tracking range when it crosses tracking bounds
            var isHmdTracked = Math.Abs(PositionX) <= 3 && Math.Abs(PositionZ) <= 3;
            if (wasHMDTracked && !isHmdTracked)
            {
                vridge.Head?.SetStatus(false);
                wasHMDTracked = false;
            }

            if (!wasHMDTracked && isHmdTracked)
            {
                vridge.Head?.SetStatus(true);
                wasHMDTracked = true;
            }

            
        }

        private void UpdateControllerState()
        {
            Vector3? position = new Vector3((float) PositionX, (float) PositionY, (float) PositionZ);
            var headRelation = (HeadRelation)SelectedHeadRelation;

            if (SelectedHeadRelation == ControllerMode.ThreeDof)
            {
                position = null;
                headRelation = HeadRelation.Unrelated;
            }
            else if (SelectedHeadRelation == ControllerMode.StickyThreeDof)
            {
                position = null;
                headRelation = HeadRelation.SticksToHead;
            }

            int controllerId = (int)SelectedControlTarget;
                vridge.Controller?.SetControllerState(controllerId,
                headRelation,
                controllerId % 2 == 0 ? HandType.Left : HandType.Right,
                Quaternion.CreateFromYawPitchRoll(
                    (float)MathHelpers.DegToRad(Yaw),
                    (float)MathHelpers.DegToRad(Pitch),
                    (float)MathHelpers.DegToRad(Roll)), 
                position, 
                AnalogX, AnalogY, AnalogTrigger,
                IsMenuPressed, IsSystemPressed, isTriggerPressed, isGripPressed, isTouchpadPressed, isTouchpadTouched);
        }

    }
}