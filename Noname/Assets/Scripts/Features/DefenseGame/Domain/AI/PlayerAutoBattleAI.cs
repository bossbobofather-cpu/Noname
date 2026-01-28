using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Domain.AI
{
    /// <summary>
    /// 플레이어 자동 전투 AI입니다.
    /// Dirty Flag 패턴을 사용하여 태그가 변경되었을 때만 능력 활성화를 체크합니다.
    /// </summary>
    public class PlayerAutoBattleAI : IDefenseAI
    {
        private readonly List<GameplayAbilitySpec> _activatableAbilities = new();

        /// <summary>
        /// 타겟팅 컨텍스트입니다. Host에서 주입합니다.
        /// </summary>
        public TargetContext TargetContext { get; set; }

        public void Update(DefenseEntity entity, float deltaTime)
        {
            if (entity is not DefensePlayer player) return;
            if (player.IsDead) return;

            var asc = player.ASC;

            // 1. ASC 활성 효과 업데이트 (쿨다운 태그 만료 처리)
            asc.TickActiveEffects(deltaTime);

            // 2. Dirty Flag 체크 - 태그가 변경되지 않았으면 스킵
            if (!asc.ConsumeTagsChanged())
            {
                return;
            }

            // 3. 활성화 가능한 능력 조회
            _activatableAbilities.Clear();
            asc.GetActivatableAbilities(_activatableAbilities);

            if (_activatableAbilities.Count == 0)
            {
                return;
            }

            // 4. 타겟 컨텍스트가 없으면 스킵
            if (TargetContext == null)
            {
                return;
            }

            // 5. 능력 활성화 시도
            foreach (var spec in _activatableAbilities)
            {
                if (asc.TryActivateAbility(spec, TargetContext))
                {
                    // 능력 발동 성공 - 첫 번째 성공한 능력만 실행
                    break;
                }
            }
        }
    }
}
