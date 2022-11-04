using System;
using System.Collections.Generic;
using System.Linq;
using gs;
using g3;
using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.VertexSource;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Imaging;
using AngleDLP.Models;

namespace AngelDLP
{
    class AngelSlicer
    {

        public static Mat GetSliceImage_OpenCV(ObservableCollection<PartModel> parts, ref List<PrintTaskModels> printTasks, int layerInd, List<SliceParas> sliceParas, int projInd, List<List<PointF>> connectPairs)
        {
            //获取切层图像最终版
            Mat sliceProj = new Mat(sliceParas[projInd].height, sliceParas[projInd].width, MatType.CV_8U);
            foreach (var part in parts)
            {
                if (part.IsToPrint && part.printInWhitchProjector == projInd)
                {
                    MeshPlanarSlicer mps = printTasks[part.index].mps;
                    bool meshcheck = printTasks[part.index].mesh.IsClosed();

                    AxisAlignedBox3d bonds = printTasks[part.index].mesh.GetBounds();
                    //if (meshcheck) { SliceCSloid(layerInd, printTasks[part.index].pss,ref slice, bonds, config.projectorConfigs[part.printInWhitchProjector],printTasks[part.index], part.printLayerOffset, part.TX - part.printInWhitchProjector * config.projectorConfigs[part.printInWhitchProjector].PrinterPixHeight / config.projectorConfigs[part.printInWhitchProjector].PrintPixScale, part.TY, part.TZ); }
                    //else { SliceBPath(layerInd, printTasks[part.index].pss, ref slice, bonds, config.projectorConfigs[part.printInWhitchProjector], printTasks[part.index], part.printLayerOffset, part.TX - part.printInWhitchProjector * config.projectorConfigs[part.printInWhitchProjector].PrinterPixHeight / config.projectorConfigs[part.printInWhitchProjector].PrintPixScale, part.TY, part.TZ); }
                    Slice_OpenCV(layerInd, printTasks[part.index].pss, ref sliceProj, bonds, sliceParas[projInd], printTasks[part.index], meshcheck, part.printLayerOffset,
                        part.TX - sliceParas[projInd].projectorOffsetX,
                        part.TY - sliceParas[projInd].projectorOffsetY, part.TZ, part.RZ, part.isManual);
                    //if (layerInd - part.printLayerOffset < sliceParas[projInd].holeCompNum)
                    //{
                    //    AngelSlicer.DrawPlatformHole_OpenCV(ref sliceProj, sliceParas[projInd], new g3.Vector2d(sliceParas[projInd].projectorPixScale*(part.TX - sliceParas[projInd].projectorOffsetX), sliceParas[projInd].projectorPixScale *( part.TY - sliceParas[projInd].projectorOffsetY)), false);
                    //}

                }
                //Cv2.ImWrite($"debug-{layerInd}-{projInd}.png", sliceProj);
            }
            double startInd = Math.Max(sliceParas[projInd].holeComps.Count, 2);
            if (layerInd> startInd && layerInd < startInd + 6 &&sliceParas[projInd].isAlignerMold)
            {
                DrawConnectPairs(ref sliceProj, sliceParas[projInd], connectPairs);
            }
            return sliceProj;
        }

        private static void DrawConnectPairs(ref Mat mat, SliceParas sliceParas, List<List<PointF>> connectPairs)
        {
            foreach (var pair in connectPairs)
            {
                if (pair[0].X > sliceParas.projectorOffsetX && pair[0].X < sliceParas.projectorOffsetX + sliceParas.width * sliceParas.projectorPixScaleX)
                {
                    double x1 = (pair[0].X - sliceParas.projectorOffsetX) / sliceParas.projectorPixScaleX;
                    double y1 = (pair[0].Y - sliceParas.projectorOffsetY - 0.7) / sliceParas.projectorPixScaleY;
                    double x2 = (pair[1].X - sliceParas.projectorOffsetX) / sliceParas.projectorPixScaleX;
                    double y2 = (pair[1].Y - sliceParas.projectorOffsetY + 0.7) / sliceParas.projectorPixScaleY;
                    Cv2.Rectangle(mat, new Point((int)Math.Round(x1 - 3), (int)(sliceParas.height - Math.Round(y1))),
                                        new Point((int)Math.Round(x2 + 3), (int)(sliceParas.height - Math.Round(y2))), Scalar.White, -1);
                }
            }
        }
        //public static List<Mat> GetSingleModelSliceImage(PrintTaskModels printTask)
        //{
        //    ProjectorConfig config = new ProjectorConfig()
        //    {
        //        PrintPixScale = 10,
        //        PrinterPixHeight = 800,
        //        PrinterPixWidth = 800
        //    };
        //    MeshPlanarSlicer mps = printTask.mps;
        //    bool meshcheck = printTask.mesh.IsClosed();
        //    AxisAlignedBox3d bonds = printTask.mesh.GetBounds();
        //    List<Mat> slices = new List<Mat>();
        //    int layers = (int)Math.Round((bonds.Max[2] + 0.06) / 0.12, 0);
        //    for (int i = 0; i < layers; i++)
        //    {
        //        //获取切层图像最终版
        //        Mat slice = new Mat(config.PrinterPixHeight, config.PrinterPixWidth, MatType.CV_8U, Scalar.Black);
        //        Slice_OpenCV(i, printTask.pss, ref slice, bonds, config, printTask, meshcheck, 0, 40, 40, 0,0,false);
        //        slices.Add(slice);
        //    }  
        //    return slices;
        //}

        public static PrintTaskModels NewPTM(int ind, string path, string filename, Stream s, bool isSTL)
        {
            PrintTaskModels ptm = new PrintTaskModels();
            ptm.index = ind;
            ptm.fileName = filename;
            ptm.path = path;
            if (isSTL)
            {
                ptm.mesh = StandardMeshReader.ReadMesh(s, "stl");
            }
            else
            {
                ptm.mesh = StandardMeshReader.ReadMesh(s, "obj");
            }
            Vector2d vol_area = MeshMeasurements.VolumeArea(ptm.mesh, ptm.mesh.TriangleIndices(), ptm.mesh.GetVertex);
            ptm.volume = vol_area[0];
            ptm.surfaceArea = vol_area[1];
            ptm.vertexCount = ptm.mesh.VertexCount;
            ptm.edgeCount = ptm.mesh.EdgeCount;
            ptm.triangleCount = ptm.mesh.TriangleCount;
            if (ptm.mesh.IsClosed())
            {
                ptm.isSolidorPath = true;
            }
            else
            {
                ptm.isSolidorPath = false;
            }

            return ptm;

        }
        private static void connectIsland(ref ImageBuffer image, int x, int y, bool lrORud)
        {
            //ImageIO.SaveImageData(".\\test.bmp", image);
            //int rib offset
            if (lrORud)
            {
                for (int j = y - 5; j < y + 5; j++)
                {
                    int min = x;
                    //找负向不为0像素
                    int itemp = x;
                    while (image.GetPixel(itemp--, j).red < 1)
                    {
                        min = itemp;
                    }
                    //找正向不为0像素
                    int max = x;
                    itemp = x;
                    while (image.GetPixel(itemp++, j).red < 1)
                    {
                        max = itemp;
                    }
                    for (int i = min - 1; i < max + 1; i++)
                    {
                        image.SetPixel(i, j, MatterHackers.Agg.Color.White);
                    }
                }
            }
            else
            {
                for (int i = x - 5; i < x + 5; i++)
                {
                    int min = y;
                    //找负向不为0像素
                    int itemp = y;
                    while (image.GetPixel(i, itemp--).red < 1)
                    {
                        min = itemp;
                    }
                    //找正向不为0像素
                    int max = y;
                    itemp = y;
                    while (image.GetPixel(i, itemp++).red < 1)
                    {
                        max = itemp;
                    }
                    for (int j = min - 1; j < max + 1; j++)
                    {
                        image.SetPixel(i, j, MatterHackers.Agg.Color.White);
                    }

                }
            }

        }

        //弃用Clipper 库偏移
        //问题1 无法作正交偏移
        //问题2 偏移后目前还未对输出的轨迹进行处理
        //后续改进计划
        //采用图像处理进行轮廓补偿
        //1-图片像素定位
        //2-5倍绘图
        //3-采用opencv进行非对称膨胀
        //4-缩小图像
        //5-移载图像


