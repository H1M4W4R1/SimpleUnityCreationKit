using UnityEngine;

namespace Systems.SimpleUI.Components.Animations.Abstract
{
    /// <summary>
    ///     Represents a base class for UI animations handling
    /// </summary>
    public abstract class UIAnimationBase : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField, HideInInspector] protected GameObject selfGameObject;
        [field: SerializeField, HideInInspector] protected Transform selfTransform;

        protected void OnEnable() => OnObjectActivated();
        protected void OnDisable() => OnObjectDeactivated();
        
        /// <summary>
        ///     Activates the GameObject
        /// </summary>
        protected void Activate()
        {
            selfGameObject.SetActive(true);
        }
        
        /// <summary>
        ///     Event called when the GameObject is activated
        /// </summary>
        protected virtual void OnObjectActivated()
        {
            
        }

        /// <summary>
        ///     Deactivates the GameObject
        /// </summary>
        protected void Deactivate()
        {
            selfGameObject.SetActive(false);
        }

        /// <summary>
        ///     Event called when the GameObject is deactivated
        /// </summary>
        protected virtual void OnObjectDeactivated()
        {
            
        }

        protected virtual void OnValidate()
        {
            selfGameObject = gameObject;
            selfTransform = transform;
        }
    }
}