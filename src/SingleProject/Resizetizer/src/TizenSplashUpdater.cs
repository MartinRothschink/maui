﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	public class TizenSplashUpdater : Task
	{
		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public ILogger Logger { get; private set; }

		static public Dictionary<(string Resolution, string Orientation), string> splashDpiMap = new Dictionary<(string, string), string>();

		const string splashDirectoryName = "splash";
		Size hdSize = new Size(720, 1080);
		Size fhdSize = new Size(1080, 1920);

		public override bool Execute()
		{
			var orientations = new List<string>() { "portrait", "landscape" };
			var splashInfo = MauiSplashScreen?.Length > 0 ? ResizeImageInfo.Parse(MauiSplashScreen[0]) : null;
			var image = splashInfo.OutputName + ".png";
			var sharedResFullPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, "shared/res/"));
			var splashFullPath = Path.Combine(sharedResFullPath, splashDirectoryName);

			if (Directory.Exists(splashFullPath))
				Directory.Delete(splashFullPath, true);
			Directory.CreateDirectory(splashFullPath);

			splashDpiMap.Clear();
			foreach (var dpi in DpiPath.Tizen.SplashScreen)
			{
				var imageOutputPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, dpi.Path));
				var imageFullPath = Path.Combine(imageOutputPath, image);

				if (File.Exists(imageFullPath))
				{
					var resolution = dpi.Path.Split('-')[1].ToLower();
					foreach (var orientation in orientations)
					{
						var newImage = splashInfo.OutputName + "." + resolution + "." + orientation + ".png";
						if (splashDpiMap.ContainsKey((resolution, orientation)))
						{
							splashDpiMap.Remove((resolution, orientation));
						}
						splashDpiMap.Add((resolution, orientation), $"{splashDirectoryName}/{newImage}");
						UpdateColorAndMoveFile(splashInfo, GetScreenSize(resolution, orientation), imageFullPath, Path.Combine(splashFullPath, newImage));
					}
				}
				else
				{
					Log.LogWarning($"Unable to find splash image at {imageFullPath}.");
					return false;
				}
			}

			foreach (var dpi in DpiPath.Tizen.Image)
			{
				var imageOutputPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, dpi.Path));
				var imageFullPath = Path.Combine(imageOutputPath, image);
				if (File.Exists(imageFullPath))
				{
					File.Delete(imageFullPath);
				}
			}

			return true;
		}

		Size GetScreenSize(string resolution, string orientation) =>
			resolution switch
			{
				"mdpi" => orientation == "portrait" ? hdSize : new Size(hdSize.Height, hdSize.Width),
				_ => orientation == "portrait" ? fhdSize : new Size(fhdSize.Height, fhdSize.Width)
			};

		void UpdateColorAndMoveFile(ResizeImageInfo splashInfo, Size screenSize, string sourceFilePath, string destFilePath)
		{
			var color = splashInfo.Color;
			if (color == null)
			{
				Log.LogWarning($"Unable to parse color for '{splashInfo.Filename}'.");
				color = SKColors.White;
			}
			using (SKBitmap bmp = SKBitmap.Decode(sourceFilePath))
			{
				SKImageInfo info = new SKImageInfo(screenSize.Width, screenSize.Height);
				using (SKSurface surface = SKSurface.Create(info))
				{
					SKCanvas canvas = surface.Canvas;
					canvas.Clear(color.Value);
					using SKPaint paint = new SKPaint
					{
						IsAntialias = true,
						FilterQuality = SKFilterQuality.High
					};

					var left = screenSize.Width <= bmp.Width ? 0 : (screenSize.Width - bmp.Width) / 2;
					var top = screenSize.Height <= bmp.Height ? 0 : (screenSize.Height - bmp.Height) / 2;
					var right = screenSize.Width <= bmp.Width ? left + screenSize.Width : left + bmp.Width;
					var bottom = screenSize.Height <= bmp.Height ? top + screenSize.Height : top + bmp.Height;
					canvas.DrawBitmap(bmp, new SKRect(left, top, right, bottom), paint);
					canvas.Flush();
					var updatedsplash = surface.Snapshot();
					using (var data = updatedsplash.Encode(SKEncodedImageFormat.Png, 100))
					{
						using (var stream = File.Create(destFilePath))
						{
							data.SaveTo(stream);
						}
					}
				}
			}
		}
	}
}