        private static void DrawOriPath(ref ImageBuffer image, ReadOnlyCollection<Vector2d> vs, ColorF colorf)
        {

            double DrawScale = 10;
            VertexStorage vertexStorage = new VertexStorage();

            for (int j = 0; j < vs.Count(); j++)
            {
                vertexStorage.Add(DrawScale * (vs[j].x + 40), DrawScale * (vs[j].y + 40), ShapePath.FlagsAndCommand.LineTo);
            }
            //vertexStorage.Add(projectorConfig.PrintPixScale * (vs[0].x + Tx), projectorConfig.PrintPixScale * (vs[0].y + Ty),ShapePath.FlagsAndCommand.LineTo);
            Contour contour = new Contour(vertexStorage);

            image.NewGraphics2D().Render(contour, colorf);

        }


        static void AddSupport_OpenCV(int ind, ref Mat mat, PrintTaskModels printTaskModel, double scaleX, double scaleY, double Tx, double Ty)
        {
            int supportRadiusPx = 4;
            int supportInsertPx = 2;
            //轮询所有supports
            foreach (var su in printTaskModel.supportModelsList)
            {
                if (ind < su.startLayer) { continue; }
                if (ind == su.startLayer)
                {
                    //新增查看support 查找过程可视化
                    bool DebugSupportStart = false;

                    //如果当前是第一层，则左右寻找，填充list
                    int x = (int)Math.Round((su.centerX + Tx) * scaleX);
                    int y = (int)Math.Round((su.CenterY + Ty) * scaleY);
                    //中心位置可视化
                    if (DebugSupportStart)
                    {
                        mat.At<byte>(y, x) = 150;
                        Cv2.ImWrite("debug.bmp", mat);
                    }
                    if (su.lrORud)
                    {
                        for (int j = y - supportRadiusPx; j < y + supportRadiusPx; j++)
                        {
                            int min = x;
                            //找负向不为0像素
                            int itemp = x;
                            //Vec3b pix = mat.At<Vec3b>( j, itemp--);
                            while (mat.At<Vec3b>(j, itemp--)[0] < 200 && itemp > 0)
                            {
                                min = itemp;
                            }
                            //找正向不为0像素
                            int max = x;
                            itemp = x;
                            while (mat.At<Vec3b>(j, itemp++)[0] < 200 && itemp < mat.Width - 1)
                            {
                                max = itemp;
                            }
                            if (Math.Abs(max - min) < 200)
                            {
                                for (int i = min - supportInsertPx; i < max + supportInsertPx; i++)
                                {
                                    mat.At<byte>(j, i) = 255;
                                    su.supportPixFills.Add(new SupportPixFill()
                                    {
                                        x = i,
                                        y = j,
                                        fillingTopCount = 0
                                    }); ;
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        for (int i = x - supportRadiusPx; i < x + supportRadiusPx; i++)
                        {
                            int min = y;
                            //找负向不为0像素
                            int itemp = y;
                            while (mat.At<Vec3b>(itemp--, i)[0] < 200 && itemp > 0)
                            {
                                min = itemp;
                            }
                            //找正向不为0像素
                            int max = y;
                            itemp = y;
                            while (mat.At<Vec3b>(itemp++, i)[0] < 200 && itemp < mat.Height - 1)
                            {
                                max = itemp;
                            }
                            if (Math.Abs(max - min) < 200)
                            {
                                for (int j = min - supportInsertPx; j < max + supportInsertPx; j++)
                                {
                                    mat.At<byte>(j, i) = 255;
                                    su.supportPixFills.Add(new SupportPixFill()
                                    {
                                        x = i,
                                        y = j,
                                        fillingTopCount = 0
                                    });
                                }
                            }
                            
                        }
                    }
                    if (su.supportPixFills.Count < 50)
                    {
                        foreach (var f in su.supportPixFills)
                        {
                            f.fillingTopCount = 4;
                        }
                    }
                }
                else
                {
                    //不是第一层，则判断，如果是黑则填充，如果已经是白，则此像素后续不填充
                    foreach (var spf in su.supportPixFills)
                    {
                        if (spf.fillingTopCount < 4)
                        {
                            if (mat.At<Vec3b>(spf.y, spf.x)[0] > 200)
                            {
                                spf.fillingTopCount++;
                            }
                            mat.At<byte>(spf.y, spf.x) = 255;
                        }
                    }

                }
            }


        }

        public static void AddImgPart_OpenCV(ref Mat mat, Mat mat_part, Vector2d start, Vector2d end, Vector2d center, SliceParas sliceParas, bool grayComp, double centerLightComp, int currInd, int fixedInd)
        {
            Vector2d centerOpenCV = new Vector2d(center.x, sliceParas.height - center.y);

            int startx = Math.Max((int)Math.Floor(start[0] + centerOpenCV[0]) - 1, 0);
            int endx = Math.Min((int)Math.Ceiling(end[0] + centerOpenCV[0]) + 1, sliceParas.width);
            int starty = Math.Max((int)Math.Floor(start[1] + centerOpenCV[1]) - 1, 0);
            int endy = Math.Min((int)Math.Ceiling(end[1] + centerOpenCV[1]) + 1, sliceParas.height);
            int centerX = (int)Math.Round(centerOpenCV[0], 0);
            int centerY = (int)Math.Round(centerOpenCV[1], 0);
            for (int i = startx; i < endx; i++)
            {
                for (int j = starty; j < endy; j++)
                {
                    byte color = mat_part.At<Vec3b>(j - centerY + 400, i - centerX + 400)[0];
                    if (color > 0)
                    {
                        double lightComp = 1;
                        if (grayComp)
                        {
                            //计算在当前投影仪下坐标
                            //获取当前坐标下的高度
                            int xi = (int)Math.Floor((double)i / 40);
                            int yi = (int)Math.Floor((double)j / 40);
                            lightComp = 1 - (sliceParas.grayCalib.values[xi][yi] - sliceParas.grayCalib.min) / sliceParas.grayCalib.values[xi][yi];
                        }
                        byte colorcomp = (byte)(centerLightComp * lightComp * color);
                        mat.At<byte>(j, i) = colorcomp;
                    }
                }
            }
        }
        public static void AddImgPart_OpenCV_Mask(ref Mat mat, Mat mat_part, Vector2d start, Vector2d end, Vector2d center, SliceParas sliceParas, bool grayComp, double centerLightComp, int currInd, int fixedInd)
        {
            //get the model mask  and copy to proj image with rate 
            //get the model contour mask and copy to proj image with rate 1

            Mat mask = new Mat(mat_part.Rows, mat_part.Cols, MatType.CV_8UC1, Scalar.Black);
            Cv2.Threshold(mat_part, mask,1,255,ThresholdTypes.Binary);
            
            Mat maskContour = new Mat(mat_part.Rows, mat_part.Cols, MatType.CV_8UC1, Scalar.Black);
            Vector2d centerOpenCV = new Vector2d(center.x, sliceParas.height - center.y);
            int startx = (int)Math.Round(- 400 + centerOpenCV[0]);
            int starty = (int)Math.Round(- 400 + centerOpenCV[1]);
            int xi = (int)Math.Floor((double)center.x / 40);
            int yi = (int)Math.Floor((double)center.y / 40);
            double lightComp = 1 - (sliceParas.grayCalib.values[xi][yi] - sliceParas.grayCalib.min) / sliceParas.grayCalib.values[xi][yi];
            OpenCvSharp.HierarchyIndex[] hierarchyIndex;
            Point[][] contours;
            Cv2.FindContours(mat_part, out contours, out hierarchyIndex, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            for (int i = 0; i < contours.Length; i++)
            {
                Cv2.DrawContours(maskContour, contours, i, Scalar.White, sliceParas.dimmingWall);
            }
            //Cv2.ImWrite("AAAmaskContour.bmp", maskContour);
            if (fixedInd > sliceParas.baseEnhanceLayer)
            {
                AddByMask(ref mat, mat_part, mask, startx, starty, lightComp * sliceParas.dimmingRate);
                AddByMask(ref mat, mat_part, maskContour, startx, starty, lightComp);
            }
            else
            {
                AddByMask(ref mat, mat_part, mask, startx, starty, lightComp);
            }
            
            
        }
        private static void Draw_OpenCV(ref Mat mat, double scaleX, double scaleY, ReadOnlyCollection<Vector2d> vs, double Tx, double Ty, bool isHole)
        {

            List<List<Point>> contours = new List<List<Point>>();
            List<Point> c = new List<Point>();

            for (int j = 0; j < vs.Count(); j++)
            {
                //double tempx1 = scale * (vs[j].x + Tx);
                //double tempy1 = scale * (vs[j].y + Ty);
                //double rxz = Math.PI * Rz / 180;
                //double X2 = tempx1*Math.Cos(rxz) - tempx1 * Math.Sin(rxz);
                //double Y2 = tempx1*Math.Sin(rxz) + tempy1 * Math.Cos(rxz);
                //c.Add(new Point()
                //{
                //    X = (int)Math.Round(X2),
                //    Y = mat.Rows - (int)Math.Round(Y2)
                //});
                c.Add(new Point()
                {
                    X = (int)Math.Round(scaleX * (vs[j].x + Tx)),
                    Y = (int)Math.Round(scaleY * (vs[j].y + Ty))
                });
            }
            contours.Add(c);


            if (isHole)
            {
                Cv2.DrawContours(mat, contours, -1, Scalar.Black, -1);
            }
            else
            {
                Cv2.DrawContours(mat, contours, -1, Scalar.White, -1);
            }
        }
        static void Slice_OpenCV(int ind, PlanarSliceStack planarSliceStack, ref Mat mat_slice, AxisAlignedBox3d bonds, SliceParas sliceParas, PrintTaskModels printTaskModel, bool meshCheck, int sliceOffset = 0, double Tx = 0, double Ty = 0, double Tz = 0, double Rz = 0, bool isManual = true, bool grayComp = true)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            int scaleTimes = 3;
            int preDrawScale = (int)Math.Pow(2, scaleTimes);
            if (ind < sliceOffset) { return; }
            int fixedInd = ind - sliceOffset - (int)sliceParas.baseCompNum;
            fixedInd = Math.Max(0, fixedInd);
            double scale_fixX = 10 / sliceParas.drawScaleX;//scale
            double scale_fixY = 10 / sliceParas.drawScaleY;//scale
            double xN_PrintPixScaleX = preDrawScale * sliceParas.drawScaleX;//scale
            double xN_PrintPixScaleY = preDrawScale * sliceParas.drawScaleY;//scale
            int xN_PrinterPixHeight = preDrawScale * 800;
            int xN_PrinterPixWidth = preDrawScale * 800;
            Mat xNmat_ori = new Mat(xN_PrinterPixHeight, xN_PrinterPixWidth, MatType.CV_8U, Scalar.Black);


            if (fixedInd > planarSliceStack.Slices.Count - 1) { return; }
            double oriOffset = preDrawScale * 10;
            double drawOffsetX = 40 * scale_fixX;
            double drawOffsetY = 40 * scale_fixY;
            if (meshCheck)
            {
                foreach (var solid in planarSliceStack.Slices[fixedInd].Solids)
                {
                    Draw_OpenCV(ref xNmat_ori, xN_PrintPixScaleX, xN_PrintPixScaleY, solid.Outer.Vertices, drawOffsetX, drawOffsetY, false);
                    foreach (var hole in solid.Holes)
                    {
                        Draw_OpenCV(ref xNmat_ori, xN_PrintPixScaleX, xN_PrintPixScaleY, hole.Vertices, drawOffsetX, drawOffsetY, true);
                    }
                }
            }
            else
            {
                foreach (var p in printTaskModel.solidPathsList[fixedInd])
                {
                    Draw_OpenCV(ref xNmat_ori, xN_PrintPixScaleX, xN_PrintPixScaleY, p.Vertices, drawOffsetX, drawOffsetY, false);
                }
                foreach (var p in printTaskModel.holePathsList[fixedInd])
                {
                    Draw_OpenCV(ref xNmat_ori, xN_PrintPixScaleX, xN_PrintPixScaleY, p.Vertices, drawOffsetX, drawOffsetY, true);
                }
                foreach (var p in printTaskModel.solidPathsInHoleList[fixedInd])
                {
                    Draw_OpenCV(ref xNmat_ori, xN_PrintPixScaleX, xN_PrintPixScaleY, p.Vertices, drawOffsetX, drawOffsetY, false);
                }
            }
            double fixedCompX = sliceParas.contourCompX;
            double fixedCompY = sliceParas.contourCompY;
            if (fixedInd < sliceParas.elephantFeetComps.Count)
            {
                fixedCompX -= sliceParas.elephantFeetComps[fixedInd];
                fixedCompY -= sliceParas.elephantFeetComps[fixedInd];
            }
            
            //定义结构元素
            int x = (int)Math.Round(Math.Pow(2, scaleTimes) * (fixedCompX * sliceParas.drawScaleX), 0);
            int y = (int)Math.Round(Math.Pow(2, scaleTimes) * (fixedCompY * sliceParas.drawScaleY), 0);
            int x_Dilate = x > 0 ? x + 1 : 1;
            int y_Dilate = y > 0 ? y + 1 : 1;
            int x_Erode = x > 0 ? 1 : -1 * x + 1;
            int y_Erode = y > 0 ? 1 : -1 * y + 1;
            InputArray kernel_Dilate = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(x_Dilate, y_Dilate), new Point(-1, -1));
            InputArray kernel_Erode = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(x_Erode, y_Erode), new Point(-1, -1));
            //开操作,也可省落变量前缀名
            Cv2.MorphologyEx(xNmat_ori, xNmat_ori, MorphTypes.Dilate, kernel_Dilate, new Point(-1, -1), 1, BorderTypes.Constant, Scalar.Black);
            Cv2.MorphologyEx(xNmat_ori, xNmat_ori, MorphTypes.Erode, kernel_Erode, new Point(-1, -1), 1, BorderTypes.Constant, Scalar.Black);

            for (int i = scaleTimes; i > 0; i--)
            {
                Cv2.PyrDown(xNmat_ori, xNmat_ori, new OpenCvSharp.Size(Math.Pow(2, i - 1) * 800, Math.Pow(2, i - 1) * 800));
            }
            if (!isManual&&sliceParas.isAlignerMold)
            {
                ProcessPlat(fixedInd, ref xNmat_ori, sliceParas);
                ProcessCorner(fixedInd, ref xNmat_ori, sliceParas);
            }

            //sw.Stop();
            //var time = sw.Elapsed;
            if (sliceParas.isAlignerMold)
            {
                AddSupport_OpenCV(fixedInd, ref xNmat_ori, printTaskModel, sliceParas.drawScaleX, sliceParas.drawScaleY, 40 * scale_fixX, 40 * scale_fixY);
            }

           
            ////ImageProcessTool.SaveBitmapImageIntoFile(ImageProcessTool.ImageBuffer_to_BitmapImage(resImage), "resImage.bmp");
            Vector2d start = new Vector2d((bonds.Min[0]) * sliceParas.drawScaleX, (-bonds.Max[1]) * sliceParas.drawScaleY);
            Vector2d end = new Vector2d((bonds.Max[0]) * sliceParas.drawScaleX, (-bonds.Min[1]) * sliceParas.drawScaleY);
            Cv2.Flip(xNmat_ori, xNmat_ori, FlipMode.X);
            if (Math.Abs(Rz - 90) < 1)
            {
                start = new Vector2d((-bonds.Max[1]) * sliceParas.drawScaleY, (-bonds.Max[0]) * sliceParas.drawScaleX);
                end = new Vector2d((-bonds.Min[1]) * sliceParas.drawScaleY, (-bonds.Min[0]) * sliceParas.drawScaleX);
                Cv2.Transpose(xNmat_ori, xNmat_ori);
                Cv2.Flip(xNmat_ori, xNmat_ori, FlipMode.X);
            }
            if (Math.Abs(Rz + 90) < 1)
            {
                start = new Vector2d((bonds.Min[1]) * sliceParas.drawScaleY, (bonds.Min[0]) * sliceParas.drawScaleX);
                end = new Vector2d((bonds.Max[1]) * sliceParas.drawScaleY, (bonds.Max[0]) * sliceParas.drawScaleX);
                Cv2.Transpose(xNmat_ori, xNmat_ori);
                Cv2.Flip(xNmat_ori, xNmat_ori, FlipMode.Y);
            }
            //Vector2d center = new Vector2d(Tx * sliceParas.drawScaleX, sliceParas.height - Ty * sliceParas.drawScaleY);
            Vector2d center = new Vector2d(Tx / sliceParas.projectorPixScaleX, Ty / sliceParas.projectorPixScaleY);
            if (fixedInd < sliceParas.holeComps.Count) //
            {
                DrawPlatformHole_OpenCV(ref xNmat_ori, sliceParas,fixedInd, center, false);

            }
            //if(fixedInd == Math.Ceiling(sliceParas.holeCompNum)&&sliceParas.holeCompNum - Math.Floor(sliceParas.holeCompNum)>0.3)
            //{
            //    DrawPlatformHole_OpenCV(ref xNmat_ori, sliceParas, center, false, sliceParas.holeCompNum - Math.Floor(sliceParas.holeCompNum));
            //}
            if (fixedInd >= sliceParas.holeComps.Count && ind < sliceParas.baseEnhanceLayer)
            {
                DrawPlatformHole_OpenCV(ref xNmat_ori, sliceParas, sliceParas.holeComps.Count -1 , center, false, 1 / sliceParas.baseEnhanceRate);
                
            }
            double centerDist = Math.Sqrt(Math.Pow(Tx - (sliceParas.width * sliceParas.projectorPixScaleX) / 2, 2) + Math.Pow(Ty - (sliceParas.height * sliceParas.projectorPixScaleY) / 2, 2));
            double centerLightComp = 1;//= 1 - 0.2 * (1 - centerDist / 220);
            if (!isManual && ClassValue.ReplaceLabel&& sliceParas.isAlignerMold)
            {
                ReplaceLabel(fixedInd, ref xNmat_ori, sliceParas);
            }
            AddImgPart_OpenCV_Mask(ref mat_slice, xNmat_ori,
                start, end, center,
                sliceParas, grayComp, centerLightComp, ind, fixedInd);

            xNmat_ori.Dispose();






            //Cv2.Resize(x4mat_ori, xNmat_dilate_erode, new OpenCvSharp.Size(800, 800), preDrawScale, preDrawScale, InterpolationFlags.Linear);
            //Cv2.ImWrite("0-xN-dilate_erode-PyrDown.bmp", xNmat_dilate_erode);


            //Cv2.ImWrite("0-dd.bmp", xNmat_dilate_erode);
        }

        private static void ProcessPlat(int layerInd, ref Mat mat, SliceParas sliceParas)
        {
            if (layerInd > 20)
            { return; }
            // 判断是否封闭
            int[] startxs = { 30, 500 };
            int[] centxs = { 120, 150 };//150,650

            int centy = 80;//550
            //int startx_right = 30;
            //int startx_left = 500;
            int starty = 450;
            int width = 270;
            int height = 250;

            int startInd = (int)Math.Round(1 / sliceParas.layerThicness - 1.5);
            if (layerInd < startInd + 1 || layerInd > startInd + 12) { return; }
            for (int s = 0; s < 2; s++)
            {
                Rect r_left = new Rect(startxs[s], starty, width, height);
                Mat roi = mat.SubMat(r_left);
                Mat roiCopy = roi.Clone();
                // 指定src 的 ROI子图像区域
                //Mat dstroi = dst(Rect(0, 10, r.width, r.height)); // 拿到 dst指定区域子图像的引用
                //src(r).convertTo(dstroi, dstroi.type(), 1, 0); // ROI子图像之间的复制

                //if (startInd == layerInd)
                //{

                //    //切出区域图像
                //    // 400 ± (120->380) /// 400 + (50  - > 350)

                //    //标识圆片位置(±25   15)
                //    //标识外侧牙龈轮廓

                //    //offset 内轮廓
                //    //计算开孔位置
                //    //
                //    Cv2.ImWrite($"a-{s}-{layerInd}-lastplate.bmp", roi_left);
                //}
                //标识外侧牙龈轮廓
                //找轮廓
                OpenCvSharp.HierarchyIndex[] hierarchyIndex_left;
                Point[][] contours_left;
                //第一行画白线
                bool isFirst = true;
                int firstInd = 0;
                for (int i = 0; i < width; i++)
                {
                    //找到第一个白
                    byte color = roiCopy.At<Vec3b>(0, i)[0];
                    if (isFirst && (color > 10))
                    {
                        isFirst = false;
                        firstInd = i;
                    }
                    //找到第二个白
                    if (!isFirst && (i > 30 + firstInd) && color > 10)
                    {
                        break;
                    }
                    if (firstInd != 0)
                    {
                        roiCopy.At<byte>(0, i) = 255;
                    }


                }
                //Cv2.ImWrite($"a-{s}-{layerInd}-first-gingiva-ori.bmp", roiCopy);
                Cv2.FindContours(roiCopy, out contours_left, out hierarchyIndex_left, RetrievalModes.External, ContourApproximationModes.ApproxNone);
                Cv2.Rectangle(roiCopy, new Point(0, 0), new Point(width, height), Scalar.Black, -1);
                Cv2.DrawContours(roiCopy, contours_left, 0, Scalar.White, -1);
                InputArray kernel_Erode = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(22, 22), new Point(-1, -1));
                Cv2.MorphologyEx(roiCopy, roiCopy, MorphTypes.Erode, kernel_Erode, new Point(-1, -1), 1, BorderTypes.Constant, Scalar.Black);
                //转移mask
                for (int mi = 0; mi < width; mi++)
                {
                    for (int mj = 0; mj < height; mj++)
                    {
                        byte colormask = roiCopy.At<Vec3b>(mj, mi)[0];
                        if (colormask > 100)
                        {
                            //mat.At<byte>(starty + mj, startxs[s] + mi) = 0;
                            roi.At<byte>(mj, mi) = 0;
                        }
                    }
                }
                //找边界
                int leftdist = 0;
                int rightdist = 0;
                int downdist = 0;
                for (int i = 0; i < 100; i++)
                {
                    //找到第一个白
                    byte colorleft = roiCopy.At<Vec3b>(centy, centxs[s] - i)[0];
                    byte colorright = roiCopy.At<Vec3b>(centy, centxs[s] + i)[0];
                    byte colordown = roiCopy.At<Vec3b>(centy + i, centxs[s])[0];
                    if (leftdist == 0 && colorleft < 10)
                    {
                        leftdist = i;
                    }
                    if (rightdist == 0 && colorright < 10)
                    {
                        rightdist = i;
                    }
                    if (downdist == 0 && colordown < 10)
                    {
                        downdist = i;
                    }
                }
                int mindist = Math.Min((int)Math.Round((float)(leftdist + rightdist) / 2, 0), downdist);
                //if (mindist > 30)
                //{
                //    int r = mindist - 10 - 3 * (layerInd - startInd);
                //    if (r > 3)
                //    {
                //        Cv2.Circle(roi, new Point((int)Math.Round((float)(centxs[s] - leftdist / 2 + rightdist / 2), 0), centy), r, Scalar.White, -1);
                //    }
                //}
                //Cv2.Circle(mat, new Point(startxs[s] + centxs[s] - leftdist + rightdist, starty + centy), (int)Math.Round((1 - 0.2 * layerInd + 0.2 * startInd) * mindist, 0), Scalar.White, -1);
                //Cv2.Circle(mat, new Point(startxs[s] + centxs[s] - leftdist - 5, starty + centy), 10, Scalar.Black, -1);
                //Cv2.Circle(mat, new Point(startxs[s] + centxs[s] + rightdist + 5, starty + centy), 10, Scalar.Black, -1);
                //Cv2.Circle(mat, new Point(startxs[s] + centxs[s], starty + centy + downdist + 5), 10, Scalar.Black, -1);
                //Cv2.Circle(mat, new Point(startxs[s] + centxs[s] - leftdist + rightdist, starty + centy), (int)Math.Round((1 - 0.2 * layerInd + 0.2 * startInd) * mindist, 0), Scalar.White, -1);
                if (layerInd < startInd + 8)
                {
                    Cv2.Circle(roi, new Point(centxs[s] - leftdist - 5, centy), 10, Scalar.Black, -1);
                    Cv2.Circle(roi, new Point(centxs[s] + rightdist + 5, centy), 10, Scalar.Black, -1);
                    Cv2.Circle(roi, new Point(centxs[s], centy + downdist + 5), 10, Scalar.Black, -1);
                }

                //计算开孔位置
                //Cv2.ImWrite($"a-{s}-{layerInd}-first-gingiva.bmp", roi);

                //第一层80%内接圆
                //第二层60%内接圆
                //第三层40%内接圆
                //第四层20%内接圆
            }
            //Cv2.ImWrite($"a-{layerInd}-all.bmp", mat);
        }

