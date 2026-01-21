using MyProject.Common.GameEvent;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 목표 도달 횟수가 증가했을 때 발행되는 이벤트입니다.
    /// </summary>
    public sealed class MergeGameGoalReachedEvent : SceneGameEventContext
    {
        /// <summary>
        /// 누적 목표 도달 횟수입니다.
        /// </summary>
        public int GoalReached { get; }

        public MergeGameGoalReachedEvent(object source, int goalReached)
            : base(source)
        {
            GoalReached = goalReached;
        }
    }
}
