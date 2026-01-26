using Noname.GameAbilitySystem;
using UnityEngine;

[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEventTriggerConfig")]
/// <summary>
/// 이벤트 트리거 설정을 담는 에셋입니다.
/// </summary>
public class GameplayEventTriggerConfig : GameplayConfig
{
    /// <summary>
    /// 트리거로 사용할 태그입니다.
    /// </summary>
    public FGameplayTag TriggerTag;

    /// <summary>
    /// 이벤트 수신 시 즉시 활성화할지 여부입니다.
    /// </summary>
    public bool ActivateOnEvent = true;
}
