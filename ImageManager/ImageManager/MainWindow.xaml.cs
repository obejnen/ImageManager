using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MahApps.Metro.Controls;

namespace ImageManager
{
	public delegate void CreateControlPanelDelegate();

	//todo: GLOBAL REVIEW move logics to managers
	//todo: use Tasks/async/await 
	//			to save settings
	//			to move files
	//			to reinit files list
	//todo: show exceptions on UI / check if WPF have global exception handling
	//todo: write UnitTests

	public partial class MainWindow : MetroWindow
	{
		private const string SettingsFileName = "settings.dat";

		private Image currentImage;
		private FileManager fileManager = new FileManager();
		private KeyManager keyManager = new KeyManager();
		private Settings settings = new Settings();
		private List<ButtonBindingControl> controlPanels = new List<ButtonBindingControl>();
		private bool isFullScreenEnabled = false;

		public MainWindow()
		{

			InitializeComponent();
			LoadSettingsFromFile();
			WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
		}

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				SetFullscreenSettings(!isFullScreenEnabled);
			}
		}

		private void SetFullscreenSettings(bool fullscreen)
		{
			WindowStyle = fullscreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;
			ButtonsGrid.Visibility = fullscreen ? Visibility.Hidden : Visibility.Visible;
			ResizeMode = fullscreen ? ResizeMode.NoResize : ResizeMode.CanResize;
			WindowState = fullscreen ? WindowState.Maximized : WindowState.Normal;

			IgnoreTaskbarOnMaximize = fullscreen;
			ShowTitleBar = !fullscreen;
			isFullScreenEnabled = fullscreen;
			ShowCloseButton = !fullscreen;
			ShowMinButton = !fullscreen;
			ShowMaxRestoreButton = !fullscreen;
		}

		//todo: move method to settings manager
		private void SaveSettings(object sender)
		{
			BinaryFormatter binFormat = new BinaryFormatter();
			using (Stream fStream = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				binFormat.Serialize(fStream, sender);
			}
		}

		//todo: move method to settings manager (probably to constructor)
		private void LoadSettingsFromFile()
		{
			BinaryFormatter binFormat = new BinaryFormatter();
			Settings settingsFromFile;

			if (!File.Exists(SettingsFileName))
				return;

			using (Stream fStream = File.OpenRead(SettingsFileName))
			{
				settingsFromFile = fStream.Length != 0 
					? (Settings) binFormat.Deserialize(fStream) 
					: new Settings();
			}

			SettingsManager.LoadSettings(controlPanels, settingsFromFile, CreateControlPanel);
		}

		private void AddControlHeadPanel()
		{
			var stackPanel = ButtonBindingControl.CreateHeaderStackPanel(AddKeyButton_Click);
			var rd = new RowDefinition
			{
				Height = new GridLength(30, GridUnitType.Pixel)
			};
			SettingsGrid.RowDefinitions.Add(rd);
			SettingsGrid.Children.Add(stackPanel);
		}

		private string ShowSelectFolderDialog()
		{
			var selectedPath = string.Empty;

			using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					selectedPath = dlg.SelectedPath;
				}
			}

			return selectedPath;
		}

		private void ShowImage(string path)
		{
			if (path != null)
			{
				currentImage = new Image(path);
				Picture.Source = currentImage.Img;
				AppWindow.Title = currentImage.Name;
			}
			else
				Picture.Source = null;
		}

		private void CreateFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		private void MoveImage(string directoryPath)
		{
			if (Picture.Source == null)
				return;

			CreateFolder(directoryPath);

			//todo: add error if file already exist
			if (!File.Exists(Path.Combine(directoryPath, currentImage.Name)))
			{
				File.Copy(currentImage.FullPath, Path.Combine(directoryPath, currentImage.Name));
			}
			if (FileModeSwitcher.IsChecked == true)
			{
				string currentImagePath = currentImage.FullPath;
				fileManager.RemoveImage(currentImagePath);

				//todo: send only path instead of full image
				var nextImage = fileManager.GetNextImagePath(currentImage);

				//todo: review this code
				//var nextImagePath = allImagesPath.Count != 0 ?
				//	allImagesPath[nextImageIndex] :
				//	null;

				ShowImage(nextImage);
				File.Delete(currentImagePath);
			}

		}

		private void ClearGrid(Grid grid)
		{
			grid.Children.Clear();
			grid.RowDefinitions.Clear();
			grid.ColumnDefinitions.Clear();
		}

		private void AddControlPanelToGrid(List<ButtonBindingControl> controlPanels)
		{
			foreach (ButtonBindingControl cp in controlPanels)
			{
				AddControlPanelToGrid(cp);
			}
		}

		private void AddControlPanelToGrid(ButtonBindingControl cp)
		{
			SettingsGrid.RowDefinitions.Add(cp.ControlRow);
			SettingsGrid.Children.Add(cp.ControlStackPanel);

			SettingsManager.RefreshSettings(controlPanels, settings);
			SaveSettings(settings);
		}

		private void CreateControlPanel()
		{
			if (controlPanels.Count >= 8)
				return;

			var cp = new ButtonBindingControl(controlPanels.Count + 1);

			cp.KeyTextBox.PreviewKeyDown += Controls_KeyDown;
			cp.DeleteKeyButton.Click += DeleteKeyButton_Click;
			cp.SubfolderTextBox.TextChanged += SubfolderTextBox_TextChanged;
			cp.KeyTextBox.TextChanged += KeyTextBox_TextChanged;

			AddControlPanelToGrid(cp);
			controlPanels.Add(cp);

			SettingsManager.RefreshSettings(controlPanels, settings);
			SaveSettings(settings);
		}

		private void AddKeyButton_Click(object sender, RoutedEventArgs e)
		{
			CreateControlPanel();
			SettingsManager.RefreshSettings(controlPanels, settings);
			SaveSettings(settings);
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsFlyout.IsOpen = true;
		}

		private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
		{
			var path = ShowSelectFolderDialog();
			var firstImage = fileManager.LoadDirectory(path);
			ShowImage(firstImage);
		}

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (SettingsFlyout.IsOpen != true && currentImage != null)
			{
				if (e.Key == Key.Right || e.Key == Key.Space)
				{
					var nextImage = fileManager.GetNextImagePath(currentImage);
					ShowImage(nextImage);
					return;
				}
				if (e.Key == Key.Left || e.Key == Key.Back)
				{
					//todo: send only path instead of full image
					var previousImage = fileManager.GetPreviousImagePath(currentImage);
					ShowImage(previousImage);
					return;
				}
				if (keyManager.IsKeyUsed(e.Key.ToString()))
				{
					var subfolderName = ButtonBindingControl.GetSubfolderName(controlPanels, e.Key.ToString());
					var imageDirectoryName = Path.GetDirectoryName(currentImage.FullPath);
					var newImagePath = Path.Combine(imageDirectoryName, subfolderName);

					CreateFolder(newImagePath);
					MoveImage(newImagePath);
				}
			}
		}

		private void Controls_KeyDown(object sender, KeyEventArgs e)
		{
			var txtBox = (TextBox)sender;
			var key = e.Key.ToString();

			//todo: move logic to key manager
			if (!keyManager.IsKeyUsed(key))
			{
				if (!string.IsNullOrEmpty(txtBox.Text))
				{
					keyManager.RemoveKey(txtBox.Text);
				}
				txtBox.Text = key;
				keyManager.AddKey(key);
			}
		}

		private void KeyTextBox_TextChanged(object sender, RoutedEventArgs e)
		{
			var txtBox = (TextBox)sender;
			if (!keyManager.IsKeyUsed(txtBox.Text))
				keyManager.AddKey(txtBox.Text);
		}

		private void SubfolderTextBox_TextChanged(object sender, RoutedEventArgs e)
		{
			var txtBox = (TextBox)sender;

			var panel = controlPanels.SingleOrDefault(p => p.KeyTextBox.Name == txtBox.Name);

			if (panel?.KeyTextBox.Text == String.Empty
				&& txtBox.Text.Length == 1
				&& !keyManager.IsKeyUsed(txtBox.Text))
			{
				panel.KeyTextBox.Text = txtBox.Text.ToUpper();
			}

			SettingsManager.RefreshSettings(controlPanels, settings);
			SaveSettings(settings);
		}

		private void DeleteKeyButton_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var index = ButtonBindingControl.GetIndex(button);
			ClearGrid(SettingsGrid);
			AddControlHeadPanel();
			var cpToDelete = ButtonBindingControl.GetControlPanel(controlPanels, index);
			controlPanels.Remove(cpToDelete);
			ButtonBindingControl.ChangeIndex(controlPanels);
			AddControlPanelToGrid(controlPanels);
			SettingsManager.RefreshSettings(controlPanels, settings);
			SaveSettings(settings);
		}
	}
}
