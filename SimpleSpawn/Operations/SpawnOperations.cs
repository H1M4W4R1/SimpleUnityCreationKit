using Systems.SimpleCore.Operations;

namespace Systems.SimpleSpawn.Operations
{
    /// <summary>
    ///     Operation results returned by SimpleSpawn.
    /// </summary>
    public static class SpawnOperations
    {
        public const ushort SYSTEM_SPAWN = 0x0012;

        public const ushort SUCCESS_SPAWNED = 1;
        public const ushort SUCCESS_DESPAWNED = 2;
        public const ushort SUCCESS_ALL_DESPAWNED = 3;
        public const ushort SUCCESS_GROUP_SPAWNED = 4;
        public const ushort SUCCESS_WAVE_SPAWNED = 5;

        public const ushort ERROR_SPAWN_LIST_NOT_ASSIGNED = 1;
        public const ushort ERROR_SPAWN_NOT_PERMITTED = 2;
        public const ushort ERROR_ENTITY_NOT_GENERATED = 3;
        public const ushort ERROR_INVALID_PREFAB = 4;
        public const ushort ERROR_ENTITY_ALREADY_DESPAWNED = 5;
        public const ushort ERROR_INVALID_SPAWN_COUNT = 6;
        public const ushort ERROR_SPAWN_POSITION_NOT_GENERATED = 7;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_SPAWN, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Spawned()
            => OperationResult.Success(SYSTEM_SPAWN, SUCCESS_SPAWNED);

        public static OperationResult Despawned()
            => OperationResult.Success(SYSTEM_SPAWN, SUCCESS_DESPAWNED);

        public static OperationResult AllDespawned()
            => OperationResult.Success(SYSTEM_SPAWN, SUCCESS_ALL_DESPAWNED);

        public static OperationResult GroupSpawned()
            => OperationResult.Success(SYSTEM_SPAWN, SUCCESS_GROUP_SPAWNED);

        public static OperationResult WaveSpawned()
            => OperationResult.Success(SYSTEM_SPAWN, SUCCESS_WAVE_SPAWNED);

        public static OperationResult SpawnListNotAssigned()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_SPAWN_LIST_NOT_ASSIGNED);

        public static OperationResult SpawnNotPermitted()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_SPAWN_NOT_PERMITTED);

        public static OperationResult EntityNotGenerated()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_ENTITY_NOT_GENERATED);

        public static OperationResult InvalidPrefab()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_INVALID_PREFAB);

        public static OperationResult EntityAlreadyDespawned()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_ENTITY_ALREADY_DESPAWNED);

        public static OperationResult InvalidSpawnCount()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_INVALID_SPAWN_COUNT);

        public static OperationResult SpawnPositionNotGenerated()
            => OperationResult.Error(SYSTEM_SPAWN, ERROR_SPAWN_POSITION_NOT_GENERATED);
    }
}
