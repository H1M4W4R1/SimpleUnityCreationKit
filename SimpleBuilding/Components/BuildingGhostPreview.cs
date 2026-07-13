using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     Owns the transient visual object shown while a building entry is selected.
    /// </summary>
    public sealed class BuildingGhostPreview : MonoBehaviour
    {
        [SerializeField] [CanBeNull] private BuildingGhostMaterialConfiguration _materialConfiguration;
        [CanBeNull] private GameObject _instance;
        [CanBeNull] private GameObject _sourcePrefab;
        private bool _isValid;

        public void Show([NotNull] BuildingEntryBase entry, Vector3 position, Quaternion rotation, bool isValid)
        {
            GameObject sourcePrefab = entry.GetGhostPrefab();
            if (ReferenceEquals(sourcePrefab, null) || !sourcePrefab)
            {
                Hide();
                return;
            }

            if (ReferenceEquals(_instance, null) || !_instance ||
                !ReferenceEquals(_sourcePrefab, sourcePrefab))
            {
                Hide();
                _instance = Object.Instantiate(sourcePrefab, position, rotation);
                _sourcePrefab = sourcePrefab;
                DisableRuntimeComponents(_instance);
                _isValid = !isValid;
            }

            _instance.transform.SetPositionAndRotation(position, rotation);
            SetValidity(isValid);
        }

        public void SetValidity(bool isValid)
        {
            if (ReferenceEquals(_instance, null) || !_instance) return;
            if (_isValid == isValid) return;

            _isValid = isValid;
            if (!ReferenceEquals(_materialConfiguration, null) && _materialConfiguration)
                _materialConfiguration.Apply(_instance, isValid);
        }

        public void Hide()
        {
            if (!ReferenceEquals(_instance, null) && _instance) DestroyPreview(_instance);
            _instance = null;
            _sourcePrefab = null;
        }

        private static void DisableRuntimeComponents([NotNull] GameObject preview)
        {
            MonoBehaviour[] behaviours = preview.GetComponentsInChildren<MonoBehaviour>(true);
            for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
            {
                MonoBehaviour behaviour = behaviours[behaviourIndex];
                if (behaviour) behaviour.enabled = false;
            }

            Collider[] colliders = preview.GetComponentsInChildren<Collider>(true);
            for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
            {
                Collider collider = colliders[colliderIndex];
                if (collider) collider.enabled = false;
            }

            Collider2D[] colliders2D = preview.GetComponentsInChildren<Collider2D>(true);
            for (int colliderIndex = 0; colliderIndex < colliders2D.Length; colliderIndex++)
            {
                Collider2D collider = colliders2D[colliderIndex];
                if (collider) collider.enabled = false;
            }
        }

        private static void DestroyPreview([NotNull] GameObject preview)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(preview);
                return;
            }
#endif

            Object.Destroy(preview);
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}
