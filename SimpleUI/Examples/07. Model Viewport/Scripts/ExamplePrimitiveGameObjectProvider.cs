using System.Collections.Generic;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;

namespace Systems.SimpleUI.Examples._07._Model_Viewport.Scripts
{
    public sealed class ExamplePrimitiveGameObjectProvider : ContextProviderBase<GameObject>
    {
        /// <summary>
        ///     List of game objects to be used as models
        /// </summary>
        [field: SerializeField] private List<GameObject> GameObjects = new();
        [field: SerializeField] private int Index { get; set; }
        
        public override GameObject GetContext() => GameObjects[Index % GameObjects.Count];
    }
}