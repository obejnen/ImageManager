using System.Linq;
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
			MessageBox.Show("Something went wrong!!! - " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
			e.Handled = true;


			// this is how to get window, to show error somewhere on it instead of MessageBox
			var window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(x => x.IsActive);

			// but may be it will be better to show another custom window, configured for error message
		}
	}
}
