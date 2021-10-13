using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SampleBA
{
    /// <summary>
    /// Interaction logic for RootView.xaml
    /// </summary>
    public partial class RootView : Window
    {
        public RootView(RootViewModel viewModel)
        {
            this.DataContext = viewModel;
            this.Loaded += (sender, e) => CustomBA.Model.Engine.CloseSplashScreen();
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();

            this.InitializeComponent();

            viewModel.ViewWindowHandle = new WindowInteropHelper(this).EnsureHandle();
        }
        private void Background_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

    }
}