        private static Point PointToPix(Point2D point, double scale)
        {
            Point resPoint = new Point();
            resPoint.X = (int)Math.Round(point.x / scale) + 400;
            resPoint.Y = (int)Math.Round(point.y / scale) + 400;
            return resPoint;
        }
        private static void ProcessCorner(int layerInd, ref Mat mat, SliceParas sliceParas)
        {
            if (layerInd * sliceParas.layerThicness > 0.88)
            { return; }
            //5.6
            //从负向寻找
            List<Point2D> startPoints = new List<Point2D>()
            {
                new Point2D(-37, 5.55),
                new Point2D(-8.5, 5.55),
                new Point2D(8.5, 5.55),
                new Point2D(37, 5.55),
            };
            List<int> directions = new List<int>()
            {
                1,
                -1,
                1,
                -1
            };
            for (int dir = 0; dir < 4; dir++)
            {
                Point px = PointToPix(startPoints[dir], sliceParas.projectorPixScaleX);

                while (mat.At<byte>(px.Y, px.X) < 30)
                {
                    px.X += directions[dir];
                }
                px.X += directions[dir] * 6;
                px.Y += 3;
                Point px2 = new Point(px.X + directions[dir] * 30, px.Y + 5);
                Point px3 = new Point(px.X + directions[dir] * 60, px.Y + 50);
                Cv2.Line(mat, px, px2, Scalar.White, 7);
                Cv2.Line(mat, px2, px3, Scalar.White, 7);
            }
            startPoints = new List<Point2D>()
            {
                new Point2D(-37, 0.5),
                new Point2D(-9, 0.5),
                new Point2D(9, 0.5),
                new Point2D(37, 0.5),
            };
            for (int dir = 0; dir < 4; dir++)
            {
                Point px = PointToPix(startPoints[dir], sliceParas.projectorPixScaleX);

                while (mat.At<byte>(px.Y, px.X) < 100)
                {
                    px.X += directions[dir];
                }
                px.X += directions[dir] * 6;
                px.Y += 3;
                Point px2 = new Point(px.X + directions[dir] * 40, px.Y - 5);
                Point px3 = new Point(px.X + directions[dir] * 55, px.Y - 35);
                Cv2.Line(mat, px, px2, Scalar.White, 7);
                Cv2.Line(mat, px2, px3, Scalar.White, 7);
            }
            //Cv2.ImWrite(".\\test.bmp", mat);

        }
        private static void ReplaceLabel(int layerInd, ref Mat mat, SliceParas sliceParas)
        {
            if ((layerInd + 0.5) * sliceParas.layerThicness > 2.7 || (layerInd + 0.5) * sliceParas.layerThicness < 1.5)
            { return; }
            Point px = PointToPix(new Point2D(-3, -4), sliceParas.projectorPixScaleX);
            px.Y = 800 - px.Y;
            Point px2 = PointToPix(new Point2D(3, -7.2), sliceParas.projectorPixScaleX);
            px2.Y = 800 - px2.Y;
            Cv2.Rectangle(mat, px, px2, Scalar.Black, -1);
            string[] ss = ClassValue.MESConfig.DeviceId.Split('-');
            string devicelabel = ss[2].Substring(2, 2);
            Point lx = PointToPix(new Point2D(-3, -7), sliceParas.projectorPixScaleX);
            lx.Y = 800 - lx.Y;
            Cv2.PutText(mat, devicelabel, lx, HersheyFonts.HersheySimplex, 1.5, Scalar.White, 3);
            //Cv2.ImWrite(".\\test.bmp", mat);

        }

