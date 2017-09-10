using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageManager
{
	internal class Image
	{
		private BitmapImage _img;

		public BitmapImage Img => _img;

		public string FullPath => _img?.UriSource.LocalPath;

		public string Name => File.Exists(FullPath)
			? Path.GetFileName(FullPath)
			: null;

		public Image(string path)
		{
			if (path != null && File.Exists(path))
			{
				_img = new BitmapImage();
				_img.BeginInit();
				_img.CacheOption = BitmapCacheOption.OnLoad;
				_img.UriSource = new Uri(path);
				_img.DecodePixelWidth = 1280;
				_img.EndInit();
			}
		}
	}
}