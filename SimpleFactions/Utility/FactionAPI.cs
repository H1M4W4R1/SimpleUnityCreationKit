using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Utility;
using Systems.SimpleSaving.Abstract;
using Systems.SimpleSaving.Utility;

namespace Systems.SimpleFactions.Utility
{
    /// <summary>Static facade for faction membership, outgoing relations, and relation persistence.</summary>
    public static class FactionAPI
    {
        /// <summary>Attempts to make the object tracked by <paramref name="membership"/> join <typeparamref name="TFaction"/>.</summary>
        public static OperationResult Join<TFaction, THolder>([NotNull] FactionMembershipBase<THolder> membership)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.JoinFaction<TFaction>();

        /// <summary>Attempts to make the object tracked by <paramref name="membership"/> leave <typeparamref name="TFaction"/>.</summary>
        public static OperationResult Leave<TFaction, THolder>([NotNull] FactionMembershipBase<THolder> membership)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.LeaveFaction<TFaction>();

        /// <summary>Changes the one-way relation from <paramref name="sourceFaction"/> to <paramref name="target"/>.</summary>
        public static OperationResult ChangeRelation<TRelationType>(
            [NotNull] FactionBase sourceFaction,
            [NotNull] IRelatable target,
            int amount)
            where TRelationType : RelationTypeBase, new()
            => RelationAPI.Change<TRelationType>(sourceFaction, target, amount);

        /// <summary>Changes a faction relation with the supplied relation-type asset.</summary>
        public static OperationResult ChangeRelation(
            [NotNull] FactionBase sourceFaction,
            [NotNull] IRelatable target,
            [NotNull] RelationTypeBase relationType,
            int amount)
            => RelationAPI.Change(sourceFaction, target, relationType, amount);

        /// <summary>Sets the one-way relation from <paramref name="sourceFaction"/> to <paramref name="target"/>.</summary>
        public static OperationResult SetRelation<TRelationType>(
            [NotNull] FactionBase sourceFaction,
            [NotNull] IRelatable target,
            int value)
            where TRelationType : RelationTypeBase, new()
            => RelationAPI.Set<TRelationType>(sourceFaction, target, value);

        /// <summary>Sets a faction relation using the supplied relation-type asset.</summary>
        public static OperationResult SetRelation(
            [NotNull] FactionBase sourceFaction,
            [NotNull] IRelatable target,
            [NotNull] RelationTypeBase relationType,
            int value)
            => RelationAPI.Set(sourceFaction, target, relationType, value);

        /// <summary>Returns the value, or the relation type's initial value when this relation is untracked.</summary>
        public static int GetRelationValue<TRelationType>(
            [NotNull] FactionBase sourceFaction,
            [NotNull] IRelatable target)
            where TRelationType : RelationTypeBase, new()
            => RelationAPI.GetValue<TRelationType>(sourceFaction, target);

        /// <summary>Returns the value, or the relation type's initial value when this relation is untracked.</summary>
        public static int GetRelationValue(
            [NotNull] FactionBase sourceFaction,
            [NotNull] IRelatable target,
            [NotNull] RelationTypeBase relationType)
            => RelationAPI.GetValue(sourceFaction, target, relationType);

        /// <summary>Saves faction-to-faction and identified faction-to-runtime-object relations through SimpleSaving.</summary>
        [CanBeNull]
        public static SaveFileBase SaveToMemory()
        {
            FactionRelationSaveData saveData = new FactionRelationSaveData();
            return SaveAPI.Save(saveData);
        }

        /// <summary>
        ///     Restores faction relations through the SimpleSaving API. Runtime targets must be registered in
        ///     <see cref="RelatableObjectDatabase"/> using their stable identifier.
        /// </summary>
        public static void Load([NotNull] SaveFileBase saveFile)
        {
            FactionRelationSaveData saveData = new FactionRelationSaveData();
            SaveAPI.Load(saveData, saveFile);
        }
    }
}
