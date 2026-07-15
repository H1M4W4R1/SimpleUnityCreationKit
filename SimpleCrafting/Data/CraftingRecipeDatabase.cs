using System.Collections.Generic;
using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleCrafting.Abstract;

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("SimpleCrafting.Tests")]
#endif

namespace Systems.SimpleCrafting.Data
{
    public sealed class CraftingRecipeDatabase : AddressableDatabase<CraftingRecipeDatabase, CraftingRecipeBase>
    {
        public const string LABEL = "SimpleCrafting.Recipes";

#if UNITY_INCLUDE_TESTS
        private static bool _isUsingTestStorage;
#endif

        [NotNull] protected override string AddressableLabel => LABEL;

        public static ROListAccess<CraftingRecipeBase> GetAllRecipes()
        {
#if UNITY_INCLUDE_TESTS
            if (!_isUsingTestStorage)
            {
                Instance.EnsureLoaded();
            }
#else
            Instance.EnsureLoaded();
#endif

            RWListAccess<CraftingRecipeBase> list = RWListAccess<CraftingRecipeBase>.Create();
            List<CraftingRecipeBase> refList = list.List;

            for (int i = 0; i < internalDataStorage.Count; i++)
            {
                CraftingRecipeBase recipe = internalDataStorage[i].entryObject;
                if (ReferenceEquals(recipe, null)) continue;
                if (refList.Contains(recipe)) continue;
                refList.Add(recipe);
            }

            return list.ToReadOnly();
        }

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] CraftingRecipeBase recipe)
        {
            _isUsingTestStorage = true;
            UseTestStorage();
            internalDataStorage.Add(
                new AddressableDatabaseEntry<CraftingRecipeBase>(HashIdentifier.New(recipe.GetType()), recipe));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            _isUsingTestStorage = false;
            internalDataStorage.Clear();
        }
#endif
    }
}
