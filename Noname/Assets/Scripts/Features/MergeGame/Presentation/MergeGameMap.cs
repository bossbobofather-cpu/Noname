using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 맵 정보를 담는 컴포넌트입니다.
    /// </summary>
    public sealed class MergeGameMap : MonoBehaviour
    {
        [Header("Map Contents")]
        [SerializeField] private MergeGameBoard _board;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private MergeGamePath _path;

        /// <summary>
        /// 보드 참조입니다.
        /// </summary>
        public MergeGameBoard Board => _board;

        /// <summary>
        /// 몬스터 스폰 기준점입니다.
        /// </summary>
        public Transform SpawnPoint => _spawnPoint;

        /// <summary>
        /// 사용할 경로입니다.
        /// </summary>
        public MergeGamePath Path => _path;

        /// <summary>
        /// 경로가 등록되어 있는지 여부입니다.
        /// </summary>
        public bool HasPath => _path != null;

        /// <summary>
        /// 맵 내부 참조를 수집합니다.
        /// </summary>
        public void Initialize()
        {
            if (_board == null)
            {
                // 자식에서 보드를 찾아 둔다.
                _board = GetComponentInChildren<MergeGameBoard>(true);
            }

            if (_path == null)
            {
                // 경로가 비어 있으면 자식에서 찾아 둔다.
                _path = GetComponentInChildren<MergeGamePath>(true);
            }
        }

        /// <summary>
        /// 기본 경로를 반환합니다.
        /// </summary>
        public MergeGamePath GetDefaultPath()
        {
            return _path;
        }

        /// <summary>
        /// 스폰 위치를 반환합니다.
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            // 스폰 포인트가 있으면 해당 위치를 사용한다.
            if (_spawnPoint != null)
            {
                return _spawnPoint.position;
            }

            // 스폰 포인트가 없으면 첫 경로 시작점을 사용한다.
            if (_path != null)
            {
                return _path.GetStartPoint();
            }

            return transform.position;
        }

        private void OnDrawGizmos()
        {
            if (_spawnPoint != null)
            {
                MergeGameGizmoUtility.DrawSpawnPoint(_spawnPoint.position, 0.3f);
            }
        }
    }
}
