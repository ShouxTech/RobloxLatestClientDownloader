using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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

namespace RobloxLatestClientDownloader {
    public partial class MainWindow : Window {
        private const string robloxClientName = "RobloxPlayerBeta.exe";

        private string latestRobloxVersion;
        private string availableRobloxVersion;

        public MainWindow() {
            InitializeComponent();

            latestRobloxVersion = GetLatestRobloxVersion();
            availableRobloxVersion = GetAvailableRobloxVersion();

            SetLatestVersionLabel();
            SetAvailableVersionLabel();
        }

        private string GetLatestRobloxVersion() {
            if (!string.IsNullOrEmpty(latestRobloxVersion)) {
                return latestRobloxVersion;
            }

            using (HttpClient client = new HttpClient()) {
                return client.GetStringAsync("http://setup.roblox.com/version").Result;
            }
        }

        private string GetWindowsPlayerLineFromAllVersionLines(string[] allVersionLines, int index) {
            string line = allVersionLines[index];

            if (!line.Contains("WindowsPlayer")) {
                return GetWindowsPlayerLineFromAllVersionLines(allVersionLines, index - 1);
            }

            return line;
        }

        private string GetAvailableRobloxVersion() {
            if (!string.IsNullOrEmpty(availableRobloxVersion)) {
                return availableRobloxVersion;
            }

            using (HttpClient httpClient = new HttpClient()) {
                string allVersions = httpClient.GetStringAsync("http://setup.roblox.com/DeployHistory.txt").Result;
                string[] lines = allVersions.Split('\n');
                string versionLine = GetWindowsPlayerLineFromAllVersionLines(lines, lines.Length - 1);
                MatchCollection regexMatch = Regex.Matches(versionLine, "version-\\S+");
                return regexMatch[0].Value;
            }
        }

        private void SetLatestVersionLabel() {
            CurrentVersionLabel.Content = "Current Roblox Version: " + latestRobloxVersion;
        }

        private void SetAvailableVersionLabel() {
            AvailableVersionLabel.Content = "Available Roblox Version: " + availableRobloxVersion;
        }

        private void DownloadAvailableVersion() {
            using (WebClient webClient = new WebClient()) {
                webClient.DownloadFile("https://setup.rbxcdn.com/" + availableRobloxVersion + "-RobloxApp.zip", availableRobloxVersion + ".zip");
            }
        }

        // Button methods.

        private void DownloadAvailableBtn_Click(object sender, RoutedEventArgs e) {
            DownloadAvailableVersion();
            MessageBox.Show("Downloaded the latest available Roblox version.", "Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReplaceVersionBtn_Click(object sender, RoutedEventArgs e) {
            string zipPath = availableRobloxVersion + ".zip";

            if (!File.Exists(zipPath)) {
                DownloadAvailableVersion();
            }

            using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                foreach (ZipArchiveEntry entry in archive.Entries) {
                    if (entry.Name == robloxClientName) {
                        entry.ExtractToFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Roblox\Versions\" + latestRobloxVersion + "\\" + entry.Name, true);

                        MessageBox.Show("Replaced the current version of Roblox with the latest available version.", "Replaced", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}