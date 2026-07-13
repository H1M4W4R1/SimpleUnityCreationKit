using System;
using JetBrains.Annotations;
using Systems.SimpleDialogue.Data;
using XNode;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Non-rendered flow node that selects a next node from an enum-defined set of outputs.
    /// </summary>
    public abstract class SwitchDialogueNode : DialogueInteractionNode
    {
        [CanBeNull] internal abstract DialogueInteractionNode GetNextNode(in DialogueContext context);

        protected internal sealed override string GetSpeakerName(in DialogueContext context) => string.Empty;

        protected internal sealed override string GetText(in DialogueContext context) => string.Empty;
    }

    /// <summary>
    ///     Enum-backed switch node. Each enum member becomes an output port named after that member.
    /// </summary>
    /// <typeparam name="TEnum">Enum that defines the switch output names.</typeparam>
    [NodeTint("#A47F43")]
    public abstract class SwitchDialogueNode<TEnum> : SwitchDialogueNode
        where TEnum : struct, Enum
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public DialogueConnection otherwise;

        /// <summary>
        ///     Gets the enum value used to select an output for this interaction.
        /// </summary>
        protected internal abstract TEnum GetSwitchValue(in DialogueContext context);

        [CanBeNull] internal sealed override DialogueInteractionNode GetNextNode(in DialogueContext context)
        {
            TEnum switchValue = GetSwitchValue(in context);
            DialogueInteractionNode nextNode = GetConnectedNode(switchValue.ToString());
            return ReferenceEquals(nextNode, null) ? GetConnectedNode(nameof(otherwise)) : nextNode;
        }

        protected override void Init()
        {
            base.Init();

            string[] enumNames = Enum.GetNames(typeof(TEnum));
            for (int enumIndex = 0; enumIndex < enumNames.Length; enumIndex++)
            {
                string enumName = enumNames[enumIndex];
                if (HasPort(enumName)) continue;

                AddDynamicOutput(
                    typeof(DialogueConnection),
                    ConnectionType.Override,
                    TypeConstraint.Strict,
                    enumName);
            }
        }

        [CanBeNull] private DialogueInteractionNode GetConnectedNode(string portName)
        {
            NodePort port = GetOutputPort(portName);
            if (ReferenceEquals(port, null)) return null;

            NodePort connectedPort = port.Connection;
            if (ReferenceEquals(connectedPort, null)) return null;

            return connectedPort.node as DialogueInteractionNode;
        }
    }
}
