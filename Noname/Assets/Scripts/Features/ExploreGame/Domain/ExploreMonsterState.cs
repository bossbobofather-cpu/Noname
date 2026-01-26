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
        public int Speed { get; }
        public int GoldReward { get; }
        public int ExpReward { get; }

        /// <summary>
        /// ATB 시스템을 위한 턴 게이지입니다 (0.0 ~ 100.0).
        /// </summary>
        public float TurnGauge { get; private set; }

        public bool IsDead => CurrentHp <= 0;

        public ExploreMonsterState(
            long uid,
            string monsterType,
            int level,
            int maxHp,
            int attackPower,
            int defense,
            int speed,
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
            Speed = speed;
            GoldReward = goldReward;
            ExpReward = expReward;
            TurnGauge = 0f;
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

        /// <summary>
        /// 턴 게이지를 증가시킵니다.
        /// </summary>
        public void AddTurnGauge(float amount)
        {
            TurnGauge += amount;
        }

        /// <summary>
        /// 턴 게이지를 소비합니다 (행동 후).
        /// </summary>
        public void ConsumeTurnGauge(float amount = 100f)
        {
            TurnGauge -= amount;
            if (TurnGauge < 0f)
            {
                TurnGauge = 0f;
            }
        }

        /// <summary>
        /// 행동 가능한 상태인지 확인합니다.
        /// </summary>
        public bool CanAct()
        {
            return !IsDead && TurnGauge >= 100f;
        }
    }
}
