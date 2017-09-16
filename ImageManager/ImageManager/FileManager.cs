using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace ImageManager
{
	internal class FileManager
	{
		private List<string> allImagesPath;
		private string openedDirectory;

		public void RemoveImage(string path)
		{
			allImagesPath.Remove(path);
			//StartUpdateTimer();
		}

        public void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

		private void UpdateFileList()
		{
			LoadDirectory(openedDirectory);
		}

		private void StartUpdateTimer()
		{
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1d);
			timer.Tick += TimerTick;
			timer.Start();
		}

		private void TimerTick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;
			timer.Stop();
			timer.Tick -= TimerTick;
			UpdateFileList();
		}

        public void MoveFile(string filePath, string subfolderName, bool? isCopyMode)
        {
			var imageDirectoryName = Path.GetDirectoryName(filePath);
			var newImagePath = Path.Combine(imageDirectoryName, subfolderName);

			CreateFolder(newImagePath);

			if (isCopyMode == true)
			{
				try
				{
					Copy(filePath, newImagePath);
				}
				catch
				{
					throw new Exception();
				}
			}
			else
			{
				Move(filePath, newImagePath);
			}
		}

        private void Copy(string imgFullPath, string directoryPath)
        {
            var newImgPath = Path.Combine(directoryPath, Path.GetFileName(imgFullPath));

            if(!File.Exists(newImgPath))
            {
                File.Copy(imgFullPath, newImgPath);
            }
        }

        private void Move(string imgFullPath, string directoryPath)
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
			openedDirectory = Path.GetDirectoryName(allImagesPath.FirstOrDefault());

			return allImagesPath?.FirstOrDefault();
		}

		public void SetImagesPath(List<string> pathList)
		{
			allImagesPath = pathList;
		}

		public string GetNextImagePath(string currentImagePath)
		{
			if (allImagesPath.Count == 0)
				return null;
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