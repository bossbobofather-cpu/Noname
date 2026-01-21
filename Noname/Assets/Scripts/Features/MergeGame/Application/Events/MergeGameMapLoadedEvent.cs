using MyProject.Common.GameEvent;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 맵 로드가 완료되었을 때 발행되는 이벤트입니다.
    /// </summary>
    public sealed class MergeGameMapLoadedEvent : SceneGameEventContext
    {
        /// <summary>
        /// 로드된 맵입니다.
        /// </summary>
        public MergeGameMap Map { get; }

        /// <summary>
        /// 맵에 포함된 보드입니다.
        /// </summary>
        public MergeGameBoard Board { get; }

        /// <summary>
        /// 몬스터 스폰 포인트입니다.
        /// </summary>
        public Transform SpawnPoint { get; }

        /// <summary>
        /// 기본 경로입니다.
        /// </summary>
        public MergeGamePath Path { get; }

        public MergeGameMapLoadedEvent(object source, MergeGameMap map)
            : base(source)
        {
            Map = map;
            Board = map != null ? map.Board : null;
            SpawnPoint = map != null ? map.SpawnPoint : null;
            Path = map != null ? map.Path : null;
        }
    }
}
