using System;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 탐험 캐릭터의 상태를 나타냅니다.
    /// </summary>
    public sealed class ExploreCharacterState
    {
        public long Uid { get; }
        public string Name { get; }
        public int Level { get; private set; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public int AttackPower { get; private set; }
        public int Defense { get; private set; }
        public int Gold { get; private set; }
        public int Experience { get; private set; }

        public bool IsAlive => CurrentHp > 0;

        public ExploreCharacterState(long uid, string name, int level, int maxHp, int attackPower, int defense)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("캐릭터 이름은 비어있을 수 없습니다.", nameof(name));
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
            Name = name;
            Level = level;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            AttackPower = attackPower;
            Defense = defense;
            Gold = 0;
            Experience = 0;
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
        /// 체력을 회복합니다.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "회복량은 음수일 수 없습니다.");
            }

            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        }

        /// <summary>
        /// 골드를 추가합니다.
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "골드량은 음수일 수 없습니다.");
            }

            Gold += amount;
        }

        /// <summary>
        /// 경험치를 추가하고 레벨업을 처리합니다.
        /// </summary>
        public bool AddExperience(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "경험치는 음수일 수 없습니다.");
            }

            Experience += amount;

            // 간단한 레벨업 공식: 100 * Level 경험치마다 레벨업
            var requiredExp = 100 * Level;
            if (Experience >= requiredExp)
            {
                Experience -= requiredExp;
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
            Level++;
            var hpIncrease = 10 + Level * 2;
            MaxHp += hpIncrease;
            CurrentHp = MaxHp; // 레벨업 시 체력 완전 회복
            AttackPower += 2 + Level / 2;
            Defense += 1 + Level / 3;
        }

        /// <summary>
        /// 전투 준비 상태로 체력을 초기화합니다.
        /// </summary>
        public void ResetForBattle()
        {
            CurrentHp = MaxHp;
        }
    }
}
