﻿using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using ClipperLib;

namespace gs
{
    using CPolygon = List<IntPoint>;
    using CPolygonList = List<List<IntPoint>>;
    using CPolyPath = List<IntPoint>;

    public static class ClipperUtil
    {
        /// <summary>
        /// Polygons with area smaller than this value are discarded internally in offset and boolean functions.
        /// This is a default, if a function takes a minArea parameter, it overrides this value.
        /// </summary>
        public static double DiscardMinArea = MathUtil.ZeroTolerancef;


        /// <summary>
        /// If enabled, we scale polygons to maximum possible integer size, instead
        /// of using fixed precision. Default false.
        /// </summary>
        public static bool ComputeToMaxAccuracy {
            get { return max_accuracy; }
            set { max_accuracy = value;  }
        }
        static bool max_accuracy = false;
         

        /// <summary>
        /// Precision of integer solution, relative to input units.
        /// EG to solve with 0.001mm precision, input values in mm, set DefaultPrecision=0.001
        /// Ignored if ComputeToMaxAccuracy=true. Default = 0.0001;
        /// </summary>
        public static double DefaultPrecision {
            get { return 1.0 / inv_default_precision; }
            set { inv_default_precision = 1.0 / DefaultPrecision; }
        }
        static double inv_default_precision = 10000;


        // Clipper uses integer coordinates, so we need to scale our doubles.
        // This value determines # of integers per mm. 8 feet ~= 2500mm
        // Clipper docs say for 32-bit ints the max is 46340, so for 64=bit... ?
        public static double GetIntScale(IEnumerable<Vector2d> vertices)
        {
            // details: http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Types/CInt.htm
            // const max_int = 9.2e18 / 2        // 9.2e18 is max int, give ourselves room to breathe
            const long max_int = int.MaxValue/2;  // int.MaxValue=2,147,483,647

            // find maximum distance to origin. not ideal, efficiency-wise...
            Vector2d maxDist = CurveUtils2.GetMaxOriginDistances(vertices);
            double max_origin_dist = Math.Max(maxDist.x, maxDist.y);

            if (max_accuracy == false) {
                // if max int coord is within our max-int range, we are safe
                int max_dist_int = (int)(max_origin_dist * inv_default_precision);
                if (max_dist_int < max_int)
                    return inv_default_precision;
                // get as close to max_int as we can
                return (double)max_int / max_origin_dist;
            } else {
                return (max_origin_dist < 1) ? (double)max_int : ((double)max_int / max_origin_dist);
            }
        }
        public static double GetIntScale(GeneralPolygon2d poly)
        {
            return GetIntScale(poly.Outer.Vertices);
        }
		public static double GetIntScale(List<GeneralPolygon2d> poly)
		{
            double max = 0;
            foreach (var v in poly)
                max = Math.Max(max, GetIntScale(v.Outer.Vertices));
            return (max <= 0) ? 1.0 : max;
		}

        public static CPolygon ConvertToClipper(Polygon2d poly, double nIntScale)
        {
            int N = poly.VertexCount;
            CPolygon clipper_poly = new CPolygon(N);
            for (int i = 0; i < N; ++i) {
                Vector2d v = poly[i];
                clipper_poly.Add(new IntPoint(nIntScale * v.x, nIntScale * v.y));
            }
            return clipper_poly;
        }

        public static CPolygonList ConvertToClipper(GeneralPolygon2d polys, double nIntScale)
        {
            List<CPolygon> clipper_polys = new List<CPolygon>();
            clipper_polys.Add(ConvertToClipper(polys.Outer, nIntScale));
            foreach (Polygon2d hole in polys.Holes)
                clipper_polys.Add(ConvertToClipper(hole, nIntScale));
            return clipper_polys;
        }

        public static CPolyPath ConvertToClipper(PolyLine2d pline, double nIntScale)
        {
            int N = pline.VertexCount;
            CPolyPath clipper_path = new CPolyPath(N);
            for (int i = 0; i < N; ++i) {
                Vector2d v = pline[i];
                clipper_path.Add(new IntPoint(nIntScale * v.x, nIntScale * v.y));
            }
            return clipper_path;
        }


        public static Polygon2d ConvertFromClipper(CPolygon clipper_poly, double nIntScale)
        {
            double scale = 1.0 / (double)nIntScale;

            int N = clipper_poly.Count;
            Polygon2d poly = new Polygon2d();
            for (int i = 0; i < N; ++i) {
                IntPoint p = clipper_poly[i];
                Vector2d v = new Vector2d((double)p.X * scale, (double)p.Y * scale);
                poly.AppendVertex(v);
            }
            return poly;
        }

