using Noname.GameAbilitySystem;
using UnityEngine;

[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEventTriggerConfig")]
public sealed class GameplayEventTriggerConfig : GameplayConfig
{
    public FGameplayTag TriggerTag;
    public bool ActivateOnEvent = true;
}
