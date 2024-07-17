//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CA1416 // Validate platform compatibility

namespace ShrinkDeck
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;

	internal class Shrinkifier(string path)
	{
		private const double MaxWidth = 650.0;

		private readonly string[] ExtensionFilters = [".bmp", ".gif", ".jpg", ".png", ".tiff"];

		private readonly DirectoryInfo dir = new(path);
		private readonly ImageDetector detector = new();

		public long BigSize { get; private set; }

		public long BittySize { get; private set; }


		public void Shrink()
		{
			Console.WriteLine($"\nscratchpad {path}\n");

			foreach (var file in dir.GetFileSystemInfos("*", SearchOption.AllDirectories)
				.Where(f => ExtensionFilters.Contains(f.Extension)))
			{
				var buffer = File.ReadAllBytes(file.FullName);
				var big = buffer.Length;
				BigSize += big;

				var signature = detector.GetSignature(buffer);
				if ($".{signature.ToString().ToLower()}" != file.Extension)
				{
					$"suspect: {file.Name} found to have {signature} content, continuing..."
						.Report(ConsoleColor.Yellow);
				}

				using var stream = new MemoryStream(buffer);
				using var image = Image.FromStream(stream);

				if (image.Width > MaxWidth)
				{
					Console.WriteLine($"resizing {file.FullName}");

					using var edit = Resize(image);

					if (edit.Width < image.Width && edit.Height < image.Height)
					{
						edit.Save(file.FullName);
						var enfo = new FileInfo(file.FullName);
						BittySize += enfo.Length;

						var pct = 100 - (((double)(enfo.Length) / big) * 100);

						($"shrunk from {big} to {enfo.Length}, " +
						$"a {pct:F2}% savings of {big - enfo.Length} bytes")
							.Report(ConsoleColor.DarkGray);
					}
				}
			}
		}


		private static Bitmap Resize(Image original)
		{
			var height = original.Height * (MaxWidth / original.Width);
			var edit = new Bitmap((int)MaxWidth, (int)height);

			edit.SetResolution(original.HorizontalResolution, original.VerticalResolution);

			using var g = Graphics.FromImage(edit);
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;

			using var attributes = new ImageAttributes();
			attributes.SetWrapMode(WrapMode.TileFlipXY);

			g.DrawImage(original,
				new Rectangle(0, 0, (int)MaxWidth, (int)height),
				0, 0, original.Width, original.Height,
				GraphicsUnit.Pixel,
				attributes);

			return edit;
		}
	}
}
