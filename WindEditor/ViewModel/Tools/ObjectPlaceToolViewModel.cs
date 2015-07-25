using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindEditor.UI
{
    public class ObjectPlaceEntryViewModel
    {
        public ImageSource DisplayImage { get; set; }
        public string DisplayName { get; set; }
    }

    public class ObjectPlaceToolViewModel
    {
        public sealed class TabItem
        {
            public string Header { get; set; }
            public List<ObjectPlaceEntryViewModel> Content { get; set; }

            public TabItem()
            {
                Content = new List<ObjectPlaceEntryViewModel>();
            }
        }

        public sealed class ObjectCategoryEntry
        {
            public string FourCC;
            public string Category;
            public string TechnicalName;
            public string DisplayName;
            public string[] Keywords;
            public string IconPath;

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public ObservableCollection<TabItem> Tabs { get; private set; }

        private readonly MainWindowViewModel m_mainWindow;

        public ObjectPlaceToolViewModel(MainWindowViewModel mainWindow)
        {
            m_mainWindow = mainWindow;
            Tabs = new ObservableCollection<TabItem>();

            LoadTemplatesFromDisk();
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

                        ObjectPlaceEntryViewModel newVM = new ObjectPlaceEntryViewModel();
                        newVM.DisplayName = entry.DisplayName;
                        newVM.DisplayImage = entryIcon;

                        tab.Content.Add(newVM);
                    }
                }

                Tabs.Add(tab);
            }
        }
    }
}
