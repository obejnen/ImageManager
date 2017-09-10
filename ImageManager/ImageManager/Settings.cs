using System;
using System.Collections.Generic;

namespace ImageManager
{
	class Setting
	{
		public string DirectoryName;
		public string Key;
		public bool Copy;
	}

	[Serializable]
	class Settings
	{
		//todo: use alsettings instead of DirectoryNames and Keys
		public List<Setting> AllSettings;

		//todo: remove
		public List<string> DirectoryNames;
		public List<string> Keys;

		public Settings()
		{
			DirectoryNames = new List<string>();
			Keys = new List<string>();
		}
	}

	static class SettingsManager
	{
		public static void RefreshSettings(List<ButtonBindingControl> controlPanels, Settings settings)
		{
			settings.DirectoryNames.Clear();
			settings.Keys.Clear();
			string dnContent = String.Empty;
			string kContent = String.Empty;
			foreach (ButtonBindingControl cp in controlPanels)
			{
				dnContent = cp.SubfolderTextBox.Text;
				kContent = cp.KeyTextBox.Text;
				if (dnContent != String.Empty && kContent != String.Empty)
				{
					settings.DirectoryNames.Add(cp.SubfolderTextBox.Text);
					settings.Keys.Add(cp.KeyTextBox.Text);
				}
			}
		}

		public static void LoadSettings(List<ButtonBindingControl> controlPanels, Settings settings, CreateControlPanelDelegate CreateCP)
		{
			controlPanels.Clear();

			for (int i = 0; i < settings.Keys.Count; i++)
			{
				CreateCP();
			}

			for (int i = 0; i < controlPanels.Count; i++)
			{
				controlPanels[i].KeyTextBox.Text = settings.Keys[i];
				controlPanels[i].SubfolderTextBox.Text = settings.DirectoryNames[i];
			}
		}
	}
}

