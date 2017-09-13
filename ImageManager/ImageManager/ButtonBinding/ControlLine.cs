using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;

namespace ImageManager.ButtonBinding
{
		public enum FileMode
		{
			Move, Copy
		}

	public class ControlLine
	{
		const string DELETE_KEY_BUTTON_NAME = "DeleteKeyButton{0}";
		const string KEY_TEXTBOX_NAME = "KeyTextBox{0}";
		const string DIRECTORY_NAME_TEXTBOX_NAME = "SubfolderTextBox{0}";
		const string CONTROL_ROW_NAME = "ControlRow{0}";
        const string TOGGLESWITCH_NAME = "ToggleSwtich{0}";
		const string ADD_KEY_BUTTON_NAME = "AddKeyButton";

        public int Index
        {
            get { return lineIndex; }
            set
            {
				if (value >= 0)
					ChangeIndex(value);
            }
        }
        public string SubfolderName => SubfolderTextBox.Text;
        public string BindedKey
		{
			get
			{
				return KeyTextBox.Text;
			}
			set
			{
				KeyTextBox.Text = value;
			}
		}
		public bool? IsMoveFileMode => FileModeToggleSwitch.IsChecked;
		public RowDefinition ControlRow { get; }
		public StackPanel ControlStackPanel { get; }
		private Button DeleteKeyButton { get; set; }
		private TextBox KeyTextBox { get; set; }
		private TextBox SubfolderTextBox { get; set; }
        private ToggleSwitch FileModeToggleSwitch { get; set; }
		private Grid ControlGrid { get; }
		

        private int lineIndex;

		private enum TextBoxType
		{
			KeyTextBox, SubfolderTextBox
		}

		private enum ButtonType
		{
			AddKey, DeleteKey
		}

		public void FillData(string bindedKey, string subfolderName, bool? isMoveFileMode)
		{
			KeyTextBox.Text = bindedKey;
			SubfolderTextBox.Text = subfolderName;
			FileModeToggleSwitch.IsChecked = isMoveFileMode;
		}

		private void ChangeIndex(int index)
		{
			lineIndex = index;
			KeyTextBox.Name = String.Format(KEY_TEXTBOX_NAME, index);
			SubfolderTextBox.Name = String.Format(DIRECTORY_NAME_TEXTBOX_NAME, index);
			DeleteKeyButton.Name = String.Format(DELETE_KEY_BUTTON_NAME, index);
			ControlRow.Name = String.Format(CONTROL_ROW_NAME, index);
			FileModeToggleSwitch.Name = String.Format(TOGGLESWITCH_NAME, index);

			Grid.SetRow(FileModeToggleSwitch, index);
			Grid.SetRow(DeleteKeyButton, index);
			Grid.SetRow(KeyTextBox, index);
			Grid.SetRow(SubfolderTextBox, index);
			Grid.SetRow(ControlStackPanel, index);
		}

		public ControlLine(int index)
		{
			lineIndex = index;
			DeleteKeyButton = CreateButton(ButtonType.DeleteKey);
			KeyTextBox = CreateTextBox(TextBoxType.KeyTextBox);
			SubfolderTextBox = CreateTextBox(TextBoxType.SubfolderTextBox);
            FileModeToggleSwitch = CreateToggleSwitch();

			ControlRow = CreateRowDefinition();
			ControlGrid = CreateGrid();
			FillGrid();
			ControlStackPanel = CreateStackPanel();
			FillStackPanel();
		}

		public void AddEventHandlers(RoutedEventHandler DeleteControlLineButton_Click,
			TextChangedEventHandler SubfolderName_Changed, TextChangedEventHandler BindKey_Changed,
			 System.Windows.Input.KeyEventHandler BindKey_KeyDown)
		{
			DeleteKeyButton.Click += DeleteControlLineButton_Click;
			SubfolderTextBox.TextChanged += SubfolderName_Changed;
			KeyTextBox.TextChanged += BindKey_Changed;
			KeyTextBox.PreviewKeyDown += BindKey_KeyDown;
		}

		public static ControlLine GetControlPanel(List<ControlLine> controlPanels, int index)
		{
			//todo: consider this approach in other places
			return controlPanels.FirstOrDefault(cp => cp.Index == index);
			
			//todo: then delete this unused message ))

			//to remove: old part

			//foreach (ControlLine cp in controlPanels)
			//{
			//	if (cp.Index == index)
			//		return cp;
			//}

			//return null;
		}

		private void FillStackPanel()
		{
			ControlStackPanel.Children.Add(ControlGrid);
		}

		private void FillGrid()
		{
			// other style

			Control[] gridControls =
			{
				KeyTextBox,
				SubfolderTextBox,
				FileModeToggleSwitch,
				DeleteKeyButton
			};

			foreach (Control gridControl in gridControls)
			{
				ControlGrid.Children.Add(gridControl);
			}
		}

		private Button CreateButton(ButtonType type)
		{
			var button = new Button
			{
				Name = type == ButtonType.DeleteKey ?
					String.Format(DELETE_KEY_BUTTON_NAME, Index) :
					ADD_KEY_BUTTON_NAME
			};

			button.Style = type == ButtonType.DeleteKey ?
				button.FindResource("DeleteKeyButtonStyle") as Style :
				button.FindResource("AddKeyButtonStyle") as Style;

			return button;
		}

		private TextBox CreateTextBox(TextBoxType type)
		{
			TextBox textBox = new TextBox
			{
				Name = type == TextBoxType.SubfolderTextBox ?
					String.Format(DIRECTORY_NAME_TEXTBOX_NAME, Index) :
					String.Format(KEY_TEXTBOX_NAME, Index)

			};

			textBox.Style = type == TextBoxType.SubfolderTextBox ?
					textBox.FindResource("SubfolderNameTextBoxStyle") as Style :
					textBox.FindResource("KeyTextBoxStyle") as Style;

			return textBox;
		}

        private ToggleSwitch CreateToggleSwitch()
        {
            ToggleSwitch ts = new ToggleSwitch
            {
                Name = String.Format(TOGGLESWITCH_NAME, Index)
            };

            ts.Style = ts.FindResource("FileModeSwitchStyle") as Style;
            return ts;
        }

		private RowDefinition CreateRowDefinition()
		{
			return new RowDefinition
			{
				Height = new GridLength(55, GridUnitType.Pixel),
				Name = String.Format(CONTROL_ROW_NAME, Index)
			};
		}

		private StackPanel CreateStackPanel()
		{
			StackPanel stackPanel = new StackPanel();

			Grid.SetRow(stackPanel, Index);

			return stackPanel;
		}

		private Grid CreateGrid()
		{
			Grid grid = new Grid();

			return grid;
		}
	}
}