        public static Tuple<bool, List<PolyLine2d>>  CombinePaths(List<PolyLine2d> paths)
        {
            //返回轨迹合并结果
            //如果未被合并的有大于5个点的轨迹则返回失败
            List<PolyLine2d> combinedPaths = new List<PolyLine2d>();
            List<PolyLine2d> closedPaths = new List<PolyLine2d>();
            List<PolyLine2d> openPaths = new List<PolyLine2d>();
            for (int i = 0; i < paths.Count; i++)
            {

                if (Math.Sqrt(Math.Pow(paths[i].Start.x - paths[i].End.x, 2) + Math.Pow(paths[i].Start.y - paths[i].End.y, 2)) > 0.001)
                {
                    openPaths.Add(paths[i]);
                }
                else
                {
                    closedPaths.Add(paths[i]);
                }
                
            }
            if (openPaths.Count ==3)
            {
                int test = 0;
            }
            List<int> combainedInd = new List<int>();

            

            for (int i = 0; i < openPaths.Count; i++)
            {
                //每一条轨迹，如果尚未封闭，就从剩余轨迹中找到能接上的
                

                if (combainedInd.Contains(i))
                { continue; }
                //取出一条轨迹
                PolyLine2d vectors = openPaths[i];
                int findCount = 0;
                while (Math.Sqrt(Math.Pow(vectors.Start.x - vectors.End.x, 2) + Math.Pow(vectors.Start.y - vectors.End.y, 2)) > 0.001)
                {
                    if (findCount++ > 20)
                    {
                        //Error;
                        if (vectors.Count() > 5)
                        {
                           //vectors.AppendVertex(vectors.Start);
                           return new Tuple<bool, List<PolyLine2d>>(false, new List<PolyLine2d>());
                            
                        }
                        else
                        {
                            break;
                        }
                    }
                    //未封闭
                    for (int j = i + 1; j < openPaths.Count; j++)
                    {
                        if (combainedInd.Contains(j))
                        { continue; }
                        if (Math.Sqrt(Math.Pow(vectors.End.x - openPaths[j].Start.x, 2) + Math.Pow(vectors.End.y - openPaths[j].Start.y, 2)) < 0.001)
                        {
                            combainedInd.Add(i);
                            combainedInd.Add(j);
                            vectors.AppendVertices(openPaths[j].Vertices);
                            break;
                        }
                        if (Math.Sqrt(Math.Pow(vectors.End.x - openPaths[j].End.x, 2) + Math.Pow(vectors.End.y - openPaths[j].End.y, 2)) < 0.001)
                        {
                            combainedInd.Add(i);
                            combainedInd.Add(j);
                            vectors.AppendVertices(openPaths[j].Vertices.Reverse());
                            break;
                        }
                    }
                }

                if (Math.Sqrt(Math.Pow(vectors.Start.x - vectors.End.x, 2) + Math.Pow(vectors.Start.y - vectors.End.y, 2)) < 0.001)
                {
                    combinedPaths.Add(vectors);
                }
                
            }
            foreach(var p in combinedPaths)
            {
                closedPaths.Add(p);
            }
            return new Tuple<bool, List<PolyLine2d>>(true, closedPaths);
        }
        public static Tuple<bool , string> ProcessPath(ref List<PrintTaskModels> TMS)
        {
            string resString = "PathResult:";
            foreach (var tm in TMS)
            {
                if (!tm.isSolidorPath)
                {
                    foreach (var slice in tm.pss.Slices)
                    {

                        List<PolyLine2d> solidPaths = new List<PolyLine2d>();
                        List<PolyLine2d> solidPathsInHole = new List<PolyLine2d>();
                        List<int> insideCounts = new List<int>();
                        List<PolyLine2d> holePaths = new List<PolyLine2d>();
                        var combineRes = CombinePaths(slice.Paths);
                        if (combineRes.Item1)
                        {
                            slice.Paths = combineRes.Item2;
                        }
                        else
                        {
                            //是否可以重新微调平面
                            //单独获取
                            MeshPlanarSlicer tempMPS = new MeshPlanarSlicer();
                            tempMPS.AddMesh(tm.mesh);
                            int retry = 0;
                            bool isRetrySuccess = false;
                            double retryOffset = 0;
                            while (retry++ < 10)
                            {
                                if (retry < 6)
                                {
                                    retryOffset = -0.002 * retry;
                                }
                                else
                                {
                                    retryOffset = 0.002 * (retry - 5);
                                }
                                var sliceRes = tempMPS.ComputeZ(slice.Z + retryOffset);
                                var combineRes2 = CombinePaths(sliceRes.ClippedPaths);
                                if (combineRes2.Item1)
                                {
                                    resString += $"{tm.fileName} reslice at {(slice.Z + retryOffset).ToString("0.000mm")}√;";
                                    slice.Paths = combineRes2.Item2;
                                    isRetrySuccess = true;
                                    break;
                                }
                                else
                                {
                                    resString += $"{tm.fileName} reslice at {(slice.Z + retryOffset).ToString("0.000mm")}×;";
                                }
                                
                            }
                            if (!isRetrySuccess)
                            {
                                resString += $"{tm.fileName} fail reslice at {(slice.Z + retryOffset).ToString("0.000mm")};";
                                return new Tuple<bool, string>(false, resString);
                            }
                        }
                        for (int i = 0; i < slice.Paths.Count(); i++)
                        {
                            PolyLine2d currPath = slice.Paths[i];
                            VertexStorage vertexStorageSolid = new VertexStorage();
                            int insideCount = 0;
                            for (int j = 0; j < slice.Paths.Count(); j++)
                            {
                                if (i == j) { continue; }

                                if (isPointInsideVV(currPath.Vertices, slice.Paths[j].Vertices))
                                {
                                    insideCount++;
                                }
                            }
                            insideCounts.Add(insideCount);
                            if (insideCount == 0)
                            {
                                solidPaths.Add(currPath);

                            }
                            else if (insideCount == 1)
                            {
                                holePaths.Add(currPath);
                            }
                            else
                            {
                                solidPathsInHole.Add(currPath);
                            }
                        }
                        tm.solidPathsList.Add(solidPaths);
                        tm.solidPathsInHoleList.Add(solidPathsInHole);
                        tm.holePathsList.Add(holePaths);
                    }
                }
            }
            resString += $"process path success!";
            return new Tuple<bool, string>(true, resString);
        }
        public static T DeepCopyByBinary<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }

