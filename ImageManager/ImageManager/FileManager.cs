using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageManager
{
	internal class FileManager
	{
		private List<string> allImagesPath;

		public void RemoveImage(string path)
		{
			allImagesPath.Remove(path);
		}

		public string LoadDirectory(string path)
		{
			if (path == String.Empty)
				return null;

			allImagesPath = Directory.GetFiles(path).ToList();

			return allImagesPath?.FirstOrDefault();
		}

		public void SetImagesPath(List<string> pathList)
		{
			allImagesPath = pathList;
		}

		public string GetNextImagePath(Image currentImage)
		{
			int currentImageIndex = allImagesPath.IndexOf(currentImage.FullPath);

			return currentImageIndex == allImagesPath.Count - 1 
				? allImagesPath[0]
				: allImagesPath[++currentImageIndex];
		}

		public string GetPreviousImagePath(Image currentImage)
		{
			int currentImageIndex = allImagesPath.IndexOf(currentImage.FullPath);

			return currentImageIndex != 0 
				? allImagesPath[--currentImageIndex] 
				: allImagesPath[allImagesPath.Count - 1];
		}
	}
}