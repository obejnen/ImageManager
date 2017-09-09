using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageManager
{
	class Image
	{
		public BitmapImage Img { get => _img; }
		private BitmapImage _img;

		public string FullPath
		{
			get => _img?.UriSource.LocalPath.ToString();
		}

		public string Name
		{
			get
			{
				return File.Exists(FullPath) ?
					Path.GetFileName(FullPath) :
					null;
			}
		}

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