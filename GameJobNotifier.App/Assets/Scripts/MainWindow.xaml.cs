using System.ComponentModel;
using System.Windows;
using GameJobNotifier.App.ViewModels;

namespace GameJobNotifier.App;

public partial class MainWindow : Window
{
    public bool ExitRequested { get; set; }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!ExitRequested)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }

        base.OnStateChanged(e);
    }
}
