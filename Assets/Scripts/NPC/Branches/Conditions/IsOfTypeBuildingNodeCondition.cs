using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsOfTypeBuildingNode", story: "[Target] is of type BuildingNode", category: "Conditions", id: "c11014dbff79a0cf516dcc078ffb243d")]
public partial class IsOfTypeBuildingNodeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        return Target.Value.GetComponent<BuildingNode>() != null;
    }
}
