using System.Collections.Generic;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임의 전투 상태입니다.
    /// </summary>
    public sealed class DefenseCombatState
    {
        private readonly List<DefenseMonster> _monsters = new();

        /// <summary>
        /// 게임 경과 시간입니다 (초).
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        /// 처치한 몬스터 수입니다.
        /// </summary>
        public int KillCount { get; private set; }

        /// <summary>
        /// 현재 살아있는 몬스터 수입니다.
        /// </summary>
        public int AliveMonsterCount => _monsters.Count;

        /// <summary>
        /// 보스 처치 수입니다.
        /// </summary>
        public int BossKillCount { get; private set; }

        /// <summary>
        /// 게임 종료 여부입니다.
        /// </summary>
        public bool IsGameOver { get; private set; }

        /// <summary>
        /// 패배 여부입니다.
        /// </summary>
        public bool IsDefeat { get; private set; }

        /// <summary>
        /// 몬스터를 추가합니다.
        /// </summary>
        public void AddMonster(DefenseMonster monster)
        {
            if (monster == null) return;
            _monsters.Add(monster);
        }

        /// <summary>
        /// 몬스터를 제거합니다.
        /// </summary>
        public void RemoveMonster(DefenseMonster monster)
        {
            if (monster == null) return;

            if (_monsters.Remove(monster))
            {
                KillCount++;

                if (monster.IsBoss)
                {
                    BossKillCount++;
                }
            }
        }

        /// <summary>
        /// 살아있는 모든 몬스터를 반환합니다.
        /// </summary>
        public IReadOnlyList<DefenseMonster> GetAliveMonsters() => _monsters;

        /// <summary>
        /// 죽은 몬스터를 수집합니다.
        /// </summary>
        public void CollectDeadMonsters(List<DefenseMonster> output)
        {
            output.Clear();

            for (var i = _monsters.Count - 1; i >= 0; i--)
            {
                if (_monsters[i].IsDead)
                {
                    output.Add(_monsters[i]);
                }
            }
        }

        /// <summary>
        /// 경과 시간을 업데이트합니다.
        /// </summary>
        public void AddElapsedTime(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }

        /// <summary>
        /// 게임 오버 처리를 합니다.
        /// </summary>
        public void SetGameOver(bool isDefeat)
        {
            IsGameOver = true;
            IsDefeat = isDefeat;
        }
    }
}
