using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Components;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Operations;
using Systems.SimpleRelations.Utility;
using UnityEngine;

namespace Systems.SimpleRelations.Tests
{
    public sealed class RelationComponentTests
    {
        private GameObject _sourceObject;
        private GameObject _targetObject;
        private TestRelatable _source;
        private TestRelatable _target;
        private TestRelationType _relationType;

        [SetUp]
        public void SetUp()
        {
            RelationTypeDatabase.ClearForTests();
            RelationTypeDatabase.GetExact<TestRelationType>();
            _sourceObject = new GameObject("Source");
            _targetObject = new GameObject("Target");
            _source = _sourceObject.AddComponent<TestRelatable>();
            _target = _targetObject.AddComponent<TestRelatable>();
            _relationType = ScriptableObject.CreateInstance<TestRelationType>();
            RelationTypeDatabase.RegisterForTests(_relationType);
        }

        [TearDown]
        public void TearDown()
        {
            RelationTypeDatabase.ClearForTests();
            Object.DestroyImmediate(_relationType);
            Object.DestroyImmediate(_sourceObject);
            Object.DestroyImmediate(_targetObject);
        }

        [Test]
        public void Change_CreatesOneSerializedEntry_AndLeavesReverseRelationUntouched()
        {
            RelationChangeContext context = new(_relationType, _target, 7);
            OperationResult result = _source.ChangeRelation(in context);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _source.Relations.Count);
            Assert.AreSame(_relationType, _source.Relations[0].RelationType);
            Assert.AreSame(_target, _source.Relations[0].Target);
            Assert.AreEqual(12, _source.Relations[0].Value);
            Assert.AreEqual(12, _source.GetRelationValue(_relationType, _target));
            Assert.IsTrue(_source.TryGetRelation(_relationType, _target, out RelationEntry plainEntry));
            Assert.IsTrue(_source.TryGetRelation<TestRelationType>(_target, out RelationEntry genericEntry));
            Assert.AreSame(plainEntry, genericEntry);
            Assert.IsTrue(_source.TryGetRelationValue(_relationType, _target, out int plainValue));
            Assert.IsTrue(_source.TryGetRelationValue<TestRelationType>(_target, out int genericValue));
            Assert.AreEqual(12, plainValue);
            Assert.AreEqual(12, genericValue);
            Assert.AreEqual(5, _target.GetRelationValue(_relationType, _source));
            Assert.AreEqual(0, _target.Relations.Count);
        }

        [Test]
        public void Set_ReusesMatchingEntry()
        {
            RelationChangeContext changeContext = new(_relationType, _target, 2);
            _source.ChangeRelation(in changeContext);
            RelationSetContext setContext = new(_relationType, _target, -30);
            OperationResult result = _source.SetRelation(in setContext);

            Assert.IsTrue(result);
            Assert.AreEqual(RelationOperations.SUCCESS_RELATION_SET, result.resultCode);
            Assert.AreEqual(1, _source.Relations.Count);
            Assert.AreEqual(-30, _source.GetRelationValue(_relationType, _target));
        }

        [Test]
        public void Change_RejectsSelfZeroAndOverflow()
        {
            RelationChangeContext selfContext = new(_relationType, _source, 1);
            RelationChangeContext zeroContext = new(_relationType, _target, 0);
            RelationSetContext setContext = new(_relationType, _target, int.MaxValue);
            RelationChangeContext overflowContext = new(_relationType, _target, 1);
            OperationResult selfResult = _source.ChangeRelation(in selfContext);
            OperationResult zeroResult = _source.ChangeRelation(in zeroContext);
            OperationResult setResult = _source.SetRelation(in setContext);
            OperationResult overflowResult = _source.ChangeRelation(in overflowContext);

            AssertError(selfResult, RelationOperations.ERROR_INVALID_TARGET);
            AssertError(zeroResult, RelationOperations.ERROR_INVALID_AMOUNT);
            Assert.IsTrue(setResult);
            AssertError(overflowResult, RelationOperations.ERROR_VALUE_OVERFLOW);
        }

        [Test]
        public void API_ResolvesRegisteredTypeAndChangesRelation()
        {
            RelationChangeContext<TestRelationType> changeContext = new(_source, _target, 4);
            RelationQueryContext<TestRelationType> queryContext = new(_source, _target);
            OperationResult result = RelationAPI.Change<TestRelationType>(in changeContext);
            bool hasValue = RelationAPI.TryGetValue<TestRelationType>(in queryContext, out int value);

            Assert.IsTrue(result);
            Assert.IsTrue(hasValue);
            Assert.AreEqual(9, value);
        }

        private static void AssertError(OperationResult result, ushort expectedResultCode)
        {
            Assert.IsTrue(OperationResult.IsError(result));
            Assert.IsTrue(OperationResult.IsFromSystem(result, RelationOperations.SYSTEM_RELATIONS));
            Assert.AreEqual(expectedResultCode, result.resultCode);
        }

        private sealed class TestRelatable : RelationComponentBase { }

        private sealed class TestRelationType : RelationTypeBase
        {
            protected internal override int InitialValue => 5;
        }
    }
}
