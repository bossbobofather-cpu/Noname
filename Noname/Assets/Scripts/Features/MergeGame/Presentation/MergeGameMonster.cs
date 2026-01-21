using System;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 몬스터 베이스 컴포넌트입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MergeGameMonster : MergeGameActorBase
    {
        [SerializeField] private MergeGameMonsterMover _mover;

        /// <summary>
        /// 목표 지점에 도착했을 때 호출됩니다.
        /// </summary>
        public event Action<MergeGameMonster> GoalReached;

        private void Awake()
        {
            if (_mover == null)
            {
                _mover = GetComponent<MergeGameMonsterMover>();
            }
        }

        protected override void OnEnable()
        {
            if (_mover != null)
            {
                _mover.ReachedGoal += HandleReachedGoal;
            }
        }

        protected override void OnDisable()
        {
            if (_mover != null)
            {
                _mover.ReachedGoal -= HandleReachedGoal;
            }
        }

        /// <summary>
        /// 몬스터의 이동 경로를 초기화합니다.
        /// </summary>
        public void InitializePath(MergeGamePath path)
        {
            if (_mover != null)
            {
                _mover.SetPath(path);
            }
        }

        private void HandleReachedGoal()
        {
            GoalReached?.Invoke(this);
        }

        protected override void HandleDeath()
        {
            base.HandleDeath();

            // 사망 시 몬스터 오브젝트 제거.
            Destroy(gameObject);
        }
    }
}
