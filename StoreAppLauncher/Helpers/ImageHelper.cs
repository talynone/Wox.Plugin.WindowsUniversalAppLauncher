using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreAppLauncher
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;

    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    using global::StoreAppLauncher.Helpers;
    using global::StoreAppLauncher.Models;

    using Brush = System.Drawing.Brush;
    using Image = System.Drawing.Image;

    public static class ImageHelper
    {
        public static Bitmap CreateIcon(string filePath, string backgroundColor, int width, int height)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var color = ColorFromString(backgroundColor);            
            var bitmap1 = Image.FromFile(filePath) as Bitmap;

            if (bitmap1 == null)
            {
                return null;
            }

            var resizedBitmap1 = ResizeImage(bitmap1, color, width, height, bitmap1.Width, bitmap1.Height) as Bitmap;          

            return resizedBitmap1;
        }

       

        private static Color GetColor(IntPtr pElementName)
        {
            var colourset = StarScreenColorsHelper.GetImmersiveUserColorSetPreference(false, false);
            uint type = StarScreenColorsHelper.GetImmersiveColorTypeFromName(pElementName);
            Marshal.FreeCoTaskMem(pElementName);
            uint colourdword = StarScreenColorsHelper.GetImmersiveColorFromColorSetEx((uint)colourset, type, false, 0);
            byte[] colourbytes = new byte[4];
            colourbytes[0] = (byte)((0xFF000000 & colourdword) >> 24); // A
            colourbytes[1] = (byte)((0x00FF0000 & colourdword) >> 16); // B
            colourbytes[2] = (byte)((0x0000FF00 & colourdword) >> 8); // G
            colourbytes[3] = (byte)(0x000000FF & colourdword); // R
            Color color = Color.FromArgb(colourbytes[0], colourbytes[3], colourbytes[2], colourbytes[1]);
            return color;
        }

        private static Color ColorFromString(string colorString)
        {
            Color color = Color.Transparent;
            
            IntPtr pElementName = Marshal.StringToHGlobalUni(ImmersiveColors.ImmersiveStartSelectionBackground.ToString());
            color = GetColor(pElementName);

            if (!string.IsNullOrEmpty(colorString))
            {
                var tempcolor = ColorTranslator.FromHtml(colorString);

                if (tempcolor != Color.Transparent)
                {
                    color = tempcolor;
                }
            }

            return color;
        }

        private static Brush BrushFromString(string colorString)
        {
            Color color = Color.White;

            if (!string.IsNullOrEmpty(colorString))
            {
                color = ColorTranslator.FromHtml(colorString);
            }
                        
            SolidBrush customColorBrush = new SolidBrush(color);

            return customColorBrush;
        }

        private static Bitmap CreateFilledRectangle(Brush brush, int x, int y)
        {            
            Bitmap bmp = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(brush, ImageSize);
            }
            return bmp;
        }

        private static Bitmap OverlayTwoImages(Bitmap source1, Bitmap source2, int width, int height)
        {
            var target = new Bitmap(source1.Width, source1.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(source1, 0, 0);
            graphics.DrawImage(source2, width, height);

            return target;
        }

        private static Image ResizeImage(Bitmap image, Color backgroundColor,
                     /* note changed names */
                     int canvasWidth, int canvasHeight,
                     /* new */
                     int originalWidth, int originalHeight)
        {
            
            System.Drawing.Image thumbnail =
                new Bitmap(canvasWidth, canvasHeight); // changed parm names
            System.Drawing.Graphics graphic =
                         System.Drawing.Graphics.FromImage(thumbnail);

            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            /* ------------------ new code --------------- */

            // Figure out the ratio
            double ratioX = (double)canvasWidth / (double)originalWidth;
            double ratioY = (double)canvasHeight / (double)originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // now we can get the new height and width
            int newHeight = Convert.ToInt32(originalHeight * ratio);
            int newWidth = Convert.ToInt32(originalWidth * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero)
            int posX = Convert.ToInt32((canvasWidth - (originalWidth * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (originalHeight * ratio)) / 2);

            graphic.Clear(backgroundColor); // white padding
            graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            /* ------------- end new code ---------------- */

            System.Drawing.Imaging.ImageCodecInfo[] info =
                             ImageCodecInfo.GetImageEncoders();
            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality,
                             100L);

            return thumbnail;            
        }
    }
}
