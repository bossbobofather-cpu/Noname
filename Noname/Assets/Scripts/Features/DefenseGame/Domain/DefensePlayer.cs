using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;
using MyProject.DefenseGame.Domain.AI;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임 플레이어입니다.
    /// </summary>
    public sealed class DefensePlayer : DefenseEntity
    {
        private readonly List<DefenseMonster> _targets = new();

        /// <summary>
        /// 플레이어 이름입니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 현재 경험치입니다.
        /// </summary>
        public int Experience => (int)ASC.Get(PlayerAttributeIds.Experience);

        /// <summary>
        /// 레벨업에 필요한 경험치입니다.
        /// </summary>
        public int RequiredExperience => GetRequiredExperience(GetLevel());

        /// <summary>
        /// 공격 대상을 찾는 함수입니다.
        /// </summary>
        public Func<Point2D, IReadOnlyList<DefenseMonster>> FindTargets { get; set; }

        /// <summary>
        /// 공격 이벤트입니다.
        /// </summary>
        public event Action<DefensePlayer, DefenseMonster, int> OnAttack;

        /// <summary>
        /// 레벨업 이벤트입니다.
        /// </summary>
        public event Action<DefensePlayer, int> OnLevelUp;

        public DefensePlayer(
            long uid,
            string name,
            Point2D position,
            AbilitySystemComponent asc) : base(uid, position, asc)
        {
            Name = name ?? "Player";
        }

        /// <summary>
        /// 매 틱마다 호출됩니다.
        /// </summary>
        public override void Tick(float deltaTime)
        {
            if (IsDead) return;

            // AI Update
            AI?.Update(this, deltaTime);
        }

        /// <summary>
        /// 가장 가까운 타겟을 찾습니다.
        /// </summary>
        public bool AddExperience(int amount)
        {
            if (amount <= 0) return false;

            var currentExp = Experience;
            var newExp = currentExp + amount;
            ASC.Set(PlayerAttributeIds.Experience, newExp);

            // 레벨업 체크
            if (newExp >= RequiredExperience)
            {
                var remaining = newExp - RequiredExperience;
                ASC.Set(PlayerAttributeIds.Experience, remaining);
                LevelUp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 레벨업을 수행합니다.
        /// </summary>
        private void LevelUp()
        {
            var currentLevel = GetLevel();
            var newLevel = currentLevel + 1;

            ASC.Set(DefenseAttributeIds.Level, newLevel);

            // 스탯 증가
            var hpIncrease = 10 + newLevel * 2;
            var newMaxHp = GetMaxHp() + hpIncrease;
            ASC.Set(DefenseAttributeIds.MaxHp, newMaxHp);
            ASC.Set(DefenseAttributeIds.Hp, newMaxHp); // 레벨업 시 풀피

            ASC.Add(DefenseAttributeIds.Attack, 2 + newLevel / 2);
            ASC.Add(DefenseAttributeIds.Defense, 1 + newLevel / 3);

            OnLevelUp?.Invoke(this, newLevel);
        }

        /// <summary>
        /// 레벨별 필요 경험치를 반환합니다.
        /// </summary>
        private static int GetRequiredExperience(int level)
        {
            return 100 + (level - 1) * 50;
        }
    }
}
