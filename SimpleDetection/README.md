# SimpleDetection

SimpleDetection is a geometry-based detection system for Unity. Detectors evaluate registered `DetectableObjectBase` detection points in `FixedUpdate`, support ghost detection and lifecycle callbacks, and draw their zones in the Scene view.

## Detection modes

The standard detectors have trigger semantics: an eligible detection point is seen whenever it is inside the shape. They do not perform a `Physics` or `Physics2D` query.

| Dimension | Trigger-style detector | Parameters |
| --- | --- | --- |
| 2D | `Circle2DDetector` | radius |
| 2D | `Box2DDetector` | size, transform Z rotation |
| 2D | `Frustum2DDetector` | angle, radius, transform up direction |
| 3D | `Sphere3DDetector` | radius |
| 3D | `Box3DDetector` | size, transform rotation |
| 3D | `Cylinder3DDetector` | radius, height, transform rotation (local Y axis) |
| 3D | `Frustum3DDetector` | horizontal FOV, aspect ratio, near and far planes |

Each trigger-style shape also has an `IDetectionZone` counterpart named `*DetectionZone`, for custom detectors or tests.

Raycasting is opt-in through the explicitly named detectors: `RaycastingCircle2DDetector`, `RaycastingFrustum2DDetector`, `RaycastingSphere3DDetector`, and `RaycastingFrustum3DDetector`. Their `*DetectionZone` counterparts check the configured Raycast Layer Mask for obstruction.

`SimpleInteract` continues to use `RaycastingCircle2DDetector` and `RaycastingSphere3DDetector`, so existing interactions retain line-of-sight behavior after this change.

## Basic setup

Create a detectable object:

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleDetection.Components.Objects.Abstract;
using Systems.SimpleDetection.Data;
using UnityEngine;

public sealed class PlayerDetectable : DetectableObjectBase
{
    protected internal override void OnDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Detected by {context.detector.name}");
    }
}
```

Then create a detector. This trigger-style circle sees eligible objects inside its radius even if an obstacle is in between:

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleDetection.Components.Detectors.Base;
using Systems.SimpleDetection.Data;
using UnityEngine;

public sealed class ProximityDetector : Circle2DDetector
{
    protected override void OnObjectDetectionStart(
        in ObjectDetectionContext context,
        in OperationResult detectionResult)
    {
        Debug.Log($"{context.detectableObject.name} entered the zone");
    }
}
```

For line-of-sight behavior, change the base class to `RaycastingCircle2DDetector` and configure its Raycast Layer Mask with obstacle layers.

## Ghost detection

If a detector implements `ISupportGhostDetection`, it receives objects whose `CanBeDetected` result is `DetectionOperations.IsGhost()`. These ghost objects still need to pass the selected detector's geometric/visibility check. Without the marker interface, failed objects are removed from the detected list.

```csharp
using Systems.SimpleDetection.Components.Detectors.Base;
using Systems.SimpleDetection.Components.Detectors.Markers;

public sealed class AlertDetector : Frustum3DDetector, ISupportGhostDetection
{
}
```

## Custom detection points

Override `GetDetectionPositionsCount` and `UpdateDetectionPositions` when an object has more than one point. The detector exits early as soon as a point is seen.

```csharp
using Systems.SimpleDetection.Components.Objects.Abstract;
using UnityEngine;

public sealed class VehicleDetectable : DetectableObjectBase
{
    [SerializeField] private Transform[] checkPoints;

    protected internal override int GetDetectionPositionsCount() => checkPoints.Length;

    protected internal override void UpdateDetectionPositions()
    {
        DetectionPositions.Clear();
        for (int index = 0; index < checkPoints.Length; index++)
        {
            DetectionPositions.Add(checkPoints[index].position);
        }
    }
}
```

## Debugging

`DetectionSettings` controls per-object gizmo points, their colors and whether detector gizmos draw always or only while selected. Trigger-style zones use `InsideSeen` for every point inside their geometry. Raycasting zones additionally distinguish `InsideObstructed` for blocked points.
