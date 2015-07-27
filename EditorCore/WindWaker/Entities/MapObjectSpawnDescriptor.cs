using Newtonsoft.Json;

namespace WEditor.WindWaker.Entities
{
    /// <summary>
    /// This describes an object which the real game uses to spawn an object. The only required part are the FourCC 
    /// and the TechnicalName which are used to find the correct template and set it as the right actor. The rest is
    /// metadata for searching, displaying, etc.
    /// </summary>
    [System.Serializable]
    public class MapObjectSpawnDescriptor
    {
        public string FourCC { get; set; }
        public string Category { get; set; }
        public string TechnicalName { get; set; }
        public string DisplayName { get; set; }

        [JsonConverter(typeof(CSVStringToArray))]
        public string[] Keywords { get; set; }
        public string IconPath { get; set; }

        public MapObjectSpawnDescriptor()
        {
            Keywords = new string[0];
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