        public static void GetFrontTeethSupport(Mat mat, ref List<SupportModels> supportModels, int currind, SliceParas sliceParas)
        {

            //x = 340 / 460
            int leftX = 340;
            int rightX = 460;
            int leftmin = 800;
            int leftmax = 0;
            int rightmin = 800;
            int rightmax = 0;
            for (int i = 0; i < 360; i++)
            {
                if (mat.At<byte>(i, leftX) > 30)
                {
                    if (i > leftmax) { leftmax = i; }
                    if (i < leftmin) { leftmin = i; }
                }
                if (mat.At<byte>(i, rightX) > 30)
                {
                    if (i > rightmax) { rightmax = i; }
                    if (i < rightmin) { rightmin = i; }
                }
            }
            //Cv2.ImWrite("test.bmp", mat);
            supportModels.Add(
                        new SupportModels()
                        {
                            centerX = (leftX - 400) / sliceParas.drawScaleX,
                            CenterY = ((leftmin + leftmax) / 2 - 400) / sliceParas.drawScaleY,
                            isIsland = true,
                            lrORud = false,
                            startLayer = currind
                        });
            supportModels.Add(
                        new SupportModels()
                        {
                            centerX = (rightX - 400) / sliceParas.drawScaleX,
                            CenterY = ((rightmin + rightmax) / 2 - 400) / sliceParas.drawScaleY,
                            isIsland = true,
                            lrORud = false,
                            startLayer = currind
                        });
        }
        public static void GetSupports_OpenCV(ref List<PrintTaskModels> TMS, List<SliceParas> sliceParas, ObservableCollection<PartModel> parts)
        {
            bool DebugSupportStart = false;//是否可视化查找到的支撑点
            foreach (var part in parts)
            {
                if (part.IsToPrint)
                {
                    var tm = TMS[part.index];
                    if (tm.isSolidorPath)
                    {

                        Mat prev_Image = new Mat();
                        int currind = 0;
                        foreach (var slice in tm.pss.Slices)
                        {
                            if (currind < 21) { currind++; continue; }
                            Mat testImage = new Mat(800, 800, MatType.CV_8U, Scalar.Black);
                            int currSolidIndex = 0;
                            foreach (var solid in slice.Solids)
                            {
                                bool check = false;
                                for (int i = 0; i < currSolidIndex; i++)
                                {
                                    if (isPointInside(solid.Outer[0].x, solid.Outer[0].y, slice.Solids[i].Outer.Vertices))
                                    {
                                        check = true;
                                        if (DebugSupportStart)
                                        {
                                            //绘制两个轮廓
                                            //当前轮廓为白，被判断者为红
                                            ImageBuffer debugImage = new ImageBuffer(800, 800, 32, new BlenderBGRA());
                                            DrawOriPath(ref debugImage, slice.Solids[i].Outer.Vertices, ColorF.rgba_pre(1, 1, 1));
                                            DrawOriPath(ref debugImage, solid.Outer.Vertices, ColorF.rgba_pre(1, 0, 0));
                                            //ImageProcessTool.SaveBitmapImageIntoFile(ImageProcessTool.ImageBuffer_to_BitmapImage(debugImage), "debug_find.bmp");
                                        }
                                    }
                                }
                                currSolidIndex++;

                                if (check)
                                {
                                    double centx = 0;
                                    double centy = 0;
                                    foreach (var v in solid.Outer.Vertices)
                                    {
                                        centx += v.x;
                                        centy += v.y;
                                    }
                                    centx /= solid.Outer.Vertices.Count();
                                    centy /= solid.Outer.Vertices.Count();
                                    bool lrOrUd = true;
                                    if (Math.Abs(centx) < 8) { lrOrUd = false; }
                                    if (IsNewSupport(centx, centy, sliceParas[part.printInWhitchProjector].layerThicness * (currind - 2 + 0.5), tm.supportModelsList))
                                    {
                                        tm.supportModelsList.Add(
                                        new SupportModels()
                                        {
                                            centerX = centx,
                                            CenterY = centy,
                                            isIsland = true,
                                            lrORud = lrOrUd,
                                            startLayer = currind - 2
                                        });
                                    }
                                }
                                Draw_OpenCV(ref testImage, sliceParas[part.printInWhitchProjector].drawScaleX, sliceParas[part.printInWhitchProjector].drawScaleY, solid.Outer.Vertices,
                                    400 / sliceParas[part.printInWhitchProjector].drawScaleX, 400 / sliceParas[part.printInWhitchProjector].drawScaleY, false);

                                foreach (var hole in solid.Holes)
                                {
                                    Draw_OpenCV(ref testImage, sliceParas[part.printInWhitchProjector].drawScaleX, sliceParas[part.printInWhitchProjector].drawScaleY, hole.Vertices, 400 / sliceParas[part.printInWhitchProjector].drawScaleX, 400 / sliceParas[part.printInWhitchProjector].drawScaleY, true);
                                }


                            }
                            //MemoryStream s1 = new MemoryStream();
                            //ImageIO.SaveImageSream(s1, testImage);
                            //Bitmap image = new Bitmap(s1);
                            //image.Save($"{currind}.bmp");
                            if (currind == 35&& sliceParas[part.printInWhitchProjector].isAlignerMold)
                            {
                                GetFrontTeethSupport(testImage, ref tm.supportModelsList, currind, sliceParas[part.printInWhitchProjector]);
                            }

                            currind++;
                            if (currind > 23)
                            {
                                Mat resLapout = GetLapout(prev_Image, testImage, ref tm.supportModelsList, currind, sliceParas[part.printInWhitchProjector]);
                                resLapout.Dispose();
                                //Mat resErode = new OpenCvSharp.Mat();
                                //Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(7, 7), new OpenCvSharp.Point(-1, -1));
                                //Cv2.Erode(res, resErode, element);
                                //resErode = resErode + 0.1 * prev_mat;
                                //OpenCvSharp.Cv2.Absdiff(mat, prev_mat, res);
                                //mat.SaveImage($"{currind}.bmp");

                                //Mat save = (resLapout + 0.1 * mat);
                                //save.SaveImage($"{currind}-.bmp");
                            }

                            prev_Image = testImage.Clone();
                            testImage.Dispose();
                            //对两张图片进行减运算
                            //对减过的图片进行腐蚀
                            //提取腐蚀过的图片轮廓有则判定为需要支撑
                            //

                        }

                        prev_Image.Dispose();

                    }
                    else
                    {

                        Mat prev_Image = new Mat();
                        int currind = 0;
                        foreach (var slice in tm.pss.Slices)
                        {
                            if (currind < 21) { currind++; continue; }
                            Mat testImage = new Mat(800, 800, MatType.CV_8U, Scalar.Black);
                            foreach (var p in tm.solidPathsList[currind])
                            {
                                Draw_OpenCV(ref testImage, sliceParas[part.printInWhitchProjector].drawScaleX, sliceParas[part.printInWhitchProjector].drawScaleY, p.Vertices, 400 / sliceParas[part.printInWhitchProjector].drawScaleX, 400 / sliceParas[part.printInWhitchProjector].drawScaleY, false);

                            }
                            foreach (var p in tm.holePathsList[currind])
                            {
                                Draw_OpenCV(ref testImage, sliceParas[part.printInWhitchProjector].drawScaleX, sliceParas[part.printInWhitchProjector].drawScaleY, p.Vertices, 400 / sliceParas[part.printInWhitchProjector].drawScaleX, 400 / sliceParas[part.printInWhitchProjector].drawScaleY, true);

                            }
                            foreach (var p in tm.solidPathsInHoleList[currind])
                            {
                                double centx = 0;
                                double centy = 0;
                                foreach (var v in p.Vertices)
                                {
                                    centx += v.x;
                                    centy += v.y;
                                }
                                centx /= p.Vertices.Count();
                                centy /= p.Vertices.Count();

                                //if (DebugSupportStart)
                                //{
                                //    //绘制两个轮廓
                                //    //当前轮廓为白，被判断者为红
                                //    DrawOriPath(ref testImage, p.Vertices, ColorF.rgba_pre(1, 0, 0));
                                //    ImageProcessTool.SaveBitmapImageIntoFile(ImageProcessTool.ImageBuffer_to_BitmapImage(testImage), "debug_find_path.bmp");
                                //}


                                bool lrOrUd = true;
                                if (Math.Abs(centx) < 8) { lrOrUd = false; }
                                if (IsNewSupport(centx, centy, sliceParas[part.printInWhitchProjector].layerThicness * (currind - 2 + 0.5), tm.supportModelsList))
                                {
                                    tm.supportModelsList.Add(
                                    new SupportModels()
                                    {
                                        centerX = centx,
                                        CenterY = centy,
                                        isIsland = true,
                                        lrORud = lrOrUd,
                                        startLayer = currind - 2
                                    });
                                }

                                ///////connectIsland(ref testImage, (int)Math.Round(projectorConfig.PrintPixScale * (centx + Tx)), (int)Math.Round(projectorConfig.PrintPixScale * (centy + Ty)), true);
                                Draw_OpenCV(ref testImage, sliceParas[part.printInWhitchProjector].drawScaleX, sliceParas[part.printInWhitchProjector].drawScaleY, p.Vertices, 400 / sliceParas[part.printInWhitchProjector].drawScaleX, 400 / sliceParas[part.printInWhitchProjector].drawScaleY, false);
                            }
                            //MemoryStream s1 = new MemoryStream();
                            //ImageIO.SaveImageSream(s1, testImage);
                            //Bitmap image = new Bitmap(s1);
                            ////image.Save($"{currind}.bmp");
                            if (currind == 35&&sliceParas[part.printInWhitchProjector].isAlignerMold)
                            {
                                GetFrontTeethSupport(testImage, ref tm.supportModelsList, currind, sliceParas[part.printInWhitchProjector]);

                            }
                            currind++;
                            if (currind > 23)
                            {



                                Mat resLapout = GetLapout(prev_Image, testImage, ref tm.supportModelsList, currind, sliceParas[part.printInWhitchProjector]);
                                //Mat resErode = new OpenCvSharp.Mat();
                                //Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(7, 7), new OpenCvSharp.Point(-1, -1));
                                //Cv2.Erode(res, resErode, element);
                                //resErode = resErode + 0.1 * prev_mat;
                                //OpenCvSharp.Cv2.Absdiff(mat, prev_mat, res);
                                //mat.SaveImage($"{currind}.bmp");

                                //Mat save = (resLapout + 0.1 * mat);
                                //save.SaveImage($"{currind}-.bmp");
                                resLapout.Dispose();
                            }

                            prev_Image = testImage.Clone();
                            testImage.Dispose();
                        }

                        prev_Image.Dispose();
                    }
                }
            }

        }
        static bool IsNewSupport(double x, double y, double startHeight, List<SupportModels> supportModels)
        {
            if (y > 0.8 && y < 5.2 && startHeight < 2.8)
            {
                return false;
            }
            foreach (var s in supportModels)
            {
                if (Math.Sqrt(Math.Pow(x - s.centerX, 2) + Math.Pow(y - s.CenterY, 2)) < 2) { return false; }
            }
            return true;
        }
        static Mat GetLapout(Mat prevMat, Mat currMat, ref List<SupportModels> supportModels, int currind, SliceParas sliceParas)
        {
            Mat res = currMat - prevMat;
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(15, 15), new OpenCvSharp.Point(-1, -1));
            //Mat resErode = new OpenCvSharp.Mat();

