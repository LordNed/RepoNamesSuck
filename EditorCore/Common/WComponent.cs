namespace WEditor
{
    public abstract class WComponent : WObject
    {
        internal Input Input { get; set; }

        public virtual void Tick(float deltaTime) { }
    }
}
