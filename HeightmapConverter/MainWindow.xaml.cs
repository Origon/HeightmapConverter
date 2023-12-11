using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Media;
using HeightmapConverter.Editing;
using System.Linq;
using IntPoint = System.Drawing.Point;
using System.Windows.Controls;

namespace HeightmapConverter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            EditSelectorOptions = new Dictionary<string, Type>()
                                  {
                                      { "Single Level", typeof(SingleLevelSelector) },
                                      { "Level Range", typeof(RangeSelector) }
                                  };

            ActiveEditorSelectorType = EditSelectorOptions.Values.First();

            EditEffectOptions = new Dictionary<string, SinglePixelEffect>()
            {
                { "Offset Level", new OffsetLevelEffect() },
                { "Scale Level", new ScaleLevelEffect() }
            };
        }

        public string RawFilePath
        {
            get { return (string)GetValue(RawFilePathProperty); }
            set { SetValue(RawFilePathProperty, value); }
        }
        public static readonly DependencyProperty RawFilePathProperty =
            DependencyProperty.Register("RawFilePath", typeof(string), typeof(MainWindow));

        public string ImageFilePath
        {
            get { return (string)GetValue(ImageFilePathProperty); }
            set { SetValue(ImageFilePathProperty, value); }
        }
        public static readonly DependencyProperty ImageFilePathProperty =
            DependencyProperty.Register("ImageFilePath", typeof(string), typeof(MainWindow));

        public TerrainSource ActiveSource
        {
            get { return (TerrainSource)GetValue(ActiveSourceProperty); }
            set { SetValue(ActiveSourceProperty, value); }
        }
        public static readonly DependencyProperty ActiveSourceProperty =
            DependencyProperty.Register("ActiveSource", typeof(TerrainSource), typeof(MainWindow), new PropertyMetadata(TerrainSource.None));

        private DependencyProperty? getFilePropertyBySourceType(TerrainSource source)
        {
            if (source == TerrainSource.Raw) return RawFilePathProperty;
            else if (source == TerrainSource.Image) return ImageFilePathProperty;
            return null;
        }

        public DependencyProperty? ActiveSourceFilePathProperty => getFilePropertyBySourceType(ActiveSource);

        public string? ActiveSourceFilePath
        {
            get
            {
                var prop = ActiveSourceFilePathProperty;
                if (prop != null) return (string)GetValue(prop);
                return null;
            }
            set
            {
                var prop = ActiveSourceFilePathProperty;
                if (prop == null) throw new InvalidOperationException("No source is currently active");
                SetValue(prop, value);
            }
        }

        public BitmapSource? TerrainDisplay
        {
            get { return (BitmapSource)GetValue(TerrainDisplayProperty); }
            set { SetValue(TerrainDisplayProperty, value); }
        }
        public static readonly DependencyProperty TerrainDisplayProperty =
            DependencyProperty.Register("TerrainDisplay", typeof(BitmapSource), typeof(MainWindow), new PropertyMetadata(null));

        private byte[]? terrainDisplayRawBytes;

        public TerrainImageMode ImageMode
        {
            get { return (TerrainImageMode)GetValue(ImageModeProperty); }
            set { SetValue(ImageModeProperty, value); }
        }
        public static readonly DependencyProperty ImageModeProperty =
            DependencyProperty.Register("ImageMode", typeof(TerrainImageMode), typeof(MainWindow),
                new PropertyMetadata(TerrainImageMode.Gray16, new PropertyChangedCallback(imageModeChanged)));

        private static void imageModeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (MainWindow)sender;
            window.changeImageMode((TerrainImageMode)e.OldValue, (TerrainImageMode)e.NewValue);
        }

        private void changeImageMode(TerrainImageMode oldMode, TerrainImageMode newMode)
        {
            if (TerrainDisplay != null)
            {
                var oldConverter = converters[oldMode];
                var newConverter = converters[newMode];
                var raw = oldConverter.ToRaw(TerrainDisplay);
                TerrainDisplay = newConverter.ToBmp(ref raw);
            }
        }

        private Dictionary<TerrainImageMode, ConverterBase> converters = new Dictionary<TerrainImageMode, ConverterBase>()
        {
            { TerrainImageMode.Gray16, new Gray16Converter() },
            { TerrainImageMode.RedGreen8, new RedGreenConverter() }
        };

        public bool EditTabActive
        {
            get { return (bool)GetValue(EditTabActiveProperty); }
            set { SetValue(EditTabActiveProperty, value); }
        }
        public static readonly DependencyProperty EditTabActiveProperty =
            DependencyProperty.Register("EditTabActive", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false, new PropertyChangedCallback(maskVisibilityPropertyChanged)));

        public bool ShowSelectorMask
        {
            get { return (bool)GetValue(ShowSelectorMaskProperty); }
            set { SetValue(ShowSelectorMaskProperty, value); }
        }
        public static readonly DependencyProperty ShowSelectorMaskProperty =
            DependencyProperty.Register("ShowSelectorMask", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(true, new PropertyChangedCallback(maskVisibilityPropertyChanged)));

        private static void maskVisibilityPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (MainWindow)sender;
            window.maskVisibilityPropertyChanged();
        }

        private bool shouldEditMaskBeVisible => ShowSelectorMask & EditTabActive;

        private void maskVisibilityPropertyChanged()
        {
            if (shouldEditMaskBeVisible)
            {
                if (terrainEditMaskOutOfDate) updateEditMask();
                TerrainEditMask = terrainMaskBitmap;
            }
            else TerrainEditMask = null;
        }

        public BitmapSource? TerrainEditMask
        {
            get { return (BitmapSource?)GetValue(TerrainEditMaskProperty); }
            set { SetValue(TerrainEditMaskProperty, value); }
        }
        public static readonly DependencyProperty TerrainEditMaskProperty =
            DependencyProperty.Register("TerrainEditMask", typeof(BitmapSource), typeof(MainWindow));

        private WriteableBitmap? terrainMaskBitmap;
        private byte[]? terrainMaskBytes;
        private bool terrainEditMaskOutOfDate;

        private void createTerrainMask()
        {
            terrainMaskBitmap = new WriteableBitmap(TerrainDisplay!.PixelWidth, TerrainDisplay.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
            terrainMaskBytes = new byte[terrainMaskBitmap.BackBufferStride * terrainMaskBitmap.PixelHeight];
            terrainEditMaskOutOfDate = true;
        }

        public IReadOnlyDictionary<string, Type> EditSelectorOptions
        {
            get { return (IReadOnlyDictionary<string, Type>)GetValue(EditSelectorOptionsProperty); }
            private set { SetValue(EditSelectorOptionsPropertyKey, value); }
        }
        public static readonly DependencyPropertyKey EditSelectorOptionsPropertyKey =
            DependencyProperty.RegisterReadOnly("EditSelectorOptions", typeof(IReadOnlyDictionary<string, Type>), typeof(MainWindow), new PropertyMetadata(default));
        public static readonly DependencyProperty EditSelectorOptionsProperty =
            EditSelectorOptionsPropertyKey.DependencyProperty;

        public Type ActiveEditorSelectorType
        {
            get { return (Type)GetValue(ActiveEditorSelectorTypeProperty); }
            set { SetValue(ActiveEditorSelectorTypeProperty, value); }
        }
        public static readonly DependencyProperty ActiveEditorSelectorTypeProperty =
            DependencyProperty.Register("ActiveEditorSelectorType", typeof(Type), typeof(MainWindow),
                new PropertyMetadata(default, new PropertyChangedCallback(activeEditorSelectorTypeChanged)));

        private static void activeEditorSelectorTypeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (MainWindow)sender;
            if (window.ActiveEditSelector != null) window.ActiveEditSelector.ScopeChanged -= window.ActiveEditSelector_ScopeChanged;
            window.ActiveEditSelector = (Selector)Activator.CreateInstance((Type)e.NewValue)!;
            window.ActiveEditSelector.ScopeChanged += window.ActiveEditSelector_ScopeChanged;
            window.ActiveEditSelector_ScopeChanged(null, null);
        }

        public Selector? ActiveEditSelector
        {
            get { return (Selector)GetValue(ActiveEditSelectorProperty); }
            private set { SetValue(ActiveEditSelectorPropertyKey, value); }
        }
        public static readonly DependencyPropertyKey ActiveEditSelectorPropertyKey =
            DependencyProperty.RegisterReadOnly("ActiveEditSelector", typeof(Selector), typeof(MainWindow), new PropertyMetadata(default));
        public static readonly DependencyProperty ActiveEditSelectorProperty =
            ActiveEditSelectorPropertyKey.DependencyProperty;

        private void ActiveEditSelector_ScopeChanged(object? sender, SelectorScopeChanged? e)
        {
            terrainEditMaskOutOfDate = true;
            if (shouldEditMaskBeVisible) updateEditMask();
        }

        public Dictionary<string, SinglePixelEffect> EditEffectOptions
        {
            get { return (Dictionary<string, SinglePixelEffect>)GetValue(EditEffectOptionsProperty); }
            private set { SetValue(EditEffectOptionsPropertyKey, value); }
        }
        public static readonly DependencyPropertyKey EditEffectOptionsPropertyKey =
            DependencyProperty.RegisterReadOnly("EditEffectOptions", typeof(Dictionary<string, SinglePixelEffect>), typeof(MainWindow), new PropertyMetadata(default));
        public static readonly DependencyProperty EditEffectOptionsProperty =
            EditEffectOptionsPropertyKey.DependencyProperty;

        public SinglePixelEffect? ActiveEditEffect
        {
            get { return (SinglePixelEffect)GetValue(ActiveEditEffectProperty); }
            set { SetValue(ActiveEditEffectProperty, value); }
        }
        public static readonly DependencyProperty ActiveEditEffectProperty =
            DependencyProperty.Register("ActiveEditEffect", typeof(SinglePixelEffect), typeof(MainWindow), new PropertyMetadata(default));

        private void CreateFileDialog(FileDialog dialog, TerrainSource sourceType, string verb)
        {
            if (sourceType == TerrainSource.Raw)
            {
                dialog.Title = verb + " Unity terrain file";
                dialog.Filter = "Unity terrain (*.raw)|*.raw";
            }
            else if (sourceType == TerrainSource.Image)
            {
                dialog.Title = verb + " image file";
                dialog.Filter = "Portable network graphics file (*.png)|*.png";
            }
            else throw new InvalidEnumArgumentException(nameof(sourceType), (int)sourceType, sourceType.GetType());
        }

        private void Browse_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Parameter is TerrainSource source && source != TerrainSource.None)
            {
                OpenFileDialog d = new();
                CreateFileDialog(d, source, "Open");
                if (!d.ShowDialog()!.Value) return;
                
                if (source == TerrainSource.Raw)
                    RawFilePath = d.FileName;
                else if (source == TerrainSource.Image)
                    ImageFilePath = d.FileName;

                TerrainDisplay = null;
                ActiveSource = source;
                Reload();

                e.Handled = true;
            }
        }

        private void Reload_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((e.Parameter is TerrainSource source && source != TerrainSource.None) || ActiveSource != TerrainSource.None);
            e.Handled = true;
        }

        private void Reload_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is TerrainSource source) ActiveSource = source;
            TerrainDisplay = null;
            Reload();
            e.Handled = true;
        }

        public void Reload()
        {
            var file = ActiveSourceFilePath;
            if (file == null) return;

            if (!File.Exists(file))
            {
                MessageBox.Show($"The file '{file}' could not be found.", "File not found");
                return;
            }            

            if (ActiveSource == TerrainSource.Raw)
            {
                terrainDisplayRawBytes = File.ReadAllBytes(file);
                var converter = converters[ImageMode];
                TerrainDisplay = converter.ToBmp(ref terrainDisplayRawBytes);
            }
            else if (ActiveSource == TerrainSource.Image)
            {
                var decoder = new PngBitmapDecoder(new Uri(file), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];

                TerrainDisplay = null;

                if (frame.Format == PixelFormats.Gray16)
                    ImageMode = TerrainImageMode.Gray16;
                else if (frame.Format == PixelFormats.Bgr32 || frame.Format == PixelFormats.Bgra32)
                    ImageMode = TerrainImageMode.RedGreen8;
                else
                {
                    MessageBox.Show($"The image file given was not in a supported format. The format read was '{frame.Format}'. Supported formats are '{PixelFormats.Gray16}', '{PixelFormats.Bgr32}' and '{PixelFormats.Bgra32}'");
                    ActiveSource = TerrainSource.None;
                    return;
                }

                var converter = converters[ImageMode];
                terrainDisplayRawBytes = converter.ToRaw(frame);
                TerrainDisplay = frame;
            }

            createTerrainMask();

            UpdateStatus($"Loaded display from file '{file}'");
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TerrainDisplay != null && e.Parameter is TerrainSource source && source != TerrainSource.None;
            e.Handled = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            var target = (TerrainSource)e.Parameter;
            var file = (string)GetValue(getFilePropertyBySourceType(target));

            if (String.IsNullOrEmpty(file))
            {
                SaveFileDialog d = new();
                CreateFileDialog(d, target, "Save");
                if (!d.ShowDialog()!.Value) return;

                file = d.FileName;
                var outputPathProp = getFilePropertyBySourceType(target);
                SetValue(outputPathProp, file);
            }

            if (target == TerrainSource.Raw)
            {
                try
                {
                    File.WriteAllBytes(file, terrainDisplayRawBytes!);
                }
                catch (Exception ex)
                {
                    showSaveFailedMessage(ex);
                    return;
                }
            }
            else if (target == TerrainSource.Image)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(TerrainDisplay));

                try
                {
                    using (var stream = new FileStream(file, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }
                catch (Exception ex)
                {
                    showSaveFailedMessage(ex);
                    return;
                }
            }
            else return;

            UpdateStatus($"Saved displayed terrain to file '{file}'");
        }

        private void showSaveFailedMessage(Exception ex)
        {
            MessageBox.Show("Unable to save file: " + ex.Message, "Save Failed");
        }

        private long getTerrainPixelNumber(MouseEventArgs e)
        {
            if (TerrainDisplay == null) throw new NullReferenceException("No terrain on display.");
            if (e.Source is not Image image) throw new ArgumentException("Mouse source must be Image.");

            var pos = e.GetPosition(image);
            var scale = TerrainDisplay.PixelWidth / image.ActualWidth;
            var x = (int)Math.Floor(pos.X * scale);
            scale = TerrainDisplay.PixelHeight / image.ActualHeight;
            var y = (int)Math.Floor(pos.Y * scale);

            return (long)y * TerrainDisplay.PixelWidth + x;
        }

        private ushort getLevelOfPixel(long pixelNumber) => BitConverter.ToUInt16(terrainDisplayRawBytes!, (int)pixelNumber * 2);

        private void TerrainImage_MouseMove(object sender, MouseEventArgs e)
        {
            var level = getLevelOfPixel(getTerrainPixelNumber(e));
            UpdateMousePos($"Level: {level} ({(double)level / ushort.MaxValue:P})");
        }

        private void TerrainImage_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateMousePos(string.Empty);
        }

        private void TerrainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var num = getTerrainPixelNumber(e);
            TerrainSampled?.Invoke(this, new TerrainSampleEventArgs() { PixelNumber = num, Level = getLevelOfPixel(num) });
        }

        public event EventHandler<TerrainSampleEventArgs>? TerrainSampled;

        private void updateEditMask()
        {
            ActiveEditSelector!.ApplyMask(ref terrainDisplayRawBytes!, ref terrainMaskBytes!);
            terrainMaskBitmap!.WritePixels(new Int32Rect(0, 0, terrainMaskBitmap.PixelWidth, terrainMaskBitmap.PixelHeight), terrainMaskBytes, terrainMaskBitmap.BackBufferStride, 0);
            terrainEditMaskOutOfDate = false;
        }

        private void B_ApplyEffect_Click(object sender, RoutedEventArgs e)
        {
            ShowSelectorMask = false;
            terrainEditMaskOutOfDate = true;
            ActiveEditSelector!.ApplyEffect(ref terrainDisplayRawBytes!, ActiveEditEffect!);
            var converter = converters[ImageMode];
            TerrainDisplay = converter.ToBmp(ref terrainDisplayRawBytes);
            UpdateStatus($"Applied effect");
        }

        private void UpdateStatus(string status) => StatusText.Text = status;
        private void UpdateMousePos(string data) => MousePosText.Text = data;

    }

    public enum TerrainImageMode
    {
        Gray16,
        RedGreen8,
    }

    public enum TerrainSource
    {
        None,
        Raw,
        Image
    }

    public class TerrainSampleEventArgs
    {
        public long PixelNumber { get; init; }
        public ushort Level { get; init; }
    }
}