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

		public void RefreshSettings(IEnumerable<Tuple<string, string, bool?>> settings)
		{
			AllSettings.Clear();
			AllSettings.AddRange(settings.Select(s => new Settings(s.Item1, s.Item2, s.Item3)));
			SaveInFile();
		}

		#region ToDelete

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
			if (AllSettings.FirstOrDefault(settings => settings.SubfolderName == subfolderName || settings.Key == bindedKey) == null)
				AllSettings.Add(new Settings(subfolderName, bindedKey, isMoveFileMode));
			SaveInFile();
		}

		public void Remove(string subfolderName)
		{
			Settings stngs = AllSettings.FirstOrDefault(settings => settings.SubfolderName == subfolderName);
			if (stngs != null)
				AllSettings.Remove(stngs);
			SaveInFile();
		}

		#endregion 

		private void SaveInFile()
		{
			//todo: this method seems like a candidate for being called async
			var binFormat = new BinaryFormatter();
			using (Stream fStream = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				binFormat.Serialize(fStream, this);
			}
		}

		private List<Settings> LoadFromFile()
		{
			if (!File.Exists(SettingsFileName))
				return new List<Settings>();

			using (Stream fStream = File.OpenRead(SettingsFileName))
			{
				var binFormat = new BinaryFormatter();
				return fStream.Length != 0
					? ((SettingsManager)binFormat.Deserialize(fStream)).AllSettings
					: new List<Settings>();
			}
		}
	}
}
