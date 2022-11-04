﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/
#define DEBUG_INTO_TGAS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MatterHackers.Agg;
using MatterHackers.PolygonMesh.Csg;
using MatterHackers.VectorMath;
using NUnit.Framework;

namespace MatterHackers.PolygonMesh.UnitTests
{
	[TestFixture, Category("Agg.PolygonMesh")]
	public class MeshTests
	{
		// [TestFixtureSetUp]
		// public void Setup()
		// {
		//    string relativePath = "../../../../../agg-sharp/PlatformWin32/bin/Debug/agg_platform_win32.dll";
		//    if(Path.DirectorySeparatorChar != '/')
		//    {
		//        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
		//    }
		//    File.Copy(relativePath, Path.GetFileName(relativePath), true);
		// }
		private static int meshSaveIndex = 0;

		public void SaveDebugInfo(Mesh mesh, string testHint = "")
		{
#if DEBUG_INTO_TGAS
			throw new NotImplementedException();
			// DebugRenderToImage debugRender = new DebugRenderToImage(mesh);
			// if (testHint != "")
			// {
			// 	debugRender.RenderToPng("debug face {0} - {1}.png".FormatWith(meshSaveIndex++, testHint));
			// }
			// else
			// {
			// 	debugRender.RenderToPng("debug face {0}.png".FormatWith(meshSaveIndex++));
			// }
#endif
		}

		// [Test]
		public void GetSliceLoop()
		{
			var cube = MeshHelper.CreatePlane(10, 10);
			var cutPlane = new Plane(Vector3.UnitX, new Vector3(3, 0, 0));
			var slice = new SliceLayer(cutPlane);
			slice.CreateSlice(cube);
			// Assert.AreEqual(1, slice.ClosedPolygons.Count);
		}

		public void DetectAndRemoveTJunctions()
		{
			// throw new NotImplementedException();
		}

		[Test]
		public void SplitFaceEdgeEdge()
		{
			void TestPositions(int p0, int p1, int p2)
			{
				var positions = new Vector3[] { default(Vector3), new Vector3(10, 0, 0), new Vector3(5, 20, 0) };
				var mesh = new Mesh();
				// .     |
				// .    /|\
				// .   / | \
				// .  /  |  \
				// . /___|___\
				// .     |
				mesh.CreateFace(new Vector3[] { positions[p0], positions[p1], positions[p2] });
				Assert.AreEqual(1, mesh.Faces.Count);
				Assert.AreEqual(3, mesh.Vertices.Count);

				// we find a split
				Assert.IsTrue(mesh.SplitFace(0, new Plane(new Vector3(1, 0, 0), 5)));
				// we now have 2 faces
				Assert.AreEqual(2, mesh.Faces.Count);
				// we now have 5 verts
				// the have all the expected x values
				Assert.AreEqual(4, mesh.Vertices.Count);
				Assert.AreEqual(1, mesh.Vertices.Where(v => v.X == 0).Count());
				Assert.AreEqual(2, mesh.Vertices.Where(v => v.X == 5).Count());
				Assert.AreEqual(1, mesh.Vertices.Where(v => v.X == 10).Count());
				// no face crosses the split line
				Assert.AreEqual(2, mesh.Faces.Where(f =>
				{
					// all face vertices are less than the split line or greater than the split line
					return (mesh.Vertices[f.v0].X <= 5 && mesh.Vertices[f.v1].X <= 5 && mesh.Vertices[f.v2].X <= 5)
						|| (mesh.Vertices[f.v0].X >= 5 && mesh.Vertices[f.v1].X >= 5 && mesh.Vertices[f.v2].X >= 5);
				}).Count());
			}

			// test every vertex orientation
			TestPositions(0, 1, 2);
			TestPositions(0, 2, 1);
			TestPositions(1, 0, 2);
			TestPositions(1, 2, 0);
			TestPositions(2, 0, 1);
			TestPositions(2, 1, 0);
		}

		[Test]
		public void SplitFaceTwoEdges()
		{
			void TestPositions(int p0, int p1, int p2)
			{
				var positions = new Vector3[] { new Vector3(0, 5, 0), default(Vector3), new Vector3(10, 0, 0) };
				var mesh = new Mesh();
				// split the face along the vertical line
				// . 0 _    |
				// .  |   __|
				// .  |     | __
				// .  |_____|_______
				// . 1      |       2
				mesh.CreateFace(new Vector3[] { positions[p0], positions[p1], positions[p2] });
				Assert.AreEqual(1, mesh.Faces.Count);
				Assert.AreEqual(3, mesh.Vertices.Count);

				// we find a split
				Assert.IsTrue(mesh.SplitFace(0, new Plane(new Vector3(1, 0, 0), 5)));
				// we now have 3 faces
				Assert.AreEqual(3, mesh.Faces.Count);
				// we now have 5 verts
				// the have all the expected x values
				Assert.AreEqual(5, mesh.Vertices.Count);
				Assert.AreEqual(2, mesh.Vertices.Where(v => v.X == 0).Count());
				Assert.AreEqual(2, mesh.Vertices.Where(v => v.X == 5).Count());
				Assert.AreEqual(1, mesh.Vertices.Where(v => v.X == 10).Count());
				// no face crosses the split line
				Assert.AreEqual(3, mesh.Faces.Where(f =>
				{
					// all face vertices are less than the split line or greater than the split line
					return (mesh.Vertices[f.v0].X <= 5 && mesh.Vertices[f.v1].X <= 5 && mesh.Vertices[f.v2].X <= 5)
						|| (mesh.Vertices[f.v0].X >= 5 && mesh.Vertices[f.v1].X >= 5 && mesh.Vertices[f.v2].X >= 5);
				}).Count());
			}

			// test every vertex orientation
			TestPositions(0, 1, 2);
			TestPositions(0, 2, 1);
			TestPositions(1, 0, 2);
			TestPositions(1, 2, 0);
			TestPositions(2, 0, 1);
			TestPositions(2, 1, 0);
		}

		[Test]
		public void CreateBspFaceTrees()
		{
			// a simple list of 3 faces
			//
			// Index 1 ^------------------- z = 3
			//
			// Index 0 ^------------------- z = 2
			//
			// Index 2 ^------------------- z = 1

			var testMesh = new Mesh();

			testMesh.CreateFace(new Vector3[]
			{
				new Vector3(0, 0, 2),
				new Vector3(10, 0, 2),
				new Vector3(5, 5, 2)
			});

			testMesh.CreateFace(new Vector3[]
			{
				new Vector3(0, 0, 3),
				new Vector3(10, 0, 3),
				new Vector3(5, 5, 3)
			});
			testMesh.CreateFace(new Vector3[]
			{
				new Vector3(0, 0, 1),
				new Vector3(10, 0, 1),
				new Vector3(5, 5, 1)
			});

			// test they are in the right order
			{
				var root = FaceBspTree.Create(testMesh);

				Assert.IsTrue(root.Index == 1);
				Assert.IsTrue(root.BackNode.Index == 0);
				Assert.IsTrue(root.BackNode.BackNode.Index == 2);

				var renderOredrList = FaceBspTree.GetFacesInVisibiltyOrder(testMesh, root, Matrix4X4.Identity, Matrix4X4.Identity).ToList();
				Assert.IsTrue(renderOredrList[0] == 1);
				Assert.IsTrue(renderOredrList[1] == 0);
				Assert.IsTrue(renderOredrList[2] == 2);
			}
		}
	}
}