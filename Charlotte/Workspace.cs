/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using AmapAPITool.AmapAPI.Entity;
using Charlotte.Util;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Charlotte
{
    public class Workspace : INotifyPropertyChanged
    {
        //private DatabaseManager _databaseManager;

        Thread fetchThread = null;

        // It's okay to save password in workspace file 'cos once the pw is set, the field itself'll be encrypted as well
        [JsonProperty]
        private string _password;

        private ObservableCollection<Configuration> cofigurationList = new ObservableCollection<Configuration>();
        public ObservableCollection<Configuration> ConfigurationList
        {
            get { return this.cofigurationList; }
            set
            {
                if (this.cofigurationList == value)
                {
                    return;
                }

                this.cofigurationList = value;
                this.NotifyPropertyChanged("ConfigurationList");
            }
        }


        private ObservableCollection<POI> displayList = new ObservableCollection<POI>();
        public ObservableCollection<POI> DisplayList
        {
            get { return this.displayList; }
            set
            {
                // Prevent avoiding notifying because we need to refresh display list by using `DisplayList = displaylist`
                //if (this.displayList == value)
                //{
                //    return;
                //}

                this.displayList = value;
                this.NotifyPropertyChanged("DisplayList");
            }
        }

        private List<POI> dataList = new List<POI>();
        public List<POI> DataList
        {
            get { return this.dataList; }
            set
            {
                if (this.dataList == value)
                {
                    return;
                }

                this.dataList = value;
                this.NotifyPropertyChanged("DataList");
            }
        }

        private List<POI> filteredList = new List<POI>();
        public List<POI> FilteredList
        {
            get { return this.filteredList; }
            set
            {
                if (this.filteredList == value)
                {
                    return;
                }

                this.filteredList = value;
                this.NotifyPropertyChanged("FilteredList");
            }
        }


        private string localSearchKeyword;
        public string LocalSearchKeyword
        {
            get { return this.localSearchKeyword; }
            set
            {
                if (this.localSearchKeyword == value)
                {
                    return;
                }

                localSearchKeyword = value;
                this.NotifyPropertyChanged("LocalSearchKeyword");

                if (String.IsNullOrEmpty(value))
                {
                    CurrentList = DataList;
                }
                else
                {
                    FilteredList = SearchInDataList(value);
                    CurrentList = FilteredList;
                }

                RefreshPagination();
            }
        }

        #region FetchSettings

        private bool append;
        public bool Append
        {
            get { return this.append; }
            set
            {
                if (append == value)
                {
                    return;
                }

                append = value;
                this.NotifyPropertyChanged("Append");
            }
        }

        private string keyword;
        public string Keyword
        {
            get { return this.keyword; }
            set
            {
                if (keyword == value)
                {
                    return;
                }

                keyword = value;
                this.NotifyPropertyChanged("Keyword");
            }
        }

        private string city;
        public string City
        {
            get { return this.city; }
            set
            {
                if (city == value)
                {
                    return;
                }

                city = value;
                this.NotifyPropertyChanged("City");
            }
        }

        private string offset;
        public string Offset
        {
            get { return this.offset; }
            set
            {
                if (offset == value)
                {
                    return;
                }

                offset = value;
                this.NotifyPropertyChanged("Offset");
            }
        }

        private string interval;
        public string Interval
        {
            get { return this.interval; }
            set
            {
                if (interval == value)
                {
                    return;
                }

                interval = value;
                this.NotifyPropertyChanged("Interval");
            }
        }

        #endregion

        #region ProgressIndicator
        private int currentProgressValue = 0;
        public int CurrentProgressValue
        {
            get
            {
                return this.currentProgressValue;
            }

            set
            {
                if (currentProgressValue == value)
                {
                    return;
                }

                this.currentProgressValue = value;
                this.NotifyPropertyChanged("CurrentProgressValue");
            }
        }

        private int maxProgressValue = 1;
        public int MaxProgressValue
        {
            get
            {
                return this.maxProgressValue;
            }

            set
            {
                if (maxProgressValue == value)
                {
                    return;
                }

                this.maxProgressValue = value;
                this.NotifyPropertyChanged("MaxProgressValue");
            }
        }

        [JsonIgnore]
        private bool _processbarIndeterminate;
        [JsonIgnore]
        public bool ProcessbarIndeterminate
        {
            get
            {
                return _processbarIndeterminate;
            }

            set
            {
                if (_processbarIndeterminate == value)
                {
                    return;
                }

                this._processbarIndeterminate = value;
                this.NotifyPropertyChanged("ProcessbarIndeterminate");
            }
        }

        [JsonIgnore]
        private bool _processing;
        [JsonIgnore]
        public bool Processing
        {
            get
            {
                return _processing;
            }

            set
            {
                if (_processing == value)
                {
                    return;
                }

                _processing = value;
                this.ProcessingBarVisible = _processing ? Visibility.Visible : Visibility.Hidden;
                this.FetchButtonText = _processing ? "Stop" : "Fetch Data";
            }
        }

        // Whether or not show progressbar
        [JsonIgnore]
        private Visibility processingBarVisible = Visibility.Hidden;
        [JsonIgnore]
        public Visibility ProcessingBarVisible
        {
            get
            {
                return processingBarVisible;
            }

            set
            {
                if (processingBarVisible == value)
                {
                    return;
                }

                processingBarVisible = value;
                this.NotifyPropertyChanged("processingBarVisible");
            }
        }

        [JsonIgnore]
        private string fetchButtonText = "Fetch Data";
        [JsonIgnore]
        public string FetchButtonText
        {
            get
            {
                return fetchButtonText;
            }
            
            set
            {
                if (fetchButtonText == value)
                {
                    return;
                }

                fetchButtonText = value;
                this.NotifyPropertyChanged("FetchButtonText");
            }
        }
        #endregion

        #region PaginationProperties
        private List<POI> _currentList;
        public List<POI> CurrentList
        {
            get { return this._currentList; }
            set
            {
                if (_currentList == value)
                {
                    return;
                }

                this._currentList = value;
                this.NotifyPropertyChanged("CurrentList");
                RefreshPagination();
            }
        }

        private int currentPage;
        public int CurrentPage
        {
            get { return this.currentPage; }
            set
            {
                if (value >= 1 && value <= MaxPage)
                {
                    currentPage = value;
                }
                else if (value < 1)
                {
                    currentPage = MaxPage;
                }
                else if (value > MaxPage)
                {
                    currentPage = Math.Min(1, MaxPage);
                }

                RefreshDataPage();
                this.NotifyPropertyChanged("CurrentPage");
            }
        }

        private int maxPage;
        public int MaxPage
        {
            get { return this.maxPage; }
            set
            {
                if (maxPage != value)
                {
                    maxPage = value;
                    this.NotifyPropertyChanged("MaxPage");
                }
            }
        }

        private int recordPerPage;
        public int RecordPerPage
        {
            get { return this.recordPerPage; }
            set
            {
                recordPerPage = value;
                MaxPage = (int)Math.Ceiling((float)CurrentList.Count / (float)recordPerPage);
                if (CurrentPage > MaxPage)
                {
                    CurrentPage = MaxPage;
                }
                this.NotifyPropertyChanged("RecordPerPage");
            }
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }


        /// <summary>
        /// Constructs a new workspace
        /// </summary>
        /// <param name="password">nullable</param>
        public Workspace(string password = null)
        {
            _password = password;
            _currentList = DataList;

            //bool needInit = !File.Exists(fileName);

            //_databaseManager = new DatabaseManager(fileName);

            //if (needInit)
            //{
            //    InitDatabase();
            //}
        }
        
        /// <summary>
        /// Load Workspace from saved file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password">nullable</param>
        /// <returns>Loaded Workspace</returns>
        public static Workspace Load(string path, string password = null)
        {
            string input = File.ReadAllText(path);
            string json = input;
            
            if (json.Length < 2) // Not a valid saved Workspace
            {
                throw new FailedWorkspaceFailException("Selected file is not a valid Workspace", false);
            }
            if (!json.StartsWith("{"))
            {
                if (password == null) // Password required
                {
                    throw new FailedWorkspaceFailException("The Workspace is protected by password", true);
                }

                json = StringCipher.Decrypt(input, password);
            }

            try
            {
                Workspace workspace = JsonConvert.DeserializeObject<Workspace>(json);
                workspace.RefreshPagination();
                workspace.Processing = false; // Init fetch button text
                //workspace.RecordPerPage = workspace.recordPerPage;
                //workspace.RefreshDataPage();
                return workspace;
            }
            catch // if deserialization not work, user might had entered a wrong password or the file corrupted
            {
                throw new FailedWorkspaceFailException("File corrupted or wrong password", true);
            }
        }

        /// <summary>
        /// Try terminating the fetchThread on destruction
        /// </summary>
        ~Workspace()
        {
            if (fetchThread != null && !fetchThread.IsAlive)
            {
                fetchThread.Abort();
            }
        }

        /// <summary>
        /// Save workspace as a json file
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename)
        {
            string json = JsonConvert.SerializeObject(this);
            if (!String.IsNullOrEmpty(_password))
            {
                json = StringCipher.Encrypt(json, _password);
            }
            File.WriteAllText(filename, json);
        }

        #region Import
        /// <summary>
        /// Import POI data from external file to datalist
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>record count</returns>
        public int Import(string filename)
        {
            if (!(filename.EndsWith(".txt") || filename.EndsWith(".csv") || filename.EndsWith(".xlsx")))
            {
                throw new Exception("Target type not supported");
            }

            List<POI> imported = new List<POI>();
            DataTable dataTable;

            if (filename.EndsWith(".txt") || filename.EndsWith(".csv"))
            { // .txt or .csv
                dataTable = PlainTextToTable(filename, filename.EndsWith(".csv") ? "," : "\t\t", filename.EndsWith(".csv"));
            }
            else // .xlsx
            {
                dataTable = ExcelToTable(filename);
            }

            foreach (DataRow row in dataTable.Rows)
            {
                POI poi = new POI();
                foreach (DataColumn column in dataTable.Columns)
                {
                    var property = typeof(POI).GetProperty(column.ColumnName);
                    if (column.DataType.Equals(typeof(string)))
                    {
                        if (row[column] != null && row[column].GetType() == typeof(string))
                        {
                            property.SetValue(poi, row[column].Equals("null") ? null : row[column], null);
                        }
                    }
                }

                imported.Add(poi);
            }
            DataList.AddRange(imported);

            CurrentList = DataList;
            RefreshPagination();

            return imported == null ? 0 : imported.Count;
        }

        /// <summary>
        /// Read plain text and parse
        /// </summary>
        /// <param name="path">suppoted format: .txt, .csv</param>
        /// <param name="separator"></param>
        /// <param name="escapeAndQuote">for .csv files</param>
        /// <returns>DataTable of POI list</returns>
        private DataTable PlainTextToTable(string path, string separator, bool escapeAndQuote = false)
        {
            DataTable dataTable = new DataTable();
            if (path.EndsWith(".csv"))
            {
                CsvReader reader = new CsvReader(path);
                dataTable = reader.Table;
            }
            else if (path.EndsWith(".txt"))
            {
                string[] seps = { "\t\t" };
                string[] colFields = null;

                foreach (var line in File.ReadLines(path))
                {
                    if (line == "")
                    {
                        continue;
                    }

                    string[] fields = line.Split(seps, StringSplitOptions.None);

                    if (colFields == null)
                    {
                        colFields = fields;
                        foreach (string column in colFields)
                        {
                            DataColumn datacolumn = new DataColumn(column);
                            datacolumn.AllowDBNull = true;
                            dataTable.Columns.Add(datacolumn);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (fields[i] == "")
                            {
                                fields[i] = null;
                            }
                        }
                        dataTable.Rows.Add(fields);
                    }
                }
            }
            else
            {
                throw new Exception("Imported file format not supported");
            }

            return dataTable;
        }

        /// <summary>
        /// Read excel and parse
        /// </summary>
        /// <param name="path">path to .xlsx</param>
        /// <returns>DataTable of POI list</returns>
        private DataTable ExcelToTable(string path)
        {
            var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheet(1);

            DataTable dataTable = new DataTable();
            for (int row = 1; row <= sheet.RowCount(); row++)
            {
                if (row == 1) // header
                {
                    for (int column = 1; column <= sheet.ColumnCount(); column++)
                    {
                        string value = (string)sheet.Cell(row, column).Value;
                        if (!String.IsNullOrEmpty(value))
                        {
                            DataColumn dataColumn = new DataColumn(value);
                            dataColumn.AllowDBNull = true;
                            dataTable.Columns.Add(dataColumn);
                        }
                    }
                }
                else
                {
                    string[] fields = new string[dataTable.Columns.Count];
                    bool hasValue = false;
                    for (int column = 1; column <= dataTable.Columns.Count ; column++)
                    {
                        fields[column - 1] = (string)sheet.Cell(row, column).Value;
                        if (!String.IsNullOrEmpty((string)sheet.Cell(row, column).Value))
                        {
                            hasValue = true;
                        }
                    }
                    if (!hasValue)// empty row
                    {
                        break;
                    }
                    dataTable.Rows.Add(fields);
                }
            }

            return dataTable;
        }
        #endregion

        #region Export
        /// <summary>
        /// Supported export file type:
        /// .txt; .csv; .xlsx
        /// </summary>
        /// <param name="filename">must contain a supported extension</param>
        /// <returns>record count</returns>
        public int Export(string filename)
        {
            if (!(filename.EndsWith(".txt") || filename.EndsWith(".csv") || filename.EndsWith(".xlsx"))) {
                throw new Exception("Target type not supported");
            }

            if (filename.EndsWith(".txt") || filename.EndsWith(".csv")) { // .txt or .csv
                CurrentListAsPlainText(filename, filename.EndsWith(".csv") ? "," : "\t\t", filename.EndsWith(".csv"));
            }
            else // .xlsx
            {
                CurrentListAsExcel(filename);
            }

            return CurrentList.Count;
        }

        /// <summary>
        /// Export CurrentList to .xlsx
        /// </summary>
        /// <param name="path">path to target file</param>
        private void CurrentListAsExcel(string path)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("POI");

                for (int i=0; i<CurrentList.Count; i++)
                {
                    POI poi = CurrentList[i];

                    List<KeyValuePair<string, string>> properties = poi.GetPropertyList();

                    if (i == 0) // Create header
                    {
                        for (int column = 1; column <= properties.Count; column++)
                        {
                            string name = properties[column - 1].Key;
                            worksheet.Cell(1, column).SetValue<string>(name);
                            worksheet.Cell(1, column).Style.Fill.BackgroundColor = XLColor.LightSlateGray;
                            worksheet.Cell(1, column).Style.Font.FontColor = XLColor.White;
                        }
                    }

                    for (int column = 1; column <= properties.Count; column++)
                    {
                        string value = properties[column - 1].Value;
                        worksheet.Cell(i+2, column).SetValue<string>(value == null ? "null" : value);
                    }
                }

                workbook.SaveAs(path);
            }
        }

        /// <summary>
        /// Export CurrentList to plain text
        /// </summary>
        /// <param name="path">supported format: .txt, .csv</param>
        /// <param name="separator"></param>
        /// <param name="escapeAndQuote"></param>
        private void CurrentListAsPlainText(string path, string separator, bool escapeAndQuote = false)
        {
            if (CurrentList == null)
            {
                return;
            }

            string result = "";
            bool firstLine = true;
            foreach (POI poi in CurrentList)
            {
                List<KeyValuePair<string, string>> properties = poi.GetPropertyList();

                if (firstLine) // Create header
                {
                    for (int i = 0; i<properties.Count; i++)
                    {
                        string name = properties[i].Key;
                        result += name + (i < properties.Count - 1 ? separator : "\n");
                    }
                    firstLine = false;
                }

                for (int i = 0; i < properties.Count; i++)
                {
                    string value = properties[i].Value;
                    if (value != null && escapeAndQuote)
                    {
                        value = value.Replace("\"", "\"\"");
                        value = $"\"{value}\"";
                    }
                    result += (value == null ? "null" : value) + (i < properties.Count - 1 ? separator : "\n");
                }
            }

            File.WriteAllText(path, result);
        }
        #endregion

        /// <summary>
        /// Clear DataList and let CurrentList=DataList
        /// </summary>
        public void EmptyDataList()
        {
            DataList.Clear();
            CurrentList = DataList;
            
        }

        /// <summary>
        /// Search keywords in DataList
        /// </summary>
        /// <param name="keywords">separate by space(" ")</param>
        /// <returns>filtered list</returns>
        public List<POI> SearchInDataList(string keywords)
        {
            List<POI> filtered = new List<POI>();
            DataList.ForEach(poi =>
            {
                if (poi.Match(keywords.Split(' ')))
                {
                    filtered.Add(poi);
                }
            });
            return filtered;
        }

        /// <summary>
        /// Start a search thread
        /// </summary>
        public void SearchPoiAction()
        {
            CurrentList = DataList;

            // Stop previous fetching if Processing=true
            if (Processing)
            {
                Processing = false;
                return;
            }

            if (!Append)
            {
                DataList.Clear();
            }

            if (fetchThread != null && !fetchThread.IsAlive)
            {
                fetchThread.Abort();
            }

            fetchThread = new Thread(e =>
            {
                this.Processing = true;
                try
                {
                    if (!String.IsNullOrEmpty(Keyword))
                    {
                        SearchPoi(Keyword, City, int.Parse(Offset));
                    }
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        DialogWindow.ShowMessage(null, "We encountered an error when fetching data:\n" + ex.Message, "Sorry");
                    });
                }
                finally
                {
                    this.Processing = false;
                }
            });

            fetchThread.Start();

            // Internal POI fetching method
            void SearchPoi(string keyword, string city, int recordPerPage = 50, int maxRecords = -1)
            {
                int totalRecord = 0;

                List<POI> resultList = SearchPOIRequest(keyword, city, recordPerPage, 1, out totalRecord);

                MaxProgressValue = (int)Math.Ceiling((float)totalRecord / (float)recordPerPage);
                CurrentProgressValue = 1;

                Thread.Sleep(int.Parse(Interval));

                if (resultList.Count >= 0)
                {
                    if (totalRecord > recordPerPage)
                    {
                        for (int i = 2; i <= MaxProgressValue && Processing; i++)
                        {
                            List<POI> pendingList = SearchPOIRequest(keyword, city, recordPerPage, i, out totalRecord);
                            resultList.AddRange(pendingList);
                            CurrentProgressValue++;
                            Thread.Sleep(int.Parse(Interval));
                        }
                    }
                }

                resultList.ForEach((Action<POI>)(poi =>
                {
                // Update displaylist in main thread
                App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.DataList.Add(poi);
                    });
                }));

                // TODO: Message when no data was fetched


                // Update displaylist in main thread
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.Processing = true;
                //MaxPage = (int)Math.Ceiling((float)DataList.Count / (float)int.Parse(offset));
                RecordPerPage = recordPerPage;
                    CurrentPage = 1;
                //RefreshDataPage();
                });
            }
        }

        /// <summary>
        /// Capsuled POI search method of API service
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="city"></param>
        /// <param name="recordPerPage"></param>
        /// <param name="page">target page</param>
        /// <param name="total">count of all records</param>
        /// <returns></returns>
        private List<POI> SearchPOIRequest(string keyword, string city, int recordPerPage, int page, out int total)
        {

            Configuration config = null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while(config == null)
            {
                ProcessbarIndeterminate = true;
                config = SelectConfig();
                if (config == null && stopwatch.ElapsedMilliseconds > 10000) // selecting time-out
                {
                    stopwatch.Stop();
                    ProcessbarIndeterminate = false;
                    throw new Exception("No configuration available");
                }
            }
            ProcessbarIndeterminate = false;

            if (config != null) // redundant, unnecessary
            {
                config.UpdateCount();
                return config.APIService.SearchPoi(keyword, city, recordPerPage, page, out total);
            }

            total = 0;
            return new List<POI>();
        }

        /// <summary>
        /// Update DisplayList page with range of records
        /// </summary>
        private void RefreshDataPage()
        {
            // TODO: Operate on Main Thread
            if (CurrentPage > 0 && CurrentPage <= MaxPage)
            {
                DisplayList.Clear();

                for (int i = (CurrentPage - 1) * RecordPerPage; i <= CurrentPage * RecordPerPage - 1; i++)
                {
                    if (i >= CurrentList.Count)
                    {
                        break;
                    }
                    DisplayList.Add(CurrentList[i]);
                }
            }
        }


        /// <summary>
        /// Repaginating
        /// </summary>
        public void RefreshPagination()
        {
            this.DisplayList.Clear();
            if (recordPerPage == 0)
            {
                recordPerPage = 20;
                Offset = "20";
            }
            this.RecordPerPage = recordPerPage;
            this.CurrentPage = 1;
            this.RefreshDataPage();
        }

        /// <summary>
        /// Remove configuration from ConfigurationList
        /// </summary>
        /// <param name="index"></param>
        public void RemoveConfiguration(int index)
        {
            if (index >= 0 && index < ConfigurationList.Count)
            {
                this.ConfigurationList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Create one empty configuration with default MRPD and MRPS
        /// </summary>
        public void CreateEmptyConfiguration()
        {
            this.ConfigurationList.Add(new Configuration("New Configuration", "", 2000, 20));
        }

        /// <summary>
        /// Automatically select one configuration from configList according to factors such as remaining count
        /// </summary>
        /// <returns></returns>
        private Configuration SelectConfig()
        {
            List<Configuration> configRangeList = new List<Configuration>();
            foreach (Configuration config in ConfigurationList)
            {
                if (config.IsAvailable())
                {
                    configRangeList.Add(config);
                }
            }

            int maximumRequestAvailable = -1;
            Configuration result = null;
            foreach (Configuration config in configRangeList)
            {
                int remaining = config.MaximumRequestPerDay - config.RequestCountToday;
                if (remaining > maximumRequestAvailable)
                {
                    maximumRequestAvailable = remaining;
                    result = config;
                }
            }

            return result;
        }


        /// <summary>
        /// Update encryption password
        /// </summary>
        /// <param name="password"></param>
        public void ChangePassword(string password)
        {
            this._password = password;
        }


        // TODO: SQLite support
        //private bool InitDatabase()
        //{
        //    string sqlCreateConfigurationTable = @"CREATE TABLE Configuration(id INTEGER PRIMARY KEY, display_name TEXT, api_key TEXT, request_count_of_date TEXT, request_per_day INT)";
        //    if (_databaseManager.ExecuteNonQuery(sqlCreateConfigurationTable, null) == -1)
        //    {
        //        return false;
        //    }

        //    string sqlCreatePoiStorageTable = @"CREATE TABLE PoiStorage(id INTEGER PRIMARY KEY, poi_id INT, name TEXT, tag TEXT, 
        //                        type TEXT, typecode TEXT, biz_type TEXT, address TEXT, location TEXT, tel TEXT, 
        //                        postcode TEXT, website TEXT, email TEXT, pcode TEXT, pname TEXT, citycode TEXT, 
        //                        adcode TEXT, adname TEXT, request_keyword TEXT)";
        //    if (_databaseManager.ExecuteNonQuery(sqlCreatePoiStorageTable, null) == -1)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

    }

    public class FailedWorkspaceFailException : Exception
    {
        public bool PasswordRequired { get; set; }

        public FailedWorkspaceFailException(string message, bool passwordRequired) : base(message)
        {
            PasswordRequired = passwordRequired;
        }
    }
}
