namespace WEditor
{
    public class WObject
    {
        public WWorld World
        {
            get { return m_world; }
            internal set { m_world = value; }
        }

        private WWorld m_world;
    }
}
