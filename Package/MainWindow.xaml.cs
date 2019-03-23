using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;
using System.IO;
using System.Security.AccessControl;
using System.Threading;


namespace Package
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool drag = false;
        string keyname = null;
       
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                keyname = check();
                if (keyname != null)
                    changeUninstallUI();
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.StackTrace);
            }
        }

        private void unInstall(string keyname)
        {
            string checkSystem_user = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\" + keyname;
            string checkSystem_root = @"WOW6432Node\CLSID\" + keyname;
            RegistryKey userKey = Registry.CurrentUser;
            userKey.DeleteSubKey(checkSystem_user);
            RegistryKey rootKey = Registry.ClassesRoot;
            RegistryKey pathKey = rootKey.OpenSubKey(checkSystem_root + @"\Instance\InitPropertyBag", true);
            string folderPath = pathKey.GetValue("target").ToString();
            pathKey.Close();
            rootKey.DeleteSubKeyTree(checkSystem_root);
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            directoryInfo.Delete(true);
        }
        private void changeUninstallUI()
        {
            button_install.Content = "Uninstall";
            textPath.Visibility = Visibility.Hidden;
            folder_grid.Visibility = Visibility.Hidden;
        }
        private string check()
        {
            string[] keyNames;
            string user_namespace = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace";
            RegistryKey userKey = Registry.CurrentUser;
            userKey.CreateSubKey(user_namespace,true);
            RegistryKey spaceKey = userKey.OpenSubKey(user_namespace, true);
            keyNames = spaceKey.GetSubKeyNames();
            if (keyNames.Length == 0)
                return null;
            foreach(string keyName in keyNames)
            {
                RegistryKey temp = spaceKey.OpenSubKey(keyName, true);
                if (temp!=null && temp.GetValue("").ToString() == "CheckSystem")
                {
                    temp.Close();
                    return keyName;
                }
                temp.Close();
            }
            spaceKey.Close();
            return null;
        }
  
        private void button_install_MouseEnter(object sender, MouseEventArgs e)
        {
            BrushConverter foreConverter = new BrushConverter();
            Brush forebrush = (Brush)foreConverter.ConvertFromString("White");
            BrushConverter backConverter = new BrushConverter();
            Brush backbrush = (Brush)backConverter.ConvertFromString("#FF80DEEA");
            button_install.Foreground = forebrush;
            button_install.Background = backbrush;
        }

        private void button_install_MouseLeave(object sender, MouseEventArgs e)
        {
            BrushConverter foreConverter = new BrushConverter();
            Brush forebrush = (Brush)foreConverter.ConvertFromString("#FF80DEEA");
            BrushConverter backConverter = new BrushConverter();
            Brush backbrush = (Brush)backConverter.ConvertFromString("White");
            button_install.Foreground = forebrush;
            button_install.Background = backbrush;
        }

        private void Grid_close_MouseEnter(object sender, MouseEventArgs e)
        {
            button_x.Opacity = 1;
        }

        private void Grid_close_MouseLeave(object sender, MouseEventArgs e)
        {
            button_x.Opacity = 0;
        }

        private void Grid_close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Grid_min_MouseEnter(object sender, MouseEventArgs e)
        {
            button__.Opacity = 1;
        }

        private void Grid_min_MouseLeave(object sender, MouseEventArgs e)
        {
            button__.Opacity = 0;
        }

        private void Grid_min_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void folder_grid_MouseEnter(object sender, MouseEventArgs e)
        {
            BrushConverter brushConverter = new BrushConverter();
            Brush brush = (Brush)brushConverter.ConvertFromString("#FF6ABFCA");
            folder.Fill = brush;
        }

        private void folder_grid_MouseLeave(object sender, MouseEventArgs e)
        {
            BrushConverter brushConverter = new BrushConverter();
            Brush brush = (Brush)brushConverter.ConvertFromString("#FF80DEEA");
            folder.Fill = brush;
        }

        private void folder_grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog newfolder = new System.Windows.Forms.FolderBrowserDialog();
            newfolder.RootFolder = Environment.SpecialFolder.CommonProgramFilesX86;
            newfolder.Description = "Plese select a path";
            newfolder.ShowDialog();
            if (newfolder.SelectedPath != null)
                textPath.Text = newfolder.SelectedPath;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && drag)
                this.DragMove();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            drag = false;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            drag = true;
        }

        private void button_install_Click(object sender, RoutedEventArgs e)
        {
            button_install.IsEnabled = false;
            if (keyname==null)
            {
                button_install.Content = "Installing...";
                textPath.Visibility = Visibility.Hidden;
                folder_grid.Visibility = Visibility.Hidden;                                
                Install();              
            }
            else
            {
                button_install.Content = "Uninstalling..";
                unInstall(keyname);
            }
            InstallProgress.Visibility = Visibility.Visible;
            ThreadPool.QueueUserWorkItem(DoProgress);
        }

        private void Install()
        {
            string newpath = @textPath.Text + "\\CheckSystem";
            string exePath = newpath + "\\CheckSystem.exe";
            string iconPath = newpath + "\\check.ico";
            if (!Directory.Exists(newpath))
                Directory.CreateDirectory(newpath);
            FileStream exeFile = new FileStream(exePath, FileMode.OpenOrCreate);
            exeFile.Write(Resource.CheckSystem, 0, Resource.CheckSystem.Length);
            exeFile.Close();
            FileStream iconFile = new FileStream(iconPath, FileMode.OpenOrCreate);
            Resource.check.Save(iconFile);
            iconFile.Close();

            Guid guid = new Guid();
            guid = Guid.NewGuid();
            string guidStr = guid.ToString();

            string guidPath_root = @"HKEY_CLASSES_ROOT\CLSID\{" + guidStr + "}";
            string DefaultIcon = guidPath_root + "\\DefaultIcon";
            string Instance = guidPath_root + "\\Instance";
            string InitPropertyBag = Instance + "\\InitPropertyBag";
            string Command = guidPath_root + @"\Shell\Open\Command";

            Registry.SetValue(guidPath_root, "InfoTip", "double click to run", RegistryValueKind.String);
            Registry.SetValue(guidPath_root, "LocalizedString", "CheckSystem", RegistryValueKind.String);
            Registry.SetValue(guidPath_root, "", "CheckSystem", RegistryValueKind.String);
            Registry.SetValue(guidPath_root, "System.ItemAuthors", "double click to run", RegistryValueKind.String);
            Registry.SetValue(guidPath_root, "TitleInfo", "prop:System.ItemAuthors", RegistryValueKind.String);

            Registry.SetValue(DefaultIcon, "", iconPath, RegistryValueKind.ExpandString);

            Registry.SetValue(Instance, "CLSID", "{" + guidStr + "}", RegistryValueKind.String);

            Registry.SetValue(InitPropertyBag, "Target", newpath, RegistryValueKind.String);

            Registry.SetValue(Command, "", exePath, RegistryValueKind.String);

            string guiaPath_user = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{" + guidStr + "}";
            Registry.SetValue(guiaPath_user, "", "CheckSystem", RegistryValueKind.String);
        }

        private void DoProgress(object obj)
        {
            for(int i=0;i<101;i++)
            {
                Thread.Sleep(10);
                Action<int> updateUI = new Action<int>(updatePrograssBar);
                this.Dispatcher.Invoke(updateUI, i);
            }
        }

        void updatePrograssBar(int theValue)
        {
            InstallProgress.Value = theValue;
            if (theValue == 100)
            {
                complete.Visibility = Visibility.Visible;
                button_install.Visibility = Visibility.Hidden;
                InstallProgress.Visibility = Visibility.Hidden;
            }
        }
    }
}
