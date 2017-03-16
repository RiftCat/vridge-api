using System.Windows;
using System.Windows.Input;
using VRE.Vridge.API.DesktopTester.ViewModel;

namespace VRE.Vridge.API.DesktopTester.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {

        private bool isMarkerBeingDragged = false;
        private Point lastMousePosition;

        public ControlWindow()
        {
            InitializeComponent();
            this.DataContext = new ControlViewModel();
        }              
        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            (DataContext as ControlViewModel)?.UpdateDrawingBounds(e);
        }

        private void Marker_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((UIElement) sender);
            isMarkerBeingDragged = true;
            lastMousePosition = e.GetPosition(Canvas);
        }

        private void Marker_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement) sender).ReleaseMouseCapture();
            isMarkerBeingDragged = false;
        }
                
        private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
        {            
            if (!isMarkerBeingDragged)
                return;

            // If mouse release event was skipped because window focus was taken by something else
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                isMarkerBeingDragged = false;
                return;
            }

            var delta = e.GetPosition(Canvas) - lastMousePosition;                        
            (DataContext as ControlViewModel)?.NotifyCanvasDrag(delta);

            lastMousePosition = e.GetPosition(Canvas);
        }
    }
}
