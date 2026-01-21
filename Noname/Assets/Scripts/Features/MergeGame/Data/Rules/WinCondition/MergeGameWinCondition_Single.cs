using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 시간 제한과 목표 도달 제한을 함께 사용하는 기본 승패 조건입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/WinCondition/Single")]
    public sealed class MergeGameWinCondition_Single : MergeGameWinCondition
    {
        //남아 있는 몬스터 제한 수 //해당 수량보다 많은 몬스터가 유지 된다면 패배
        [SerializeField] private int _limitEnemies = 10;

        /// <summary>
        /// 현재 컨텍스트로 승패를 판단합니다.
        /// </summary>
        public override MergeGameRuleResult Evaluate(MergeGameRuleContext context)
        {
            if (context == null)
            {
                return MergeGameRuleResult.None;
            }

            if (context.SpawnCount >= _limitEnemies)
            {
                return MergeGameRuleResult.Lose;
            }

            if (context.ElapsedTime <= 0)
            {
                return MergeGameRuleResult.Win;
            }

            return MergeGameRuleResult.None;
        }
    }
}
