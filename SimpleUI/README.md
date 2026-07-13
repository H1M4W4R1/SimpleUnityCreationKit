# SimpleUI

A lightweight, modular UI framework for Unity that provides reusable components and patterns for building user interfaces with minimal boilerplate. Built on top of Unity's Canvas and EventSystem, SimpleUI streamlines common UI tasks like window management, list rendering, animations, and interactive elements.

## Requirements

### Dependencies
- **Unity 2022.3+** (uses TextMeshPro)
- **SimpleCore** (internal dependency)
- **DOTween** (for UI animations)
- **Unity.Addressables** (for window asset management)
- **TextMeshPro** (for text rendering)
- **Unity.Collections** (required by list system)
- **Unity.Mathematics** (required by assembly)
- **Unity.Burst** (referenced by assembly)

### Assembly Definition
- `SimpleUserInterface.asmdef` - Main assembly with unsafe code enabled

## Usage

### Basic Components

#### Creating Custom Buttons
```csharp
using Systems.SimpleUI.Components.Buttons;
using UnityEngine;

public sealed class MyCustomButton : UIButtonBase
{
    protected override void OnClick()
    {
        Debug.Log("Button clicked!");
    }
}
```
Inherit from `UIButtonBase` and implement `OnClick()`. A `Button` component is automatically required via `[RequireComponent]`.

#### Working with Windows
```csharp
using Systems.SimpleUI.Components.Windows;
using Systems.SimpleUI.Utility;

// Open a window
UserInterface.OpenWindow<MyWindowType>();

// Open with context
UserInterface.OpenWindow<MyWindowType>(context: myData);

// Open as dependent of another window (closes automatically when parent closes)
UserInterface.OpenWindow<MyWindowType>(parentWindow: parentWindow);

// Close a specific window
UserInterface.CloseWindow(window);

// Close all open windows
UserInterface.CloseAll();
```
Windows are fetched from `WindowsDatabase` (Addressables label `SimpleUI.Windows`), cached after first use, and can be configured to allow single or multiple instances by overriding `AllowMultipleInstancesWithSameContext` and `AllowMultipleInstancesWithDifferentContext`. Root prefab objects with a `UIWindowBase` component are automatically registered with the `SimpleUI.Windows` Addressables label by editor automation; nested window components and scene objects are ignored.

#### Working with Popups
```csharp
using Systems.SimpleUI.Components.Windows;
using Systems.SimpleUI.Utility;

// Open a popup (queued automatically if another popup is already open)
UserInterface.OpenPopup<MyPopupType>();
```
Popups extend `UIPopupBase` and only one is shown at a time. Closing a popup automatically opens the next one in the queue.

#### Creating Custom Text Elements
```csharp
using Systems.SimpleUI.Components.Text;

public sealed class DynamicLabel : UITextObject
{
    // Inherits from UIObjectWithContextBase<string>
    // Automatically renders text from string context
    public override void OnRender(string withContext)
    {
        base.OnRender(withContext);
        // Additional custom rendering logic here
    }
}
```

### Advanced Components

#### Lists and Dynamic Content
```csharp
using Systems.SimpleUI.Components.Lists;
using Systems.SimpleUI.Components.Abstract.Markers;

// Define the list container
public sealed class MyList : UIListBase<MyDataType>
{
    // Reads context of type ListContext<MyDataType> from a parent ContextProviderBase<ListContext<MyDataType>>
    // Assign an ElementPrefab in the Inspector to enable automatic element creation
}

// Define how each item is rendered
public sealed class MyListElement : UIListElementBase<MyDataType>, IRenderable<MyDataType>
{
    public void OnRender(MyDataType withContext)
    {
        // Update element UI based on withContext
    }
}
```
Lists automatically pool and recycle `UIListElementBase` elements. Set the `ElementPrefab` field in the Inspector, and the list handles instantiation, pooling, and per-element context updates.

#### Context-Driven UI
```csharp
using Systems.SimpleUI.Context.Abstract;

public sealed class MyContextProvider : ContextProviderBase<MyDataType>
{
    public override MyDataType GetContext()
    {
        // Return the data to expose to child UI elements
        return myData;
    }
}
```
Attach a `ContextProviderBase<T>` MonoBehaviour as a parent of UI elements that declare `IWithContext<T>`. The framework automatically discovers providers by walking up the hierarchy. Use context providers to pass data to UI elements without tight coupling.

### Animations

#### Adding Show/Hide Animations
```csharp
using Systems.SimpleUI.Components.Animations;

// Built-in animations:
// - ScaleShowHideAnimation: Scales element in/out
// - EnableShowAnimation: Simply activates GameObject
// - DisableHideAnimation: Simply deactivates GameObject
```
Attach animation components to your UI objects. They automatically integrate with the show/hide lifecycle.
Without an animation component, `Show()` activates the GameObject and `Hide()` deactivates it.

#### Custom Animations
```csharp
using Systems.SimpleUI.Components.Animations.Abstract;
using DG.Tweening;

public sealed class MyCustomAnimation : UIAnimationBase, IUIShowAnimation, IUIHideAnimation
{
    public Sequence OnShow()
    {
        // Return a DOTween Sequence for show animation
    }

    public Sequence OnHide()
    {
        // Return a DOTween Sequence for hide animation
    }
}
```

### Interactive Features

#### Drag and Drop
```csharp
using Systems.SimpleUI.Components.Features.Drag;

// Attach DragFeature to make objects draggable
// Attach DropZoneFeature to define drop targets
// Use SlotFeature for inventory-style slot systems
```

#### Positioning Constraints
```csharp
using Systems.SimpleUI.Components.Features.Positioning;

// LimitObjectToParent: Keeps UI within parent bounds
// LimitObjectToViewport: Keeps UI within screen bounds
```
`DraggableWindowFeature` keeps free-dropped windows under their original canvas and relies on `LimitObjectToViewport` to keep them inside the root UI canvas bounds.

### Common Patterns

#### Managing Multiple Windows
Windows are automatically managed with sorting orders:
- Regular windows: `UI_WINDOW_SORTING_ORDER = 15000`
- Popups: `UI_POPUP_SORTING_ORDER = 20000`
- Overlays: `UI_OVERLAY_SORTING_ORDER = 25000`
- Tooltips: `UI_TOOLTIP_SORTING_ORDER = 30000`

#### Responsive Rendering
```csharp
using Systems.SimpleUI.Components.Abstract.Markers;

// Implement IRenderable<TContext> to respond to context changes
public sealed class MyElement : UIObjectWithContextBase<MyContext>, IRenderable<MyContext>
{
    public void OnRender(MyContext withContext)
    {
        // Update UI based on context
    }
}
```

## Examples

The `Examples/` directory includes demonstrations of:
- Buttons, text display, input fields, sliders, scrollbars, toggles, and selectors (carousel, dropdown, spinner) (00)
- Progress bars (01)
- Windows and popups (02)
- List rendering with pooled elements (03)
- Drag and drop with slots and drop zones (04)
- Tab systems (05)
- Tooltips (06)
- 3D model viewports (07)

## Architecture

SimpleUI follows a component-based architecture:
- **Abstract Base Classes**: `UIObjectBase`, `UIInteractableObjectBase`, `UIObjectWithContextBase` provide core functionality
- **Markers/Interfaces**: `IRenderable`, `IWithContext` define behavior contracts
- **Context System**: Decouples data from presentation through context providers
- **Animation System**: Extensible animation framework using DOTween sequences
- **Window Management**: Centralized window lifecycle and focus management via `UserInterface` utility class

## License

Copyright 2025 H1M4W4R1 - MIT License
