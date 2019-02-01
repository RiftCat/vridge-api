using System.Windows;
using System.Windows.Controls;

namespace VRE.Vridge.API.DesktopTester.View
{
    /// <summary>
    /// Interaction logic for LabeledSlider.xaml
    /// </summary>
    public partial class LabeledSlider : UserControl
    {

        public static readonly DependencyProperty SliderValueProperty = DependencyProperty.Register("SliderValue", typeof(double), typeof(LabeledSlider), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(LabeledSlider), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(LabeledSlider), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(LabeledSlider), new PropertyMetadata(default(double)));

        public string Label
        {
            get { return (string) GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public double SliderValue
        {
            get { return (double)GetValue(SliderValueProperty); }
            set { SetValue(SliderValueProperty, value); }
        }

        public double Minimum
        {
            get { return (double) GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double Maximum
        {
            get { return (double) GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public LabeledSlider()
        {
            InitializeComponent();
        }
    }
}
