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

using MatterHackers.Agg.Transform;
using MatterHackers.VectorMath;

namespace MatterHackers.GCodeVisualizer
{
	public class GCodeRenderInfo
	{
		private Vector2[] extruderOffsets;

		public Vector2 GetExtruderOffset(int index)
		{
			if (extruderOffsets != null
				&& extruderOffsets.Length > index)
			{
				return extruderOffsets[index];
			}

			return Vector2.Zero;
		}

		public int startLayerIndex;

		public int StartLayerIndex { get { return startLayerIndex; } }

		private int endLayerIndex;

		public int EndLayerIndex { get { return endLayerIndex; } }

		private Affine transform;

		public Affine Transform { get { return transform; } }

		private double layerScale;

		public double LayerScale { get { return layerScale; } }

		private RenderType currentRenderType;

		public RenderType CurrentRenderType { get { return currentRenderType; } }

		private double featureToStartOnRatio0To1;

		public double FeatureToStartOnRatio0To1 { get { return featureToStartOnRatio0To1; } }

		private double featureToEndOnRatio0To1;

		public double FeatureToEndOnRatio0To1 { get { return featureToEndOnRatio0To1; } }

		public GCodeRenderInfo()
		{
		}

		public GCodeRenderInfo(int startLayerIndex, int endLayerIndex,
			Affine transform, double layerScale, RenderType renderType,
			double featureToStartOnRatio0To1, double featureToEndOnRatio0To1,
			Vector2[] extruderOffsets)
		{
			this.startLayerIndex = startLayerIndex;
			this.endLayerIndex = endLayerIndex;
			this.transform = transform;
			this.layerScale = layerScale;
			this.currentRenderType = renderType;
			this.featureToStartOnRatio0To1 = featureToStartOnRatio0To1;
			this.featureToEndOnRatio0To1 = featureToEndOnRatio0To1;
			this.extruderOffsets = extruderOffsets;
		}
	}
}