            Cv2.Erode(res, res, element);

            //找轮廓
            OpenCvSharp.HierarchyIndex[] hierarchyIndex;
            Point[][] contours;

            Cv2.FindContours(res, out contours, out hierarchyIndex, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            if (contours.Length > 0)
            {
                //Mat res2 = currMat - 0.9 * prevMat;
                //Cv2.ImWrite("0-bwImage.bmp", res);
                //Cv2.ImWrite("0-res2.bmp", res2);
                //res2.Dispose();
                foreach (var c in contours)
                {
                    double centx = 0;
                    double centy = 0;
                    foreach (var p in c)
                    {
                        centx += (p.X - 400) / sliceParas.drawScaleX;
                        centy += (p.Y - 400) / sliceParas.drawScaleY;
                    }
                    centx /= c.Count();
                    centy /= c.Count();



                    bool lrOrUd = true;
                    if (Math.Abs(centx) < 8) { lrOrUd = false; }

                    if (IsNewSupport(centx, centy, sliceParas.layerThicness * (currind - 2 + 0.5), supportModels))
                    {
                        supportModels.Add(
                        new SupportModels()
                        {
                            centerX = centx,
                            CenterY = centy,
                            isIsland = false,
                            lrORud = lrOrUd,
                            startLayer = currind - 4
                        });
                    }

                }
            }
            //找中心
            return res;
        }

