using DEKafkaMessageViewer.Views;
using Prism.Ioc;
using Prism.Unity;
using System.Windows;

namespace DEKafkaMessageViewer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : PrismApplication
	{
		//protected override void OnStartup(StartupEventArgs e)
		//{
		//	base.OnStartup(e);

		//	var bootstrapper = new Bootstrapper();
		//	bootstrapper.Run();
		//}

		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}
	}
}
