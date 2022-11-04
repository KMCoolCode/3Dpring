using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AngelModelPreprocess
{

    public class ModelPreprocesser
    {
        public static byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);//高8位
            src[0] = (byte)(value & 0xFF);//低位
            return src;
        }
        public class STLFile
        {
            public double Zmax { get; set; }
            public double Zmin { get; set; }
            public double Ymax { get; set; }
            public double Ymin { get; set; }
            public double Xmax { get; set; }
            public double Xmin { get; set; }
            public STLFile()
            {
                _triangles = new List<Triangle>();
                Zmax = -10000;
                Zmin = 10000;
                Ymax = -10000;
                Ymin = 10000;
                Xmax = -10000;
                Xmin = 10000;
            }

            public string Name { get; set; }

            private List<Triangle> _triangles;

            public List<Triangle> Triangles
            {
                get { return _triangles; }
            }
            public void GetRange()
            {
                //get the xyz max and min wile load the tringles
                foreach (Triangle tr in Triangles)
                {
                    double tempXmax = Math.Max(Math.Max(tr.Vertex1.X, tr.Vertex2.X), tr.Vertex3.X);
                    double tempXmin = Math.Min(Math.Min(tr.Vertex1.X, tr.Vertex2.X), tr.Vertex3.X);
                    double tempYmax = Math.Max(Math.Max(tr.Vertex1.Y, tr.Vertex2.Y), tr.Vertex3.Y);
                    double tempYmin = Math.Min(Math.Min(tr.Vertex1.Y, tr.Vertex2.Y), tr.Vertex3.Y);
                    double tempZmax = Math.Max(Math.Max(tr.Vertex1.Z, tr.Vertex2.Z), tr.Vertex3.Z);
                    double tempZmin = Math.Min(Math.Min(tr.Vertex1.Z, tr.Vertex2.Z), tr.Vertex3.Z);
                    if (tempXmax > Xmax) { Xmax = tempXmax; }
                    if (tempYmax > Ymax) { Ymax = tempYmax; }
                    if (tempZmax > Zmax) { Zmax = tempZmax; }
                    if (tempXmin < Xmin) { Xmin = tempXmin; }
                    if (tempYmin < Ymin) { Ymin = tempYmin; }
                    if (tempZmin < Zmin) { Zmin = tempZmin; }
                }
            }
            public void LoadBinary(string path)
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                {
                    byte[] data;
                    int count;

                    data = reader.ReadBytes(80);
                    count = (int)reader.ReadUInt32();

                    if (reader.BaseStream.Length != count * 50 + 84)
                        throw new InvalidDataException("STL文件长度无效");

                   

                    Name = Encoding.Default.GetString(data);

                    for (int i = 0; i < count; i++)
                    {
                        Triangles.Add(Triangle.Read(reader));
                    }

                    
                }
                GetRange();
            }
            public void LoadBinaryStream(Stream stream)
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] data;
                    int count;

                    data = reader.ReadBytes(80);
                    count = (int)reader.ReadUInt32();

                    if (reader.BaseStream.Length != count * 50 + 84)
                        throw new InvalidDataException("STL文件长度无效");

                   

                    Name = Encoding.Default.GetString(data);

                    for (int i = 0; i < count; i++)
                    {
                        Triangles.Add(Triangle.Read(reader));
                    }

                    GetRange();
                }
            }
            public void SaveBinary(string path)
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                bw.Write(Encoding.UTF8.GetBytes("                    "));
                bw.Write(Encoding.UTF8.GetBytes("                    "));
                bw.Write(Encoding.UTF8.GetBytes("                    "));
                bw.Write(Encoding.UTF8.GetBytes("                    "));
                bw.Write(Triangles.Count);
                foreach (Triangle tr in Triangles)
                {
                    bw.Write(tr.NormalVector.X);
                    bw.Write(tr.NormalVector.Y);
                    bw.Write(tr.NormalVector.Z);
                    bw.Write(tr.Vertex1.X);
                    bw.Write(tr.Vertex1.Y);
                    bw.Write(tr.Vertex1.Z);
                    bw.Write(tr.Vertex2.X);
                    bw.Write(tr.Vertex2.Y);
                    bw.Write(tr.Vertex2.Z);
                    bw.Write(tr.Vertex3.X);
                    bw.Write(tr.Vertex3.Y);
                    bw.Write(tr.Vertex3.Z);
                    string s = "1111111111111111";
                    bw.Write(Convert.ToInt16(s, 2));
                }

                bw.Close();
                fs.Close();
            }
            public void SaveBinaryStream(Stream stream)
            {

                byte[] bytes = Encoding.UTF8.GetBytes("lxh                                                                         test");
                int count = bytes.Length;
                //Console.WriteLine($"the length of blanks is {count}");
                stream.Write(bytes, 0, count);
                //Console.WriteLine($"file.Triangles.Count is {file.Triangles.Count}");
                byte[] bytes2 = intToBytes(Triangles.Count);
                count = bytes2.Length;
                //Console.WriteLine($"the length of nums is {count}");
                int int32 = BitConverter.ToInt32(bytes2, 0);
                string hexStr = "0x" + Convert.ToString(int32, 16);
                //Console.WriteLine($"the byte of nums is {int32}");
                stream.Write(bytes2, 0, count);

                foreach (Triangle tr in Triangles)
                {
                    byte[] buffer = BitConverter.GetBytes(tr.NormalVector.X);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.NormalVector.Y);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.NormalVector.Z);
                    stream.Write(buffer, 0, 4);

                    buffer = BitConverter.GetBytes(tr.Vertex1.X);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.Vertex1.Y);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.Vertex1.Z);
                    stream.Write(buffer, 0, 4);

                    buffer = BitConverter.GetBytes(tr.Vertex2.X);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.Vertex2.Y);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.Vertex2.Z);
                    stream.Write(buffer, 0, 4);

                    buffer = BitConverter.GetBytes(tr.Vertex3.X);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.Vertex3.Y);
                    stream.Write(buffer, 0, 4);
                    buffer = BitConverter.GetBytes(tr.Vertex3.Z);
                    stream.Write(buffer, 0, 4);

                    byte[] endbuffer = { 16, 254 };
                    stream.Write(endbuffer, 0, 2);

                }
                stream.Flush();
            }
        }
        public class OBJFile
        {
            public double Zmax { get; set; }
            public double Zmin { get; set; }
            public double Ymax { get; set; }
            public double Ymin { get; set; }
            public double Xmax { get; set; }
            public double Xmin { get; set; }
            public OBJFile()
            {
                _triangles = new List<TriangleInd>();
                _vertices = new List<Coordinate>();
                Zmax = -10000;
                Zmin = 10000;
                Ymax = -10000;
                Ymin = 10000;
                Xmax = -10000;
                Xmin = 10000;
            }

            public string Name { get; set; }

            private List<TriangleInd> _triangles;

            public List<TriangleInd> Triangles
            {
                get { return _triangles; }
            }
            private List<Coordinate> _vertices;

            public List<Coordinate> Vertices
            {
                get { return _vertices; }
            }
            public void GetRange()
            {
                //get the xyz max and min wile load the tringles
                foreach (Coordinate co in Vertices)
                {
                    if (co.X > Xmax) { Xmax = co.X; }
                    if (co.Y > Ymax) { Ymax = co.Y; }
                    if (co.Z > Zmax) { Zmax = co.Z; }
                    if (co.X < Xmin) { Xmin = co.X; }
                    if (co.Y < Ymin) { Ymin = co.Y; }
                    if (co.Z < Zmin) { Zmin = co.Z; }
                }
            }
            public void LoadObjBinaryStream(Stream stream)
            {
                
                using (StreamReader sr = new StreamReader(stream))
                {
                    string lineStr;
                    while ((lineStr = sr.ReadLine()) != null)
                    {
                        string[] sStrs = lineStr.Split(' ');
                        if (sStrs[0] == "v")
                        {
                            Vertices.Add(new Coordinate()
                            {
                                X = float.Parse(sStrs[1]),
                                Y = float.Parse(sStrs[2]),
                                Z = float.Parse(sStrs[3])
                            });
                        }
                        else
                        {
                            Triangles.Add(new TriangleInd()
                            {
                                Vertex1 = int.Parse(sStrs[1]) - 1,
                                Vertex2 = int.Parse(sStrs[2]) - 1,
                                Vertex3 = int.Parse(sStrs[3]) - 1
                            });
                        }
                    } 
                }
                GetRange();
            }
            public void LoadAmfBinaryStream(Stream stream)
            {
                
                XmlDocument doc = new XmlDocument();
                //DateTime beforeDT1 = System.DateTime.Now;
                try
                {
                    doc.Load(stream);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                
                //DateTime afterDT1 = System.DateTime.Now;
                //TimeSpan ts1 = afterDT1.Subtract(beforeDT1);
                //string test = ts1.TotalMilliseconds.ToString();
                XmlNode amf = doc.SelectSingleNode("amf");
                XmlNode obj = amf.SelectSingleNode("object");
                XmlNode mesh = obj.SelectSingleNode("mesh");
                XmlNode vertices = mesh.SelectSingleNode("vertices");
                XmlNode volume = mesh.SelectSingleNode("volume");
                XmlNodeList vList = vertices.ChildNodes;
                
                
                foreach (XmlElement xn1 in vList)
                {
                    //XmlElement xxe = (XmlElement)xn1;
                    XmlElement coordinates = (XmlElement)xn1.SelectSingleNode("coordinates");
                    Vertices.Add(new Coordinate()
                    {
                        X = float.Parse(coordinates.FirstChild.InnerText),
                        Y = float.Parse(coordinates.ChildNodes[1].InnerText),
                        Z = float.Parse(coordinates.LastChild.InnerText)
                    });
                }
                XmlNodeList fList = volume.ChildNodes;
                foreach (XmlElement fe in fList)
                {
                    //XmlElement fe = (XmlElement)xn1;
                    Triangles.Add(new TriangleInd()
                    {
                        Vertex1 = int.Parse(fe.FirstChild.InnerText),
                        Vertex2 = int.Parse(fe.ChildNodes[1].InnerText),
                        Vertex3 = int.Parse(fe.LastChild.InnerText)
                    });
                }
                GetRange();
                //TriangleInd testr = Triangles[Triangles.Count - 1];
                
            }

            public void LoadAmfBinaryStream2(Stream stream)
            {
                
                
                
                DateTime beforeDT1 = System.DateTime.Now;
                try
                {
                    // 加载XML文件
                    XDocument xElement = XDocument.Load(stream);
                    XElement amf = xElement.Root;
                    XElement obj = amf.Element("object");
                    XElement mesh = obj.Element("mesh");
                    XElement vertices = mesh.Element("vertices");
                    XElement volume = mesh.Element("volume");
                    IEnumerable<XElement> vList = vertices.Elements();


                    foreach (XElement xn1 in vList)
                    {
                        //XmlElement xxe = (XmlElement)xn1;
                        XElement coordinates = xn1.Element("coordinates"); 
                        
                        Vertices.Add(new Coordinate()
                        {
                            X = float.Parse(coordinates.Element("x").Value),
                            Y = float.Parse(coordinates.Element("y").Value),
                            Z = float.Parse(coordinates.Element("z").Value)
                        });
                    }
                    IEnumerable<XElement> fList = volume.Elements();
                    foreach (XElement fe in fList)
                    {
                        //XmlElement fe = (XmlElement)xn1;
                        Triangles.Add(new TriangleInd()
                        {
                            Vertex1 = int.Parse(fe.Element("v1").Value),
                            Vertex2 = int.Parse(fe.Element("v2").Value),
                            Vertex3 = int.Parse(fe.Element("v3").Value)
                        });
                    }
                    GetRange();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }

                DateTime afterDT1 = System.DateTime.Now;
                TimeSpan ts1 = afterDT1.Subtract(beforeDT1);
                string test = ts1.TotalMilliseconds.ToString();

                //TriangleInd testr = Triangles[Triangles.Count - 1];

            }

            public void SaveObjBinaryStream(Stream stream)
            {
                stream.Position = 0;
                StreamWriter sw = new StreamWriter(stream);
                foreach (Coordinate c in Vertices)
                {
                    sw.WriteLine($"v {c.X}   {c.Y}   {c.Z}");
                }
                foreach (TriangleInd tr in Triangles)
                {
                    string tri = "f " + (tr.Vertex1 + 1).ToString() + " " + (tr.Vertex2 + 1).ToString() + " " + (tr.Vertex3 + 1).ToString();
                    sw.WriteLine(tri);
                }
                sw.Flush();
                //stream.Position = 0;
                //stream.Flush();
            }
        }
        public static Coordinate CompArch(Coordinate coordinate, double xCenter, double yOrigin, double para)
        {
            Coordinate coo = new Coordinate();
            //double temp = -Math.Sign(coordinate.X - xCenter) * 0.017 * (coordinate.Y - yOrigin) * (1 - Math.Pow(0.5, Math.Abs(coordinate.X - xCenter) / 8));
            double temp = -Math.Sign(coordinate.X - xCenter) * para * (coordinate.Y - yOrigin) * (1 - Math.Pow(0.5, Math.Abs(coordinate.X - xCenter) / 8));
            coo.X = coordinate.X;
            if (coordinate.Y > yOrigin)
            {
                coo.X = coo.X + float.Parse(temp.ToString());
            }
            coo.Y = coordinate.Y;
            coo.Z = coordinate.Z;
            return coo;
        }
        public static Coordinate FlipManual(Coordinate coordinate, float baseCoo, bool isUpper)
        {
            Coordinate coo = new Coordinate();
            //double temp = -Math.Sign(coordinate.X - xCenter) * 0.017 * (coordinate.Y - yOrigin) * (1 - Math.Pow(0.5, Math.Abs(coordinate.X - xCenter) / 8));
            if (isUpper)
            {
                coo.X = coordinate.X;
                coo.Y = coordinate.Y;
                coo.Z = coordinate.Z - baseCoo;
            }
            else
            {
                coo.X = -coordinate.X;
                coo.Y = coordinate.Y;
                coo.Z = -(coordinate.Z - baseCoo);
            }



            return coo;
        }

        public static Coordinate CompScaleX(Coordinate coordinate, double xCenter, double yOrigin, double k)
        {
            Coordinate coo = new Coordinate();
            double temp = -(coordinate.X - xCenter) * k;
            coo.X = coordinate.X;
            coo.X = coo.X + (float)temp;
            coo.Y = coordinate.Y;
            coo.Z = coordinate.Z;
            return coo;
        }
        public static Coordinate CompScaleY(Coordinate coordinate, double yCenter, double k)
        {
            Coordinate coo = new Coordinate();
            double temp = (coordinate.Y - yCenter) * k;
            coo.X = coordinate.X;
            coo.Y = coordinate.Y;
            coo.Y = coo.Y + (float)temp;
            coo.Z = coordinate.Z;
            return coo;
        }
        public struct Coordinate
        {
            public float X;
            public float Y;
            public float Z;

            internal static Coordinate Read(BinaryReader reader)
            {
                Coordinate coor = new Coordinate();
                coor.X = reader.ReadSingle();
                coor.Y = reader.ReadSingle();
                coor.Z = reader.ReadSingle();

                return coor;
            }
        }

        public class Triangle
        {
            public Coordinate NormalVector { get; set; }

            public Coordinate Vertex1 { get; set; }

            public Coordinate Vertex2 { get; set; }

            public Coordinate Vertex3 { get; set; }

            public int Attributes { get; set; }


            internal static Triangle Read(BinaryReader reader)
            {
                Triangle triangle = new Triangle();
                triangle.NormalVector = Coordinate.Read(reader);
                triangle.Vertex1 = Coordinate.Read(reader);
                triangle.Vertex2 = Coordinate.Read(reader);
                triangle.Vertex3 = Coordinate.Read(reader);
                triangle.Attributes = reader.ReadUInt16();
                return triangle;
            }
        }

        public class TriangleInd
        {
            public int Vertex1 { get; set; }

            public int Vertex2 { get; set; }

            public int Vertex3 { get; set; }


            
        }
        public bool ModelProcess(Stream inStream, int para, double archCompPara, Stream outStream, string formate)
        {
            //para:1-新病例，2-老病例
            if (para != 1 && para != 2 && para != 99)
                throw new InvalidDataException("预变形参数无效");
            //Console.WriteLine($"ModelPreprocess执行完毕，传入参数为：{inpath},{para},{outpath}");
            DateTime beforeDT = System.DateTime.Now;
            STLFile file = new STLFile();
            OBJFile objFile = new OBJFile();
            bool isSTL = true;


            
            if (formate == "amf")
            {
                objFile.LoadAmfBinaryStream(inStream);
                isSTL = false;
            }
            if (formate == "obj")
            {
                objFile.LoadObjBinaryStream(inStream);
                isSTL = false;
            }
            if (formate == "stl")
            {
                try
                {
                    file.LoadBinaryStream(inStream);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("STL文件长度无效");
                }
                isSTL = true;
            }


            


            //get the num of points within zmax±0.05 and zmin ±0.05 
            //get the points that x within Box center x ±0.5 and z >3 
            List<Coordinate> ZmaxTriangles = new List<Coordinate>();
            List<Coordinate> ZminTriangles = new List<Coordinate>();
            List<Coordinate> XCenterPoints = new List<Coordinate>();
            double xCenter = (file.Xmin + file.Xmax) / 2;
            double yCenter = (file.Ymin + file.Ymax) / 2;
            if (isSTL)
            {
                foreach (Triangle tr in file.Triangles)
                {
                    if (tr.Vertex1.Z > (file.Zmax - 0.05) & tr.Vertex2.Z > (file.Zmax - 0.05) & tr.Vertex3.Z > (file.Zmax - 0.05))
                    {
                        ZmaxTriangles.Add(tr.Vertex1);
                    }
                    if (tr.Vertex1.Z < (file.Zmin + 0.05) & tr.Vertex2.Z < (file.Zmin + 0.05) & tr.Vertex3.Z < (file.Zmin + 0.05))
                    {
                        ZminTriangles.Add(tr.Vertex1);
                    }
                    if (tr.Vertex1.X > xCenter - 4 & tr.Vertex1.X < xCenter + 4)
                    {
                        XCenterPoints.Add(tr.Vertex1);
                    }
                    if (tr.Vertex2.X > xCenter - 4 & tr.Vertex2.X < xCenter + 4)
                    {
                        XCenterPoints.Add(tr.Vertex2);
                    }
                    if (tr.Vertex3.X > xCenter - 4 & tr.Vertex3.X < xCenter + 4)
                    {
                        XCenterPoints.Add(tr.Vertex3);
                    }
                }
            }
            else
            {
                foreach (Coordinate co in objFile.Vertices)
                {
                    if (co.Z > (file.Zmax - 0.05) )
                    {
                        ZmaxTriangles.Add(co);
                    }
                    if (co.Z < (file.Zmin + 0.05) )
                    {
                        ZminTriangles.Add(co);
                    }
                    if (co.X > xCenter - 4 & co.X < xCenter + 4)
                    {
                        XCenterPoints.Add(co);
                    }
                }
            }
            
            //judge the upper or lower 
            //get the typical point of the arch
            double yOirginMax = -30;
            bool filpManual = true;
            bool isManual = false;
            bool isUpper = true;
            if (ZmaxTriangles.Count > ZminTriangles.Count)
            {
                isManual = true;
                isUpper = false;
                //手工线下颌
                foreach (Coordinate co in XCenterPoints)
                {
                    if (co.Z < (file.Zmax - 3) & co.Y > yOirginMax)
                    {
                        yOirginMax = co.Y;
                    }
                }
            }
            else
            {
                if (Math.Abs(file.Zmin) > 1)
                {
                    isManual = true;
                    isUpper = true;
                }
                //自动线或手工线上颌
                foreach (Coordinate co in XCenterPoints)
                {
                    if (co.Z > (file.Zmin + 3) & co.Y > yOirginMax)
                    {
                        yOirginMax = co.Y;
                    }
                }
            }
            yOirginMax = yOirginMax + 3;




            //process the compensation and save file
            if (isSTL)
            {
                foreach (Triangle tr in file.Triangles)
                {
                    //判断是否是小板区域
                    if (file.Zmin > -0.03 & file.Zmin < 0.03)
                    {
                        //自动线模型
                    }
                    //预缩弓补偿
                    if (para == 2)
                    {
                        tr.Vertex1 = CompArch(tr.Vertex1, xCenter, yOirginMax, archCompPara);
                        tr.Vertex2 = CompArch(tr.Vertex2, xCenter, yOirginMax, archCompPara);
                        tr.Vertex3 = CompArch(tr.Vertex3, xCenter, yOirginMax, archCompPara);
                    }
                    else if (para == 1)//老病例，仅做X轴缩放
                    {
                        tr.Vertex1 = CompScaleX(tr.Vertex1, xCenter, yOirginMax, 0.011);
                        tr.Vertex2 = CompScaleX(tr.Vertex2, xCenter, yOirginMax, 0.011);
                        tr.Vertex3 = CompScaleX(tr.Vertex3, xCenter, yOirginMax, 0.011);
                    }
                    else if (para == 99)
                    {
                        tr.Vertex1 = CompScaleY(tr.Vertex1, yCenter, archCompPara);
                        tr.Vertex2 = CompScaleY(tr.Vertex2, yCenter, archCompPara);
                        tr.Vertex3 = CompScaleY(tr.Vertex3, yCenter, archCompPara);
                    }

                }
                if (filpManual)
                {
                    foreach (Triangle tr in file.Triangles)
                    {
                        float baseCoo = (float)file.Zmax;
                        if (isUpper)
                        {
                            baseCoo = (float)file.Zmin;
                        }
                        tr.Vertex1 = FlipManual(tr.Vertex1, baseCoo, isUpper);
                        tr.Vertex2 = FlipManual(tr.Vertex2, baseCoo, isUpper);
                        tr.Vertex3 = FlipManual(tr.Vertex3, baseCoo, isUpper);

                    }
                }
                file.SaveBinaryStream(outStream);
            }
            else
            {
                for (int i = 0; i <  objFile.Vertices.Count; i++)
                {
                    //判断是否是小板区域
                    if (objFile.Zmin > -0.03 & objFile.Zmin < 0.03)
                    {
                        //自动线模型
                    }
                    //预缩弓补偿
                    if (para == 2)
                    {
                        objFile.Vertices[i] = CompArch(objFile.Vertices[i], xCenter, yOirginMax, archCompPara);
                    }
                    else if (para == 1)//老病例，仅做X轴缩放
                    {
                        objFile.Vertices[i] = CompScaleX(objFile.Vertices[i], xCenter, yOirginMax, 0.011);
                    }
                    else if (para == 99)
                    {
                        objFile.Vertices[i] = CompScaleY(objFile.Vertices[i], yCenter, archCompPara);
                    }

                }
                if (filpManual)
                {
                    for (int i = 0; i < objFile.Vertices.Count; i++)
                    {
                        float baseCoo = (float)objFile.Zmax;
                        if (isUpper)
                        {
                            baseCoo = (float)objFile.Zmin;
                        }
                        objFile.Vertices[i] = FlipManual(objFile.Vertices[i], baseCoo, isUpper);
                    }
                   
                }
                
                objFile.SaveObjBinaryStream(outStream);

            }
            
            //Console.WriteLine($"the num of verticals is {file.Triangles.Count}");
            //Console.WriteLine($"the length of outstream before save is {outStream.Length}");

            //Console.WriteLine($"the length of outstream after save is {outStream.Length}");
            //Console.WriteLine("DateTime costed for Shuffle function is: {0}ms", ts.TotalMilliseconds);
            //Console.ReadLine();
            if (isManual)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        //public bool ModelFileProcess(string inFile, int para, double archCompPara, string outFile)
        //{
        //    //para:1-老病例，2-新病例 99-预定义xy缩放
        //    if (para != 1 && para != 2 && para != 99)
        //        throw new InvalidDataException("预变形参数无效");
        //    //Console.WriteLine($"ModelPreprocess执行完毕，传入参数为：{inpath},{para},{outpath}");
        //    //DateTime beforeDT = System.DateTime.Now;
        //    STLFile file = new STLFile();
        //    try
        //    {
        //        file.LoadBinary(inFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidDataException("STL文件长度无效");
        //    }

        //    //DateTime afterDT = System.DateTime.Now;
        //    //TimeSpan ts = afterDT.Subtract(beforeDT);
        //    //get the xyz max and min wile load the tringles
        //    foreach (Triangle tr in file.Triangles)
        //    {
        //        double tempXmax = Math.Max(Math.Max(tr.Vertex1.X, tr.Vertex2.X), tr.Vertex3.X);
        //        double tempXmin = Math.Min(Math.Min(tr.Vertex1.X, tr.Vertex2.X), tr.Vertex3.X);
        //        double tempYmax = Math.Max(Math.Max(tr.Vertex1.Y, tr.Vertex2.Y), tr.Vertex3.Y);
        //        double tempYmin = Math.Min(Math.Min(tr.Vertex1.Y, tr.Vertex2.Y), tr.Vertex3.Y);
        //        double tempZmax = Math.Max(Math.Max(tr.Vertex1.Z, tr.Vertex2.Z), tr.Vertex3.Z);
        //        double tempZmin = Math.Min(Math.Min(tr.Vertex1.Z, tr.Vertex2.Z), tr.Vertex3.Z);
        //        if (tempXmax > file.Xmax) { file.Xmax = tempXmax; }
        //        if (tempYmax > file.Ymax) { file.Ymax = tempYmax; }
        //        if (tempZmax > file.Zmax) { file.Zmax = tempZmax; }
        //        if (tempXmin < file.Xmin) { file.Xmin = tempXmin; }
        //        if (tempYmin < file.Ymin) { file.Ymin = tempYmin; }
        //        if (tempZmin < file.Zmin) { file.Zmin = tempZmin; }
        //    }
        //    //get the num of points within zmax±0.05 and zmin ±0.05 
        //    //get the points that x within Box center x ±0.5 and z >3 
        //    List<Triangle> ZmaxTriangles = new List<Triangle>();
        //    List<Triangle> ZminTriangles = new List<Triangle>();
        //    List<Coordinate> XCenterPoints = new List<Coordinate>();
        //    double xCenter = (file.Xmin + file.Xmax) / 2;
        //    double yCenter = (file.Ymin + file.Ymax) / 2;
        //    foreach (Triangle tr in file.Triangles)
        //    {
        //        if (tr.Vertex1.Z > (file.Zmax - 0.05) & tr.Vertex2.Z > (file.Zmax - 0.05) & tr.Vertex3.Z > (file.Zmax - 0.05))
        //        {
        //            ZmaxTriangles.Add(tr);
        //        }
        //        if (tr.Vertex1.Z < (file.Zmin + 0.05) & tr.Vertex2.Z < (file.Zmin + 0.05) & tr.Vertex3.Z < (file.Zmin + 0.05))
        //        {
        //            ZminTriangles.Add(tr);
        //        }
        //        if (tr.Vertex1.X > xCenter - 4 & tr.Vertex1.X < xCenter + 4)
        //        {
        //            XCenterPoints.Add(tr.Vertex1);
        //        }
        //        if (tr.Vertex2.X > xCenter - 4 & tr.Vertex2.X < xCenter + 4)
        //        {
        //            XCenterPoints.Add(tr.Vertex2);
        //        }
        //        if (tr.Vertex3.X > xCenter - 4 & tr.Vertex3.X < xCenter + 4)
        //        {
        //            XCenterPoints.Add(tr.Vertex3);
        //        }
        //    }
        //    //judge the upper or lower 
        //    //get the typical point of the arch
        //    double yOirginMax = -30;
        //    if (ZmaxTriangles.Count > ZminTriangles.Count)
        //    {
        //        //手工线下颌
        //        foreach (Coordinate co in XCenterPoints)
        //        {
        //            if (co.Z < (file.Zmax - 3) & co.Y > yOirginMax)
        //            {
        //                yOirginMax = co.Y;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //自动线或手工线上颌
        //        foreach (Coordinate co in XCenterPoints)
        //        {
        //            if (co.Z > (file.Zmin + 3) & co.Y > yOirginMax)
        //            {
        //                yOirginMax = co.Y;
        //            }
        //        }
        //    }
        //    yOirginMax = yOirginMax + 3;
        //    //process the compensation and save file
        //    foreach (Triangle tr in file.Triangles)
        //    {
        //        //判断是否是小板区域
        //        if (file.Zmin > -0.03 & file.Zmin < 0.03)
        //        {
        //            //自动线模型
        //        }
        //        //预缩弓补偿
        //        if (para == 2)
        //        {
        //            tr.Vertex1 = CompArch(tr.Vertex1, xCenter, yOirginMax, archCompPara);
        //            tr.Vertex2 = CompArch(tr.Vertex2, xCenter, yOirginMax, archCompPara);
        //            tr.Vertex3 = CompArch(tr.Vertex3, xCenter, yOirginMax, archCompPara);
        //        }
        //        else if (para == 1)//老病例，仅做X轴缩放
        //        {
        //            tr.Vertex1 = CompScaleX(tr.Vertex1, xCenter, yOirginMax, 0.011);
        //            tr.Vertex2 = CompScaleX(tr.Vertex2, xCenter, yOirginMax, 0.011);
        //            tr.Vertex3 = CompScaleX(tr.Vertex3, xCenter, yOirginMax, 0.011);
        //        }
        //        else if (para == 99)
        //        {
        //            tr.Vertex1 = CompScaleY(tr.Vertex1, yCenter, archCompPara);
        //            tr.Vertex2 = CompScaleY(tr.Vertex2, yCenter, archCompPara);
        //            tr.Vertex3 = CompScaleY(tr.Vertex3, yCenter, archCompPara);
        //        }

        //    }

        //    //Console.WriteLine($"the num of verticals is {file.Triangles.Count}");
        //    //Console.WriteLine($"the length of outstream before save is {outStream.Length}");
        //    file.SaveBinary(outFile);
        //    //Console.WriteLine($"the length of outstream after save is {outStream.Length}");
        //    //Console.WriteLine("DateTime costed for Shuffle function is: {0}ms", ts.TotalMilliseconds);
        //    //Console.ReadLine();
        //    return true;
        //}
    }
}
