using System.Collections.Generic;
using MyProject.DefenseGame.Domain;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// ���潺 ���� ��ƼƼ(�÷��̾�/����) ������ ASC �ʱ�ȭ�� ����մϴ�.
    /// </summary>
    public sealed class DefenseEntityFactory
    {
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
            string typeName;
            DefenseMonsterType type;

            if (isBoss)
            {
                typeName = "Boss";
                type = DefenseMonsterType.Boss;
                maxHp = (int)(_config.NormalMonsterHp * _config.BossHpMultiplier) + level * 20;
                attack = (int)(_config.NormalMonsterAttack * _config.BossAttackMultiplier) + level * 2;
                defense = _config.NormalMonsterDefense * 2 + level;
                expReward = (int)(_config.NormalMonsterExp * _config.BossExpMultiplier) + level * 10;
            }
            else
            {
                typeName = "Slime";
                type = DefenseMonsterType.Normal;
                maxHp = _config.NormalMonsterHp + level * 5;
                attack = _config.NormalMonsterAttack + level;
                defense = _config.NormalMonsterDefense + level / 2;
                expReward = _config.NormalMonsterExp + level * 2;
            }

            var abilityIds = isBoss ? _config.BossMonsterAbilityIds : _config.NormalMonsterAbilityIds;
            var asc = BuildMonsterASC(level, maxHp, attack, defense, expReward, abilityIds);

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
            attributes.SetAttribute(DefenseAttributeIds.Level, 1, 1, 999);
            attributes.SetAttribute(DefenseAttributeIds.MaxHp, _config.PlayerMaxHp, 0, 999999);
            attributes.SetAttribute(DefenseAttributeIds.Hp, _config.PlayerMaxHp, 0, 999999);
            attributes.SetAttribute(DefenseAttributeIds.Attack, _config.PlayerAttackPower, 0, 999999);
            attributes.SetAttribute(DefenseAttributeIds.Defense, _config.PlayerDefense, 0, 999999);
            attributes.SetAttribute(PlayerAttributeIds.Experience, 0, 0, 999999);

            var abilities = DefenseAbilityUtility.CreateAbilities(_config.PlayerAbilityIds);
            var tags = new GameplayTagContainer();
            return new AbilitySystemComponent(attributes, abilities, tags);
        }

        private AbilitySystemComponent BuildMonsterASC(
            int level,
            int maxHp,
            int attack,
            int defense,
            int expReward,
            IEnumerable<int> abilityIds)
        {
            var attributes = new AttributeSet();
            attributes.SetAttribute(DefenseAttributeIds.Level, level, 1, 999);
            attributes.SetAttribute(DefenseAttributeIds.MaxHp, maxHp, 0, 999999);
            attributes.SetAttribute(DefenseAttributeIds.Hp, maxHp, 0, 999999);
            attributes.SetAttribute(DefenseAttributeIds.Attack, attack, 0, 999999);
            attributes.SetAttribute(DefenseAttributeIds.Defense, defense, 0, 999999);
            attributes.SetAttribute(MonsterAttributeIds.ExpReward, expReward, 0, 999999);

            var abilities = DefenseAbilityUtility.CreateAbilities(abilityIds);
            var tags = new GameplayTagContainer();
            return new AbilitySystemComponent(attributes, abilities, tags);
        }
    }
}
