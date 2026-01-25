using System;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 몬스터의 상태를 나타냅니다.
    /// </summary>
    public sealed class ExploreMonsterState
    {
        public long Uid { get; }
        public string MonsterType { get; }
        public int Level { get; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; }
        public int AttackPower { get; }
        public int Defense { get; }
        public int GoldReward { get; }
        public int ExpReward { get; }

        public bool IsDead => CurrentHp <= 0;

        public ExploreMonsterState(
            long uid,
            string monsterType,
            int level,
            int maxHp,
            int attackPower,
            int defense,
            int goldReward,
            int expReward)
        {
            if (string.IsNullOrEmpty(monsterType))
            {
                throw new ArgumentException("몬스터 타입은 비어있을 수 없습니다.", nameof(monsterType));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "레벨은 1 이상이어야 합니다.");
            }

            if (maxHp < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp), "최대 HP는 1 이상이어야 합니다.");
            }

            Uid = uid;
            MonsterType = monsterType;
            Level = level;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            AttackPower = attackPower;
            Defense = defense;
            GoldReward = goldReward;
            ExpReward = expReward;
        }

        /// <summary>
        /// 데미지를 받습니다.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "데미지는 음수일 수 없습니다.");
            }

            CurrentHp = Math.Max(0, CurrentHp - damage);
        }
    }
}
