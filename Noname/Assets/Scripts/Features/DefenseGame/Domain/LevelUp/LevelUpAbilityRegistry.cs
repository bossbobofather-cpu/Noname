using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Domain.LevelUp
{
    /// <summary>
    /// 레벨업 어빌리티 레지스트리입니다.
    /// 모든 레벨업 어빌리티를 관리하고 랜덤 선택을 제공합니다.
    /// </summary>
    public sealed class LevelUpAbilityRegistry
    {
        private readonly Dictionary<FGameplayTag, LevelUpAbilityDefinition> _definitions = new();
        private readonly List<LevelUpAbilityDefinition> _tempAvailable = new();
        private readonly Random _random;

        public LevelUpAbilityRegistry(Random random = null)
        {
            _random = random ?? new Random();
            RegisterAllAbilities();
        }

        /// <summary>
        /// 모든 어빌리티를 등록합니다. (하드코딩)
        /// </summary>
        private void RegisterAllAbilities()
        {
            // 기본 어빌리티 (선행조건 없음)
            Register(new LevelUpAbilityDefinition(
                abilityTag: DefenseTags.Ability_FullHealthRestore,
                displayName: "완전 회복",
                description: "체력을 완전히 회복합니다.",
                isStackable: true,
                applyAction: player =>
                {
                    var maxHp = player.GetMaxHp();
                    player.ASC.Set(AttributeId.Health, maxHp);
                }
            ));

            Register(new LevelUpAbilityDefinition(
                abilityTag: DefenseTags.Ability_AttackSpeedUp,
                displayName: "기본 공격 속도 증가",
                description: "현재 기본 공격 쿨다운이 10% 감소합니다.",
                isStackable: true,
                applyAction: player =>
                {
                    var atkSpd = player.ASC.Get(AttributeId.AttackSpeed);
                    atkSpd += atkSpd * 0.1f;

                    //최고 공격속도 제한이 있어야 할 듯 5보다 크면 5로 고정
                    atkSpd = Math.Min(5f, atkSpd);
                    player.ASC.Set(AttributeId.AttackSpeed, atkSpd);
                }
            ));

            Register(new LevelUpAbilityDefinition(
                abilityTag: DefenseTags.Ability_AreaAttack,
                displayName: "범위 공격",
                description: "가장 가까운 적 3기를 동시에 공격합니다.",
                isStackable: false,
                applyAction: player =>
                {
                    // 범위 공격 어빌리티 부여
                    var areaAttack = DefenseAbilityUtility.CreateAreaAttack(
                        cooldown: 2.0f,
                        damageAmount: player.GetAttack() * 0.8f,
                        maxTargets: 3
                    );
                    player.ASC.GiveAbility(areaAttack);
                }
            ));


            // 종속 어빌리티
            Register(new LevelUpAbilityDefinition(
                abilityTag: DefenseTags.Ability_AreaAttackTargetUp,
                displayName: "범위 공격 강화",
                description: "범위 공격 대상이 2 증가합니다.",
                prerequisiteTag: DefenseTags.Ability_AreaAttack,
                isStackable: true,
                applyAction: player =>
                {
                    var extraTargetCount = player.ASC.Get(AttributeId.ExtraTargetCount);
                    extraTargetCount += 2;
                    player.ASC.Set(AttributeId.ExtraTargetCount, extraTargetCount);
                }
            ));
        }

        /// <summary>
        /// 어빌리티를 등록합니다.
        /// </summary>
        private void Register(LevelUpAbilityDefinition definition)
        {
            _definitions[definition.AbilityTag] = definition;
        }

        /// <summary>
        /// 플레이어가 선택 가능한 어빌리티 목록을 반환합니다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="grantedAbilities">이미 획득한 어빌리티 목록</param>
        /// <param name="count">선택할 개수</param>
        /// <param name="results">결과 목록 (out)</param>
        public void GetRandomAvailableAbilities(
            DefensePlayer player,
            HashSet<FGameplayTag> grantedAbilities,
            int count,
            List<LevelUpAbilityDefinition> results)
        {
            results.Clear();
            _tempAvailable.Clear();

            // 선택 가능한 어빌리티 수집
            foreach (var kvp in _definitions)
            {
                var def = kvp.Value;

                // 선행조건 체크
                if (def.PrerequisiteTag.IsValid)
                {
                    if (!grantedAbilities.Contains(def.PrerequisiteTag))
                    {
                        continue;
                    }
                }

                // 중복 획득 불가능한 경우 이미 획득했는지 체크
                if (!def.IsStackable && grantedAbilities.Contains(def.AbilityTag))
                {
                    continue;
                }

                _tempAvailable.Add(def);
            }

            // 랜덤 선택
            ShuffleList(_tempAvailable);

            var selectCount = Math.Min(count, _tempAvailable.Count);
            for (var i = 0; i < selectCount; i++)
            {
                results.Add(_tempAvailable[i]);
            }
        }

        /// <summary>
        /// ID로 어빌리티 정의를 가져옵니다.
        /// </summary>
        public LevelUpAbilityDefinition GetDefinition(FGameplayTag abilityTag)
        {
            return _definitions.TryGetValue(abilityTag, out var def) ? def : null;
        }

        /// <summary>
        /// 리스트를 셔플합니다. 현재는 등급이 없기때문에 균등 확률 부여(피셔-예이츠 셔플)
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
