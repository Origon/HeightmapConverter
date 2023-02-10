using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeightmapConverter.Editing
{

    public partial class SampleBox : UserControl
    {
        public SampleBox()
        {
            InitializeComponent();
            this.Loaded += SampleBox_Loaded;
        }

        private void SampleBox_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyObject? parent = this;

            while (parent is not MainWindow)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent == null) throw new InvalidOperationException("SampleBox must be added to MainWindow");
            }

            window = (MainWindow)parent;
        }

        private MainWindow? window;

        public object? Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SampleBox),
                new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public TerrainSampleMode Mode
        {
            get { return (TerrainSampleMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(TerrainSampleMode), typeof(SampleBox), new PropertyMetadata(TerrainSampleMode.Level));

        public bool IsAwaitingSample
        {
            get { return (bool)GetValue(IsAwaitingSampleProperty); }
            private set { SetValue(IsAwaitingSamplePropertyKey, value); }
        }
        public static readonly DependencyPropertyKey IsAwaitingSamplePropertyKey =
            DependencyProperty.RegisterReadOnly("IsAwaitingSample", typeof(bool), typeof(SampleBox), new PropertyMetadata(false));
        public static readonly DependencyProperty IsAwaitingSampleProperty =
            IsAwaitingSamplePropertyKey.DependencyProperty;

        private void B_Sample_Click(object sender, RoutedEventArgs e)
        {
            toggle();
        }

        private void toggle()
        {
            if (IsAwaitingSample)
            {
                window!.TerrainSampled -= TerrainSampled;
                IsAwaitingSample = false;
            }
            else
            {
                window!.TerrainSampled += TerrainSampled;
                IsAwaitingSample = true;
            }
        }

        private void TerrainSampled(object? sender, TerrainSampleEventArgs e)
        {
            if (Mode == TerrainSampleMode.Level)
            {
                Value = e.Level;
            }

            toggle();
        }
    }

    public enum TerrainSampleMode
    {
        Level
    }
}