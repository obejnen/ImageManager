using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows.Threading;
using System.Windows;

namespace ImageManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show(e.Exception.Message, "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);

			e.Handled = true;

			var window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(x => x.IsActive);
		}
	}
}
