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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                ImageNameLabel.Content = selectedFileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);
                bitmap.EndInit();
                OriginalImageViewer.Source = bitmap;
                ModifiedImageViewer.Source = bitmap;
            }
        }

        private void GammaButton_Click(object sender, RoutedEventArgs e)
        {
            // Define your gamma value here
            double gammaValue = 2.2; // You can change this value as needed

            // Access the image displayed in the ModifiedImageViewer
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                // Apply gamma correction to the image
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

                // Display the modified image in the ModifiedImageViewer
                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void BrightenButton_Click(object sender, RoutedEventArgs e)
        {
            // Define your brightness adjustment factor here
            double brightnessFactor = 50; // You can change this value as needed

            // Access the image displayed in the ModifiedImageViewer
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                // Apply brightness adjustment to the image
                FormatConvertedBitmap formattedBitmapSource = new FormatConvertedBitmap();
                formattedBitmapSource.BeginInit();
                formattedBitmapSource.Source = bitmapSource;
                formattedBitmapSource.DestinationFormat = bitmapSource.Format;
                formattedBitmapSource.DestinationFormat = PixelFormats.Bgra32; // Change to the appropriate pixel format if needed
                formattedBitmapSource.EndInit();

                byte[] pixels = new byte[formattedBitmapSource.PixelWidth * formattedBitmapSource.PixelHeight * 4];
                formattedBitmapSource.CopyPixels(pixels, formattedBitmapSource.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    // Adjust brightness for each color channel
                    pixels[i + 2] = Clamp((byte)(pixels[i + 2] + brightnessFactor)); // Red channel
                    pixels[i + 1] = Clamp((byte)(pixels[i + 1] + brightnessFactor)); // Green channel
                    pixels[i] = Clamp((byte)(pixels[i] + brightnessFactor));         // Blue channel
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight), pixels, formattedBitmapSource.PixelWidth * 4, 0);

                // Display the adjusted image in the ModifiedImageViewer
                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void InvertButton_Click(object sender, RoutedEventArgs e)
        {
            // Access the image displayed in the ModifiedImageViewer
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                // Invert the colors of the image
                FormatConvertedBitmap formattedBitmapSource = new FormatConvertedBitmap();
                formattedBitmapSource.BeginInit();
                formattedBitmapSource.Source = bitmapSource;
                formattedBitmapSource.DestinationFormat = bitmapSource.Format;
                formattedBitmapSource.DestinationFormat = PixelFormats.Bgra32; // Change to the appropriate pixel format if needed
                formattedBitmapSource.EndInit();

                byte[] pixels = new byte[formattedBitmapSource.PixelWidth * formattedBitmapSource.PixelHeight * 4];
                formattedBitmapSource.CopyPixels(pixels, formattedBitmapSource.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    // Invert each color channel
                    pixels[i + 2] = (byte)(255 - pixels[i + 2]); // Red channel
                    pixels[i + 1] = (byte)(255 - pixels[i + 1]); // Green channel
                    pixels[i] = (byte)(255 - pixels[i]);         // Blue channel
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight), pixels, formattedBitmapSource.PixelWidth * 4, 0);

                // Display the inverted image in the ModifiedImageViewer
                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        private void ContrastButton_Click(object sender, RoutedEventArgs e)
        {
            // Define your contrast enhancement factor here
            double contrastFactor = 1.5; // You can change this value as needed

            // Access the image displayed in the ModifiedImageViewer
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                // Apply contrast enhancement to the image
                FormatConvertedBitmap formattedBitmapSource = new FormatConvertedBitmap();
                formattedBitmapSource.BeginInit();
                formattedBitmapSource.Source = bitmapSource;
                formattedBitmapSource.DestinationFormat = bitmapSource.Format;
                formattedBitmapSource.DestinationFormat = PixelFormats.Bgra32; // Change to the appropriate pixel format if needed
                formattedBitmapSource.EndInit();

                byte[] pixels = new byte[formattedBitmapSource.PixelWidth * formattedBitmapSource.PixelHeight * 4];
                formattedBitmapSource.CopyPixels(pixels, formattedBitmapSource.PixelWidth * 4, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    // Apply contrast enhancement to each color channel
                    pixels[i + 2] = Clamp((byte)((pixels[i + 2] - 128) * contrastFactor + 128));
                    pixels[i + 1] = Clamp((byte)((pixels[i + 1] - 128) * contrastFactor + 128));
                    pixels[i] = Clamp((byte)((pixels[i] - 128) * contrastFactor + 128));
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, formattedBitmapSource.PixelWidth, formattedBitmapSource.PixelHeight), pixels, formattedBitmapSource.PixelWidth * 4, 0);

                // Display the modified image in the ModifiedImageViewer
                ModifiedImageViewer.Source = writeableBitmap;
            }
        }

        // Helper method to clamp a value between 0 and 255
        private byte Clamp(byte value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value > 255)
            {
                return 255;
            }
            else
            {
                return value;
            }
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if(OriginalImageViewer != null)
                ModifiedImageViewer.Source = OriginalImageViewer.Source;
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Access the image displayed in the ModifiedImageViewer
            BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

            if (bitmapSource != null)
            {
                // Prompt the user to select a location and filename for the saved image
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Save the modified image to the specified location
                    string filename = saveFileDialog.FileName;

                    // Determine the image format based on the file extension
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

                    // Save the image using the selected encoder
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

                // Access the image displayed in the ModifiedImageViewer
                BitmapSource bitmapSource = ModifiedImageViewer.Source as BitmapSource;

                if (bitmapSource != null)
                {
                    // Apply the selected filter to the image
                    BitmapSource filteredBitmap = ApplyFilter(bitmapSource, filterName);

                    // Display the filtered image in the ModifiedImageViewer
                    ModifiedImageViewer.Source = filteredBitmap;
                }
            }
        }

        private BitmapSource ApplyFilter(BitmapSource source, string filterName)
        {
            // Define convolution kernels for different filters
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

            // Apply the convolution filter to the image
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
                        resultPixels[resultIndex] = Clamp((byte)color[0]);
                        resultPixels[resultIndex + 1] = Clamp((byte)color[1]);
                        resultPixels[resultIndex + 2] = Clamp((byte)color[2]);
                        resultPixels[resultIndex + 3] = 255; // Alpha channel
                    }
                }

                WriteableBitmap resultBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                resultBitmap.WritePixels(new Int32Rect(0, 0, width, height), resultPixels, stride, 0);
                return resultBitmap;
            }

            private byte Clamp(byte value)
            {
                return value < 0 ? (byte)0 : value > 255 ? (byte)255 : value;
            }
        }
    }
}
