using System;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 몬스터를 경로를 따라 이동시키는 컴포넌트입니다.
    /// </summary>
    public sealed class MergeGameMonsterMover : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _arriveDistance = 0.1f;
        [SerializeField] private MergeGamePath _path;

        private int _index;
        private bool _reachedGoal;

        /// <summary>
        /// 목표 지점에 도착했을 때 호출됩니다.
        /// </summary>
        public event Action ReachedGoal;

        /// <summary>
        /// 이동 경로를 설정합니다.
        /// </summary>
        public void SetPath(MergeGamePath path)
        {
            _path = path;
            _index = 0;
            _reachedGoal = false;

            if (_path != null)
            {
                transform.position = _path.GetStartPoint();
            }
        }

        private void Update()
        {
            if (_reachedGoal || _path == null || _path.Count == 0)
            {
                return;
            }

            // 다음 경로 포인트로 이동한다.
            var target = _path.GetPoint(_index);
            var position = transform.position;
            var direction = target - position;
            var distance = direction.magnitude;

            if (distance <= _arriveDistance)
            {
                _index++;
                if (_index >= _path.Count)
                {
                    // 목표 지점 도달.
                    _reachedGoal = true;
                    ReachedGoal?.Invoke();
                }

                return;
            }

            var move = direction.normalized * (_moveSpeed * Time.deltaTime);
            transform.position = position + move;
        }
    }
}
