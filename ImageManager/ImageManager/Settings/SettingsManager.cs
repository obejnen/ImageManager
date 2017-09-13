using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace ImageManager.Settings
{
	[Serializable]
	class SettingsManager
	{
		public List<Settings> AllSettings { get; }
		private string SettingsFileName { get; }

		public SettingsManager(string fileName)
		{
			SettingsFileName = fileName;
			AllSettings = LoadFromFile();
		}

		public void RefreshSettings(List<string> subfolderNames, List<string> bindedKeys, List<bool?> isMoveFileModes)
		{
			AllSettings.Clear();
			for(int i = 0; i < subfolderNames.Count; i++)
			{
				AllSettings.Add(new Settings(
					subfolderNames[i], bindedKeys[i], isMoveFileModes[i]));
			}
			SaveInFile();
		}

		public void Add(string subfolderName, string bindedKey, bool? isMoveFileMode)
		{
			if (AllSettings.Where(settings => settings.SubfolderName == subfolderName
											 || settings.Key == bindedKey).
						FirstOrDefault() == null)
				AllSettings.Add(new Settings(subfolderName, bindedKey, isMoveFileMode));
			SaveInFile();
		}

		public void Remove(string subfolderName)
		{
			Settings stngs = AllSettings
				.Where(settings => settings.SubfolderName == subfolderName)
				.FirstOrDefault();
			if (stngs != null)
				AllSettings.Remove(stngs);
			SaveInFile();
		}

		private void SaveInFile()
		{
			BinaryFormatter binFormat = new BinaryFormatter();
			using (Stream fStream = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				binFormat.Serialize(fStream, this);
			}
		}

		private List<Settings> LoadFromFile()
		{
			BinaryFormatter binFormat = new BinaryFormatter();

			if (!File.Exists(SettingsFileName))
				return new List<Settings>();

			using (Stream fStream = File.OpenRead(SettingsFileName))
			{
				 return fStream.Length != 0
					? ((SettingsManager)binFormat.Deserialize(fStream)).AllSettings
					: new List<Settings>();
			}
		}
	}
}
