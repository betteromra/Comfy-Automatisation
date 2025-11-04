using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SwitchTarget", story: "Set [Target] to element of [WalkingPoints] using [index] and store previous in [PreviousTarget]", category: "Action", id: "7cda82a0186de7341309d79069c5b26e")]
public partial class SwitchTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<List<GameObject>> WalkingPoints;
    [SerializeReference] public BlackboardVariable<int> Index;
    [SerializeReference] public BlackboardVariable<GameObject> PreviousTarget;

    protected override Status OnStart()
    {
        Index.Value++;

        if (Index.Value >= WalkingPoints.Value.Count)
            Index.Value = 0;

        if (PreviousTarget != null)
            PreviousTarget.Value = Target.Value;
            
        Target.Value = WalkingPoints.Value[Index];

        return Status.Success;
    }
}

