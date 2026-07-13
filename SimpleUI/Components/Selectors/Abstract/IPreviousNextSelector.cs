namespace Systems.SimpleUI.Components.Selectors.Abstract
{
    public interface IPreviousNextSelector
    {
        public bool TrySelectPrevious();
        public bool TrySelectNext();
        
        public bool IsLooping { get;  }
        public bool HasPrevious { get; }
        public bool HasNext { get; }
        
    }
}