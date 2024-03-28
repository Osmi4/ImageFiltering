using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MedianCut;

namespace ImageFiltering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() ?? false)
            {
                string selectedFileName = dlg.FileName;
                ImageNameLabel.Content = System.IO.Path.GetFileName(selectedFileName);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                OriginalImageViewer.Source = bitmap;
                ModifiedImageViewer.Source = bitmap;
            }
        }

        private void GammaButton_Click(object sender, RoutedEventArgs e)
        {
            double gammaValue = 2.2; 

            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                FormatConvertedBitmap formattedBitmapSource = new FormatConvertedBitmap();
                formattedBitmapSource.BeginInit();
                formattedBitmapSource.Source = bitmapSource;
                formattedBitmapSource.DestinationFormat = bitmapSource.Format;
                formattedBitmapSource.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
                formattedBitmapSource.EndInit();

                byte[] pixels = new byte[formattedBitmapSource.PixelWidth * formattedBitmapSource.PixelHeight * 4];
                formattedBitmapSource.CopyPixels(pixels, formattedBitmapSource.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    pixels[i + 2] = (byte)Math.Min(255, Math.Pow(pixels[i + 2] / 255.0, 1 / gammaValue) * 255);
                    pixels[i + 1] = (byte)Math.Min(255, Math.Pow(pixels[i + 1] / 255.0, 1 / gammaValue) * 255);
                    pixels[i] = (byte)Math.Min(255, Math.Pow(pixels[i] / 255.0, 1 / gammaValue) * 255);
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight), pixels, formattedBitmapSource.PixelWidth * 4, 0);

                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void BrightenButton_Click(object sender, RoutedEventArgs e)
        {
            double brightnessFactor = 20; 

            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapSource);

                byte[] pixels = new byte[writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4];
                writeableBitmap.CopyPixels(pixels, writeableBitmap.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    pixels[i + 2] = (byte)Math.Min(255, Math.Max(0, pixels[i + 2] + brightnessFactor));
                    pixels[i + 1] = (byte)Math.Min(255, Math.Max(0, pixels[i + 1] + brightnessFactor));
                    pixels[i] = (byte)Math.Min(255, Math.Max(0, pixels[i] + brightnessFactor));

                }

                writeableBitmap.WritePixels(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight), pixels, writeableBitmap.PixelWidth * 4, 0);

                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void InvertButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                FormatConvertedBitmap formattedBitmapSource = new FormatConvertedBitmap();
                formattedBitmapSource.BeginInit();
                formattedBitmapSource.Source = bitmapSource;
                formattedBitmapSource.DestinationFormat = bitmapSource.Format;
                formattedBitmapSource.DestinationFormat = PixelFormats.Bgra32;
                formattedBitmapSource.EndInit();

                byte[] pixels = new byte[formattedBitmapSource.PixelWidth * formattedBitmapSource.PixelHeight * 4];
                formattedBitmapSource.CopyPixels(pixels, formattedBitmapSource.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    pixels[i + 2] = (byte)(255 - pixels[i + 2]);
                    pixels[i + 1] = (byte)(255 - pixels[i + 1]); 
                    pixels[i] = (byte)(255 - pixels[i]);         
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight), pixels, formattedBitmapSource.PixelWidth * 4, 0);

                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void ContrastButton_Click(object sender, RoutedEventArgs e)
        {
            double contrastFactor = 1.5;

            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                FormatConvertedBitmap formattedBitmapSource = new FormatConvertedBitmap();
                formattedBitmapSource.BeginInit();
                formattedBitmapSource.Source = bitmapSource;
                formattedBitmapSource.DestinationFormat = bitmapSource.Format;
                formattedBitmapSource.DestinationFormat = PixelFormats.Bgra32;
                formattedBitmapSource.EndInit();

                byte[] pixels = new byte[formattedBitmapSource.PixelWidth * formattedBitmapSource.PixelHeight * 4];
                formattedBitmapSource.CopyPixels(pixels, formattedBitmapSource.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    pixels[i + 2] = (byte)Math.Min(255, Math.Max(0, (pixels[i + 2] - 128) * contrastFactor + 128));
                    pixels[i + 1] = (byte)Math.Min(255, Math.Max(0, (pixels[i + 1] - 128) * contrastFactor + 128));
                    pixels[i] = (byte)Math.Min(255, Math.Max(0, (pixels[i] - 128) * contrastFactor + 128));

                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight), pixels, formattedBitmapSource.PixelWidth * 4, 0);

                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if(OriginalImageViewer != null)
                ModifiedImageViewer.Source = OriginalImageViewer.Source;
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
    
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {

                    string filename = saveFileDialog.FileName;

 
                    BitmapEncoder encoder;
                    switch (System.IO.Path.GetExtension(filename).ToLower())
                    {
                        case ".png":
                            encoder = new PngBitmapEncoder();
                            break;
                        case ".jpeg":
                        case ".jpg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        default:
                            MessageBox.Show("Unsupported file format.");
                            return;
                    }
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (FileStream stream = new FileStream(filename, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    MessageBox.Show("Image saved successfully.");
                }
            }
            else
            {
                MessageBox.Show("No image to save.");
            }
        }
            
        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {   
            Button button = sender as Button;
            if (button != null)
            {
                string filterName = button.Content.ToString();

                BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

                if (bitmapSource != null)
                {

                    BitmapSource filteredBitmap = ApplyFilter(bitmapSource, filterName);

                    ModifiedImageViewer.Source = filteredBitmap;
                }
            }
        }

        private BitmapSource ApplyFilter(BitmapSource source, string filterName)
        {
            double[,] kernel;
            switch (filterName)
            {
                case "Blur":
                    kernel = new double[,]
                    {
                { 1.0 / 9, 1.0 / 9, 1.0 / 9 },
                { 1.0 / 9, 1.0 / 9, 1.0 / 9 },
                { 1.0 / 9, 1.0 / 9, 1.0 / 9 }
                    };
                    break;
                case "Gaussian Blur":
                    kernel = new double[,]
                    {
                { 1.0 / 16, 2.0 / 16, 1.0 / 16 },
                { 2.0 / 16, 4.0 / 16, 2.0 / 16 },
                { 1.0 / 16, 2.0 / 16, 1.0 / 16 }
                    };
                    break;
                case "Sharpen":
                    kernel = new double[,]
                    {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
                    };
                    break;
                case "Edge Detect":
                    kernel = new double[,]
                    {
                { -1, -1, -1 },
                { -1, 8, -1 },
                { -1, -1, -1 }
                    };
                    break;
                case "Emboss":
                    kernel = new double[,]
                    {
                { -1, -1, 0 },
                { -1, 1, 1 },
                { 0, 1, 1 }
                    };
                    break;
                default:
                    return null;
            }

            ConvolutionFilter filter = new ConvolutionFilter(kernel);
            return filter.Apply(source);
        }

        public class ConvolutionFilter
        {
            private double[,] kernel;

            public ConvolutionFilter(double[,] kernel)
            {
                this.kernel = kernel;
            }

            public BitmapSource Apply(BitmapSource source)
            {
                int width = source.PixelWidth;
                int height = source.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                source.CopyPixels(pixels, stride, 0);

                byte[] resultPixels = new byte[height * stride];

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        double[] color = { 0, 0, 0 };

                        for (int j = 0; j < 3; j++)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                int index = ((y - 1 + j) * width + (x - 1 + i)) * 4;
                                color[0] += pixels[index] * kernel[j, i];
                                color[1] += pixels[index + 1] * kernel[j, i];
                                color[2] += pixels[index + 2] * kernel[j, i];
                            }
                        }

                        int resultIndex = (y * width + x) * 4;
                        resultPixels[resultIndex] = (byte)Math.Min(255, Math.Max(0, color[0]));
                        resultPixels[resultIndex + 1] = (byte)Math.Min(255, Math.Max(0, color[1]));
                        resultPixels[resultIndex + 2] = (byte)Math.Min(255, Math.Max(0, color[2]));
                        resultPixels[resultIndex + 3] = 255; 
                    }
                }

                WriteableBitmap resultBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                resultBitmap.WritePixels(new Int32Rect(0, 0, width, height), resultPixels, stride, 0);
                return resultBitmap;
            }
        }

        private void HSVButton_Click(object sender, RoutedEventArgs e)
        {

            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                BitmapSource hsvBitmap = ConvertRGBtoHSV(bitmapSource);

                ModifiedImageViewer.Source = hsvBitmap;
            }
        }

        private void RGBButton_Click(object sender, RoutedEventArgs e)
        {

            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                BitmapSource rgbBitmap = ConvertHSVtoRGB(bitmapSource);

                ModifiedImageViewer.Source = rgbBitmap;
            }
        }

        private BitmapSource ConvertRGBtoHSV(BitmapSource rgbBitmap)
        {
            int width = rgbBitmap.PixelWidth;
            int height = rgbBitmap.PixelHeight;

            byte[] pixels = new byte[width * height * 4];
            rgbBitmap.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                float r = pixels[i] / 255.0f;
                float g = pixels[i + 1] / 255.0f;
                float b = pixels[i + 2] / 255.0f;

                float cmax = Math.Max(r, Math.Max(g, b));
                float cmin = Math.Min(r, Math.Min(g, b));
                float delta = cmax - cmin;

                float hue = 0;
                if (delta != 0)
                {
                    if (cmax == r)
                        hue = 60 * (((g - b) / delta) % 6);
                    else if (cmax == g)
                        hue = 60 * (((b - r) / delta) + 2);
                    else if (cmax == b)
                        hue = 60 * (((r - g) / delta) + 4);
                }

                float saturation = (cmax == 0) ? 0 : (delta / cmax);
                float value = cmax;

                pixels[i] = (byte)(hue / 360 * 255);
                pixels[i + 1] = (byte)(saturation * 255);
                pixels[i + 2] = (byte)(value * 255);
            }

            BitmapSource hsvBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, width * 4);
            return hsvBitmap;
        }

        private BitmapSource ConvertHSVtoRGB(BitmapSource hsvBitmap)
        {
            int width = hsvBitmap.PixelWidth;
            int height = hsvBitmap.PixelHeight;

            byte[] pixels = new byte[width * height * 4];
            hsvBitmap.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                float h = pixels[i] / 255.0f * 360; // Convert back to degrees
                float s = pixels[i + 1] / 255.0f;
                float v = pixels[i + 2] / 255.0f;

                float c = v * s;
                float x = c * (1 - Math.Abs((h / 60) % 2 - 1));
                float m = v - c;

                float r = 0, g = 0, b = 0;

                if (h < 60)
                {
                    r = c;
                    g = x;
                }
                else if (h < 120)
                {
                    r = x;
                    g = c;
                }
                else if (h < 180)
                {
                    g = c;
                    b = x;
                }
                else if (h < 240)
                {
                    g = x;
                    b = c;
                }
                else if (h < 300)
                {
                    r = x;
                    b = c;
                }
                else if (h < 360)
                {
                    r = c;
                    b = x;
                }

                pixels[i] = (byte)((r + m) * 255);
                pixels[i + 1] = (byte)((g + m) * 255);
                pixels[i + 2] = (byte)((b + m) * 255);
            }

            BitmapSource rgbBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, width * 4);
            return rgbBitmap;
        }

        private void GrayscaleButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmapSource = OriginalImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                FormatConvertedBitmap grayscaleBitmap = (FormatConvertedBitmap)ConvertToGrayscale(bitmapSource);

                ModifiedImageViewer.Source = grayscaleBitmap;
            }
        }

        private void RandomDitheringButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmapSource = OriginalImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                BitmapSource ditheredBitmap = ApplyRandomDithering(bitmapSource);

                ModifiedImageViewer.Source = ditheredBitmap;
            }

        }

        private BitmapSource ApplyRandomDithering(BitmapSource sourceBitmap)
        {
            int width = sourceBitmap.PixelWidth;
            int height = sourceBitmap.PixelHeight;

            byte[] pixelValues = new byte[width * height * 4];

            sourceBitmap.CopyPixels(pixelValues, width * 4, 0);

            Random random = new Random();

            bool isGrayscale = IsGrayscale(sourceBitmap);

            for (int i = 0; i < pixelValues.Length; i += 4)
            {
                byte r = pixelValues[i];  
                byte g = pixelValues[i + 1]; 
                byte b = pixelValues[i + 2];

                byte grayscaleValue;

                if (isGrayscale)
                {
                    grayscaleValue = r;
                }
                else
                {
                    grayscaleValue = (byte)((0.299 * r) + (0.587 * g) + (0.114 * b));
                }

                byte threshold = (byte)random.Next(256);

                byte ditheredValue = (grayscaleValue > threshold) ? (byte)255 : (byte)0;

                if (isGrayscale)
                {
                    pixelValues[i] = ditheredValue;
                    pixelValues[i + 1] = ditheredValue;
                    pixelValues[i + 2] = ditheredValue;
                    pixelValues[i + 3] = ditheredValue;
                }
                else
                {
                    pixelValues[i] = ditheredValue;   
                    pixelValues[i + 1] = ditheredValue;
                    pixelValues[i + 2] = ditheredValue; 
                }
            }

            WriteableBitmap ditheredBitmap = new WriteableBitmap(width, height, sourceBitmap.DpiX, sourceBitmap.DpiY, sourceBitmap.Format, sourceBitmap.Palette);
            ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelValues, width * 4, 0);

            return ditheredBitmap;
        }

        private bool IsGrayscale(BitmapSource bitmap)
        {
            bool isGrayscale = bitmap.Format == PixelFormats.Gray8 || bitmap.Format == PixelFormats.Gray16;
            return isGrayscale;
        }

        private BitmapSource ConvertToGrayscale(BitmapSource originalBitmap)
        {
            FormatConvertedBitmap grayscaleBitmap = new FormatConvertedBitmap(originalBitmap, PixelFormats.Gray8, null, 0);

            return grayscaleBitmap;
        }

        private void MedianCutButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource originalBitmapSource = OriginalImageViewer.Source as BitmapSource;

            if (originalBitmapSource != null)
            {
                BitmapSource modifiedBitmapSource = ApplyMedianCut(originalBitmapSource);

                ModifiedImageViewer.Source = modifiedBitmapSource;
            }
        }

        private BitmapSource ApplyMedianCut(BitmapSource bitmapSource)
        {
            int nbColors = (int)MedianCutSlider.Value;

            mc medianCut = new mc(bitmapSource,nbColors);

            medianCut.create_main_list();

            BitmapSource modifiedBitmapSource = medianCut.median_cut();

            return modifiedBitmapSource;
        }

        private void PixelizeButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource originalBitmapSource = OriginalImageViewer.Source as BitmapSource;

            int pixelsPerColor = (int)PixelizeSlider.Value;
            if (originalBitmapSource != null)
            {
                ImageSource modifiedBitmapSource = PixelizeImage(originalBitmapSource,pixelsPerColor );

                ModifiedImageViewer.Source = modifiedBitmapSource;
            }
        }

        private ImageSource PixelizeImage(BitmapSource originalBitmap, int pixelSize)
        {
            int stride = originalBitmap.PixelWidth * (originalBitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[originalBitmap.PixelHeight * stride];
            originalBitmap.CopyPixels(pixels, stride, 0);

            int width = originalBitmap.PixelWidth;
            int height = originalBitmap.PixelHeight;

            WriteableBitmap writableBitmap = new WriteableBitmap(originalBitmap);

            for (int y = 0; y < height; y += pixelSize)
            {
                for (int x = 0; x < width; x += pixelSize)
                {
                    int totalR = 0, totalG = 0, totalB = 0;

                    for (int offsetY = 0; offsetY < pixelSize && y + offsetY < height; offsetY++)
                    {
                        for (int offsetX = 0; offsetX < pixelSize && x + offsetX < width; offsetX++)
                        {
                            int pixelIndex = (y + offsetY) * stride + (x + offsetX) * 4;
                            totalB += pixels[pixelIndex];
                            totalG += pixels[pixelIndex + 1];
                            totalR += pixels[pixelIndex + 2];
                        }
                    }

                    int chunkPixels = Math.Min(pixelSize * pixelSize, (height - y) * (width - x));

                    byte averageR = (byte)(totalR / chunkPixels);
                    byte averageG = (byte)(totalG / chunkPixels);
                    byte averageB = (byte)(totalB / chunkPixels);

                    for (int offsetY = 0; offsetY < pixelSize && y + offsetY < height; offsetY++)
                    {
                        for (int offsetX = 0; offsetX < pixelSize && x + offsetX < width; offsetX++)
                        {
                            int pixelIndex = (y + offsetY) * stride + (x + offsetX) * 4;
                            pixels[pixelIndex] = averageB;
                            pixels[pixelIndex + 1] = averageG;
                            pixels[pixelIndex + 2] = averageR;
                        }
                    }
                }
            }

            writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return writableBitmap;
        }
    }
}
