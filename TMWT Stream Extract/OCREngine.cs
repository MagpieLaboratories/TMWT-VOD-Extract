using IronOcr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TMWT_Stream_Extract
{
    public class OCREngine
    {
        IronTesseract OCR;
        public OCREngine()
        {
            OCR = new IronTesseract();
        }

        public OcrResult GetTextFromBitMap(Bitmap bmap)
        {
            return OCR.Read(bmap);
        }


        public OcrResult GetTextFromImage(string file)
        {
            var Input = new OcrInput(file);
            var Result = OCR.Read(Input);
            return Result;
        }

        //only returns a single word. good for score and names, but not teams, maps, etc
        public static string GetclosestTextInOCR(OcrResult ocrResult, Bitmap bmap, int XPosGuess, int YPosGuess, int WidthGuess, int HeightGuess, int requiredStringLength = 3, bool strictMode = false)
        {
            //adjust for 1850 by 480
            XPosGuess = Convert.ToInt32(Math.Ceiling((double)bmap.Width * XPosGuess / 1850));
            YPosGuess = Convert.ToInt32(Math.Ceiling((double)bmap.Height * YPosGuess / 480));
            WidthGuess = Convert.ToInt32(Math.Ceiling((double)bmap.Width * WidthGuess / 1850));
            HeightGuess = Convert.ToInt32(Math.Ceiling((double)bmap.Height * HeightGuess / 480));
            
            var closestText = "";
            double closestDistance = 185;
            foreach (var word in ocrResult.Words)
            {
                var distance_X = Math.Abs(XPosGuess - word.X);
                var distance_Y = Math.Abs(YPosGuess - word.Y);
                var distance_X_WidthAdj = Math.Abs((XPosGuess + WidthGuess) - (word.X + word.Width));
                var distance_Y_HeightAdj = Math.Abs((YPosGuess + HeightGuess) - (word.Y + word.Height));

                var totalDistance = distance_X + distance_Y + distance_X_WidthAdj + distance_Y_HeightAdj;
                if (totalDistance < closestDistance)
                {
                    if (word.Text.Trim().Length >= requiredStringLength)
                    {
                        if (strictMode)
                        {
                            //YPos can't be wrong by more than 10
                            if (distance_Y < 12)
                            {
                                closestText = word.Text;
                                closestDistance = totalDistance;
                            }
                        }
                        else
                        {
                            closestDistance = totalDistance;
                            closestText = word.Text.Trim();
                        }
                    }
                }
            }
            return closestText;
        }

        public static string GetAllEnclosedTextInOCR(OcrResult ocrResult, Bitmap bmap, int XPosGuess, int YPosGuess, int WidthGuess, int HeightGuess)
        {
            //adjust for 1850 by 480
            XPosGuess = Convert.ToInt32(Math.Ceiling((double) bmap.Width * XPosGuess / 1850));
            YPosGuess = Convert.ToInt32(Math.Ceiling((double) bmap.Height * YPosGuess / 480));
            WidthGuess = Convert.ToInt32(Math.Ceiling((double) bmap.Width * WidthGuess / 1850));
            HeightGuess = Convert.ToInt32(Math.Ceiling((double) bmap.Height * HeightGuess / 480));
            
            var enclosedText = "";
            foreach (var word in ocrResult.Words)
            {
                if (word.X >= XPosGuess && ( word.X + Math.Min(130,word.Width)) <= (XPosGuess + WidthGuess))
                {
                    if (word.Y >= YPosGuess && ( word.Y + Math.Min(70,word.Height) ) <= (YPosGuess + HeightGuess))
                    {
                        enclosedText += word.Text.Trim() + " ";
                    }
                }
            }
            return enclosedText.Trim();
        }
    }
}
