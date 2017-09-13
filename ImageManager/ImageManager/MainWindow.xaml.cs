using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
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
			foreach(var set in settingsManager.AllSettings)
			{
				ControlLine cl = controlLinesManager.CreateControlLine(DeleteKeyButton_Click, SubfolderTextBox_TextChanged,
														KeyTextBox_TextChanged, Controls_KeyDown,
														set.Key, set.SubfolderName, set.IsCopyFileMode);
				AddControlLine(cl);
			}
		}

		private async void UpdateSettings()
		{
			List<string> bindedKeys = controlLinesManager.GetBindedKeys();
			List<string> subfolderNames = controlLinesManager.GetSubfolderNames();
			List<bool?> isMoveFileModes = controlLinesManager.GetFileMods();

			await Task.Factory.StartNew(() => settingsManager.RefreshSettings(subfolderNames, bindedKeys, isMoveFileModes));
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

		private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
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
					var previousImage = fileManager.GetPreviousImagePath(currentImage.FullPath);
					ShowImage(previousImage);
					return;
				}
				if (!keyManager.IsKeyAvaivable(e.Key.ToString()))
				{
					var subfolderName = controlLinesManager.GetSubfolderName(e.Key.ToString());
					bool? isCopyFileMode = controlLinesManager.IsMoveFileMode(key);
					var imagePath = currentImage.FullPath;

					await Task.Factory.StartNew(() => fileManager.MoveFile(imagePath,
											subfolderName,
											isCopyFileMode));
					
					var nextImage = fileManager.GetNextImagePath(currentImage.FullPath);
					ShowImage(nextImage);
                }
				//UpdateSettings();
			}
		}

		private void FlyoutClose_Click(object sender, KeyEventArgs e)
		{
			//UpdateSettings();
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

			//UpdateSettings();
		}

		private void SettingsFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
		{
			//UpdateSettings();
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
#endregion