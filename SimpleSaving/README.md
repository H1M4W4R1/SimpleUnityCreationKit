# SimpleSaving

SimpleSaving supplies save-file payload abstractions and conversion paths between file versions. It does not serialize to disk; the host application owns serialization, storage, and encryption.

## Basic use

```csharp
using System;
using Systems.SimpleSaving.Abstract;
using Systems.SimpleSaving.Utility;

[Serializable]
public sealed class PlayerSaveFile : SaveFileBase
{
    public int Level;
}

public sealed class PlayerData : ISaveData<PlayerSaveFile>
{
    public int Level { get; private set; }

    public PlayerSaveFile BuildSaveFile()
    {
        return new PlayerSaveFile { Level = Level };
    }

    public void ParseSaveFile(PlayerSaveFile saveFile)
    {
        Level = saveFile.Level;
    }
}
```

Use `SaveAPI.Save`, `SaveAPI.SaveAs<TSaveFile>`, and `SaveAPI.Load` when an object supports more than one save-file version. Implement `IUpgradeableSaveFile<TToFile, TFromFile>` or `IDowngradableSaveFile<TToFile, TFromFile>` to declare conversion edges.

## Tests

EditMode tests are in `Tests/EditMode/SimpleSaving.Tests.asmdef`.
