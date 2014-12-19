using System.Windows;
using System.Windows.Media;

namespace ImageResizer
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void StartupHandler ( object sender, System.Windows.StartupEventArgs e )
		{
			Elysium.Manager.Apply( this, Elysium.Theme.Dark, Elysium.AccentBrushes.Blue, Brushes.White );
		}
	}
}
