using UnityEngine;
using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.GameplayAbilitySystem.Bridge
{
    /// <summary>
    /// 애니메이션 이벤트를 수신하여 AbilitySystemComponent(ASC)에 GameplayEvent로 전달하는 브리지 컴포넌트입니다.
    /// 애니메이션에서 발생한 문자열 이벤트를 GameplayTag로 변환하여 처리합니다.
    /// </summary>
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

            // Bridge인데 IAnimEventReceiver가 없으면 기능 수행이 불가능하므로 비활성화
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

            // ASC가 없으면 자동으로 추가하여 기능 보장
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

        /// <summary>
        /// 애니메이션 이벤트 수신 시 호출됩니다.
        /// 이벤트 데이터를 태그로 변환하여 ASC에 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="eventData">이벤트 이름(태그 문자열)</param>
        private void OnAnimEventReceived(string eventData)
        {
            if(string.IsNullOrEmpty(eventData)) return;

            var registry = GameplayTagRegistry.RuntimeRegistry;
            if (registry == null) return;

            // 정의된 태그인지 확인 (정확히 일치해야 함)
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