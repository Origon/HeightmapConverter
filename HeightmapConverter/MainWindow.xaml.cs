using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Media;

namespace HeightmapConverter
{
    public partial class MainWindow : Window
    {
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
                var converter = converters[ImageMode];
                TerrainDisplay = converter.ToBmp(file);
            }
            else if (ActiveSource == TerrainSource.Image)
            {
                var decoder = new PngBitmapDecoder(new Uri(file), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
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

                TerrainDisplay = frame;
            }

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
                var converter = converters[ImageMode];
                var data = converter.ToRaw(TerrainDisplay!);

                try
                {
                    File.WriteAllBytes(file, data);
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

        private void UpdateStatus(string status) => StatusText.Text = status;
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
}