        public static PolyLine2d ConvertFromClipperPath(CPolyPath clipper_poly, double nIntScale)
        {
            double scale = 1.0 / (double)nIntScale;

            int N = clipper_poly.Count;
            PolyLine2d poly = new PolyLine2d();
            for (int i = 0; i < N; ++i) {
                IntPoint p = clipper_poly[i];
                Vector2d v = new Vector2d((double)p.X * scale, (double)p.Y * scale);
                poly.AppendVertex(v);
            }
            return poly;
        }


        public static List<GeneralPolygon2d> ConvertFromClipper(List<List<IntPoint>> clipper_polys, double nIntScale)
        {
            List<GeneralPolygon2d> result = new List<GeneralPolygon2d>();
            try {

                // convert clipper polys to Polygon2d
                List<Polygon2d> polys = new List<Polygon2d>();
                int N = clipper_polys.Count;
                for (int i = 0; i < N; ++i) {
                    Polygon2d poly = ConvertFromClipper(clipper_polys[i], nIntScale);
                    if ( poly != null ) 
                        polys.Add(poly);
                }

                // sort polygons into outer/holes
                // [TODO] clipper can figure this out for us...perhaps faster??


                // find the 'outer' polygons. Here we are assuming
                // that outer polygons are CCW...
                bool[] done = new bool[N];
                Array.Clear(done, 0, N);
                for ( int i = 0; i < N; ++i ) {
                    if (polys[i].IsClockwise == false) {
                        GeneralPolygon2d gp = new GeneralPolygon2d();
                        gp.Outer = polys[i];
                        result.Add(gp);
                        done[i] = true;
                    }
                }

                // compute bboxes
                AxisAlignedBox2d[] outerBounds = new AxisAlignedBox2d[result.Count];
                for (int i = 0; i < result.Count; ++i)
                    outerBounds[i] = result[i].Outer.GetBounds();

                // remaining polygons are holes. Figure out which outer
                // they belong too. Only difficult if there is more than one option.
                for ( int i = 0; i < N; ++i ) {
                    if (done[i])
                        continue;
                    if (result.Count == 1) {
                        result[0].AddHole(polys[i], false);
                        done[i] = true;
                        continue;
                    }

                    AxisAlignedBox2d box = polys[i].GetBounds();
                    for ( int j = 0; j < result.Count; ++j ) {
                        if (outerBounds[j].Contains(box) == false)
                            continue;
                        if (result[j].Outer.Contains(polys[i]))
                            result[j].AddHole(polys[i], false);
                        done[i] = true;
                    }

                    // uh-oh...now what? perhaps should force a full N-pair test/sort if this happens?
                    if (done[i] == false)
                        System.Diagnostics.Debug.WriteLine("ClipperUtil.ConvertFromClipper: could not find parent for polygon " + i.ToString());
                }

            } catch /*( Exception e )*/ {
                //System.Diagnostics.Debug.WriteLine("ClipperUtil.ConvertFromClipper: caught exception: " + e.Message);
            }
            return result;
        }



		public static List<GeneralPolygon2d> ComputeOffsetPolygon(Polygon2d poly, double fOffset, bool bSharp = false)
        {
            double nIntScale = GetIntScale(poly.Vertices);

            List<IntPoint> clipper_poly = ClipperUtil.ConvertToClipper(poly, nIntScale);
            CPolygonList clipper_polys = new CPolygonList() { clipper_poly };

            CPolygonList dilate_solution = new CPolygonList();

            try {
                ClipperOffset co = new ClipperOffset();
                if ( bSharp )
                    co.AddPaths(clipper_polys, JoinType.jtMiter, EndType.etClosedPolygon);
                else
                    co.AddPaths(clipper_polys, JoinType.jtRound, EndType.etClosedPolygon);
                co.Execute(ref dilate_solution, fOffset * nIntScale);
            } catch /*( Exception e )*/ {
                //System.Diagnostics.Debug.WriteLine("ClipperUtil.ComputeOffsetPolygon: Clipper threw exception: " + e.Message);
                return new List<GeneralPolygon2d>();
            }

            if (dilate_solution.Count == 0)
                return new List<GeneralPolygon2d>();

            List<GeneralPolygon2d> polys = ClipperUtil.ConvertFromClipper(dilate_solution, nIntScale);
            return polys;
        }




