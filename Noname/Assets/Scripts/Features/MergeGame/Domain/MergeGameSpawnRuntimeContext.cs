namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 정책의 런타임 상태를 담는 컨텍스트입니다.
    /// </summary>
    public sealed class MergeGameSpawnRuntimeContext
    {
        /// <summary>
        /// 누적 스폰 횟수입니다.
        /// </summary>
        public int SpawnCount { get; private set; }

        /// <summary>
        /// 스폰 정책 기준 경과 시간입니다.
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        /// 스폰 로직이 실행 중인지 여부입니다.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 컨텍스트를 초기화합니다.
        /// </summary>
        public void Reset()
        {
            // 누적 값을 초기화합니다.
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
        /// 스폰 횟수를 1 증가시킵니다.
        /// </summary>
        public void AddSpawn()
        {
            SpawnCount++;
        }

        /// <summary>
        /// 경과 시간을 누적합니다.
        /// </summary>
        public void AdvanceTime(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            // 실행 중일 때만 시간을 누적합니다.
            ElapsedTime += deltaTime;
        }
    }
}
