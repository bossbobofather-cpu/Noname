using System.Collections;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 애니메이션 재생을 기다리는 태스크입니다.
    /// </summary>
    public sealed class AbilityTask_PlayMontageAndWait : AbilityTask
    {
        private Animator _animator;
        private int _stateHash;
        private int _layer;
        private float _normalizedTime;
        private float _fallbackDuration;
        private bool _hasState;

        /// <summary>
        /// 애니메이션 재생 태스크를 생성하고 활성화합니다.
        /// </summary>
        /// <param name="owner">태스크 소유자</param>
        /// <param name="animator">애니메이터</param>
        /// <param name="stateName">재생할 상태 이름</param>
        /// <param name="layer">재생 레이어</param>
        /// <param name="normalizedTime">시작 시간</param>
        /// <param name="fallbackDuration">대체 대기 시간</param>
        /// <returns>생성된 태스크</returns>
        public static AbilityTask_PlayMontageAndWait PlayMontageAndWait(
            IAbilityTaskOwner owner,
            Animator animator,
            string stateName,
            int layer = 0,
            float normalizedTime = 0f,
            float fallbackDuration = 0f)
        {
            var task = new AbilityTask_PlayMontageAndWait
            {
                _animator = animator,
                _layer = layer,
                _normalizedTime = normalizedTime,
                _fallbackDuration = fallbackDuration,
                _hasState = !string.IsNullOrWhiteSpace(stateName),
                _stateHash = !string.IsNullOrWhiteSpace(stateName) ? Animator.StringToHash(stateName) : 0
            };

            task.Initialize(owner);
            task.Activate();
            return task;
        }

        protected override void OnActivate()
        {
            if (_animator == null || !_hasState)
            {
                // 재생 조건이 없으면 바로 종료한다.
                EndTask();
                return;
            }

            StartRoutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            // 지정된 상태를 재생한다.
            _animator.Play(_stateHash, _layer, _normalizedTime);
            _animator.Update(0f);
            yield return null;

            if (_animator == null)
            {
                EndTask();
                yield break;
            }

            while (IsActive)
            {
                var info = _animator.GetCurrentAnimatorStateInfo(_layer);
                var isState = info.fullPathHash == _stateHash || info.shortNameHash == _stateHash;
                if (isState)
                {
                    if (info.normalizedTime >= 1f)
                    {
                        break;
                    }
                }
                else if (_fallbackDuration > 0f)
                {
                    // 다른 상태가 나오면 대체 대기 후 종료한다.
                    yield return new WaitForSeconds(_fallbackDuration);
                    break;
                }

                yield return null;
            }

            if (!IsActive)
            {
                yield break;
            }

            EndTask();
        }
    }
}
