using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 승패 판정 로직의 베이스입니다.
    /// </summary>
    public abstract class MergeGameWinCondition : ScriptableObject
    {
        /// <summary>
        /// 새 게임 시작 시 초기화를 처리합니다.
        /// </summary>
        public virtual void ResetCondition(MergeGameRuleContext context)
        {
        }

        /// <summary>
        /// 현재 컨텍스트로 승패를 판단합니다.
        /// </summary>
        public abstract MergeGameRuleResult Evaluate(MergeGameRuleContext context);
    }
}
