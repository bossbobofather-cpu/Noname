using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public struct GameplayEventData
    {
        public FGameplayTag EventTag;
        public GameObject Instigator;
        public GameObject Target;
        public Vector3 TargetLocation;
        public object Payload; // 필요 시 커스텀 타입
    }
}
