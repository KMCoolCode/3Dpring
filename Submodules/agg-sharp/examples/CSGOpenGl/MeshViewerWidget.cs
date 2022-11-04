﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;

using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.Agg.Transform;
using MatterHackers.Agg.VertexSource;
using MatterHackers.VectorMath;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.RasterizerScanline;
using MatterHackers.Agg.OpenGlGui;
using MatterHackers.PolygonMesh;
using MatterHackers.RenderOpenGl;
using MatterHackers.PolygonMesh.Processors;

namespace MatterHackers.MeshVisualizer
{
    public class MeshViewerWidget : GuiWidget
    {
        BackgroundWorker backgroundWorker = null;

        public RGBA_Bytes PartColor { get; set; }

        public bool RenderBed
        {
            get;
            set;
        }

        public bool ShowWireFrame
        {
            get;
            set;
        }

        double partScale;
        ImageBuffer bedCentimeterGridImage;
        TextWidget meshLoadingStateInfoText;
        TrackballTumbleWidget trackballTumbleWidget;
        public TrackballTumbleWidget TrackballTumbleWidget
        {
            get
            {
                return trackballTumbleWidget;
            }
        }

        List<Mesh> meshsToRender = new List<Mesh>();
        public List<Mesh> Meshs { get { return meshsToRender; } }

        Mesh printerBed = null;

        public MeshViewerWidget(double bedXSize, double bedYSize, double scale)
        {
            ShowWireFrame = false;
            RenderBed = true;
            PartColor = RGBA_Bytes.White;

            this.partScale = scale;
            trackballTumbleWidget = new TrackballTumbleWidget();
            trackballTumbleWidget.DrawRotationHelperCircle = false;
            trackballTumbleWidget.DrawGlContent += trackballTumbleWidget_DrawGlContent;

            AddChild(trackballTumbleWidget);

            CreateBedGridImage((int)(bedXSize / 10), (int)(bedYSize / 10));

            printerBed = PlatonicSolids.CreateCube(bedXSize, bedYSize, 2);
            Face face = printerBed.Faces[0];
            {
                FaceData faceData = new FaceData();
                faceData.Textures.Add(bedCentimeterGridImage);
                face.Data = faceData;
                foreach (FaceEdge faceEdge in face.FaceEdgeIterator())
                {
                    FaceEdgeData edgeUV = new FaceEdgeData();
                    edgeUV.TextureUV.Add(new Vector2((bedXSize / 2 + faceEdge.vertex.Position.x) / bedXSize,
                        (bedYSize / 2 + faceEdge.vertex.Position.y) / bedYSize));
                    faceEdge.Data = edgeUV;
                }
            }

            foreach (Vertex vertex in printerBed.Vertices)
            {
                vertex.Position = vertex.Position - new Vector3(0, 0, 1);
            }

            trackballTumbleWidget.AnchorAll();
        }

        private void PlaceMeshOnBed(int index)
        {
            AxisAlignedBoundingBox bounds = meshsToRender[index].GetAxisAlignedBoundingBox();
            Vector3 boundsCenter = (bounds.maxXYZ + bounds.minXYZ) / 2;
            meshsToRender[index].Translate(-boundsCenter + new Vector3(0, 0, bounds.ZSize / 2 + .02));
        }

        public override void OnClosed(EventArgs e)
        {
            if (backgroundWorker != null)
            {
                backgroundWorker.CancelAsync();
            }
            base.OnClosed(e);
        }

        void trackballTumbleWidget_DrawGlContent(object sender, EventArgs e)
        {
            foreach(Mesh meshToRender in meshsToRender)
            {
                if (ShowWireFrame)
                {
                    RenderMeshToGl.Render(meshToRender, PartColor, true);
                }
                else
                {
                    RenderMeshToGl.Render(meshToRender, PartColor);
                }
            }

            if (RenderBed && meshsToRender.Count > 0)
            {
                RGBA_Floats bedColor = new RGBA_Floats(.8, .8, .8, .5);
                RenderMeshToGl.Render(printerBed, bedColor);
            }
        }

        public void LoadMesh(string meshPathAndFileName)
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            bool loadingMeshFile = false;
            switch(Path.GetExtension(meshPathAndFileName).ToUpper())
            {
                case ".STL":
                    {
                        StlProcessing stlLoader = new StlProcessing();
                        stlLoader.LoadInBackground(backgroundWorker, meshPathAndFileName);
                        loadingMeshFile = true;
                    }
                    break;

                case ".AMF":
                    {
                        AmfProcessing amfLoader = new AmfProcessing();
                        amfLoader.LoadInBackground(backgroundWorker, meshPathAndFileName);
                        loadingMeshFile = true;
                    }
                    break;

                default:
                    loadingMeshFile = false;
                    break;
            }

