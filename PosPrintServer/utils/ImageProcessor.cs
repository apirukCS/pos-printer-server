using System;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;

using System.Drawing;
using System.Drawing.Text;
using System.IO;

public static class ImageProcessor
{
    public static async Task ProcessImageFromUrlAsync(string url, string outputPath, uint width = 200)
    {
        try
        {
            byte[] imageBytes = await DownloadImageFromUrlAsync(url);

            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new Exception("No image data received");
            }

            // Create settings for reading the image
            var settings = new MagickReadSettings
            {
                BackgroundColor = MagickColors.White, // Set white background
                ColorSpace = ColorSpace.RGB        // Auto-detect format
            };

            using (var image = new MagickImage(imageBytes, settings))
            {
                // Debug information
                Console.WriteLine($"Original format: {image.Format}");
                Console.WriteLine($"Color space: {image.ColorSpace}");
                Console.WriteLine($"Depth: {image.Depth}");
                Console.WriteLine($"Original dimensions: {image.Width}x{image.Height}");

                // Convert to RGB if not already
                if (image.ColorSpace != ColorSpace.RGB)
                {
                    image.ColorSpace = ColorSpace.RGB;
                }

                // Ensure image has an alpha channel and flatten it
                image.Alpha(AlphaOption.Remove);

                // Adjust image properties
                image.Format = MagickFormat.Jpg;
                image.Quality = 90; // Increase quality

                // Resize maintaining aspect ratio
                var geometry = new MagickGeometry(width)
                {
                    IgnoreAspectRatio = false
                };
                image.Resize(geometry);

                // Optional: Adjust brightness/contrast if needed
                image.Contrast();    // -100 to 100

                // Save with specific format
                image.Write(outputPath);

                // Verify the output file exists and has content
                if (new FileInfo(outputPath).Length == 0)
                {
                    throw new Exception("Output file is empty");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing image: {ex.Message}");
        }
    }

    //public static async Task ProcessImageFromUrlAsync(string url, string outputPath,uint width = 200)
    //{
    //    byte[] imageBytes = await DownloadImageFromUrlAsync(url);
    //    //string p = "C:\\dotnet\\PosPrintServer\\PosPrintServer\\images\\numbers\\5.jpg";
    //    //byte[] imageBytes = File.ReadAllBytes(p);
    //    using (MagickImage image = new MagickImage(imageBytes))
    //    {
    //        //image.Threshold(new Percentage(70));
    //        image.Resize(new MagickGeometry(width, 0));
    //        image.Quality = 60;
    //        //image.Density = new Density(72, 72);
    //        image.Write(outputPath, MagickFormat.Jpg);
    //    }
    //}

    private static async Task<byte[]> DownloadImageFromUrlAsync(string url)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to download image: {ex.Message}");
            }
        }
    }

    //private static async Task<byte[]> DownloadImageFromUrlAsync(string url)
    //{
    //    using (HttpClient client = new HttpClient())
    //    {
    //        return await client.GetByteArrayAsync(url);
    //    }
    //}

    public static void MergeTwoImages(string imagePath1, string imagePath2, string outputPath)
    {
        using (Bitmap image1 = new Bitmap(imagePath1))
        using (Bitmap image2 = new Bitmap(imagePath2))
        {
            int newWidth = image1.Width + image2.Width;
            int newHeight = Math.Max(image1.Height, image2.Height);
            using (Bitmap combinedImage = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    combinedImage.SetResolution(72, 72);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(image1, new Rectangle(0, 0, image1.Width, image1.Height));
                    g.DrawImage(image2, new Rectangle(image1.Width, 0, image2.Width, image2.Height));
                }
                combinedImage.Save(outputPath);
                combinedImage.Dispose();
            }
        }
    }

    public static void MergeThreeImages(string imagePath1, string imagePath2, string imagePath3, string outputPath)
    {
        using (Bitmap image1 = new Bitmap(imagePath1))
        using (Bitmap image2 = new Bitmap(imagePath2))
        using (Bitmap image3 = new Bitmap(imagePath3))
        {
            int newWidth = image1.Width + image2.Width + image3.Width;
            int newHeight = Math.Max(image1.Height, Math.Max(image2.Height, image3.Height));
            using (Bitmap combinedImage = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    combinedImage.SetResolution(72, 72);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(image1, new Rectangle(0, 0, image1.Width, image1.Height));
                    g.DrawImage(image2, new Rectangle(image1.Width, 0, image2.Width, image2.Height));
                    g.DrawImage(image3, new Rectangle(image1.Width + image2.Width, 0, image3.Width, image3.Height));
                }
                combinedImage.Save(outputPath);
                combinedImage.Dispose();
            }
        }
    }
}
