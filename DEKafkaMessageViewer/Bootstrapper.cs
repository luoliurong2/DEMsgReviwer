using Unity;
using Prism.Unity;
using System.Windows;
using DEKafkaMessageViewer.Views;

namespace DEKafkaMessageViewer
{
	public class Bootstrapper:UnityBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void InitializeShell()
		{
			Application.Current.MainWindow.Show();
		}
	}
}
