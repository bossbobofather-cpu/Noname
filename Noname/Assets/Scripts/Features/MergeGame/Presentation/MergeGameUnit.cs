using UnityEngine;
namespace MyProject.MergeGame
{
    /// <summary>
    /// 보드에 배치되는 유닛입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MergeGameUnit : MergeGameActorBase
    {
        [SerializeField] private MergeGameUnitType _unitType = MergeGameUnitType.Ranged;
        [SerializeField] private int _grade = 1;
        [SerializeField] private MergeGameUnitAttackBase _attackBehavior;
        [SerializeField] private MergeGameUnitDrag _drag;
        private MergeGameBoard _board;
        private MergeGameSlot _currentSlot;
        private bool _initialized;

        /// <summary>
        /// 유닛 타입입니다.
        /// </summary>
        public MergeGameUnitType UnitType => _unitType;

        /// <summary>
        /// 유닛 등급입니다.
        /// </summary>
        public int Grade => _grade;

        /// <summary>
        /// 현재 소속된 보드입니다.
        /// </summary>
        public MergeGameBoard Board => _board;

        /// <summary>
        /// 현재 배치된 슬롯입니다.
        /// </summary>
        public MergeGameSlot CurrentSlot => _currentSlot;

        private void Awake()
        {
            if (_attackBehavior == null)
            {
                _attackBehavior = GetComponent<MergeGameUnitAttackBase>();
            }

            if (_drag == null)
            {
                _drag = GetComponent<MergeGameUnitDrag>();
            }

            UpdateDisplayName();
        }

        /// <summary>
        /// 보드와 정의 정보를 기준으로 유닛을 초기화합니다.
        /// </summary>
        public void Initialize(MergeGameBoard board, MergeGameUnitDefinition definition)
        {
            _board = board;

            if (definition != null)
            {
                _unitType = definition.UnitType;
                _grade = Mathf.Max(1, definition.Grade);
            }

            _attackBehavior?.Initialize(this);
            _drag?.Initialize(this);
            _initialized = true;
            UpdateDisplayName();
        }

        private void Start()
        {
            if (_initialized)
            {
                return;
            }

            // 수동 배치된 유닛을 위해 기본 초기화를 수행한다.
            if (_board == null)
            {
                _board = FindFirstObjectByType<MergeGameBoard>();
            }

            _attackBehavior?.Initialize(this);
            _drag?.Initialize(this);
            _initialized = true;
        }

        /// <summary>
        /// 슬롯 정보를 갱신합니다.
        /// </summary>
        public void SetSlot(MergeGameSlot slot)
        {
            _currentSlot = slot;
            _drag?.SetSlot(slot);
        }

        /// <summary>
        /// 슬롯 연결을 해제합니다.
        /// </summary>
        public void ClearSlot()
        {
            _currentSlot = null;
            _drag?.SetSlot(null);
        }

        /// <summary>
        /// 등급을 1 올립니다.
        /// </summary>
        public void Upgrade()
        {
            _grade = Mathf.Max(1, _grade + 1);
            UpdateDisplayName();
        }

        /// <summary>
        /// 등급을 1 낮추거나 1등급이면 제거합니다.
        /// </summary>
        public void DowngradeOrRemove()
        {
            if (_grade <= 1)
            {
                RemoveFromBoard();
                Destroy(gameObject);
                return;
            }

            _grade = Mathf.Max(1, _grade - 1);
            UpdateDisplayName();
        }

        /// <summary>
        /// 보드 연결을 해제하고 슬롯을 비웁니다.
        /// </summary>
        public void RemoveFromBoard()
        {
            if (_currentSlot != null && _currentSlot.Occupant == this)
            {
                _currentSlot.ClearUnit();
            }

            _currentSlot = null;
        }

        protected override void HandleDeath()
        {
            base.HandleDeath();

            // 사망 시 보드 연결을 정리한다.
            RemoveFromBoard();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // 제거 시 슬롯 연결을 정리한다.
            if (_currentSlot != null && _currentSlot.Occupant == this)
            {
                _currentSlot.ClearUnit();
            }
        }

        private void UpdateDisplayName()
        {
            // 에디터/디버그용 이름을 갱신한다.
            gameObject.name = $"Unit_{_unitType}_G{_grade}";
        }
    }
}
