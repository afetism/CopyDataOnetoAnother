using System.Windows;

namespace CopyDataOnetoAnother;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		DataContext=new MainViewModel();
	}
}