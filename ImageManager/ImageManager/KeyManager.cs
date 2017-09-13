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
			//the better variant
			return !usedKeys.Contains(key.ToUpper()) && !controlKeys.Contains(key.ToUpper());

			//var index = usedKeys.IndexOf(key.ToUpper());
			//if (index == -1)
			//	index = controlKeys.IndexOf(key.ToUpper());
			//if (index == -1)
			//	return true;
			//return false;
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