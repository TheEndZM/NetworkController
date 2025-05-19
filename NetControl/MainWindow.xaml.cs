using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace NetControl;

public partial class MainWindow : Window {
    private string _currentRuleName;

    public MainWindow() {
        InitializeComponent();
    }


    private void SelectButton_Click(object sender, RoutedEventArgs e) {
        var openFileDialog = new OpenFileDialog {
            Filter = "可执行文件|*.exe",
            Title = "选择要控制的程序"
        };

        if (openFileDialog.ShowDialog() == true) {
            ExePathTextBox.Text = openFileDialog.FileName;
            _currentRuleName = $"Block_{Path.GetFileName(openFileDialog.FileName)}";
        }
    }

    private void ToggleNetworkButton_Click(object sender, RoutedEventArgs e) {
        if (string.IsNullOrEmpty(ExePathTextBox.Text)) return;

        var isBlocked = ToggleNetworkButton.Content.ToString() == "Disconnect";

        var ruleName = _currentRuleName;
        var exePath = ExePathTextBox.Text;

        if (isBlocked) {
            ExecuteCmd(
                $"netsh advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block program=\"{exePath}\" enable=yes");
            ToggleNetworkButton.Content = "Connect";
        }
        else {
            ExecuteCmd($"netsh advfirewall firewall delete rule name=\"{ruleName}\"");
            ToggleNetworkButton.Content = "Disconnect";
        }
    }

    protected override void OnClosed(EventArgs e) {
        if (!string.IsNullOrEmpty(_currentRuleName))
            ExecuteCmd($"netsh advfirewall firewall delete rule name=\"{_currentRuleName}\"");

        base.OnClosed(e);
    }

    private void ExecuteCmd(string command) {
        var process = new Process();
        var startInfo = new ProcessStartInfo {
            FileName = "cmd.exe",
            Arguments = "/C " + command,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.Verb = "runas";
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }
}