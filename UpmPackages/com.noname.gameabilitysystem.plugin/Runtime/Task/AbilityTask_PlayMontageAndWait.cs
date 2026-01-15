using System.Collections;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public sealed class AbilityTask_PlayMontageAndWait : AbilityTask
    {
        private Animator _animator;
        private int _stateHash;
        private int _layer;
        private float _normalizedTime;
        private float _fallbackDuration;
        private bool _hasState;

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
                EndTask();
                return;
            }

            StartRoutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
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
