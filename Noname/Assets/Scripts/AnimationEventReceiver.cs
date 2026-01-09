using System;
using MergeGame.Config;
using UnityEngine;

namespace MergeGame.Unit
{
    [DisallowMultipleComponent]
    public sealed class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        public Animator Animator => _animator;

        public event Action<AnimationEventDataConfig> OnEventReceived;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        public void OnAnimationEventReceive(AnimationEventDataConfig eventData)
        {
            OnEventReceived?.Invoke(eventData);
        }
    }
}
