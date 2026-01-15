using UnityEngine;
using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.GameplayAbilitySystem.Bridge
{
    [DisallowMultipleComponent]
    public class AnimationEventBridge : MonoBehaviour
    {
        [SerializeField] private IAnimEventReceiver _receiver;
        [SerializeField] private AbilitySystemComponent _abilitySystem;

        private void Awake()
        {
            if (_receiver == null)
            {
                _receiver = GetComponent<IAnimEventReceiver>();
            }

            //Bridge인데 IAnimEventReceiver 없으면 의미가 없으니 자동 추가
            if (_receiver == null)
            {
                Debug.LogError($"[IAnimEventReceiver] Missing on {gameObject.name}.");
                enabled = false;
                return;
            }

            if (_abilitySystem == null)
            {
                _abilitySystem = GetComponent<AbilitySystemComponent>();
            }

            //Bridge인데 AbilitySystemComponent가 없으면 의미가 없으니 자동 추가
            if (_abilitySystem == null)
            {
                _abilitySystem = gameObject.AddComponent<AbilitySystemComponent>();
                Debug.LogWarning($"ASC가 없어서 추가되었습니다. {gameObject.name}.");
            }
        }

        private void OnEnable()
        {
            if(_receiver != null) _receiver.OnAnimEventReceived += OnAnimEventReceived;
        }

        private void OnDisable()
        {
            if(_receiver != null) _receiver.OnAnimEventReceived -= OnAnimEventReceived;
        }

        //해당 브릿지는 애니메이션 이벤트 발생 시 string 타입의 Data를 Tag로 변환하여 ASC에 Event Trigger로써 사용 한다.
        private void OnAnimEventReceived(string eventData)
        {
            if(string.IsNullOrEmpty(eventData)) return;

            var registry = GameplayTagRegistry.RuntimeRegistry;
            if (registry == null) return;

            //정확히 일치한 것만
            if (!registry.IsTagDefined(eventData, includeParents: false))
            {
                Debug.LogWarning($"Unknown tag: {eventData}");
                return;
            }

            var tag = new FGameplayTag(eventData);

            if(_abilitySystem == null) return;

            _abilitySystem.HandleGameplayEvent(new GameplayEventData(tag));
        }
    }
}