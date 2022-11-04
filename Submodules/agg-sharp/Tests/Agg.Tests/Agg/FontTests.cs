﻿using MatterHackers.Agg.Image;
using NUnit.Framework;
using System.Collections.Generic;

namespace MatterHackers.Agg.Font
{
	[TestFixture, Category("Agg.Font")]
	public class FontTests
	{
		[Test]
		public void CanPrintTests()
		{
			// Invoke DrawString with a carriage return. If any part of the font pipeline throws, this test fails
			ImageBuffer testImage = new ImageBuffer(300, 300);
			testImage.NewGraphics2D().DrawString("\r", 30, 30);
		}

		[Test]
		public void TextWrappingTest()
		{
			EnglishTextWrapping englishWrapping = new EnglishTextWrapping(8);
			List<string> wrappedLines = englishWrapping.WrapSingleLineOnWidth("Layers or MM", 30);
			Assert.IsTrue(wrappedLines.Count == 3);
			Assert.IsTrue(wrappedLines[0] == "Layer");
			Assert.IsTrue(wrappedLines[1] == "s or");
			Assert.IsTrue(wrappedLines[2] == "MM");
		}
	}
}