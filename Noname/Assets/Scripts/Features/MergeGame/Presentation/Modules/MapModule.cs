using UnityEngine;
using MyProject.Common.GameMode;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 맵 구성을 담당하는 모듈입니다.
    /// </summary>
    public class MapModule : ModuleBase
    {
        [Header("Map References")]
        [SerializeField] private MergeGameMap _mapPrefab;

        private MergeGameMap _mapInstance;

        /// <summary>
        /// 현재 로드된 맵입니다.
        /// </summary>
        public MergeGameMap Map => _mapInstance;

        /// <summary>
        /// 보드 참조입니다.
        /// </summary>
        public MergeGameBoard Board => _mapInstance != null ? _mapInstance.Board : null;

        /// <summary>
        /// 몬스터 스폰 기준 위치입니다.
        /// </summary>
        public Transform SpawnPoint => _mapInstance != null ? _mapInstance.SpawnPoint : null;

        /// <summary>
        /// 기본 이동 경로입니다.
        /// </summary>
        public MergeGamePath Path => _mapInstance != null ? _mapInstance.Path : null;

        /// <summary>
        /// 경로가 설정되어 있는지 여부입니다.
        /// </summary>
        public bool HasPath => _mapInstance != null && _mapInstance.HasPath;

        /// <summary>
        /// 맵을 로드하고 초기화합니다.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // 맵 프리팹이 있으면 생성하고, 없으면 씬에 있는 맵을 사용한다.
            if (_mapPrefab != null)
            {
                _mapInstance = Instantiate(_mapPrefab, transform);
            }
            else
            {
                // 프리팹이 없으면 기존 씬에 배치된 맵을 찾는다.
                _mapInstance = GetComponentInChildren<MergeGameMap>(true);
            }

            if (_mapInstance != null)
            {
                _mapInstance.Initialize();
            }
        }

        /// <summary>
        /// 맵 오브젝트를 활성화합니다.
        /// </summary>
        protected override void OnStartup()
        {
            base.OnStartup();

            if (_mapInstance != null)
            {
                // 맵을 활성화한다.
                _mapInstance.gameObject.SetActive(true);
            }

            // 맵 로드 완료 이벤트를 발행한다.
            Mode?.Publish(new MergeGameMapLoadedEvent(this, _mapInstance));
        }

        /// <summary>
        /// 맵 오브젝트를 비활성화합니다.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();

            if (_mapInstance != null)
            {
                // 맵을 비활성화한다.
                _mapInstance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 기본 경로를 반환합니다.
        /// </summary>
        public MergeGamePath GetDefaultPath()
        {
            return _mapInstance != null ? _mapInstance.GetDefaultPath() : null;
        }

        /// <summary>
        /// 스폰 위치를 반환합니다.
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            if (_mapInstance != null)
            {
                return _mapInstance.GetSpawnPosition();
            }

            return transform.position;
        }
    }
}
