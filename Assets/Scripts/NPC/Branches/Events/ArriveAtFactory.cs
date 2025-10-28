using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ArriveAtFactory")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ArriveAtFactory", message: "[NPC] Arrived at [Factory]", category: "NPC", id: "28bf55142ce0b970778e21345c690bd3")]
public sealed partial class ArriveAtFactory : EventChannel<GameObject, GameObject> { }

