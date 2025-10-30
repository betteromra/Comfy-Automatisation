using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsOfTypeInputNode", story: "[Target] is of type InputNode", category: "Conditions", id: "c4d7808fd205381ed5ef068782e1cfc1")]
public partial class IsOfTypeInputNodeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        return Target.Value.GetComponent<InputNode>() != null;
    }
}
