using System.Collections.Generic;

namespace ImageManager
{
	class KeyManager
	{
		private List<string> usedKeys;
		private List<string> controlKeys;

		public KeyManager()
		{
			usedKeys = new List<string>();
			controlKeys = new List<string>
			{
				"Left", "Right", "Space", "Back", "Return", "Tab"
			};
		}

		public bool IsKeyUsed(string key)
		{
			var index = usedKeys.IndexOf(key);
			if (index == -1)
				index = controlKeys.IndexOf(key);
			if (index == -1)
				return false;
			return true;
		}

		public void AddKey(string key)
		{
			usedKeys.Add(key);
		}

		public void RemoveKey(string key)
		{
			usedKeys.Remove(key);
		}
	}
}