using MyProject.Common.UI;
using UnityEngine;

using MyProject.Common.GameEvent;
using MyProject.Common.GameMode;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 유닛 스폰을 관리하는 모듈입니다.
    /// </summary>
    public sealed class UnitSpawnModule : ModuleBase
    {
        [SerializeField] private MergeGameUnitCatalog _unitCatalog;
        [SerializeField] private int _defaultCost = 1;
        [Header("Spawn Policies")]
        [SerializeField] private MergeGameSpawnCostPolicy _spawnCostPolicy;
        [SerializeField] private MergeGameSpawnSelectionPolicy _spawnSelectionPolicy;
        [SerializeField] private MergeGameSpawnSpecPolicy _spawnSpecPolicy;
        [SerializeField] private MergeGamePlacementPolicy _placementPolicy;

        private MergeGameBoard _board;
        private readonly MergeGameSpawnRuntimeContext _spawnContext = new MergeGameSpawnRuntimeContext();

        /// <summary>
        /// 이벤트 구독을 준비합니다.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // 맵 로드와 UI 입력 이벤트를 먼저 연결합니다.
            GameEventHub.Subscribe<MergeGameMapLoadedEvent>(OnMapLoaded);
            GameEventHub.Subscribe<GameUIContextSupplyEvent>(OnUIInput);
        }

        /// <summary>
        /// 모듈 초기화 훅입니다.
        /// </summary>
        protected override void OnStartup()
        {
            base.OnStartup();

            _spawnContext.Reset();
            _spawnContext.SetRunning(true);
        }

        /// <summary>
        /// 모듈 종료 시 구독을 정리합니다.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();

            _spawnContext.SetRunning(false);
            GameEventHub.Unsubscribe<MergeGameMapLoadedEvent>(OnMapLoaded);
            GameEventHub.Unsubscribe<GameUIContextSupplyEvent>(OnUIInput);
        }

        /// <summary>
        /// 맵 로드 이벤트를 처리합니다.
        /// </summary>
        private void OnMapLoaded(MergeGameMapLoadedEvent mapLoaded)
        {
            if (mapLoaded == null)
            {
                return;
            }

            _board = mapLoaded.Board;
        }

        /// <summary>
        /// UI 입력 이벤트를 처리합니다.
        /// </summary>
        private void OnUIInput(GameUIContextSupplyEvent uiInput)
        {
            if (uiInput == null)
            {
                return;
            }

            var context = uiInput.UIEventCtx;
            if (context == null)
            {
                return;
            }

            if (context.EventType != UIEventType.Button_Click)
            {
                return;
            }

            if (context is UIEventTryUnitSpawn)
            {
                // 스폰 버튼 클릭에만 반응합니다.
                TrySpawnUnit(out _);
            }
        }

        /// <summary>
        /// 규칙에 따라 유닛 스폰을 시도합니다.
        /// </summary>
        public bool TrySpawnUnit(int availableCost, out int consumedCost)
        {
            consumedCost = 0;

            if (_board == null || _unitCatalog == null)
            {
                return false;
            }

            var definition = SelectSpawnDefinition(_unitCatalog);
            if (definition == null)
            {
                return false;
            }

            var baseCost = definition.Cost > 0 ? definition.Cost : _defaultCost;
            var cost = GetSpawnCost(baseCost);
            if (availableCost < cost)
            {
                return false;
            }

            var slot = SelectSpawnSlot(_board, definition);
            if (slot == null)
            {
                return false;
            }

            var unit = _board.SpawnUnit(definition, slot);
            if (unit == null)
            {
                return false;
            }

            NotifySpawned();
            consumedCost = cost;
            return true;
        }

        /// <summary>
        /// 비용 제한 없이 유닛 스폰을 시도합니다.
        /// </summary>
        public bool TrySpawnUnit(out int consumedCost)
        {
            return TrySpawnUnit(int.MaxValue, out consumedCost);
        }

        /// <summary>
        /// 스폰 비용을 계산합니다.
        /// </summary>
        private int GetSpawnCost(int baseCost)
        {
            if (_spawnCostPolicy == null)
            {
                return Mathf.Max(0, baseCost);
            }

            var context = new MergeGameSpawnCostContext(baseCost, _spawnContext.SpawnCount, _spawnContext.ElapsedTime);
            return Mathf.Max(0, _spawnCostPolicy.GetCost(context));
        }

        /// <summary>
        /// 스폰 완료를 기록합니다.
        /// </summary>
        private void NotifySpawned()
        {
            if (!_spawnContext.IsRunning)
            {
                return;
            }

            _spawnContext.AddSpawn();
        }

        /// <summary>
        /// 스폰 스펙을 가져옵니다.
        /// </summary>
        private MergeGameSpawnSpec GetSpawnSpec()
        {
            if (_spawnSpecPolicy == null)
            {
                return MergeGameSpawnSpec.Default;
            }

            var context = new MergeGameSpawnSpecContext(_spawnContext);
            return _spawnSpecPolicy.GetSpec(context);
        }

        /// <summary>
        /// 스폰할 유닛 정의를 선택합니다.
        /// </summary>
        private MergeGameUnitDefinition SelectSpawnDefinition(MergeGameUnitCatalog catalog)
        {
            if (catalog == null)
            {
                return null;
            }

            var spec = GetSpawnSpec();
            var selectionContext = new MergeGameSpawnSelectionContext(catalog, _spawnContext, spec);
            var unitType = _spawnSelectionPolicy != null
                ? _spawnSelectionPolicy.SelectUnitType(selectionContext)
                : MergeGameUnitType.Ranged;

            var definition = catalog.FindDefinition(unitType, spec.Grade);
            if (definition != null)
            {
                return definition;
            }

            // 지정 타입이 없으면 같은 등급에서 임의로 고릅니다.
            return catalog.GetRandomDefinitionByGrade(spec.Grade);
        }

        /// <summary>
        /// 스폰 슬롯을 선택합니다.
        /// </summary>
        private MergeGameSlot SelectSpawnSlot(MergeGameBoard board, MergeGameUnitDefinition definition)
        {
            if (board == null)
            {
                return null;
            }

            if (_placementPolicy == null)
            {
                return board.TryGetRandomEmptySlot(out var slot) ? slot : null;
            }

            var context = new MergeGamePlacementContext(board, definition, _spawnContext);
            return _placementPolicy.SelectSlot(context);
        }

        /// <summary>
        /// 스폰 정책의 시간 누적을 처리합니다.
        /// </summary>
        private void Update()
        {
            if (!_spawnContext.IsRunning)
            {
                return;
            }

            _spawnContext.AdvanceTime(Time.deltaTime);
        }
    }
}
