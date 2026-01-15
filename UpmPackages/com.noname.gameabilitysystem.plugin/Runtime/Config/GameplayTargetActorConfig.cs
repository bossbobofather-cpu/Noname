using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public abstract class GameplayTargetActorConfig : GameplayConfig
    {
        [SerializeField] private GameplayTargetActor _actorPrefab;
        [SerializeField] private string _anchorKey = string.Empty;
        [SerializeField] private Vector3 _centerOffset = Vector3.zero;
        [SerializeField] private Vector3 _spawnOffset = Vector3.zero;
        [SerializeField] private bool _includeOwner;
        [SerializeField] private int _maxTargets = 1;
        [SerializeField] private AttributeDefinition _selectionAttribute;

        public GameplayTargetActor ActorPrefab => _actorPrefab;
        public string AnchorKey => _anchorKey;
        public Vector3 CenterOffset => _centerOffset;
        public Vector3 SpawnOffset => _spawnOffset;
        public bool IncludeOwner => _includeOwner;
        public int MaxTargets => _maxTargets;
        public AttributeDefinition SelectionAttribute => _selectionAttribute;

        public GameplayTargetActor Spawn(Transform parent)
        {
            if (_actorPrefab == null)
            {
                Debug.LogError("Target Actor Prefab is missing.");
                return null;
            }

            var position = parent != null ? parent.TransformPoint(_spawnOffset) : _spawnOffset;
            var rotation = parent != null ? parent.rotation : Quaternion.identity;
            var actor = Instantiate(_actorPrefab, position, rotation, parent);
            actor.Initialize(this);
            return actor;
        }
    }
}
