using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace WindEditor.UI
{
    public class ObjectPlaceToolViewModel
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
            public string FourCC;
            public string Category;
            public string TechnicalName;
            public string DisplayName;
            public string[] Keywords;

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
            string filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            filePath += "/WindWaker/Templates/ActorCategoryList.json";

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
                tab.Content = kvp.Value;

                Tabs.Add(tab);
            }
        }
    }
}
