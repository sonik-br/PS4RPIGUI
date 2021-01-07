using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PS4RPI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private bool _isbusy;
        public bool IsBusy
        {
            get { return _isbusy; }
            set { _isbusy = value; RaisePropertyChanged(); }
        }

        private string _pcIp;
        public string PcIp
        {
            get { return _pcIp; }
            set { _pcIp = value?.Trim(); RaisePropertyChanged(); }
        }

        private int _pcPort = 8080;
        public int PcPort
        {
            get { return _pcPort; }
            set { _pcPort = value; RaisePropertyChanged(); }
        }

        private string _ps4Ip;
        public string Ps4Ip
        {
            get { return _ps4Ip; }
            set { _ps4Ip = value?.Trim(); RaisePropertyChanged(); }
        }

        private string _folder;
        public string Folder
        {
            get { return _folder; }
            set { _folder = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<string> LocalIpList { get; } = new ObservableCollection<string>();


        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Get host name
            var strHostName = Dns.GetHostName();

            // Find host by name
            var iphostentry = Dns.GetHostEntry(strHostName);//Dns.GetHostByName(strHostName);

            //Enumerate IP addresses
            foreach (var item in iphostentry.AddressList.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x.ToString()))
                LocalIpList.Add(item);

            PcIp = ConfigurationManager.AppSettings["pcip"];

            if (int.TryParse(ConfigurationManager.AppSettings["pcport"], out int pcport))
                PcPort = pcport;

            Ps4Ip = ConfigurationManager.AppSettings["ps4ip"];
            Folder = ConfigurationManager.AppSettings["folder_or_url"];

            if (string.IsNullOrEmpty(PcIp))
                PcIp = LocalIpList.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Folder))
                    dialog.SelectedPath = Folder;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Folder = dialog.SelectedPath;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (!IPAddress.TryParse(PcIp, out _))
            {
                MessageBox.Show("Invalid PC IP", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IPAddress.TryParse(Ps4Ip, out _))
            {
                MessageBox.Show("Invalid PS4 IP", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Folder) || !new System.IO.DirectoryInfo(Folder).Exists)
            {
                MessageBox.Show("Invalid Folder", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings.Clear();
                config.AppSettings.Settings.Add("pcip", PcIp);
                config.AppSettings.Settings.Add("pcport", PcPort.ToString());
                config.AppSettings.Settings.Add("ps4ip", Ps4Ip);
                config.AppSettings.Settings.Add("folder_or_url", Folder.Trim());

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                MessageBox.Show("Settings Saved!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

    }
}
