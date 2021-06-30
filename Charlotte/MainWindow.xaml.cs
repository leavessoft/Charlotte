/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using ESRI.ArcGIS.Controls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Charlotte
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        readonly static string SupportedInExportFilter = "Plain text(*.txt)|*.txt|Comma Separated(*.csv)|*.csv|ESRI Shapefile(*.shp)|*.shp|Microsoft Excel(*.xlsx)|*.xlsx";

        string _workspacePath;
        string WorkspacePath
        {
            get
            {
                return _workspacePath;
            }

            set
            {
                _workspacePath = value;

                // Change title according to opened file
                titleLabel.Content = "Charlotte - " + (String.IsNullOrEmpty(value) ? "New Workspace" : value.Substring(value.LastIndexOf('\\') + 1).Replace("_", "__"));
                titleLabel.ToolTip = String.IsNullOrEmpty(value) ? "Path not specified" : value;
            }
        }

        private Workspace workspace;

        public Workspace Workspace
        {
            get { return workspace; }
            set
            {
                if (workspace == value)
                {
                    return;
                }

                workspace = value;
                this.DataContext = workspace;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // ESRI License
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.EngineOrDesktop);

            MouseDown += Window_MouseDown;

            WorkspacePath = null;

            // Load Workspace provided in the cmdline arguement
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (TryLoadWorkspace(args[1])) // success
                {
                    WorkspacePath = args[1];
                }
                else
                {
                    Workspace = new Workspace();
                }
            }
            else
            {
                Workspace = new Workspace();
            }

            //workspace.ConfigurationList.Add(new Configuration("Test", "key111", "2020-12-02=1024", 2000, 3));

            //_ = new WelcomePage().ShowAsync();

        }


        /// <summary>
        /// Move window when user dragging w/ cursor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Drop file on main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                int workspaceFileCount = 0;
                // Can have multiple files
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".clt"))
                    {
                        if (workspaceFileCount == 0)
                        { // for the first .clt file
                            if (TryLoadWorkspace(files[i])) // success
                            {
                                WorkspacePath = files[i];
                            }
                        }
                        else
                        {
                            // Run another copy with path as argument
                            Process.Start(Environment.GetCommandLineArgs()[0], files[i]);
                        }
                        workspaceFileCount++;
                    }
                    else if (files[i].EndsWith(".txt") || files[i].EndsWith(".csv") || files[i].EndsWith(".xlsx"))
                    {
                        DialogWindow dialog = DialogWindow.CreateConfirmCancelDialog(this, $"Confirm importing POI data from '{files[i]}'?", "Continue?");
                        dialog.PasswordBoxVisibility = Visibility.Hidden;
                        dialog.TextBoxVisibility = Visibility.Hidden;
                        if (dialog.ShowDialog() == true)
                        {
                            ImportPOIData(files[i]);
                        }
                    }
                    else
                    {
                        DialogWindow.ShowMessage(this, $"File format not supported. ({files[i].Substring(files[i].LastIndexOf('\\') + 1)})\n" +
                            "Supported format:\n" +
                            "* Charlotte Workspace:  .clt\n" +
                            "* Plain Text:   .txt .csv\n" +
                            "* Microsoft Excel:  .xlsx", "Input File Not Supported");
                    }
                }
            }
        }

        #region Menu Click Events
        private void MenuClick_New(object sender, RoutedEventArgs e)
        {
            WorkspacePath = null;
            Workspace = new Workspace();
        }

        private void MenuClick_Open(object sender, RoutedEventArgs e)
        {
            string path = SelectOpenPath();
            if (path != null)
            {
                if (TryLoadWorkspace(path))
                {
                    WorkspacePath = path;
                }
            }
        }

        private void MenuClick_Save(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            if (WorkspacePath == null)
            {
                MenuClick_SaveAs(sender, e);
                return;
            }

            workspace.Save(WorkspacePath);
        }

        private void MenuClick_SaveAs(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            string path = SelectSavePath();
            if (path != null) // do not change workspacePath unless user did select a new path
            {
                WorkspacePath = path;
            }

            workspace.Save(WorkspacePath);
        }

        private void MenuClick_Encrypt(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            string password = AskPassword((String.IsNullOrEmpty(_workspacePath)?"New Workspace": _workspacePath), 
                "Leave it empty to remove the password protection.");

            // Fix: Cannot remove password protection
            // We'll NOT check null/empty here
            //if (!String.IsNullOrEmpty(password))
            //{
                workspace.ChangePassword(password);
            //}
        }


        private void MenuClick_Export(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            string path;
            if ((path = SelectSavePath("Export", SupportedInExportFilter, "POI")) != null)
            {
                //try
                //{
                    int count = workspace.Export(path);

                    DialogWindow dialog = new DialogWindow(this)
                    {
                        PrimaryButtonText = "Done",
                        SecondaryButtonText = "Open",
                        MessageText = $"Successfully exported {count} record(s) to: \n{path}\n" +
                                        "You can click `Open` to open it.",
                        Title = "Finished"
                    };
                    dialog.SecondaryButtonCall = secondaryButtonCall;
                    dialog.PrimaryButtonCall = primaryButtonCall;
                    dialog.ShowDialog();

                    bool primaryButtonCall()
                    {
                        dialog.DialogResult = true;
                        dialog.Close();
                        return true;
                    };

                    bool secondaryButtonCall()
                    {
                        // Open exported document with default application
                        Process.Start(path);

                        dialog.DialogResult = true;
                        dialog.Close();
                        return true;
                    };
                //}
                //catch (Exception ex)
                //{
                //    DialogWindow.ShowMessage(this, "Cannot finish exporting because an error occured: \n" + ex.Message, "Failed");
                //}
            }
        }

        private void MenuClick_Import(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            string path;
            if ((path = SelectOpenPath("Import", SupportedInExportFilter)) != null)
            {
                ImportPOIData(path);
            }
        }

        private void MenuClick_EmptyPOIList(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            DialogWindow dialog = DialogWindow.CreateConfirmCancelDialog(this, $"Confirm emptying POI list?\nATTENTION: This operation cannot be undone.", "Continue?");
            dialog.PasswordBoxVisibility = Visibility.Hidden;
            dialog.TextBoxVisibility = Visibility.Hidden;
            if (dialog.ShowDialog() == true)
            {
                workspace.EmptyDataList();
            }
        }
        #endregion


        void ImportPOIData(string path)
        {
            try
            {
                int count = workspace.Import(path);
                DialogWindow.ShowMessage(this, $"Successfully imported {count} record(s) from: \n{path}", "Done");
            }
            catch (Exception ex)
            {
                DialogWindow.ShowMessage(this, "Cannot finish importing because an error occured: \n" + ex.Message, "Failed");
            }
        }

        bool TryLoadWorkspace(string path)
        {
            bool success = false;
            try
            {
                Workspace = Workspace.Load(path);
                success = true;
            }
            catch (FailedWorkspaceFailException ex)
            {
                // If first attempt fail, the file may have been encrypted or corrupted
                // We ask the user for password if PasswordRequired is true
                if (ex.PasswordRequired)
                {
                    string passwordAttempt = AskPassword(path);
                    if (!String.IsNullOrEmpty(passwordAttempt))
                    {
                        try
                        {
                            Workspace = Workspace.Load(path, passwordAttempt);
                            success = true;
                        }
                        catch (Exception ex2)
                        {
                            // If this fail again, the user shoud start over by clicking Open again
                            // There are only two attempts for solely one click so far
                            DialogWindow.ShowMessage(this, "Failed decrypting Workspace: wrong password or corrupted file.\n" +
                                "Error message: " + ex2.Message, "Failed Loading Workspace");
                        }
                    }
                }
                else
                {
                    DialogWindow.ShowMessage(this, "Failed loading Workspace:\n" + ex.Message);
                }
            }
            return success;
        }

        /// <summary>
        /// Ask for password
        /// </summary>
        /// <returns></returns>
        private string AskPassword(string workspacePathText = "", string extraMessage = "")
        {
            DialogWindow dialog = DialogWindow.CreateConfirmCancelDialog(this, "Password for the workspace:" + 
                (workspacePathText == "" ? "" : "\n" + workspacePathText) + 
                (extraMessage == "" ? "" : "\n" + extraMessage), "Password");
            dialog.PasswordBoxVisibility = Visibility.Visible;
            dialog.TextBoxVisibility = Visibility.Hidden;
            if (dialog.ShowDialog() == true)
            {
                return dialog.GetInput();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Open a file dialog for selecting file to OPEN
        /// </summary>
        /// <param name="title">title of dialog</param>
        /// <param name="filter">specify filter</param>
        /// <returns>path</returns>
        private string SelectOpenPath(string title = "Select Workspace", string filter = "Charlotte Workspace(*.clt)|*.clt")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.Title = title;
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Open a file dialog for selecting file to SAVE
        /// </summary>
        /// <param name="title">title of dialog</param>
        /// <param name="filter">specify filter</param>
        /// <returns>path</returns>
        private string SelectSavePath(string title = "Save Workspace", string filter = "Charlotte Workspace(*.clt)|*.clt", string defaultFile = "NewWorkspace.clt")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            saveFileDialog.Title = title;
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = defaultFile;
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        #region Data operations
        private void fetchDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (workspace != null)
            {
                workspace.SearchPoiAction();
            }
        }

        private void Command_Search(object sender, ExecutedRoutedEventArgs e)
        {
            searchBox.Focus();
        }

        private void View_Map(object sender, RoutedEventArgs e)
        {
            if (workspace != null)
            {
                new MapView(workspace).Show();
            }
        }
        #endregion

        #region Page control
        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (workspace != null)
            {
                workspace.CurrentPage--;
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (workspace != null)
            {
                workspace.CurrentPage++;
            }
        }
        #endregion

        #region Configuration Control
        private void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            workspace.CreateEmptyConfiguration();
        }

        private void RemoveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (workspace == null)
            {
                return;
            }

            workspace.RemoveConfiguration(configurationList.SelectedIndex);
        }
        #endregion

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }

    public class TitleTextConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">file path</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            string title = "Charlotte - ";
            if (String.IsNullOrEmpty(path))
            {
                title += "New Workspace";
            }
            else
            {
                int startAt = path.LastIndexOf('/') + 1;
                title += path.Substring(startAt);
            }
            return title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToFalseOtherwiseTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
