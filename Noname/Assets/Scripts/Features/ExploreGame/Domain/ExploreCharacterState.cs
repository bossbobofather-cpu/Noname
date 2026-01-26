using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using Noname.GameAbilitySystem.Domain;

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
        public int Speed { get; private set; }
        public int Gold { get; private set; }
        public int Experience { get; private set; }

        /// <summary>
        /// ATB 시스템을 위한 턴 게이지입니다 (0.0 ~ 100.0).
        /// </summary>
        public float TurnGauge { get; private set; }

        /// <summary>
        /// AbilitySystemComponent 모델입니다.
        /// </summary>
        public AbilitySystemModel AbilitySystem { get; }

        /// <summary>
        /// 사용 가능한 어빌리티 목록입니다.
        /// </summary>
        public List<ExploreAbilityState> Abilities { get; }

        public bool IsAlive => CurrentHp > 0;

        public ExploreCharacterState(
            long uid,
            string name,
            int level,
            int maxHp,
            int attackPower,
            int defense,
            int speed)
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
            Speed = speed;
            Gold = 0;
            Experience = 0;
            TurnGauge = 0f;

            // AbilitySystem 초기화
            AbilitySystem = new AbilitySystemModel();
            Abilities = new List<ExploreAbilityState>();
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
            Speed += 1 + Level / 5; // 속도도 증가
        }

        /// <summary>
        /// 전투 준비 상태로 체력을 초기화합니다.
        /// </summary>
        public void ResetForBattle()
        {
            CurrentHp = MaxHp;
            TurnGauge = 0f;
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
            return IsAlive && TurnGauge >= 100f;
        }

        /// <summary>
        /// 어빌리티를 추가합니다.
        /// </summary>
        public void AddAbility(ExploreAbilityState ability)
        {
            if (ability == null)
            {
                throw new ArgumentNullException(nameof(ability));
            }

            Abilities.Add(ability);
        }

        /// <summary>
        /// 사용 가능한 어빌리티를 반환합니다.
        /// </summary>
        public ExploreAbilityState GetReadyAbility()
        {
            for (var i = 0; i < Abilities.Count; i++)
            {
                if (Abilities[i].IsReady)
                {
                    return Abilities[i];
                }
            }

            return null;
        }
    }
}
