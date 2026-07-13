using Systems.SimpleLoading.Utility;
using UnityEngine;

namespace Systems.SimpleLoading.Components
{
    /// <summary>Scene component that advances requests started through <see cref="LoadingAPI"/>.</summary>
    /// <remarks>
    ///     Place one active instance in each scene that processes loading. It deliberately does not create a singleton;
    ///     projects can instead call <see cref="LoadingAPI.Advance"/> from their own game loop.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class LoadingSystem : MonoBehaviour
    {
        private void Update()
        {
            LoadingAPI.Advance(Time.deltaTime);
        }
    }
}
