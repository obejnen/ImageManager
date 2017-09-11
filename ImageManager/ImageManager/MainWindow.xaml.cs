using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MahApps.Metro.Controls;
using ImageManager.ButtonBinding;

namespace ImageManager
{

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
		//private Settings settings = new Settings();
        private ControlLinesManager controlLinesManager = new ControlLinesManager();
		private bool isFullScreenEnabled = false;

		public MainWindow()
		{
			InitializeComponent();
			//LoadSettingsFromFile();
			WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
		}

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				SetFullscreenSettings(!isFullScreenEnabled);
			}
		}

		private void FullScreenButton_Click(object sender, RoutedEventArgs e)
		{
			SetFullscreenSettings(!isFullScreenEnabled);
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
		//private void SaveSettings(object sender)
		//{
		//	BinaryFormatter binFormat = new BinaryFormatter();
		//	using (Stream fStream = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write, FileShare.None))
		//	{
		//		binFormat.Serialize(fStream, sender);
		//	}
		//}

		//todo: move method to settings manager (probably to constructor)
		//private void LoadSettingsFromFile()
		//{
		//	BinaryFormatter binFormat = new BinaryFormatter();
		//	Settings settingsFromFile;

		//	if (!File.Exists(SettingsFileName))
		//		return;

		//	using (Stream fStream = File.OpenRead(SettingsFileName))
		//	{
		//		settingsFromFile = fStream.Length != 0 
		//			? (Settings) binFormat.Deserialize(fStream) 
		//			: new Settings();
		//	}

		//	SettingsManager.LoadSettings(controlPanels, settingsFromFile, CreateControlPanel);
		//}

        private string ShowSelectImageDialog()
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.Filter = "Jpg images |*.jpg|Png images|*.png|All files|*.*";
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dlg.FileName;
            }

            return String.Empty;
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

		#region EventsHandlers
		private void AddKeyButton_Click(object sender, RoutedEventArgs e)
		{
			//CreateControlPanel();
			controlLinesManager.AddControlLine(SettingsGrid, DeleteKeyButton_Click, SubfolderTextBox_TextChanged,
				KeyTextBox_TextChanged, Controls_KeyDown);
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


        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var path = ShowSelectImageDialog();
            fileManager.LoadDirectory(Path.GetDirectoryName(path));
            ShowImage(path);
        }

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (SettingsFlyout.IsOpen != true && currentImage != null && Picture.Source != null)
			{
				string key = e.Key.ToString();
				if (e.Key == Key.Right || e.Key == Key.Space)
				{
					var nextImage = fileManager.GetNextImagePath(currentImage.FullPath);
					ShowImage(nextImage);
					return;
				}
				if (e.Key == Key.Left || e.Key == Key.Back)
				{
					//todo: send only path instead of full image
					var previousImage = fileManager.GetPreviousImagePath(currentImage.FullPath);
					ShowImage(previousImage);
					return;
				}
				if (!keyManager.IsKeyAvaivable(e.Key.ToString()))
				{
					var subfolderName = controlLinesManager.GetSubfolderName(e.Key.ToString());
					var imageDirectoryName = Path.GetDirectoryName(currentImage.FullPath);
					var newImagePath = Path.Combine(imageDirectoryName, subfolderName);

                    fileManager.CreateFolder(newImagePath);
                    var nextImage = fileManager.GetNextImagePath(currentImage.FullPath);
                    ShowImage(nextImage);

                    if(controlLinesManager.GetFileMode(key) == ButtonBinding.FileMode.Move)
					{ 
                        fileManager.MoveImage(currentImage.FullPath, newImagePath);
                    }
                    else
                    {
                        fileManager.CopyImage(currentImage.FullPath, newImagePath);
                    }
                }
			}
		}

		private void Controls_KeyDown(object sender, KeyEventArgs e)
		{
			var txtBox = (TextBox)sender;
			var key = e.Key.ToString();

			if(keyManager.AddKey(key) == KeyManager.KeyChangeStatus.ChangedSuccessfully)
			{
				keyManager.RemoveKey(txtBox.Text);
				txtBox.Text = key;
			}
		}

		private void KeyTextBox_TextChanged(object sender, RoutedEventArgs e)
		{
			var txtBox = (TextBox)sender;
			if (keyManager.IsKeyAvaivable(txtBox.Text))
			{
				keyManager.AddKey(txtBox.Text);
			}
		}

		private void SubfolderTextBox_TextChanged(object sender, RoutedEventArgs e)
		{
			var txtBox = (TextBox)sender;

			if (txtBox.Text.Length == 1 && keyManager.IsKeyAvaivable(txtBox.Text))
			{
				controlLinesManager.BindKeyFromTextBox(txtBox);
			}
		}

		private void DeleteKeyButton_Click(object sender, RoutedEventArgs e)
		{
			var btn = (Button)sender;
			keyManager.RemoveKey(controlLinesManager.GetBindedKey(btn));
            controlLinesManager.RemoveControlLine(btn, SettingsGrid);
		}
	}
}
#endregion