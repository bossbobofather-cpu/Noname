using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임 호스트 설정입니다.
    /// </summary>
    [System.Serializable]
    public sealed class DefenseHostConfig
    {
        /// <summary>
        /// State.Invincible Tag를 부여하여 기본 공격을 무시합니다.
        /// </summary>
        public bool InvincibleMode = false;

        /// <summary>
        /// 플레이어 스폰 위치입니다.
        /// </summary>
        public Point2D PlayerSpawnPosition = Point2D.zero;

        /// <summary>
        /// 몬스터 스폰 위치입니다.
        /// </summary>
        public Point2D MonsterSpawnPosition = Point2D.zero;

        /// <summary>
        /// 기본 몬스터 스폰 간격입니다 (초).
        /// </summary>
        public float BaseSpawnInterval = 1.0f;

        /// <summary>
        /// 스폰 수량이 증가하는 간격입니다 (초).
        /// </summary>
        public float SpawnRateIncreaseInterval = 10.0f;

        /// <summary>
        /// 보스 스폰 간격입니다 (초).
        /// </summary>
        public float BossSpawnInterval = 30.0f;

        /// <summary>
        /// 패배 조건: 최대 몬스터 수입니다.
        /// </summary>
        public int MaxMonsterCount = 100;

        /// <summary>
        /// 플레이어 초기 체력입니다.
        /// </summary>
        public int PlayerMaxHp = 100;

        /// <summary>
        /// 플레이어 초기 공격력입니다.
        /// </summary>
        public float PlayerAttackPower = 10;

        /// <summary>
        /// 플레이어 초기 공격속도 입니다. (초당 공격 횟수)
        /// </summary>
        public float PlayerAttackSpeed = 1;

        /// <summary>
        /// 플레이어 초기 방어력입니다.
        /// </summary>
        public float PlayerDefense = 5;

        /// <summary>
        /// 플레이어 공격 쿨다운입니다 (초).
        /// </summary>
        public float PlayerAttackCooldown = 1.0f;

        public List<string> PlayerTags = new();
        /// <summary>
        /// 플레이어 스타트업 능력 ID 입니다.
        /// </summary>
        public List<int> PlayerAbilityIds = new();

        /// <summary>
        /// 일반 몬스터 체력입니다.
        /// </summary>
        public int NormalMonsterHp = 30;

        /// <summary>
        /// 일반 몬스터 공격력입니다.
        /// </summary>
        public int NormalMonsterAttack = 5;

        /// <summary>
        /// 일반 몬스터 공격력입니다.
        /// </summary>
        public float NormalMonsterAttackSpeed = 1;

        /// <summary>
        /// 일반 몬스터 방어력입니다.
        /// </summary>
        public int NormalMonsterDefense = 2;

        /// <summary>
        /// 일반 몬스터 경험치 보상입니다.
        /// </summary>
        public int NormalMonsterExp = 10;

        /// <summary>
        /// 일반 몬스터 스타트업 능력 ID 입니다.
        /// </summary>
        public List<int> NormalMonsterAbilityIds = new();

        /// <summary>
        /// 보스 몬스터 체력 배수입니다.
        /// </summary>
        public float BossHpMultiplier = 5f;

        /// <summary>
        /// 보스 몬스터 공격력입니다.
        /// </summary>
        public float BossMonsterAttackSpeed = 1;

        /// <summary>
        /// 보스 몬스터 공격력 배수입니다.
        /// </summary>
        public float BossAttackMultiplier = 2f;

        /// <summary>
        /// 보스 몬스터 경험치 보상 배수입니다.
        /// </summary>
        public float BossExpMultiplier = 10f;

        /// <summary>
        /// 보스 몬스터 스타트업 능력 ID 입니다.
        /// </summary>
        public List<int> BossMonsterAbilityIds = new();
    }
}
