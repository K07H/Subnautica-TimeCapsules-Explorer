using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Json;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Collections;

namespace Subnautica_TimeCapsulesExplorer
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global attributes.

        public ObservableCollection<TimeCapsule> test { get; set; }
        private List<Window> _windows = null;
        private List<TimeCapsule> _capsules = null;
        private List<TimeCapsulesPage> _capsulesPages = null;
        private BackgroundWorker _capsulesLoader = null;
        public int _defaultStartPage = 2114;
        private int lastPage = -1;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            myInit();
        }

        #region Initialization.
        public bool myInit()
        {
            // Initialize our list of windows.
            this._windows = new List<Window>();
            // Initialize datagrid.
            this.DataContext = this;
            test = new ObservableCollection<TimeCapsule>();
            // Set default font family.
            
            return true;
        }

        private void dataGrid1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Hide some useless columns.
            if (e.PropertyName.CompareTo("class_id") == 0
                || e.PropertyName.CompareTo("is_active") == 0
                || e.PropertyName.CompareTo("image") == 0
                || e.PropertyName.CompareTo("items") == 0)
                e.Cancel = true;
        }
        #endregion

        #region Get TimeCapsules from official API.
        public bool isEmptyPage(int pageNumber)
        {
            using (WebClient wc = new WebClient())
            {
                // Get timecapsule page.
                string jsonText = wc.DownloadString("https://subnautica.unknownworlds.com/api/time-capsules-voting-queue?page=" + pageNumber);
                return (jsonText.CompareTo("{\"capsules\":[]}") == 0);
            }
        }

        public int getLastPage(int defaultSearchEnd)
        {
            int cnt = defaultSearchEnd;
            bool isEmpty = isEmptyPage(cnt);

            if (isEmpty)
            {
                while (isEmpty && (cnt > 1))
                {
                    --cnt;
                    isEmpty = isEmptyPage(cnt);
                }
                ++cnt;
            }
            else
            {
                while (!isEmpty && (cnt < Properties.Settings.Default.MaxPage))
                {
                    ++cnt;
                    isEmpty = isEmptyPage(cnt);
                }
            }
            return cnt;
        }

        public List<TimeCapsule> getTimeCapsules(int fromPage, int toPage)
        {
            bool work = true;
            string jsonText = "";
            int cnt = fromPage;
            string url = "https://subnautica.unknownworlds.com/api/time-capsules-voting-queue?page=";
            string splitA = "{\"capsules\":[";
            string splitB = "},{\\\"_id\\\":";
            List<string> capsules = new List<string>();

            List<TimeCapsule> timecapsules = new List<TimeCapsule>();

            using (WebClient wc = new WebClient())
            {
                while (work)
                {
                    capsules.Clear();

                    jsonText = wc.DownloadString(url + cnt.ToString());

                    if (jsonText.CompareTo("{\"capsules\":[]}") == 0)
                        work = false;
                    else
                    {
                        jsonText = jsonText.Substring(splitA.Length);
                        jsonText = jsonText.Substring(0, jsonText.Length - 2);
                        string[] lines = Regex.Split(jsonText, splitB);
                        if (lines.Length > 0)
                        {
                            capsules.Add(lines[0] + "}");
                            int i = 1;
                            while (i < lines.Length)
                            {
                                capsules.Add("{\"_id\":" + lines[i] + "}");
                                ++i;
                            }
                        }

                        foreach (string capsule in capsules)
                        {
                            TimeCapsule tmp = parseTimeCapsule(capsule);
                            if (tmp != null)
                                timecapsules.Add(tmp);
                        }
                    }

                    // Go to next page
                    ++cnt;

                    if (cnt > toPage)
                        work = false;
                }
            }
            return timecapsules;
        }

        public List<TimeCapsulesPage> getTimeCapsulePages()
        {
            bool work = true;
            string jsonText = "";
            int cnt = 1;
            int reverseSearchStart = 2500; // We set 2500 here but it's never used (it's replaced with tb_ReverseSearchStart.Text).
            int nbPagesToGet = 10; // We set 10 here but it's never used (it's replaced with tb_NbPages.Text).

            tb_NbPages.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
            {
                nbPagesToGet = Convert.ToInt32(tb_NbPages.Text);
                return null;
            }), null);

            if (lastPage == -1)
            {
                tb_ReverseSearchStart.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                {
                    reverseSearchStart = Convert.ToInt32(tb_ReverseSearchStart.Text);
                    this.lastPage = this.getLastPage(reverseSearchStart);
                    tb_ReverseSearchStart.Text = Convert.ToString(this.lastPage);
                    return null;
                }), null);
                
                cnt = this.lastPage;
                if (cnt > nbPagesToGet)
                    cnt -= nbPagesToGet;
                else
                    cnt = 1;
            }
            else
            {
                if (lastPage > nbPagesToGet)
                    cnt = lastPage - nbPagesToGet;
                else
                    cnt = 1;
            }
            string url = "https://subnautica.unknownworlds.com/api/time-capsules-voting-queue?page=";
            string splitA = "{\"capsules\":[";
            string splitB = "},{\\\"_id\\\":";
            List<string> capsules = new List<string>();

            List<TimeCapsulesPage> timecapsulePages = new List<TimeCapsulesPage>();

            while (work)
            {
                // Parse timecapsules page
                using (WebClient wc = new WebClient())
                {
                    jsonText = wc.DownloadString(url + cnt.ToString());
                }

                if (jsonText.CompareTo("{\"capsules\":[]}") == 0)
                    work = false;
                else
                {
                    TimeCapsulesPage currentPage = new TimeCapsulesPage(cnt);
                    jsonText = jsonText.Substring(splitA.Length);
                    jsonText = jsonText.Substring(0, jsonText.Length - 2);
                    string[] lines = Regex.Split(jsonText, splitB);
                    if (lines.Length > 0)
                    {
                        capsules.Add(lines[0] + "}");
                        int i = 1;
                        while (i < lines.Length)
                        {
                            capsules.Add("{\"_id\":" + lines[i] + "}");
                            ++i;
                        }
                    }

                    foreach (string capsule in capsules)
                    {
                        currentPage._tcpCapsules.Add(parseTimeCapsule(capsule));
                    }
                    timecapsulePages.Add(currentPage);
                }

                // Go to next page
                ++cnt;

                // Clear temp list
                capsules.Clear();
                capsules = new List<string>();

                // Hard stop at page 9999 to prevent infinite loop.
                if (cnt > Properties.Settings.Default.MaxPage)
                    work = false;
            }

            return timecapsulePages;
        }
        #endregion

        #region Parsing TimeCapsule from JSON.
        public TimeCapsule parseTimeCapsule(string timecapsulejson)
        {
            TimeCapsule result = new TimeCapsule();
            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(timecapsulejson);

            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                string name = field.Name;
                string value = string.Empty;
                string type = string.Empty;
                if (field.GetValue() == null)
                    type = "null";
                else
                    type = field.GetValue().GetType().Name;

                // Try to get value.
                switch (type)
                {
                    case "String":
                        value = (string)field.GetValue();
                        break;

                    case "Double":
                        value = field.GetValue().ToString();
                        break;

                    case "Boolean":
                        value = field.GetValue().ToString();
                        break;

                    case "null":
                        value = "null";
                        break;

                    default:
                        value = "NotSupportedException";
                        break;
                    // DEBUG: in this sample we'll not parse nested arrays or objects.
                    //throw new NotSupportedException();
                }

                switch (name)
                {
                    case "_id":
                        result._id = value;
                        break;

                    case "platform":
                        result.platform = value;
                        break;

                    case "platform_user_id":
                        result.platform_user_id = value;
                        break;

                    case "user_name":
                        result.user_name = value;
                        break;

                    case "title":
                        result.title = value;
                        break;

                    case "text":
                        result.text = value;
                        break;

                    case "language":
                        result.language = value;
                        break;

                    case "items":
                        result.items = value;
                        break;

                    case "is_active":
                        result.is_active = Convert.ToBoolean(value);
                        break;

                    case "votes_up":
                        result.votes_up = Convert.ToInt32(value);
                        break;

                    case "votes_down":
                        result.votes_down = Convert.ToInt32(value);
                        break;

                    case "copies_found":
                        result.copies_found = Convert.ToInt32(value);
                        break;

                    case "class_id":
                        result.class_id = value;
                        break;

                    case "modified_at":
                        result.modified_at = value;
                        break;

                    case "updated_at":
                        result.updated_at = value;
                        break;

                    case "created_at":
                        result.created_at = value;
                        break;

                    case "image":
                        result.image = value;
                        break;

                    case "time_ago":
                        result.time_ago = value;
                        break;

                    case "item_list":
                        result.item_list = value;
                        break;

                    default:
                        throw new NotSupportedException();
                }
                // DEBUG
                //Console.WriteLine("{0} {1} {2}", name.PadLeft(15), type.PadLeft(10), value.PadLeft(15));
            }

            if (obj != null)
                obj = null;
            if (parser != null)
                parser = null;
            return result;
        }
        #endregion

        #region Background workers.
        void _capsulesLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            rct_Loading.Visibility = System.Windows.Visibility.Hidden;
            lbl_Loading.Visibility = System.Windows.Visibility.Hidden;
            if (this._capsulesLoader != null)
            {
                this._capsulesLoader.Dispose();
                this._capsulesLoader = null;
            }
        }

        void _capsulesLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<int, int> fromToPage = (Tuple<int, int>)e.Argument;
            this._capsules = getTimeCapsules(fromToPage.Item1, fromToPage.Item2);
            foreach (TimeCapsule tc in this._capsules)
            {
                dataGrid1.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                {
                    test.Add(tc);
                    return null;
                }), null);
            }
        }

        void _capsulesLoaderByID_DoWork(object sender, DoWorkEventArgs e)
        {
            this._capsulesPages = this.getTimeCapsulePages();
            foreach (TimeCapsulesPage tcp in this._capsulesPages)
            {
                List<TimeCapsule> tcs = tcp.getTimeCapsulesByID((string)e.Argument);
                foreach (TimeCapsule tc in tcs)
                {
                    dataGrid1.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                    {
                        test.Add(tc);
                        return null;
                    }), null);
                }
            }
        }

        void _capsulesLoaderByUserName_DoWork(object sender, DoWorkEventArgs e)
        {
            this._capsulesPages = getTimeCapsulePages();
            foreach (TimeCapsulesPage tcp in this._capsulesPages)
            {
                List<TimeCapsule> tcs = tcp.getTimeCapsulesByUserName((string)e.Argument);
                foreach (TimeCapsule tc in tcs)
                {
                    dataGrid1.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                    {
                        test.Add(tc);
                        return null;
                    }), null);
                }
            }
        }

        void _capsulesLoaderByUserID_DoWork(object sender, DoWorkEventArgs e)
        {
            this._capsulesPages = getTimeCapsulePages();
            foreach (TimeCapsulesPage tcp in this._capsulesPages)
            {
                List<TimeCapsule> tcs = tcp.getTimeCapsulesByUserID((string)e.Argument);
                foreach (TimeCapsule tc in tcs)
                {
                    dataGrid1.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                    {
                        test.Add(tc);
                        return null;
                    }), null);
                }
            }
        }

        void _capsulesLoaderContainingText_DoWork(object sender, DoWorkEventArgs e)
        {
            this._capsulesPages = getTimeCapsulePages();
            foreach (TimeCapsulesPage tcp in this._capsulesPages)
            {
                List<TimeCapsule> tcs = tcp.getTimeCapsulesContainingText((string)e.Argument);
                foreach (TimeCapsule tc in tcs)
                {
                    dataGrid1.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
                    {
                        test.Add(tc);
                        return null;
                    }), null);
                }
            }
        }

        void _capsulesLoaderReverseSearchStart_DoWork(object sender, DoWorkEventArgs e)
        {
            int reverseSearchStart = (int)e.Argument;
            bool isEmpty = isEmptyPage(reverseSearchStart);
            if (isEmpty)
            {
                while (isEmpty && reverseSearchStart > 1)
                {
                    --reverseSearchStart;
                    isEmpty = isEmptyPage(reverseSearchStart);
                }
                ++reverseSearchStart;
            }
            else
            {
                while (!isEmpty && (reverseSearchStart < Properties.Settings.Default.MaxPage))
                {
                    ++reverseSearchStart;
                    isEmpty = isEmptyPage(reverseSearchStart);
                }
            }
            tb_ReverseSearchStart.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
            {
                tb_ReverseSearchStart.Text = Convert.ToString(reverseSearchStart);
                return null;
            }), null);
        }

        void _capsulesLoaderReverseSearchStart_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbl_Loading.Visibility = System.Windows.Visibility.Hidden;
            lbl_Config1.Visibility = System.Windows.Visibility.Visible;
            lbl_Config2.Visibility = System.Windows.Visibility.Visible;
            tb_ReverseSearchStart.Visibility = System.Windows.Visibility.Visible;
            btn_UpdateReverseSearchStart.Visibility = System.Windows.Visibility.Visible;
            btn_ValidateConfiguration.Visibility = System.Windows.Visibility.Visible;
            lbl_Loading.Content = "Loading, please wait...";
            if (this._capsulesLoader != null)
            {
                this._capsulesLoader.Dispose();
                this._capsulesLoader = null;
            }
        }
        #endregion

        #region Buttons click.

        private void downloadPages_Click(object sender, RoutedEventArgs e)
        {
            if (this._capsulesLoader != null)
                return;

            int fromPage = Convert.ToInt32(tb_FromPage.Text);
            int toPage = Convert.ToInt32(tb_ToPage.Text);
            if (this._capsulesPages == null)
            {
                rct_Loading.Visibility = System.Windows.Visibility.Visible;
                lbl_Loading.Visibility = System.Windows.Visibility.Visible;
                this._capsulesLoader = new BackgroundWorker();
                this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoader_DoWork);
                this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoader_RunWorkerCompleted);
                this._capsulesLoader.RunWorkerAsync(new Tuple<int, int>(fromPage, toPage));
            }
            else
            {
                bool allPagesAlreadyDownloaded = true;
                int cnt = fromPage;
                while (cnt <= toPage)
                {
                    bool pageExist = false;
                    foreach (TimeCapsulesPage tcp in this._capsulesPages)
                    {
                        if (tcp._pageID == cnt)
                        {
                            pageExist = true;
                            break;
                        }
                    }
                    if (!pageExist)
                    {
                        allPagesAlreadyDownloaded = false;
                        break;
                    }
                    ++cnt;
                }
                if (allPagesAlreadyDownloaded)
                {
                    cnt = fromPage;
                    while (cnt <= toPage)
                    {
                        foreach (TimeCapsulesPage tcp in this._capsulesPages)
                        {
                            if (tcp._pageID == cnt)
                            {
                                foreach (TimeCapsule tc in tcp._tcpCapsules)
                                {
                                    test.Add(tc);
                                }
                            }
                        }
                        ++cnt;
                    }
                }
                else
                {
                    rct_Loading.Visibility = System.Windows.Visibility.Visible;
                    lbl_Loading.Visibility = System.Windows.Visibility.Visible;
                    this._capsulesLoader = new BackgroundWorker();
                    this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoader_DoWork);
                    this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoader_RunWorkerCompleted);
                    this._capsulesLoader.RunWorkerAsync(new Tuple<int, int>(fromPage, toPage));
                }
            }
        }

        private void btn_CapsulesByID_Click(object sender, RoutedEventArgs e)
        {
            if (this._capsulesLoader != null)
                return;

            if (this._capsulesPages == null)
            {
                rct_Loading.Visibility = System.Windows.Visibility.Visible;
                lbl_Loading.Visibility = System.Windows.Visibility.Visible;
                this._capsulesLoader = new BackgroundWorker();
                this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoaderByID_DoWork);
                this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoader_RunWorkerCompleted);
                this._capsulesLoader.RunWorkerAsync(tb_CapsulesByID.Text);
            }
            else
            {
                foreach (TimeCapsulesPage tcp in this._capsulesPages)
                {
                    List<TimeCapsule> tcs = tcp.getTimeCapsulesByID(tb_CapsulesByID.Text);
                    foreach (TimeCapsule tc in tcs)
                    {
                        test.Add(tc);
                    }
                }
            }
        }

        private void btn_CapsulesByUserName_Click(object sender, RoutedEventArgs e)
        {
            if (this._capsulesLoader != null)
                return;

            if (this._capsulesPages == null)
            {
                rct_Loading.Visibility = System.Windows.Visibility.Visible;
                lbl_Loading.Visibility = System.Windows.Visibility.Visible;
                this._capsulesLoader = new BackgroundWorker();
                this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoaderByUserName_DoWork);
                this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoader_RunWorkerCompleted);
                this._capsulesLoader.RunWorkerAsync(tb_CapsulesByUserName.Text);
            }
            else
            {
                foreach (TimeCapsulesPage tcp in this._capsulesPages)
                {
                    List<TimeCapsule> tcs = tcp.getTimeCapsulesByUserName(tb_CapsulesByUserName.Text);
                    foreach (TimeCapsule tc in tcs)
                    {
                        test.Add(tc);
                    }
                }
            }
        }

        private void btn_CapsulesByUserID_Click(object sender, RoutedEventArgs e)
        {
            if (this._capsulesLoader != null)
                return;

            if (this._capsulesPages == null)
            {
                rct_Loading.Visibility = System.Windows.Visibility.Visible;
                lbl_Loading.Visibility = System.Windows.Visibility.Visible;
                this._capsulesLoader = new BackgroundWorker();
                this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoaderByUserID_DoWork);
                this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoader_RunWorkerCompleted);
                this._capsulesLoader.RunWorkerAsync(tb_CapsulesByUserID.Text);
            }
            else
            {
                foreach (TimeCapsulesPage tcp in this._capsulesPages)
                {
                    List<TimeCapsule> tcs = tcp.getTimeCapsulesByUserID(tb_CapsulesByUserID.Text);
                    foreach (TimeCapsule tc in tcs)
                    {
                        test.Add(tc);
                    }
                }
            }
        }

        private void btn_CapsulesContainingText_Click(object sender, RoutedEventArgs e)
        {
            if (this._capsulesLoader != null)
                return;

            if (this._capsulesPages == null)
            {
                rct_Loading.Visibility = System.Windows.Visibility.Visible;
                lbl_Loading.Visibility = System.Windows.Visibility.Visible;
                this._capsulesLoader = new BackgroundWorker();
                this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoaderContainingText_DoWork);
                this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoader_RunWorkerCompleted);
                this._capsulesLoader.RunWorkerAsync(tb_CapsulesContainingText.Text);
            }
            else
            {
                foreach (TimeCapsulesPage tcp in this._capsulesPages)
                {
                    List<TimeCapsule> tcs = tcp.getTimeCapsulesContainingText(tb_CapsulesContainingText.Text);
                    foreach (TimeCapsule tc in tcs)
                    {
                        test.Add(tc);
                    }
                }
            }
        }

        private void btn_ClearList_Click(object sender, RoutedEventArgs e)
        {
            this.dataGrid1.ItemsSource = null;
            test.Clear();
            this.dataGrid1.ItemsSource = test;
            this.dataGrid1.Items.Refresh();
        }

        private void btn_UpdateReverseSearchStart_Click(object sender, RoutedEventArgs e)
        {
            if (this._capsulesLoader != null)
                return;

            lbl_Loading.Content = "Searching last page, please wait...";
            lbl_Loading.Visibility = System.Windows.Visibility.Visible;
            lbl_Config1.Visibility = System.Windows.Visibility.Hidden;
            lbl_Config2.Visibility = System.Windows.Visibility.Hidden;
            tb_ReverseSearchStart.Visibility = System.Windows.Visibility.Hidden;
            btn_UpdateReverseSearchStart.Visibility = System.Windows.Visibility.Hidden;
            btn_ValidateConfiguration.Visibility = System.Windows.Visibility.Hidden;
            this._capsulesLoader = new BackgroundWorker();
            this._capsulesLoader.DoWork += new DoWorkEventHandler(_capsulesLoaderReverseSearchStart_DoWork);
            this._capsulesLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_capsulesLoaderReverseSearchStart_RunWorkerCompleted);
            this._capsulesLoader.RunWorkerAsync(Convert.ToInt32(tb_ReverseSearchStart.Text));
        }

        private void btn_About_Click(object sender, RoutedEventArgs e)
        {
            Window about = new AboutWindow();
            about.Closed += new EventHandler(window_Closed);
            this._windows.Add(about);
            about.Show();
        }
        #endregion

        #region Events.
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimeCapsule tc = (TimeCapsule)this.dataGrid1.SelectedItem;
            if (tc != null)
            {
                Window w = new TimeCapsuleWindow(tc);
                w.Closed += new EventHandler(window_Closed);
                this._windows.Add(w);
                w.Show();
            }

            // Clear selected items.
            this.dataGrid1.UnselectAll();
        }

        void window_Closed(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            if (this._windows.Contains(window))
                this._windows.Remove(window);
        }
        #endregion

        #region Open/close advanced configuration.
        private void btn_Configuration_Click(object sender, RoutedEventArgs e)
        {
            rct_Loading.Visibility = System.Windows.Visibility.Visible;
            lbl_Config1.Visibility = System.Windows.Visibility.Visible;
            lbl_Config2.Visibility = System.Windows.Visibility.Visible;
            tb_ReverseSearchStart.Visibility = System.Windows.Visibility.Visible;
            btn_UpdateReverseSearchStart.Visibility = System.Windows.Visibility.Visible;
            btn_ValidateConfiguration.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void btn_ValidateConfiguration_Click(object sender, RoutedEventArgs e)
        {
            lbl_Config1.Visibility = System.Windows.Visibility.Hidden;
            lbl_Config2.Visibility = System.Windows.Visibility.Hidden;
            tb_ReverseSearchStart.Visibility = System.Windows.Visibility.Hidden;
            btn_UpdateReverseSearchStart.Visibility = System.Windows.Visibility.Hidden;
            btn_ValidateConfiguration.Visibility = System.Windows.Visibility.Hidden;
            rct_Loading.Visibility = System.Windows.Visibility.Hidden;
        }
        #endregion

        #region Clear capsules pages when parameters are modified.
        private void tb_NbPages_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._capsulesPages != null)
            {
                this._capsulesPages.Clear();
                this._capsulesPages = null;
            }
        }

        private void tb_ReverseSearchStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._capsulesPages != null)
            {
                this._capsulesPages.Clear();
                this._capsulesPages = null;
            }
        }
        #endregion
    }
}