        public static bool isPointInside(double x, double y, ReadOnlyCollection<Vector2d> vs)
        {
            int j = 0, cnt = 0;
            for (int i = 0; i < vs.Count; i++)
            {
                j = (i == vs.Count - 1) ? 0 : j + 1;
                if ((vs[i].y != vs[j].y) && (((y >= vs[i].y) && (y < vs[j].y)) || ((y >= vs[j].y) && (y < vs[i].y))) && (x < (vs[j].x - vs[i].x) * (y - vs[i].y) / (vs[j].y - vs[i].y) + vs[i].x)) cnt++;
            }
            return (cnt % 2 > 0) ? true : false;
        }

        public static bool isPointInsideVV(ReadOnlyCollection<Vector2d> vs1, ReadOnlyCollection<Vector2d> vs)
        {
            if (vs1.Count > 5)
            {
                List<bool> res = new List<bool>();
                for (int vi = 0; vi < vs1.Count; vi += (int)Math.Floor((double)vs1.Count / 4.4))
                {
                    double x = vs1[vi].x;
                    double y = vs1[vi].y;
                    int j = 0, cnt = 0;
                    for (int i = 0; i < vs.Count; i++)
                    {
                        j = (i == vs.Count - 1) ? 0 : j + 1;
                        if ((vs[i].y != vs[j].y) && (((y >= vs[i].y) && (y < vs[j].y)) || ((y >= vs[j].y) && (y < vs[i].y))) && (x < (vs[j].x - vs[i].x) * (y - vs[i].y) / (vs[j].y - vs[i].y) + vs[i].x)) cnt++;
                    }
                    if (cnt % 2 > 0)
                    { res.Add(true); }
                    else
                    { res.Add(false); }
                }
                int tc = 0;
                foreach (var vi in res)
                {
                    if (vi) { tc++; }
                }
                if (tc > res.Count/2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                double x = vs1[0].x;
                double y = vs1[0].y;
                int j = 0, cnt = 0;
                for (int i = 0; i < vs.Count; i++)
                {
                    j = (i == vs.Count - 1) ? 0 : j + 1;
                    if ((vs[i].y != vs[j].y) && (((y >= vs[i].y) && (y < vs[j].y)) || ((y >= vs[j].y) && (y < vs[i].y))) && (x < (vs[j].x - vs[i].x) * (y - vs[i].y) / (vs[j].y - vs[i].y) + vs[i].x)) cnt++;
                }
                return (cnt % 2 > 0) ? true : false;
            }

        }


        public static void DrawPlatformHole_OpenCV(ref Mat img, SliceParas sliceParas, int fixedInd , Vector2d center, bool isFullImage = false,double rate = 0)
        {
            double startX = -10;
            double startY = -10;
            int rowNum = 68;
            int clumnNum = 45;
            double Yd = 0;
            Mat mask = new Mat(img.Rows, img.Cols, MatType.CV_8UC1,Scalar.White);
            

            for (int i = 0; i < clumnNum; i++)
            {
                if (i % 2 != 0)
                {
                    Yd = 3;
                }
                else
                {
                    Yd = 0;
                }
                for (int j = 0; j < rowNum; j++)
                {
                    double tempx = (startX + i * 5.5) + sliceParas.holeOffsetX;
                    double tempy = (startY + Yd + j * 6) + sliceParas.holeOffsetY;
                    double tempxR = Math.Cos(sliceParas.holeRz) * tempx - Math.Sin(sliceParas.holeRz) * tempy;
                    double tempyR = Math.Sin(sliceParas.holeRz) * tempx + Math.Cos(sliceParas.holeRz) * tempy;
                    MathNet.Numerics.LinearAlgebra.Double.DenseMatrix holeCoor = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(3, 1);
                    holeCoor[0, 0] = sliceParas.calibScaleFix * tempxR / sliceParas.projectorPixScaleX;
                    holeCoor[1, 0] = sliceParas.calibScaleFix * tempyR / sliceParas.projectorPixScaleY;
                    holeCoor[2, 0] = 1;
                    //MathNet.Numerics.LinearAlgebra.Double.DenseMatrix holePixCoor = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(3, 1);
                    //holePixCoor = (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)sliceParas.calibMartix.Inverse() * holeCoor;
                    double tempPixX = holeCoor[0, 0];
                    double tempPixY = sliceParas.height - holeCoor[1, 0];
                    int gray0 = (int)Math.Round(0.8*255 * rate);
                    int gray1 = (int)Math.Round(0.9*255 * rate);
                    int gray2 = (int)Math.Round(255 * rate);
                    if (isFullImage)
                    {
                        mask.Circle(new Point(tempPixX, tempPixY), (int)(sliceParas.holeComps[fixedInd] / ((sliceParas.projectorPixScaleX+ sliceParas.projectorPixScaleY)/2)), gray0, -1);
                    }
                    else
                    {
                        double CX = center.x;
                        double CY = sliceParas.height - center.y;
                        if (tempPixX > CX + 400 || tempPixX < CX - 400 || tempPixY > CY + 400 || tempPixY < CY - 400)
                        { continue; }
                        mask.Circle(new Point((int)Math.Round(tempPixX - CX + 400), (int)Math.Round(tempPixY - CY + 400)), (int)(sliceParas.holeComps[fixedInd] / ((sliceParas.projectorPixScaleX + sliceParas.projectorPixScaleY) / 2)), gray0, -1);
                        mask.Circle(new Point((int)Math.Round(tempPixX - CX + 400), (int)Math.Round(tempPixY - CY + 400)), (int)(0.9*sliceParas.holeComps[fixedInd] / ((sliceParas.projectorPixScaleX + sliceParas.projectorPixScaleY) / 2)), gray1, -1);
                        mask.Circle(new Point((int)Math.Round(tempPixX - CX + 400), (int)Math.Round(tempPixY - CY + 400)), (int)(0.8*sliceParas.holeComps[fixedInd] / ((sliceParas.projectorPixScaleX + sliceParas.projectorPixScaleY) / 2)), gray2, -1);
                    }

                }
            }
            Mat res = new Mat(img.Rows, img.Cols, MatType.CV_8U, Scalar.Black);
            AddByMask(ref res,img, mask);
            img = res;
        }
        private static void AddByMask(ref Mat outMat , Mat img, Mat mask,int offsetX = 0 ,int offsetY = 0, double rate = 1)
        {
            
            for (int i = 0; i < img.Cols; i++)
            {
                for (int j = 0; j < img.Rows; j++)
                {
                    if (i + offsetX < 0 || i + offsetX > outMat.Cols) { continue; }
                    if (j + offsetY < 0 || j + offsetY > outMat.Rows) { continue; }
                    byte color = img.At<Vec3b>(j , i)[0];
                    byte maskF = mask.At<Vec3b>(j, i)[0];
                    if (maskF > 5)
                    {
                        byte gray = (byte)(rate * color * maskF / 255);
                        outMat.At<byte>(j + offsetY, i + offsetX) = gray;
                    }
                }
            }

        }
        

        public static void BaseCaliTest(double scale)
        {
            int startX = 180;
            int startY = 120;
            int diffX = 600;
            int diffY = 600;
            int rowNum = 7;
            int clumnNum = 4;
            int imgCount = 0;
            List<List<double>> zss = new List<List<double>>();
            zss.Add(new List<double>() { 2.26, 2.22, 2.16, 2.08, 2.04, 1.96, 1.92 });
            zss.Add(new List<double>() { 2.2, 2.18, 2.16, 2.10, 2.04, 1.94, 1.88 });
            zss.Add(new List<double>() { 2.3, 2.26, 2.18, 2.10, 2.06, 1.94, 1.86 });
            zss.Add(new List<double>() { 2.34, 2.28, 2.20, 2.16, 2.12, 1.92, 1.88 });
            //save to json

            double zmax = 2.34;
            for (int layer = 0; layer < 30; layer++)
            {
                ImageBuffer img = new ImageBuffer(2160, 3840, 32, new BlenderBGRA());
                img.NewGraphics2D().FillRectangle(0, 0, 2160, 3840, ColorF.Black);
                for (int i = 0; i < clumnNum; i++)
                {
                    for (int j = 0; j < rowNum; j++)
                    {
                        ImageBuffer img2 = new ImageBuffer(2160, 3840, 32, new BlenderBGRA());
                        int currComp = (int)Math.Round((zmax - zss[i][j]) / 0.12);

                        int fixedlayer = layer - currComp;
                        if (fixedlayer < 20 && fixedlayer > -1)
                        {
                            img2.NewGraphics2D().FillRectangle(startX + i * diffX - 60, startY + j * diffY - 60, startX + i * diffX + 60, startY + j * diffY + 60, ColorF.White);
                            if (fixedlayer < 1)
                            {
                                //DrawPlatformHole(ref img2, projectorConfig, projectorConfig.HoleOffsetX, projectorConfig.HoleOffsetY, new Vector2d((startX + i * diffX - 60) / projectorConfig.PrintPixScale, (startY + j * diffY - 60) / projectorConfig.PrintPixScale), new Vector2d((startX + i * diffX + 60) / projectorConfig.PrintPixScale, (startY + j * diffY + 60) / projectorConfig.PrintPixScale));
                            }
                        }
                        else if (fixedlayer < 27 && fixedlayer > -1)
                        {
                            img2.NewGraphics2D().DrawString($"{i + 1}-{j + 1}", new MatterHackers.VectorMath.Vector2(startX + i * diffX, startY + j * diffY), 40, MatterHackers.Agg.Font.Justification.Center, MatterHackers.Agg.Font.Baseline.Text, MatterHackers.Agg.Color.White);
                        }
                        //imgCount++;
                        //AddImgFull(ref img, img2, new Vector2d(startX + i * diffX - 60, startY + j * diffY - 60), new Vector2d(startX + i * diffX + 60, startY + j * diffY + 60), projectorConfig);
                    }
                }
                //img.FlipY();

                ImageIO.SaveImageData(string.Format(@"C:\Users\xiaof\Documents\PrinterDev\slices\{0}.png", imgCount++), img);
            }

        }
        static void DrawBox(ref ImageBuffer img, int diskCx, int diskCy, int radius)
        {
            for (int i = diskCx - radius; i < diskCx + radius + 1; i++)
            {
                for (int j = diskCy - radius; j < diskCy + radius + 1; j++)
                {
                    img.SetPixel(i, j, new MatterHackers.Agg.Color(1, 1, 1));
                }
            }

        }

        public static bool CheckMesh(string path)
        {
            DMesh3 mesh = StandardMeshReader.ReadMesh(path);
            return mesh.IsClosed();
        }


    }