            if (loadingMeshFile)
            {
                meshLoadingStateInfoText = new TextWidget("Loading Mesh...");
                meshLoadingStateInfoText.HAnchor = HAnchor.ParentCenter;
                meshLoadingStateInfoText.VAnchor = VAnchor.ParentCenter;
                meshLoadingStateInfoText.AutoExpandBoundsToText = true;

                GuiWidget labelContainer = new GuiWidget();
                labelContainer.AnchorAll();
                labelContainer.AddChild(meshLoadingStateInfoText);
                labelContainer.Selectable = false;

                this.AddChild(labelContainer);
            }
            else
            {
                TextWidget no3DView = new TextWidget(string.Format("No 3D view for this file type '{0}'.", Path.GetExtension(meshPathAndFileName).ToUpper()));
                no3DView.Margin = new BorderDouble(0, 0, 0, 0);
                no3DView.VAnchor = Agg.UI.VAnchor.ParentCenter;
                no3DView.HAnchor = Agg.UI.HAnchor.ParentCenter;
                this.AddChild(no3DView);
            }
        }

        public void SetMeshAfterLoad(Mesh loadedMesh)
        {
            meshsToRender.Clear();

            if (loadedMesh == null)
            {
                TextWidget no3DView = new TextWidget(string.Format("No 3D view for this file."));
                no3DView.Margin = new BorderDouble(0, 0, 0, 0);
                no3DView.VAnchor = Agg.UI.VAnchor.ParentCenter;
                no3DView.HAnchor = Agg.UI.HAnchor.ParentCenter;
                this.AddChild(no3DView);
            }
            else
            {
                meshsToRender.Add(loadedMesh);

                trackballTumbleWidget.TrackBallController = new TrackBallController();
                trackballTumbleWidget.OnBoundsChanged(null);
                trackballTumbleWidget.TrackBallController.Scale = .03;
                trackballTumbleWidget.TrackBallController.Rotate(Quaternion.FromEulerAngles(new Vector3(0, 0, MathHelper.Tau / 16)));
                trackballTumbleWidget.TrackBallController.Rotate(Quaternion.FromEulerAngles(new Vector3(-MathHelper.Tau * .19, 0, 0)));
                if (partScale != 1)
                {
                    foreach (Vertex vertex in meshsToRender[0].Vertices)
                    {
                        vertex.Position = vertex.Position * partScale;
                    }
                }

                PlaceMeshOnBed(0);
            }
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetMeshAfterLoad((Mesh)e.Result);
            meshLoadingStateInfoText.Text = "";
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            meshLoadingStateInfoText.Text = string.Format("Loading Mesh {0}%...", e.ProgressPercentage);
        }

        public override void OnMouseDown(MouseEventArgs mouseEvent)
        {
            base.OnMouseDown(mouseEvent);

            if (trackballTumbleWidget.MouseCaptured)
            {
                trackballTumbleWidget.DrawRotationHelperCircle = true;
            }
        }

        public override void OnMouseUp(MouseEventArgs mouseEvent)
        {
            trackballTumbleWidget.DrawRotationHelperCircle = false;
            Invalidate();

            base.OnMouseUp(mouseEvent);
        }

        void CreateBedGridImage(int linesInX, int linesInY)
        {
            Vector2 bedImageCentimeters = new Vector2(linesInX, linesInY);
            bedCentimeterGridImage = new ImageBuffer(1024, 1024, 32, new BlenderBGRA());
            Graphics2D graphics2D = bedCentimeterGridImage.NewGraphics2D();
            graphics2D.Clear(RGBA_Bytes.White);
            {
                double lineDist = bedCentimeterGridImage.Width / (double)linesInX;

                int count = 1;
                int pointSize = 20;
                graphics2D.DrawString(count.ToString(), 0, 0, pointSize);
                for (double linePos = lineDist; linePos < bedCentimeterGridImage.Width; linePos += lineDist)
                {
                    count++;
                    int linePosInt = (int)linePos;
                    graphics2D.Line(linePosInt, 0, linePosInt, bedCentimeterGridImage.Height, RGBA_Bytes.Black);
                    graphics2D.DrawString(count.ToString(), linePos, 0, pointSize);
                }
            }
            {
                double lineDist = bedCentimeterGridImage.Height / (double)linesInY;

                int count = 1;
                int pointSize = 20;
                for (double linePos = lineDist; linePos < bedCentimeterGridImage.Height; linePos += lineDist)
                {
                    count++;
                    int linePosInt = (int)linePos;
                    graphics2D.Line(0, linePosInt, bedCentimeterGridImage.Height, linePosInt, RGBA_Bytes.Black);
                    graphics2D.DrawString(count.ToString(), 0, linePos, pointSize);
                }
            }
        }
    }
}