        public static List<GeneralPolygon2d> ComputeOffsetPolygon(GeneralPolygon2d poly, double fOffset, bool bMiter = false)
        {
            double nIntScale = GetIntScale(poly);

            CPolygonList clipper_polys = new CPolygonList();
            clipper_polys.Add(ClipperUtil.ConvertToClipper(poly.Outer, nIntScale));
            foreach (Polygon2d hole in poly.Holes)
                clipper_polys.Add(ClipperUtil.ConvertToClipper(hole, nIntScale));

            CPolygonList dilate_solution = new CPolygonList();

            try {
                ClipperOffset co = new ClipperOffset();
                if ( bMiter )
                    co.AddPaths(clipper_polys, JoinType.jtMiter, EndType.etClosedPolygon);
                else
                    co.AddPaths(clipper_polys, JoinType.jtRound, EndType.etClosedPolygon);
                co.Execute(ref dilate_solution, fOffset * nIntScale);
            } catch /*( Exception e )*/ {
                //System.Diagnostics.Debug.WriteLine("ClipperUtil.ComputeOffsetPolygon: Clipper threw exception: " + e.Message);
                return new List<GeneralPolygon2d>();
            }

            List<GeneralPolygon2d> polys = ClipperUtil.ConvertFromClipper(dilate_solution, nIntScale);
            return polys;
        }



		/// <summary>
		/// Compute offset polygon from all input polys (ie separate islands may merge)
		/// </summary>
		public static List<GeneralPolygon2d> ComputeOffsetPolygon(List<GeneralPolygon2d> polys, 
            double fOffset, bool bMiter = false, double minArea = -1)
		{
			double nIntScale = GetIntScale(polys);
            if (minArea < 0)
                minArea = DiscardMinArea;

			ClipperOffset co = new ClipperOffset();
			PolyTree tree = new PolyTree();
			try {
				foreach ( GeneralPolygon2d poly in polys ) {
					CPolygonList clipper_poly = ConvertToClipper(poly, nIntScale);
					if (bMiter)
						co.AddPaths(clipper_poly, JoinType.jtMiter, EndType.etClosedPolygon);
					else
						co.AddPaths(clipper_poly, JoinType.jtRound, EndType.etClosedPolygon);
				}
				co.Execute(ref tree, fOffset * nIntScale);

				List<GeneralPolygon2d> result = new List<GeneralPolygon2d>();
				for (int ci = 0; ci < tree.ChildCount; ++ci)
					Convert(tree.Childs[ci], result, nIntScale, minArea);
				return result;

			} catch /*(Exception e)*/ {
                //System.Diagnostics.Debug.WriteLine("ClipperUtil.ComputeOffsetPolygon: Clipper threw exception: " + e.Message);
                return new List<GeneralPolygon2d>();
			}
		}
		public static List<GeneralPolygon2d> MiterOffset(List<GeneralPolygon2d> polys, double fOffset, double minArea = -1) {
			return ComputeOffsetPolygon(polys, fOffset, true, minArea);
		}
		public static List<GeneralPolygon2d> MiterOffset(GeneralPolygon2d poly, double fOffset, double minArea = -1) {
			return ComputeOffsetPolygon(new List<GeneralPolygon2d>() { poly }, fOffset, true, minArea);
		}
		public static List<GeneralPolygon2d> RoundOffset(List<GeneralPolygon2d> polys, double fOffset, double minArea = -1) {
			return ComputeOffsetPolygon(polys, fOffset, false, minArea);
		}
		public static List<GeneralPolygon2d> RoundOffset(GeneralPolygon2d poly, double fOffset, double minArea = -1) {
			return ComputeOffsetPolygon(new List<GeneralPolygon2d>() { poly }, fOffset, false, minArea);
		}


        /// <summary>
        /// Utility function that does a separate miter offset for each poly. The results may overlap.
        /// Not safe to use unless you are passing polys into another function.
        /// </summary>
		public static List<GeneralPolygon2d> SeparateMiterOffsets(List<GeneralPolygon2d> polys, double fOffset, double minArea = -1) {
            List<GeneralPolygon2d> offsets = new List<GeneralPolygon2d>();
            foreach (var p in polys)
                offsets.AddRange(MiterOffset(p, fOffset, minArea));
            return offsets;
		}



