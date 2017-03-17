using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OperatorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ViewModel _viewModel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            RobotServiceConnection.OnPong += OnPong;
            RobotServiceConnection.OnLog += OnLog;
            var connTask = RobotServiceConnection.Run(_cts.Token);

            DataContext = _viewModel;
            _viewModel.InitializeLayoutImage(this);
        }

        

        ~MainWindow()
        {
            RobotServiceConnection.OnPong -= OnPong;
        }
        
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _cts.Cancel();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await RobotServiceConnection.Ping(_cts.Token);
        }

        private void OnPong()
        {
            ++_viewModel.Counter;
        }

        private void OnLog(string msg)
        {
            if (msg.StartsWith("TRACE", StringComparison.Ordinal))
            {
                _viewModel.AddLogOutput(new Trace(msg));
            }
            else if (msg.StartsWith("DEBUG", StringComparison.Ordinal))
            {
                _viewModel.AddLogOutput(new Debug(msg));
            }
            else if (msg.StartsWith("ERROR", StringComparison.Ordinal))
            {
                _viewModel.AddLogOutput(new Error(msg));
            }
            else if (msg.StartsWith("FATAL", StringComparison.Ordinal))
            {
                _viewModel.AddLogOutput(new Fatal(msg));
            }
            else if (msg.StartsWith("INFO", StringComparison.Ordinal))
            {
                _viewModel.AddLogOutput(new Info(msg));
            }
            else
            {
                return;
            }

        }
    }
}
