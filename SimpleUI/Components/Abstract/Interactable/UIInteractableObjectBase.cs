namespace Systems.SimpleUI.Components.Abstract.Interactable
{
    /// <summary>
    ///     Represents an interactable UI object
    /// </summary>
    public abstract class UIInteractableObjectBase : UIObjectBase
    {
        /// <summary>
        ///     Checks if the object is interactable
        /// </summary>
        public abstract bool IsInteractable { get; }

        /// <summary>
        ///     Changes the interactable state of the object
        /// </summary>
        public abstract void SetInteractable(bool interactable);
        
        /// <summary>
        ///     Makes the object interactable 
        /// </summary>
        public void MakeInteractable() => SetInteractable(true);
        
        /// <summary>
        ///     Makes the object non-interactable
        /// </summary>
        public void MakeNonInteractable() => SetInteractable(false);
    }
}