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

        public void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void CopyImage(string imgFullPath, string directoryPath)
        {
            var newImgPath = Path.Combine(directoryPath, Path.GetFileName(imgFullPath));

            if(!File.Exists(newImgPath))
            {
                File.Copy(imgFullPath, newImgPath);
            }
        }

        public void MoveImage(string imgFullPath, string directoryPath)
        {
            var newImgPath = Path.Combine(directoryPath, Path.GetFileName(imgFullPath));
            //todo: add error if file already exist
            if (!File.Exists(newImgPath))
            {
                File.Move(imgFullPath, newImgPath);
                RemoveImage(imgFullPath);
            }
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

		public string GetNextImagePath(string currentImagePath)
		{
			int currentImageIndex = allImagesPath.IndexOf(currentImagePath);

			return currentImageIndex == allImagesPath.Count - 1 
				? allImagesPath.First()
				: allImagesPath[currentImageIndex + 1];
		}

		public string GetPreviousImagePath(string currentImagePath)
		{
			int currentImageIndex = allImagesPath.IndexOf(currentImagePath);

			return currentImageIndex != 0 
				? allImagesPath[--currentImageIndex] 
				: allImagesPath[allImagesPath.Count - 1];
		}
	}
}