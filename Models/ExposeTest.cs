using AngelDLP;
using AngelDLP.PPC;
using MatterHackers.Agg.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AngleDLP.Models
{
    public static class ExposeTest
    {
        public static void ProjRect(Projector projector, int DAC, double time)
        {
            ImageBuffer testImage = new ImageBuffer(projector.projectorConfig.projectorPixHeight, projector.projectorConfig.projectorPixWidth, 32, new BlenderBGRA());
            DrawBox(ref testImage, projector.projectorConfig.projectorPixHeight / 2, projector.projectorConfig.projectorPixWidth / 2, 100);
            BitmapImage bm = ImageProcessTool.ImageBuffer_to_BitmapImage(testImage);
            
            projector.ProjectImageLED(bm, time, DAC);
        }
        static void DrawBox(ref ImageBuffer img, int diskCx, int diskCy, int radius)
        {
            for (int i = diskCx - radius; i < diskCx + radius + 1; i++)
            {
                for (int j = diskCy - radius; j < diskCy + radius + 1; j++)
                {
                    img.SetPixel(i, j, MatterHackers.Agg.Color.White);
                }
            }

        }
    }
}