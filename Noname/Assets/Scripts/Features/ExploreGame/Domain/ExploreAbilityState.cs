using Noname.GameAbilitySystem;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 전투 중 사용할 수 있는 어빌리티의 상태입니다.
    /// 충전 기반 쿨다운 시스템을 사용합니다.
    /// </summary>
    public sealed class ExploreAbilityState
    {
        /// <summary>
        /// 어빌리티 고유 ID입니다.
        /// </summary>
        public string AbilityId { get; }

        /// <summary>
        /// 어빌리티 설정 정보입니다.
        /// </summary>
        public GameplayAbilityModel Config { get; }

        /// <summary>
        /// 현재 충전량입니다 (0.0 ~ MaxCharge).
        /// </summary>
        public float CurrentCharge { get; private set; }

        /// <summary>
        /// 최대 충전량입니다. 이 값에 도달하면 사용 가능합니다.
        /// </summary>
        public float MaxCharge { get; }

        /// <summary>
        /// 매턴마다 증가하는 충전량입니다.
        /// </summary>
        public float ChargeRate { get; }

        /// <summary>
        /// 사용 가능한 상태인지 확인합니다.
        /// </summary>
        public bool IsReady => CurrentCharge >= MaxCharge;

        public ExploreAbilityState(
            string abilityId,
            GameplayAbilityModel config,
            float maxCharge,
            float chargeRate)
        {
            AbilityId = abilityId;
            Config = config;
            MaxCharge = maxCharge;
            ChargeRate = chargeRate;
            CurrentCharge = maxCharge; // 전투 시작 시 즉시 사용 가능
        }

        /// <summary>
        /// 충전량을 증가시킵니다.
        /// </summary>
        public void AddCharge(float amount)
        {
            CurrentCharge += amount;
            if (CurrentCharge > MaxCharge)
            {
                CurrentCharge = MaxCharge;
            }
        }

        /// <summary>
        /// 어빌리티를 사용하고 충전량을 초기화합니다.
        /// </summary>
        public void Consume()
        {
            CurrentCharge = 0f;
        }

        /// <summary>
        /// 충전 퍼센트를 반환합니다 (0.0 ~ 1.0).
        /// </summary>
        public float GetChargePercent()
        {
            return CurrentCharge / MaxCharge;
        }
    }
}
