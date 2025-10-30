using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasComponentOfType", story: "[Target] has component of [MonoBehaviourOfType]", category: "Conditions", id: "f674259b54cc181a3685043c8873ec9d")]
public partial class HasComponentOfTypeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<MonoBehaviour> MonoBehaviourOfType;

    public override bool IsTrue()
    {
        Type typeToCheck = MonoBehaviourOfType.Value.GetType();
        return Target.Value.GetComponent(typeToCheck) != null;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
