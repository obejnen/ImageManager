using System.Collections.Generic;

namespace ImageManager
{
	class KeyManager
	{
		public enum KeyChangeStatus
		{
			ChangedSuccessfully, ChangeFailed 
		}

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

		public bool IsKeyAvaivable(string key)
		{
			return !usedKeys.Contains(key.ToUpper()) && !controlKeys.Contains(key);
		}

		public KeyChangeStatus AddKey(string key)
		{
			if (IsKeyAvaivable(key) && key.Length == 1 && !string.IsNullOrEmpty(key))
			{
				usedKeys.Add(key);
				return KeyChangeStatus.ChangedSuccessfully;
			}
			return KeyChangeStatus.ChangeFailed;
		}

		public void RemoveKey(string key)
		{
			if (!IsKeyAvaivable(key))
			{
				usedKeys.Remove(key);
			}
		}
	}
}