    class ImageProcessTool
    {
        public static BitmapImage ImageBuffer_to_BitmapImage(ImageBuffer imageBuffer)
        {
            MemoryStream s = new MemoryStream();
            ImageIO.SaveImageSream(s, imageBuffer);
            BitmapImage bm = new BitmapImage();
            bm.BeginInit();
            bm.StreamSource = s;
            bm.EndInit();
            return bm;
        }
        public static void SaveBitmapImageIntoFile(BitmapImage bitmapImage, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        public static Bitmap ImageBufferToBitmap(ImageBuffer imagebuffer)
        {

            MemoryStream s1 = new MemoryStream();
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            ImageIO.SaveImageSream(s1, imagebuffer);
            sw.Stop();
            double time = sw.Elapsed.TotalSeconds;
            Bitmap image = new Bitmap(s1);

            return image;
        }

        public static Mat ImageBufferToMat(ImageBuffer imagebuffer)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(ImageBufferToBitmap(imagebuffer));

        }
        public static BitmapImage MatToBitmap(Mat matimg)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new MemoryStream(MatToByteArray(matimg));
            bmp.EndInit();
            return bmp;
        }
        public static byte[] MatToByteArray(Mat mat)
        {
            List<byte> lstbyte = new List<byte>();
            byte[] btArr = lstbyte.ToArray();
            int[] param = new int[2] { 1, 20 };
            Cv2.ImEncode(".bmp", mat, out btArr, param);
            return btArr;
        }
    }
}
