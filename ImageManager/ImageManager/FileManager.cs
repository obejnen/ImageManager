using System.Collections.Generic;

namespace ImageManager
{
	static class FileManager
	{
		public static int GetNextImageIndex(List<string> allImagesPath, Image currentImage)
		{
			int currentImageIndex = allImagesPath.IndexOf(currentImage.FullPath);

			if (currentImageIndex == allImagesPath.Count - 1)
				return 0;
			return ++currentImageIndex;
		}

		public static int GetPreviousImageIndex(List<string> allImagePath, Image currentImage)
		{
			int currentImageIndex = allImagePath.IndexOf(currentImage.FullPath);

			if (currentImageIndex != 0)
				return --currentImageIndex;
			return allImagePath.Count - 1;
		}
	}
}