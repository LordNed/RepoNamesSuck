using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindEditor.UI
{
    public sealed class TabItem
    {
        public string Header { get; set; }
        public List<ObjectCategoryEntry> Content { get; set; }

        public TabItem()
        {
            Content = new List<ObjectCategoryEntry>();
        }
    }

    public sealed class ObjectCategoryEntry
    {
        public string FourCC { get; set; }
        public string Category { get; set; }
        public string TechnicalName { get; set; }
        public string DisplayName { get; set; }
        public string[] Keywords { get; set; }
        public string IconPath { get; set; }
        public ImageSource DisplayImage { get; set; }

        public ObjectCategoryEntry()
        {
            Keywords = new string[0];
            DisplayImage = null;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class ObjectPlaceToolViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TabItem> Tabs { get; private set; }
        public CollectionViewSource FullList { get; private set; }
        public bool IsSearching
        {
            get { return SearchFilterText.Length > 0;}
        }
        public bool CanPlaceObject
        {
            get { return !IsSearching;}
        }

        public string SearchFilterText
        {
            get { return m_searchFilterText; }
            set
            {
                m_searchFilterText = value;

                if (!string.IsNullOrEmpty(m_searchFilterText))
                    AddFilter();

                FullList.View.Refresh();

                OnPropertyChanged("SearchFilterText");
                OnPropertyChanged("IsSearching");
                OnPropertyChanged("CanPlaceObject");
            }
        }

        private readonly MainWindowViewModel m_mainWindow;
        private string m_searchFilterText;

        public ObjectPlaceToolViewModel(MainWindowViewModel mainWindow)
        {
            m_mainWindow = mainWindow;
            Tabs = new ObservableCollection<TabItem>();
            FullList = new CollectionViewSource();

            LoadTemplatesFromDisk();

            // Set the search filter text to an empty string so it triggers the IsSearching/CanPlaceObject OnPropertyChanged
            // events so that the view sets the visibility of both controls correctly.
            SearchFilterText = string.Empty;
        }

        private void LoadTemplatesFromDisk()
        {
            string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = executionPath + "/WindWaker/Templates/ActorCategoryList.json";

            string fileContents = File.ReadAllText(filePath);

            var categoryData = JsonConvert.DeserializeObject<List<ObjectCategoryEntry>>(fileContents);

            // Sort them by category
            var objByCategory = new Dictionary<string, List<ObjectCategoryEntry>>();
            for(int i = 0; i < categoryData.Count; i++)
            {
                if(!objByCategory.ContainsKey(categoryData[i].Category))
                    objByCategory[categoryData[i].Category] = new List<ObjectCategoryEntry>();

                objByCategory[categoryData[i].Category].Add(categoryData[i]);
            }

            var fullEntryList = new List<ObjectCategoryEntry>();

            // Create tabs for each unique category
            foreach(var kvp in objByCategory)
            {
                TabItem tab = new TabItem();
                tab.Header = kvp.Key.ToUpper(CultureInfo.CurrentCulture);

                foreach(var entry in kvp.Value)
                {
                    // Create a new BitmapImage to represent the icon - caching the icon on load so that it doesn't
                    // have atomic focus on the file and lock others from using the same icon.
                    using(FileStream fs = new FileStream(entry.IconPath, FileMode.Open))
                    {
                        BitmapImage entryIcon = new BitmapImage();
                        entryIcon.BeginInit();
                        entryIcon.CacheOption = BitmapCacheOption.OnLoad;
                        entryIcon.StreamSource = fs;
                        entryIcon.EndInit();

                        entry.DisplayImage = entryIcon;
                        tab.Content.Add(entry);
                        fullEntryList.Add(entry);
                    }
                }

                Tabs.Add(tab);
            }

            // Use the flat-list of our entries and assign it as the source of the CollectionViewSource so we can filter it.
            FullList.Source = fullEntryList;
        }

        private void AddFilter()
        {
            FullList.Filter -= new FilterEventHandler(Filter);
            FullList.Filter += new FilterEventHandler(Filter);
        }

        private void Filter(object sender, FilterEventArgs e)
        {
            var src = e.Item as ObjectCategoryEntry;
            e.Accepted = false;
            if (src == null)
                return;

            string searchTerm = SearchFilterText.ToLowerInvariant();
            
            // See if the keywords array for the object contains the search term
            for (int i = 0; i < src.Keywords.Length; i++)
            {
                if(src.Keywords[i].ToLowerInvariant().Contains(searchTerm))
                {
                    e.Accepted = true;
                    return;
                }
            }

            // See if the technical name contains the search term.
            if(src.TechnicalName.ToLowerInvariant().Contains(searchTerm))
            {
                e.Accepted = true;
                return;
            }

            // And finally, see if the display name contains the search term.
            if(src.DisplayName.ToLowerInvariant().Contains(searchTerm))
            {
                e.Accepted = true;
                return;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
