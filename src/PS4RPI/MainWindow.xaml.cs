using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using McMaster.DotNet.Serve;
using PS4_Tools.LibOrbis.PKG;

//https://gist.github.com/flatz/60956f2bf1351a563f625357a45cd9c8

namespace PS4RPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string version = "v2.0-pre1";
        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private readonly bool argAutostart;
        private string pcip = null;
        private int pcport = 0;
        private string pcfolder = null;
        private RestClient client = null;
        private SimpleServer server = null;


        private bool _isbusy;
        public bool IsBusy
        {
            get { return _isbusy; }
            set { _isbusy = value; RaisePropertyChanged(); }
        }

        private PkgFile _selectedFile;
        public PkgFile SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<object> RootDirectoryItems { get; } = new ObservableCollection<object>();


        public MainWindow(string[] args)
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded; ;
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

            argAutostart = args.Contains("/autostart");
            Title += $" {version}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IsBusy = true;

            try
            {
                var settings = new SettingsWindow() { Owner = this };
                if (argAutostart || settings.ShowDialog().GetValueOrDefault())
                {
                    //todo validate all
                    pcip = settings.PcIp;
                    pcport = settings.PcPort;
                    var psip = settings.Ps4Ip;
                    pcfolder = settings.Folder.Trim();

                    if (!Uri.IsWellFormedUriString(pcfolder, UriKind.Absolute) && !Directory.Exists(pcfolder))
                    {
                        Application.Current.Shutdown(0);
                        return;
                    }
                    Title += $" | PC {pcip}:{pcport} | PS4 {psip}";
                    client = new RestClient(psip);
                    server = new SimpleServer(new IPAddress[] { IPAddress.Parse(pcip) }, pcport, pcfolder);
                    await server.Start(Cts.Token);
                }
                else
                {
                    Application.Current.Shutdown(0);
                    return;
                }

                //enumerate files
                await loadFileList();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (argAutostart)
                    message += "\n\nPlease run without /autostart argument and properly configure the app.";

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(0);
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;
            if (server!= null)
                server.Stop(new CancellationTokenSource(2000).Token).GetAwaiter().GetResult();
            Cts.Cancel();
        }

        private async Task loadFileList()
        {
            RootDirectoryItems.Clear();
            var root = new DirectoryInfo(pcfolder);

            //var dir = new PkgDirectory { DirectoryPath = root.FullName };
            //await Task.Run(() => getSubDirectoriesAndFiles(root, dir));
            //RootDirectoryItems.Add(dir);

            var list = new List<PkgFile>();

            await Task.Run(() =>
            {
                foreach (var file in root.GetFiles("*.pkg").OrderBy(x => x.Name))
                    list.Add(new PkgFile { FilePath = file.FullName, Length = ByteSizeLib.ByteSize.FromBytes(file.Length) });
            });

            foreach (var item in list)
                RootDirectoryItems.Add(item);
        }

        //private void getSubDirectoriesAndFiles(DirectoryInfo dir, UserDirectory ud)
        //{
        //    foreach (var file in dir.GetFiles("*.pkg").OrderBy(x => x.Name))
        //    {
        //        ud.Files.Add(new UserFile
        //        {
        //            FilePath = file.FullName
        //        });
        //    }

        //    foreach (var item in dir.GetDirectories().OrderBy(x => x.Name))
        //    {
        //        var d = new UserDirectory
        //        {
        //            DirectoryPath = item.FullName,
        //        };
        //        ud.Subfolders.Add(d);
        //        getSubDirectoriesAndFiles(item, d);
        //    }
        //}

        private async void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            //SelectedFile = tvPkgList.SelectedItem as UserFile;
            //if (SelectedFile == null)
            //    return;

            IsBusy = true;
            tbStats.Text = string.Empty;

            try
            {
                var relative = Path.GetRelativePath(pcfolder, SelectedFile.FilePath);
                var escapedRelative = string.Join('/', relative.Split(Path.DirectorySeparatorChar).Select(x => Uri.EscapeUriString(x)));
                var itemUrl = $"http://{pcip}:{pcport}/{escapedRelative}";

                var ins = new Models.RequestInstall
                {
                    packages = new List<string>() { itemUrl }
                };

                var result = await client.Install(ins, Cts.Token);

                if (result.status == "success")
                {
                    await Task.Delay(500);
                    
                    tbStats.Text = $"Success! Task id {result.task_id}\nKeep this progrem open while the console is downloading the file";

                    long progressError = 0;
                    try
                    {
                        var progress = await client.GetTaskProcess(new Models.RequestTaskID { task_id = result.task_id }, Cts.Token);
                        if (progress.status == "success")
                            progressError = progress.error;
                    }
                    catch (Exception)
                    {

                    }
                    
                    if (progressError != 0)
                        tbStats.Text += $"\nTask error! Error code {progressError}";
                }
                else
                {
                    tbStats.Text = JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                tbStats.Text = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void ButtonReloadFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsBusy = true;
                await loadFileList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }

        }

        private async void ButtonPkgInfo_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            IsBusy = true;
            tbStats.Text = null;

            try
            {
                var p = await Task.Run(() =>
                {
                    using (var stream = File.OpenRead(SelectedFile.FilePath))
                        return new PkgReader(stream).ReadPkg();
                });

                var paramSfo = p.ParamSfo.ParamSfo;
                var titleid = paramSfo.Values.Where(x => x.Name == "TITLE_ID").FirstOrDefault()?.ToString();

                var sb = new StringBuilder();
                sb.AppendLine("Reading PKG info from file");
                sb.AppendLine(SelectedFile.Name);
                sb.AppendLine(paramSfo.Values.Where(x => x.Name == "TITLE").FirstOrDefault()?.ToString());
                sb.AppendLine(titleid);
                sb.AppendLine(p.Header.content_id);

                try
                {
                    var a = await client.IsExists(new Models.RequestTitleID { title_id = titleid }, Cts.Token);
                    if (a.status == "success")
                    {
                        if (a.exists)
                            sb.AppendLine($"Package is installed | {ByteSizeLib.ByteSize.FromBytes(a.size).ToBinaryString()}");
                        else
                            sb.AppendLine("Package is not installed");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine("Error connecting to console");

                }


                tbStats.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"PS4RPI {version} - by Sonik\n\nSpecial thanks to flatz and all devs who keep the scene alive", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    //public class PkgDirectory
    //{
    //    public ObservableCollection<PkgFile> Files { get; } = new ObservableCollection<PkgFile>();
    //    public ObservableCollection<PkgDirectory> Folders { get; } = new ObservableCollection<PkgDirectory>();
    //    public IEnumerable Items { get { return Folders?.Cast<object>().Concat(Files); } }
    //    public string DirectoryPath { get; set; }
    //    public string Name
    //    {
    //        get
    //        {
    //            var dname = Path.GetFileName(DirectoryPath);
    //            return string.IsNullOrEmpty(dname) ? DirectoryPath : dname;
    //        }
    //    }
    //}

    public class PkgFile
    {
        public string FilePath { get; set; }
        public ByteSizeLib.ByteSize Length { get; set; }
        public string Name { get { return Path.GetFileName(FilePath); } }
    }
}
