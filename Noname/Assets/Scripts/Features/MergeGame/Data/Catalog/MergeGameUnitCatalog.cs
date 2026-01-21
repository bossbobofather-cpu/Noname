using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 유닛 프리팹과 등급 정보를 관리하는 카탈로그입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Unit Catalog")]
    public sealed class MergeGameUnitCatalog : ScriptableObject
    {
        [SerializeField] private List<MergeGameUnitDefinition> _definitions = new();

        /// <summary>
        /// 등록된 유닛 정의 목록입니다.
        /// </summary>
        public IReadOnlyList<MergeGameUnitDefinition> Definitions => _definitions;

        /// <summary>
        /// 특정 타입과 등급에 해당하는 정의를 찾습니다.
        /// </summary>
        public MergeGameUnitDefinition FindDefinition(MergeGameUnitType type, int grade)
        {
            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (definition.UnitType == type && definition.Grade == grade)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// 지정한 등급의 유닛 중 랜덤으로 하나를 반환합니다.
        /// </summary>
        public MergeGameUnitDefinition GetRandomDefinitionByGrade(int grade)
        {
            if (_definitions.Count == 0)
            {
                return null;
            }

            // 후보군을 수집한다.
            var candidates = ListPool<MergeGameUnitDefinition>.Get();
            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];
                if (definition == null || definition.Grade != grade)
                {
                    continue;
                }

                candidates.Add(definition);
            }

            if (candidates.Count == 0)
            {
                ListPool<MergeGameUnitDefinition>.Release(candidates);
                return null;
            }

            var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            ListPool<MergeGameUnitDefinition>.Release(candidates);
            return selected;
        }

        /// <summary>
        /// 특정 타입의 가장 낮은 등급 정의를 찾습니다.
        /// </summary>
        public MergeGameUnitDefinition GetLowestGradeDefinition(MergeGameUnitType type)
        {
            MergeGameUnitDefinition selected = null;
            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];
                if (definition == null || definition.UnitType != type)
                {
                    continue;
                }

                if (selected == null || definition.Grade < selected.Grade)
                {
                    selected = definition;
                }
            }

            return selected;
        }
    }

    /// <summary>
    /// 유닛 타입/등급별 프리팹 정의입니다.
    /// </summary>
    [Serializable]
    public sealed class MergeGameUnitDefinition
    {
        [SerializeField] private MergeGameUnitType _unitType = MergeGameUnitType.Ranged;
        [SerializeField] private int _grade = 1;
        [SerializeField] private int _cost = 1;
        [SerializeField] private MergeGameUnit _prefab;

        /// <summary>
        /// 유닛 타입입니다.
        /// </summary>
        public MergeGameUnitType UnitType => _unitType;

        /// <summary>
        /// 유닛 등급입니다.
        /// </summary>
        public int Grade => _grade;

        /// <summary>
        /// 소환 비용입니다.
        /// </summary>
        public int Cost => _cost;

        /// <summary>
        /// 유닛 프리팹입니다.
        /// </summary>
        public MergeGameUnit Prefab => _prefab;
    }

    /// <summary>
    /// MergeGameUnitCatalog에서 사용하는 임시 리스트 풀입니다.
    /// </summary>
    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new();

        public static List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            if (list == null)
            {
                return;
            }

            list.Clear();
            Pool.Push(list);
        }
    }
}
