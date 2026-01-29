using System.Collections.Generic;
using MyProject.DefenseGame.Domain;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// DefenseGame 전용 엔티티(플레이어/몬스터)를 생성하고 ASC를 초기화하는 팩토리입니다.
    /// </summary>
    public sealed class DefenseEntityFactory
    {
        /// <summary>
        /// 호스트 설정입니다.
        /// </summary>
        private readonly DefenseHostConfig _config;

        public DefenseEntityFactory(DefenseHostConfig config)
        {
            _config = config;
        }

        public DefensePlayer CreatePlayer(long uid)
        {
            var asc = BuildPlayerASC();
            return new DefensePlayer(
                uid: uid,
                name: "Player",
                position: _config.PlayerSpawnPosition,
                asc: asc
            );
        }

        public DefenseMonster CreateMonster(long uid, int waveLevel, bool isBoss)
        {
            var level = waveLevel;
            int maxHp, attack, defense, expReward;
            float attackSpeed = 0f;
            string typeName;
            DefenseMonsterType type;

            if (isBoss)
            {
                typeName = "Boss";
                type = DefenseMonsterType.Boss;
                maxHp = (int)(_config.NormalMonsterHp * _config.BossHpMultiplier) + level * 20;
                attack = (int)(_config.NormalMonsterAttack * _config.BossAttackMultiplier) + level * 2;
                attackSpeed = _config.NormalMonsterAttackSpeed;
                defense = _config.NormalMonsterDefense * 2 + level;
                expReward = (int)(_config.NormalMonsterExp * _config.BossExpMultiplier) + level * 10;
            }
            else
            {
                typeName = "Slime";
                type = DefenseMonsterType.Normal;
                maxHp = _config.NormalMonsterHp + level * 5;
                attack = _config.NormalMonsterAttack + level;
                attackSpeed = _config.NormalMonsterAttackSpeed;
                defense = _config.NormalMonsterDefense + level / 2;
                expReward = _config.NormalMonsterExp + level * 2;
            }

            var abilityIds = isBoss ? _config.BossMonsterAbilityIds : _config.NormalMonsterAbilityIds;
            var asc = BuildMonsterASC(level, maxHp, attack, attackSpeed, defense, expReward, abilityIds);

            return new DefenseMonster(
                uid,
                typeName,
                type,
                _config.MonsterSpawnPosition,
                asc
            );
        }

        private AbilitySystemComponent BuildPlayerASC()
        {
            var attributes = new AttributeSet();
            attributes.SetAttribute(AttributeId.Level, 1, 1, 999);
            attributes.SetAttribute(AttributeId.MaxHealth, _config.PlayerMaxHp, 0, 999999);
            attributes.SetAttribute(AttributeId.Health, _config.PlayerMaxHp, 0, 999999);
            attributes.SetAttribute(AttributeId.AttackDamage, _config.PlayerAttackPower, 0, 999999);
            attributes.SetAttribute(AttributeId.AttackSpeed, _config.PlayerAttackSpeed, 1, 5);
            attributes.SetAttribute(AttributeId.Defense, _config.PlayerDefense, 0, 999999);
            attributes.SetAttribute(AttributeId.Experience, 0, 0, 999999);

            var abilities = DefenseAbilityUtility.CreateAbilities(_config.PlayerAbilityIds);
            var tags = new GameplayTagContainer();

            foreach (var tag in _config.PlayerTags)
            {
                tags.AddTag(new FGameplayTag(tag));
            }

            if(_config.InvincibleMode)
                tags.AddTag(DefenseTags.State_Invincible);

            return new AbilitySystemComponent(attributes, abilities, tags);
        }

        private AbilitySystemComponent BuildMonsterASC(
            int level,
            int maxHp,
            int attack,
            float attackSpeed,
            int defense,
            int expReward,
            IEnumerable<int> abilityIds)
        {
            var attributes = new AttributeSet();
            attributes.SetAttribute(AttributeId.Level, level, 1, 999);
            attributes.SetAttribute(AttributeId.MaxHealth, maxHp, 0, 999999);
            attributes.SetAttribute(AttributeId.Health, maxHp, 0, 999999);
            attributes.SetAttribute(AttributeId.AttackDamage, attack, 0, 999999);
            attributes.SetAttribute(AttributeId.AttackSpeed, attackSpeed, 1, 5);
            attributes.SetAttribute(AttributeId.Defense, defense, 0, 999999);
            attributes.SetAttribute(AttributeId.ExpReward, expReward, 0, 999999);

            var abilities = DefenseAbilityUtility.CreateAbilities(abilityIds);
            var tags = new GameplayTagContainer();
            return new AbilitySystemComponent(attributes, abilities, tags);
        }
    }
}
