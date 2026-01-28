using Noname.GameAbilitySystem.Domain;
using UnityEngine;
using MyProject.DefenseGame.Domain.AI;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 몬스터 타입입니다.
    /// </summary>
    public enum DefenseMonsterType
    {
        Normal,
        Boss
    }

    /// <summary>
    /// 디펜스 게임 몬스터입니다.
    /// </summary>
    public sealed class DefenseMonster : DefenseEntity
    {
        /// <summary>
        /// 몬스터 타입 이름입니다.
        /// </summary>
        public string MonsterTypeName { get; }

        /// <summary>
        /// 몬스터 종류입니다.
        /// </summary>
        public DefenseMonsterType Type { get; }

        /// <summary>
        /// 보스 여부입니다.
        /// </summary>
        public bool IsBoss => Type == DefenseMonsterType.Boss;

        /// <summary>
        /// 경험치 보상입니다.
        /// </summary>
        public int ExpReward => (int)ASC.Get(MonsterAttributeIds.ExpReward);

        public DefenseMonster(
            long uid,
            string monsterTypeName,
            DefenseMonsterType type,
            Point2D position,
            AbilitySystemComponent asc) : base(uid, position, asc)
        {
            MonsterTypeName = monsterTypeName ?? "Unknown";
            Type = type;
        }

        /// <summary>
        /// 매 틱마다 호출됩니다.
        /// </summary>
        public override void Tick(float deltaTime)
        {
            AI?.Update(this, deltaTime);
        }
    }
}
