namespace WEditor.Maps.Entities
{
    public class MapObjectDataDescriptor
    {
        public class FileDescriptor
        {
            public string ArchiveName;
            public string FileName;
        }

        public class ArgumentParameter
        {
            public string Name;
            public string Type;
            public int Length;
            public byte DefaultValue;
        }

        public class DisplayOverride
        {
            public string DataParamName;
            public DisplayFlagConditional[] ConditionFlags;
        }

        public class DisplayFlagConditional
        {
            public byte Value;
            public FileDescriptor Model;
            public FileDescriptor Material;
        }

        public class DisplayParameter
        {
            public FileDescriptor DefaultModel;
            public FileDescriptor DefaultMaterial;

            public DisplayOverride DisplayOverrides;
        }

        public string FourCC;
        public string TechnicalName;
        public ArgumentParameter[] DataParameters;
        public DisplayParameter VisualParameters;
    }
}