		public enum BooleanOp {
			Union, Difference, Intersection, Xor
		}
		public static List<GeneralPolygon2d> PolygonBoolean(GeneralPolygon2d poly1, GeneralPolygon2d poly2, BooleanOp opType, double minArea = -1)
		{
			return PolygonBoolean(new List<GeneralPolygon2d>() { poly1 }, 
			                      new List<GeneralPolygon2d>() { poly2 }, opType, minArea);
		}
		public static List<GeneralPolygon2d> PolygonBoolean(GeneralPolygon2d poly1, List<GeneralPolygon2d> poly2, BooleanOp opType, double minArea = -1)
		{
			return PolygonBoolean(new List<GeneralPolygon2d>() { poly1 }, poly2, opType, minArea);
		}
		public static List<GeneralPolygon2d> PolygonBoolean(List<GeneralPolygon2d> poly1, GeneralPolygon2d poly2, BooleanOp opType, double minArea = -1)
		{
			return PolygonBoolean(poly1, new List<GeneralPolygon2d>() { poly2 }, opType, minArea);
		}
		public static List<GeneralPolygon2d> PolygonBoolean(
            List<GeneralPolygon2d> poly1, List<GeneralPolygon2d> poly2, 
            BooleanOp opType, double minArea = -1 ) 
		{
			// handle cases where one list is empty
			if ( poly1.Count == 0 ) {
				if (opType == BooleanOp.Difference || opType == BooleanOp.Intersection)
					return new List<GeneralPolygon2d>();
				else
					return DeepCopy.List(poly2);
			} else if ( poly2.Count == 0 ) {
				if (opType == BooleanOp.Intersection)
					return new List<GeneralPolygon2d>();
				else
					return DeepCopy.List(poly1);
			}


			double nIntScale = Math.Max(GetIntScale(poly1), GetIntScale(poly2));
            if (minArea < 0)
                minArea = DiscardMinArea;

            try {
				Clipper clipper = new Clipper(Clipper.ioStrictlySimple);

				foreach (GeneralPolygon2d sub in poly1) {
					CPolygonList cpoly = ConvertToClipper(sub, nIntScale);
					clipper.AddPaths(cpoly, PolyType.ptSubject, true);
				}
				foreach (GeneralPolygon2d clip in poly2) {
					CPolygonList cpoly = ConvertToClipper(clip, nIntScale);
					clipper.AddPaths(cpoly, PolyType.ptClip, true);
				}

				ClipType cType = ClipType.ctUnion;
				if (opType == BooleanOp.Difference)
					cType = ClipType.ctDifference;
				else if (opType == BooleanOp.Intersection)
					cType = ClipType.ctIntersection;
				else if (opType == BooleanOp.Xor)
					cType = ClipType.ctXor;

				PolyTree tree = new PolyTree();
				bool bOK = clipper.Execute(cType, tree);
				if (bOK == false) {
					//System.Diagnostics.Debug.WriteLine("ClipperUtil.PolygonBoolean: Clipper failed");
					return new List<GeneralPolygon2d>();					
				}

				List<GeneralPolygon2d> result = new List<GeneralPolygon2d>();
				for (int ci = 0; ci < tree.ChildCount; ++ci)
					Convert(tree.Childs[ci], result, nIntScale, minArea);
				return result;

			} catch /*(Exception e)*/ {
				//System.Diagnostics.Debug.WriteLine("ClipperUtil.PolygonBoolean: Clipper threw exception: " + e.Message);
				return new List<GeneralPolygon2d>();
			}

		}




