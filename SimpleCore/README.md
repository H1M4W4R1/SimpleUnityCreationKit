# SimpleCore

SimpleCore is the shared foundation for the Simple systems. It provides identifiers, operation results, addressable storage, timing, automation, localization helpers, math utilities, and the shared example UI helper.

## Dependencies

- Unity Addressables and ResourceManager
- Unity Burst, Collections, and Mathematics
- Unity Localization
- Unity uGUI

Input rebinding is provided by **SimpleInput**. Save-file abstractions and conversion pipelines are provided by **SimpleSaving**.

## Included modules

- `Automation/` – ScriptableObject and addressable generation automation.
- `Identifiers/` – fixed-size IDs, `HashIdentifier`, and `Snowflake128`.
- `Operations/` – allocation-free `OperationResult` values.
- `Storage/` – addressable databases and pooled list-access wrappers.
- `Timing/` – global tick scheduling.
- `Utility/` – localization and math helpers.
- `Examples/` – `ExampleRuntimePanel`, used by package example scenes.

## Tests

EditMode tests are in `Tests/EditMode/SimpleCore.Tests.asmdef`. Run them from Unity Test Runner after changing core APIs.
