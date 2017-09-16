using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace ImageManager.ButtonBinding
{
	internal class ControlLinesManager
	{
		private List<ControlLine> BindingLines { get; set; }
		public int ControlLinesCount => BindingLines.Count;

		public ControlLinesManager()
		{
			BindingLines = new List<ControlLine>();
		}

        public void RemoveControlLine(IFrameworkInputElement element, Grid grid)
        {
            var index = Convert.ToInt32(element.Name.Remove(0, element.Name.Length - 1));
			BindingLines.RemoveAt(index);
			ChangeIndexes();
			grid.RowDefinitions.RemoveAt(index);
			grid.Children.RemoveAt(index);
        }

		public ControlLine CreateControlLine(RoutedEventHandler DeleteControlLineButton_Click,
			TextChangedEventHandler SubfolderName_Changed, TextChangedEventHandler BindKey_Changed,
			 System.Windows.Input.KeyEventHandler BindKey_KeyDown)
		{
			var cl = new ControlLine(BindingLines.Count);
			cl.AddEventHandlers(DeleteControlLineButton_Click, SubfolderName_Changed, BindKey_Changed, BindKey_KeyDown);
			BindingLines.Add(cl);
			return cl;
		}

		public ControlLine CreateControlLine(RoutedEventHandler DeleteControlLineButton_Click,
			TextChangedEventHandler SubfolderName_Changed, TextChangedEventHandler BindKey_Changed,
			 System.Windows.Input.KeyEventHandler BindKey_KeyDown, string bindedKey, string subfolderName, bool? isMoveFileMode)
		{
			var cl = new ControlLine(BindingLines.Count);
			cl.FillData(bindedKey, subfolderName, isMoveFileMode);
			cl.AddEventHandlers(DeleteControlLineButton_Click, SubfolderName_Changed, BindKey_Changed, BindKey_KeyDown);
			BindingLines.Add(cl);
			return cl;
		}

		public List<Tuple<string, string, bool?>> GetBindedSettings()
		{
			return BindingLines
				.Where(line => line.SubfolderName != String.Empty)
				.Select(line => new Tuple<string, string, bool?>(line.BindedKey, line.SubfolderName, line.IsMoveFileMode)).ToList();
		}

		public List<string> GetBindedKeys()
		{
			return BindingLines
				.Where(line => line.SubfolderName != String.Empty)
				.Select(line => line.BindedKey).ToList();
		}

		public List<string> GetSubfolderNames()
		{
			return BindingLines
				.Where(line => line.SubfolderName != String.Empty)
				.Select(line => line.SubfolderName).ToList();
		}

		public List<bool?> GetFileMods()
		{
			return BindingLines
				.Where(line => line.SubfolderName != String.Empty)
				.Select(line => line.IsMoveFileMode).ToList();
		}

		public string GetBindedKey(Button btn)
		{
			return BindingLines.FirstOrDefault(line => line.Index == Convert.ToInt32(btn.Name.Remove(0, btn.Name.Length - 1))).BindedKey;
		}

		public string GetSubfolderName(string key)
		{
			return BindingLines.FirstOrDefault(line => line.BindedKey == key).SubfolderName;
		}

		public bool? IsMoveFileMode(string key)
		{
			return BindingLines.FirstOrDefault(line => line.BindedKey == key).IsMoveFileMode;
		}

		public void BindKeyFromTextBox(TextBox textBox)
		{
			int index = Convert.ToInt32(textBox.Name.Remove(0, textBox.Name.Length - 1));

			if (BindingLines[index].BindedKey == String.Empty)
				BindingLines[index].BindedKey = textBox.Text.ToUpper();
		}

		private void ChangeIndexes()
		{
			int index = 0;
			foreach(ControlLine cl in BindingLines)
			{
				cl.Index = index;
				index++;
			}
		}
	}
}
