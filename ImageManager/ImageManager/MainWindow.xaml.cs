using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using MahApps.Metro.Controls;
using ImageManager.ButtonBinding;
using ImageManager.Settings;

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
		private const string FileTypesFilter = "Jpg images |*.jpg|Png images|*.png|All files|*.*";

		private Image currentImage;
		private FileManager fileManager = new FileManager();
		private KeyManager keyManager = new KeyManager();
		private SettingsManager settingsManager;
        private ControlLinesManager controlLinesManager = new ControlLinesManager();
		private bool isFullScreenEnabled = false;
		private bool isTimerEnabled = false;
		ProcessStartInfo pi = new ProcessStartInfo();

		public MainWindow()
		{
			InitializeComponent();
			//LoadSettingsFromFile();
			WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
			settingsManager = new SettingsManager("settings.dat");
			LoadSettings();

		}

		private void LoadSettings()
		{
			foreach(var setting in settingsManager.AllSettings)
			{
				ControlLine cl = controlLinesManager.CreateControlLine(DeleteKeyButton_Click, SubfolderTextBox_TextChanged,
														KeyTextBox_TextChanged, Controls_KeyDown,
														setting.Key, setting.SubfolderName, setting.IsCopyFileMode);
				AddControlLine(cl);
			}
		}

		private async void UpdateSettings()
		{
			var settings = controlLinesManager.GetBindedSettings();
			await Task.Factory.StartNew(() => settingsManager.RefreshSettings(settings));

			//List<string> bindedKeys = controlLinesManager.GetBindedKeys();
			//List<string> subfolderNames = controlLinesManager.GetSubfolderNames();
			//List<bool?> isMoveFileModes = controlLinesManager.GetFileMods();

			//await Task.Factory.StartNew(() => settingsManager.RefreshSettings(subfolderNames, bindedKeys, isMoveFileModes));
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
		
        private string ShowSelectImageDialog()
        {
	        var dlg = new System.Windows.Forms.OpenFileDialog { Filter = FileTypesFilter };

	        return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK 
				? dlg.FileName 
				: String.Empty;
        }

        private string ShowSelectFolderDialog()
		{
			using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					return dlg.SelectedPath;
				}
			}

			return string.Empty;
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

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				SetFullscreenSettings(!isFullScreenEnabled);
			}
		}

		private void FullScreenButton_Click(object sender, RoutedEventArgs e)
		{
			if (Picture.Source != null)
			{
				SetFullscreenSettings(!isFullScreenEnabled);
			}

			//temporrary exception for test
			throw new Exception("Wrong button pressed");
		}

		private void AddKeyButton_Click(object sender, RoutedEventArgs e)
		{
			ControlLine cl = controlLinesManager.CreateControlLine(DeleteKeyButton_Click, SubfolderTextBox_TextChanged,
				KeyTextBox_TextChanged, Controls_KeyDown);
			AddControlLine(cl);
		}

		private void AddControlLine(ControlLine cl)
		{
			SettingsGrid.RowDefinitions.Add(cl.ControlRow);
			SettingsGrid.Children.Add(cl.ControlStackPanel);
			//UpdateSettings();
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
			if (path != String.Empty)
			{
				fileManager.LoadDirectory(Path.GetDirectoryName(path));
				ShowImage(path);
			}
        }

		//todo: check if it works after refactoring
		private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (SettingsFlyout.IsOpen || currentImage == null || Picture.Source == null)
				return;

			string nextImage;
			string key = e.Key.ToString();

			switch (e.Key)
			{
				case Key.Right:
				case Key.Space:
					nextImage = fileManager.GetNextImagePath(currentImage.FullPath);
					break;
				case Key.Left:
				case Key.Back:
					nextImage = fileManager.GetPreviousImagePath(currentImage.FullPath);
					break;
				default:
					if (keyManager.IsKeyAvaivable(e.Key.ToString()))
						return;

					var subfolderName = controlLinesManager.GetSubfolderName(e.Key.ToString());
					var isCopyFileMode = controlLinesManager.IsMoveFileMode(key);
					var imagePath = currentImage.FullPath;

					await Task.Factory.StartNew(() => fileManager.MoveFile(imagePath,
						subfolderName,
						isCopyFileMode));

					nextImage = fileManager.GetNextImagePath(currentImage.FullPath);
					break;
			}

			if (!string.IsNullOrEmpty(nextImage))
				ShowImage(nextImage);
		}

		private void FlyoutClose_Click(object sender, KeyEventArgs e)
		{
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
			UpdateSettings();
		}

		private void AppWindow_MouseMove(object sender, MouseEventArgs e)
		{
			if (ButtonsGrid.Visibility != Visibility.Visible)
			{
				ButtonsGrid.Visibility = Visibility.Visible;
				this.Cursor = Cursors.Arrow;
			}
			if (!isTimerEnabled)
			{
				StartCloseTimer();
			}
		}

		#endregion

		private void StartCloseTimer()
		{
			isTimerEnabled = true;
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(2d);
			timer.Tick += TimerTick;
			timer.Start();
		}

		private void TimerTick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;
			timer.Stop();
			timer.Tick -= TimerTick;
			UpdateSettings();
			ButtonsGrid.Visibility = Visibility.Hidden;
			this.Cursor = Cursors.None;
			isTimerEnabled = false;
		}
	}
}
