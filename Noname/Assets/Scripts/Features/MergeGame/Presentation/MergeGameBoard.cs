using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 보드와 슬롯 배치를 관리하는 컴포넌트입니다.
    /// </summary>
    public sealed class MergeGameBoard : MonoBehaviour
    {
        [Header("Units")]
        [SerializeField] private Transform _unitRoot;
        [SerializeField] private MergeGameUnitCatalog _unitCatalog;

        [SerializeField] private List<MergeGameSlot> _slots = new();

        private readonly List<MergeGameSlot> _emptySlots = new();

        /// <summary>
        /// 보드 슬롯 목록입니다.
        /// </summary>
        public IReadOnlyList<MergeGameSlot> Slots => _slots;

        private void Awake()
        {
            // 직접 배치된 슬롯을 수집한다.
            CollectSlots();
        }

        /// <summary>
        /// 랜덤 빈 슬롯을 반환합니다.
        /// </summary>
        public bool TryGetRandomEmptySlot(out MergeGameSlot slot)
        {
            slot = null;
            _emptySlots.Clear();

            for (var i = 0; i < _slots.Count; i++)
            {
                var candidate = _slots[i];
                if (candidate != null && candidate.IsEmpty)
                {
                    _emptySlots.Add(candidate);
                }
            }

            if (_emptySlots.Count == 0)
            {
                return false;
            }

            slot = _emptySlots[Random.Range(0, _emptySlots.Count)];
            return true;
        }

        /// <summary>
        /// 정의를 이용해 유닛을 생성하고 슬롯에 배치합니다.
        /// </summary>
        public MergeGameUnit SpawnUnit(MergeGameUnitDefinition definition, MergeGameSlot slot)
        {
            if (definition == null || definition.Prefab == null || slot == null)
            {
                return null;
            }

            // 유닛을 생성하고 초기화한다.
            var parent = _unitRoot != null ? _unitRoot : transform;
            var unit = Instantiate(definition.Prefab, slot.Position, Quaternion.identity, parent);
            unit.Initialize(this, definition);
            slot.PlaceUnit(unit);
            return unit;
        }

        /// <summary>
        /// 드래그 드롭 결과에 따라 배치 또는 머지를 시도합니다.
        /// </summary>
        public bool TryPlaceOrMerge(MergeGameUnit unit, MergeGameSlot targetSlot)
        {
            if (unit == null || targetSlot == null)
            {
                return false;
            }

            if (targetSlot.IsEmpty)
            {
                // 빈 슬롯이면 단순 배치.
                targetSlot.PlaceUnit(unit);
                return true;
            }

            var target = targetSlot.Occupant;
            if (target == null)
            {
                targetSlot.PlaceUnit(unit);
                return true;
            }

            // 동일 타입 + 동일 등급일 때만 머지한다.
            if (target.UnitType != unit.UnitType || target.Grade != unit.Grade)
            {
                return false;
            }

            return MergeUnits(unit, target, targetSlot);
        }

        /// <summary>
        /// 현재 보드에서 최고 등급 유닛을 찾습니다.
        /// </summary>
        public MergeGameUnit GetHighestGradeUnit()
        {
            MergeGameUnit selected = null;
            var bestGrade = int.MinValue;

            for (var i = 0; i < _slots.Count; i++)
            {
                var unit = _slots[i]?.Occupant;
                if (unit == null)
                {
                    continue;
                }

                if (unit.Grade > bestGrade)
                {
                    bestGrade = unit.Grade;
                    selected = unit;
                }
            }

            return selected;
        }

        private bool MergeUnits(MergeGameUnit source, MergeGameUnit target, MergeGameSlot targetSlot)
        {
            if (source == null || target == null)
            {
                return false;
            }

            // 머지 결과는 다음 등급의 랜덤 유닛으로 만든다.
            var nextGrade = target.Grade + 1;
            MergeGameUnitDefinition nextDefinition = null;
            if (_unitCatalog != null)
            {
                nextDefinition = _unitCatalog.GetRandomDefinitionByGrade(nextGrade);
            }

            if (nextDefinition != null)
            {
                // 기존 유닛을 제거하고 새 유닛을 만든다.
                source.RemoveFromBoard();
                target.RemoveFromBoard();
                Destroy(source.gameObject);
                Destroy(target.gameObject);

                SpawnUnit(nextDefinition, targetSlot);
                return true;
            }

            // 정의가 없으면 동일 타입 유지로 업그레이드한다.
            source.RemoveFromBoard();
            Destroy(source.gameObject);
            target.Upgrade();
            targetSlot.PlaceUnit(target);
            return true;
        }

        private void CollectSlots()
        {
            // 자식에서 슬롯을 모은다.
            _slots.Clear();
            GetComponentsInChildren(true, _slots);
        }
    }
}
