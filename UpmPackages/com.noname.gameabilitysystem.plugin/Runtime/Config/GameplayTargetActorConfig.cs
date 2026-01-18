using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 타겟 액터 설정의 기본 클래스입니다.
    /// </summary>
    public abstract class GameplayTargetActorConfig : GameplayConfig
    {
        [SerializeField] private GameplayTargetActor _actorPrefab;
        [SerializeField] private string _anchorKey = string.Empty;
        [SerializeField] private Vector3 _centerOffset = Vector3.zero;
        [SerializeField] private Vector3 _spawnOffset = Vector3.zero;
        [SerializeField] private bool _includeOwner;
        [SerializeField] private int _maxTargets = 1;
        [SerializeField] private AttributeDefinition _selectionAttribute;

        /// <summary>
        /// 사용할 타겟 액터 프리팹입니다.
        /// </summary>
        public GameplayTargetActor ActorPrefab => _actorPrefab;

        /// <summary>
        /// 중심 기준이 되는 앵커 키입니다.
        /// </summary>
        public string AnchorKey => _anchorKey;

        /// <summary>
        /// 타겟 중심 오프셋입니다.
        /// </summary>
        public Vector3 CenterOffset => _centerOffset;

        /// <summary>
        /// 생성 위치 오프셋입니다.
        /// </summary>
        public Vector3 SpawnOffset => _spawnOffset;

        /// <summary>
        /// 소유자를 포함할지 여부입니다.
        /// </summary>
        public bool IncludeOwner => _includeOwner;

        /// <summary>
        /// 최대 타겟 수입니다.
        /// </summary>
        public int MaxTargets => _maxTargets;

        /// <summary>
        /// 선택 기준이 되는 속성입니다.
        /// </summary>
        public AttributeDefinition SelectionAttribute => _selectionAttribute;

        /// <summary>
        /// 타겟 액터를 생성합니다.
        /// </summary>
        /// <param name="parent">부모 트랜스폼</param>
        /// <returns>생성된 타겟 액터</returns>
        public GameplayTargetActor Spawn(Transform parent)
        {
            if (_actorPrefab == null)
            {
                // 프리팹이 없으면 생성할 수 없다.
                Debug.LogError("Target Actor Prefab is missing.");
                return null;
            }

            // 부모 기준으로 위치와 회전을 계산한다.
            var position = parent != null ? parent.TransformPoint(_spawnOffset) : _spawnOffset;
            var rotation = parent != null ? parent.rotation : Quaternion.identity;
            var actor = Instantiate(_actorPrefab, position, rotation, parent);
            actor.Initialize(this);
            return actor;
        }
    }
}
