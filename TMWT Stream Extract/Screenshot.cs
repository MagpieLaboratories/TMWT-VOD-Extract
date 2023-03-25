using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace TMWT_Stream_Extract
{
    public class Screenshot
    {
        //take a screenshot of the screen and save it to a local folder
        public static List<bool> TakeAndSaveScreenshot(string baseFileName, List<bool> prevFileHash)
        {
            //take a black and white image of the screen
            var screenwidth = Screen.PrimaryScreen.Bounds.Width;
            var screenheight = Screen.PrimaryScreen.Bounds.Height;
            
            using var bitmap = new Bitmap(1850, 480);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            //convert bitmap to greyscale
            var greyBitMap = MakeGreyScale(bitmap);

            var newHash = GetHash(greyBitMap);

            //check hash similarity 
            if (newHash.SequenceEqual(prevFileHash))
            {
                //dont save dupe
                return newHash;
            }
            //delete curr image
            System.IO.File.Delete(baseFileName);
            greyBitMap.Save(baseFileName, ImageFormat.Tiff);
            return newHash;
        }

        public static Bitmap TakeScreenshot()
        {

            var screenwidth = Screen.PrimaryScreen.Bounds.Width;
            var screenheight = Screen.PrimaryScreen.Bounds.Height;
            //1/3 of screen height
            //0.725 of width

            //using var bitmap = new Bitmap(1850, 480);
            using var bitmap = new Bitmap(Convert.ToInt32(0.725 * screenwidth), Convert.ToInt32(screenheight * 0.333));
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            //convert bitmap to greyscale
            var greyBitMap = MakeGreyScale(bitmap);
            return greyBitMap;
        }

        public static List<bool> GetHash(Bitmap bmpSource)
        {
            List<bool> lResult = new List<bool>();
            //create new image with 16x16 pixel
            Bitmap bmpMin = new Bitmap(bmpSource, new Size(32, 32));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduction of colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }
            return lResult;
        }

        public static Bitmap MakeGreyScale(Bitmap original)
        {
            //get a graphics object from the new image
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
    }
}
