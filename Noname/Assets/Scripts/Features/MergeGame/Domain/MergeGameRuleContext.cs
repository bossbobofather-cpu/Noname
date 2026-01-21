namespace MyProject.MergeGame
{
    /// <summary>
    /// 룰 판단에 필요한 상태를 담는 컨텍스트입니다.
    /// </summary>
    public sealed class MergeGameRuleContext
    {
        /// <summary>
        /// 목표 도달 횟수입니다.
        /// </summary>
        public int GoalReached { get; private set; }

        /// <summary>
        /// 스폰된 유닛 횟수입니다.
        /// </summary>
        public int SpawnCount { get; private set; }

        /// <summary>
        /// 누적 경과 시간입니다.
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        /// 현재 실행 중인지 여부입니다.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 컨텍스트를 초기화합니다.
        /// </summary>
        public void Reset()
        {
            GoalReached = 0;
            SpawnCount = 0;
            ElapsedTime = 0f;
        }

        /// <summary>
        /// 실행 상태를 설정합니다.
        /// </summary>
        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;
        }

        /// <summary>
        /// 목표 도달 횟수를 증가시킵니다.
        /// </summary>
        public void AddGoal()
        {
            GoalReached++;
        }

        /// <summary>
        /// 스폰 횟수를 증가시킵니다.
        /// </summary>
        public void AddSpawn()
        {
            SpawnCount++;
        }

        /// <summary>
        /// 경과 시간을 더합니다.
        /// </summary>
        public void AdvanceTime(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            ElapsedTime += deltaTime;
        }
    }
}
