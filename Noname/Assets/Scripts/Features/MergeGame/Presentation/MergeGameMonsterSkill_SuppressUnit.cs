using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 일정 주기로 최고 등급 유닛을 약화시키는 몬스터 스킬입니다.
    /// </summary>
    public sealed class MergeGameMonsterSkill_SuppressUnit : MonoBehaviour
    {
        [SerializeField] private MergeGameBoard _board;
        [SerializeField] private float _cooldown = 3f;

        private float _nextUseTime;

        private void Awake()
        {
            if (_board == null)
            {
                _board = FindFirstObjectByType<MergeGameBoard>();
            }
        }

        private void Update()
        {
            if (_board == null)
            {
                return;
            }

            if (Time.time < _nextUseTime)
            {
                return;
            }

            var target = _board.GetHighestGradeUnit();
            if (target == null)
            {
                _nextUseTime = Time.time + _cooldown;
                return;
            }

            // 최고 등급 유닛을 한 단계 약화한다.
            target.DowngradeOrRemove();
            _nextUseTime = Time.time + _cooldown;
        }
    }
}
