# SimpleDialogue

SimpleDialogue is a xNode-backed branching dialogue system. It keeps graph traversal separate from presentation so the same dialogue can be rendered as a visual novel panel, a list, a bark overlay, or a custom in-game UI.

## Setup

SimpleDialogue depends on:

- `com.github.siccity.xnode`
- `SimpleCore`
- `SimpleUI`
- `Unity.TextMeshPro`
- `Unity.ugui`

The project manifest already includes xNode through the Git URL:

```json
"com.github.siccity.xnode": "https://github.com/siccity/xNode.git"
```

If you move SimpleDialogue into another project, add xNode through Package Manager and make sure asmdefs reference `XNode`. Editor helpers additionally reference `XNodeEditor`.

## Graphs

Create a graph through `Assets > Create > Simple Dialogue > Dialogue Graph`.

The built-in nodes are:

- `DialogueEntryNode`: named graph entry point. The default entry id is `default`.
- `BasicNPCDialogueNode`: an NPC line with player-answer outputs and an optional `next` output for NPC-only sequences.
- `BasicPlayerDialogueNode`: a player answer with a single next output.
- `DialogueExitNode`: ends the active dialogue.
- `SubDialogueNode`: enters another graph and entry id.

The graph menu uses clear `Dialogue/...` paths and only shows concrete `DialogueInteractionNode` implementations, so nodes from other xNode graph types cannot be added accidentally.

Custom dialogue nodes inherit from `NPCDialogueNode`, `PlayerDialogueNode`, or `DialogueInteractionNode`. Provide text through methods, not base fields:

```csharp
protected internal override string GetSpeakerName(in DialogueContext context)
{
    return "Archivist";
}

protected internal override string GetText(in DialogueContext context)
{
    return "The gate remembers every name.";
}
```

Use `IsVisible`, `IsAvailable`, and `CanEnter` for entry conditions. Invisible answers are not rendered; unavailable answers are rendered disabled.

For non-rendered graph flow, derive from `ConditionalDialogueNode` and implement `EvaluateCondition`. Its `whenTrue` and `whenFalse` ports act as an `if` branch. To branch by an enum, derive from `SwitchDialogueNode<TEnum>` and implement `GetSwitchValue`; every enum member automatically becomes an output port, with `otherwise` as a fallback.

## Running Dialogue

Add `Dialogue` to a GameObject and assign a `DialogueGraph`.

```csharp
OperationResult result = DialogueAPI.Begin(dialogue);
```

To select an answer:

```csharp
DialogueOption option = dialogue.Options[0];
OperationResult result = DialogueAPI.Select(in option);
```

For an NPC line without answer options, connect its `next` port to the following NPC node and call `Advance` after the player interacts:

```csharp
OperationResult canAdvanceResult = dialogue.CanAdvance();
if (!canAdvanceResult) return;

OperationResult result = dialogue.Advance();
```

Renderers receive the same state through `DialogueViewContext.CanAdvance`, allowing a continue prompt or button to be displayed only when the next NPC node can be entered.

Only one dialogue may run at a time. Starting another dialogue while one is active returns `DialogueOperations.AnotherDialogueRunning()`.

`Dialogue` automatically finds the first `IDialogueRenderer` on itself or inactive/active children. No renderer field is serialized on the runner.

## Renderers

Presentation is handled by `IDialogueRenderer`:

```csharp
public sealed class CustomDialogueRenderer : MonoBehaviour, IDialogueRenderer
{
    public void RenderDialogue(DialogueViewContext context)
    {
        // Render context.SpeakerName, context.Text, and context.Options.
    }

    public void ClearDialogue()
    {
        // Hide or reset the UI.
    }
}
```

The package includes `SimpleVisualNovelDialogueRenderer`, a bottom-of-screen SimpleUI panel using TextMeshPro. It uses:

- `SimpleDialogueText` for speaker/body text.
- `SimpleDialogueAnswerContainer` for answer lists.
- `SimpleDialogueAnswerOption` for answer buttons.

This renderer is intentionally just one implementation. You can replace it with a typewriter renderer, radial answers, subtitle-only renderer, or any other `IDialogueRenderer`.

## Examples

Open `Examples/Scene - Dialogue.unity` and enter Play Mode to run the included `Examples/Dialogue Graph.asset`.

The scene uses the built-in SimpleUI visual novel renderer. `DialogueExampleStarter` begins the dialogue automatically, and the answer buttons advance through the graph.
