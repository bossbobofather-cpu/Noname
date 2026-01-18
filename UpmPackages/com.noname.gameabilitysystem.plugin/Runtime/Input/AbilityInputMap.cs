using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 입력 트리거 타입입니다.
    /// </summary>
    public enum AbilityInputTrigger
    {
        /// <summary>
        /// 눌림 순간입니다.
        /// </summary>
        Pressed,
        /// <summary>
        /// 뗌 순간입니다.
        /// </summary>
        Released,
        /// <summary>
        /// 수행됨 상태입니다.
        /// </summary>
        Performed,
    }

    /// <summary>
    /// 입력 액션과 이벤트 태그를 연결하는 바인딩입니다.
    /// </summary>
    [Serializable]
    public sealed class AbilityInputBinding
    {
        [SerializeField] private InputActionReference _action;
        [SerializeField] private string _actionName;
        [SerializeField] private FGameplayTag _eventTag;
        [SerializeField] private AbilityInputTrigger _trigger = AbilityInputTrigger.Pressed;

        /// <summary>
        /// 발생시킬 이벤트 태그입니다.
        /// </summary>
        public FGameplayTag EventTag => _eventTag;

        /// <summary>
        /// 입력 트리거 방식입니다.
        /// </summary>
        public AbilityInputTrigger Trigger => _trigger;

        /// <summary>
        /// 입력 액션을 찾아 반환합니다.
        /// </summary>
        /// <param name="asset">액션 에셋</param>
        /// <returns>찾아낸 액션</returns>
        public InputAction ResolveAction(InputActionAsset asset)
        {
            if (_action != null)
            {
                // 참조가 있으면 바로 반환한다.
                return _action.action;
            }

            if (string.IsNullOrWhiteSpace(_actionName) || asset == null)
            {
                // 이름이나 에셋이 없으면 찾을 수 없다.
                return null;
            }

            return asset.FindAction(_actionName, false);
        }
    }

    /// <summary>
    /// 능력 입력 바인딩 목록을 담는 에셋입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/Input/AbilityInputMap")]
    public sealed class AbilityInputMap : ScriptableObject
    {
        [SerializeField] private List<AbilityInputBinding> _bindings = new();

        /// <summary>
        /// 입력 바인딩 목록입니다.
        /// </summary>
        public IReadOnlyList<AbilityInputBinding> Bindings => _bindings;
    }
}
