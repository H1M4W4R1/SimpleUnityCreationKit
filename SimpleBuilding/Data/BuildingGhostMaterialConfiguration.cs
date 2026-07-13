using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleBuilding.Data
{
    /// <summary>
    ///     Material pair applied to ghost previews. Assign shader-based materials here for project-specific effects.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Building Ghost Material Configuration",
        menuName = "Simple Building/Ghost Material Configuration")]
    public sealed class BuildingGhostMaterialConfiguration : ScriptableObject
    {
        [SerializeField] [CanBeNull] private Material _validMaterial;
        [SerializeField] [CanBeNull] private Material _invalidMaterial;

        public void Apply([NotNull] GameObject preview, bool isValid)
        {
            Material material = isValid ? _validMaterial : _invalidMaterial;
            if (ReferenceEquals(material, null) || !material) return;

            Renderer[] renderers = preview.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (!renderer) continue;

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    materials[materialIndex] = material;

                renderer.sharedMaterials = materials;
            }
        }
    }
}
