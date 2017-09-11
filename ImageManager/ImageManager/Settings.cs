using System;
using System.Collections.Generic;

namespace ImageManager
{
	[Serializable]
	class Setting
	{
		public string DirectoryName;
		public string Key;
		public bool Copy;
	}


	class SettingsManager
	{
		private List<Setting> AllSettings;

		public SettingsManager()
		{
			AllSettings = new List<Setting>();
		}
	}
}