		public static List<GeneralPolygon2d> Union(GeneralPolygon2d poly1, GeneralPolygon2d poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Union, minArea);
		}
		public static List<GeneralPolygon2d> Union(GeneralPolygon2d poly1, List<GeneralPolygon2d> poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Union, minArea);
		}
		public static List<GeneralPolygon2d> Union(List<GeneralPolygon2d> poly1, GeneralPolygon2d poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Union, minArea);
		}
		public static List<GeneralPolygon2d> Union(List<GeneralPolygon2d> poly1, List<GeneralPolygon2d> poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Union, minArea);
		}



		public static List<GeneralPolygon2d> Intersection(GeneralPolygon2d poly1, GeneralPolygon2d poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Intersection, minArea);
		}
		public static List<GeneralPolygon2d> Intersection(GeneralPolygon2d poly1, List<GeneralPolygon2d> poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Intersection, minArea);
		}
		public static List<GeneralPolygon2d> Intersection(List<GeneralPolygon2d> poly1, GeneralPolygon2d poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Intersection, minArea);
		}
		public static List<GeneralPolygon2d> Intersection(List<GeneralPolygon2d> poly1, List<GeneralPolygon2d> poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Intersection, minArea);
		}


		public static List<GeneralPolygon2d> Difference(GeneralPolygon2d poly1, GeneralPolygon2d poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Difference, minArea);
		}
		public static List<GeneralPolygon2d> Difference(GeneralPolygon2d poly1, List<GeneralPolygon2d> poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Difference, minArea);
		}
		public static List<GeneralPolygon2d> Difference(List<GeneralPolygon2d> poly1, GeneralPolygon2d poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Difference, minArea);
		}
		public static List<GeneralPolygon2d> Difference(List<GeneralPolygon2d> poly1, List<GeneralPolygon2d> poly2, double minArea = -1) {
			return PolygonBoolean(poly1, poly2, BooleanOp.Difference, minArea);
		}



		/// <summary>
		/// Extract set of nested solids (ie polygon-with-holes) from treeNode
		/// </summary>
		public static void Convert(PolyNode treeNode, List<GeneralPolygon2d> polys, double nIntScale, double minArea) {
			if (treeNode.IsHole)
				throw new Exception("ClipperUtil.Convert: should not have a hole here");
			if (treeNode.IsOpen)
				throw new Exception("ClipperUtil.Convert: found open contour??");

			GeneralPolygon2d poly = new GeneralPolygon2d();
            poly.Outer = ConvertFromClipper(treeNode.Contour, nIntScale);
            if (Math.Abs(poly.Outer.SignedArea) < minArea)
                return;

            polys.Add(poly);

			for (int ci = 0; ci < treeNode.ChildCount; ++ci) {
				PolyNode holeNode = treeNode.Childs[ci];
				if (holeNode.IsHole == false)
					throw new Exception("CliperUtil.Convert: how is this not a hole?");
				if (holeNode.IsOpen)
					throw new Exception("ClipperUtil.Convert: found open hole contour??");

				Polygon2d hole = ConvertFromClipper(holeNode.Contour, nIntScale);

                // [RMS] discard extremely tiny holes, for which we cannot reliably
                //   determine orientation, which causes AddHole() to assert
                if (Math.Abs(hole.SignedArea) < minArea)
                    continue;

				poly.AddHole(hole, false);

				// recurse for new top-level children
				for (int ti = 0; ti < holeNode.ChildCount; ti++ )
					Convert(holeNode.Childs[ti], polys, nIntScale, minArea);
			}
		}



        /// <summary>
        /// remove portions of polyline that are inside set of solids
        /// </summary>
        public static List<PolyLine2d> ClipAgainstPolygon(List<GeneralPolygon2d> solids, PolyLine2d polyline, bool bIntersect = false)
        {
            double nIntScale = Math.Max(GetIntScale(solids), GetIntScale(polyline.Vertices));
            List<PolyLine2d> result = new List<PolyLine2d>();

            Clipper clip = new Clipper();
            PolyTree tree = new PolyTree();
            try {

                foreach (GeneralPolygon2d poly in solids) {
                    CPolygonList clipper_poly = ConvertToClipper(poly, nIntScale);
                    clip.AddPaths(clipper_poly, PolyType.ptClip, true);
                }

                CPolyPath path = ConvertToClipper(polyline, nIntScale);
                clip.AddPath(path, PolyType.ptSubject, false);

                if (bIntersect)
                    clip.Execute(ClipType.ctIntersection, tree);
                else
                    clip.Execute(ClipType.ctDifference, tree);

                for (int ci = 0; ci < tree.ChildCount; ++ci) {
                    if (tree.Childs[ci].IsOpen == false)
                        continue;
                    PolyLine2d clippedPath = ConvertFromClipperPath(tree.Childs[ci].Contour, nIntScale);

                    // clipper just cuts up the polylines, we still have to figure out containment ourselves.
                    // Currently just checking based on point around middle of polyline...
                    // [TODO] can we get clipper to not return the inside ones?
                    Vector2d qp = (clippedPath.VertexCount > 2) ?
                        clippedPath[clippedPath.VertexCount / 2] : clippedPath.Segment(0).Center;
                    bool inside = false;
                    foreach (var poly in solids) {
                        if (poly.Contains(qp)) {
                            inside = true;
                            break;
                        }
                    }
                    if (inside == bIntersect)
                        result.Add(clippedPath);
                }

            } catch /*(Exception e)*/ {
                // [TODO] what to do here?
                //System.Diagnostics.Debug.WriteLine("ClipperUtil.ClipAgainstPolygon: Clipper threw exception: " + e.Message);
                return new List<PolyLine2d>();
            }

            return result;
        }


    }
}
