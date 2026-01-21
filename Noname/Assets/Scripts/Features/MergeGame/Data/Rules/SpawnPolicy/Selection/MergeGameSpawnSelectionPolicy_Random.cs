using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 가능한 유닛 중 랜덤으로 선택하는 정책입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/SpawnSelection/Random")]
    public sealed class MergeGameSpawnSelectionPolicy_Random : MergeGameSpawnSelectionPolicy
    {
        /// <summary>
        /// 입력 컨텍스트를 기반으로 유닛 타입을 선택합니다.
        /// </summary>
        public override MergeGameUnitType SelectUnitType(MergeGameSpawnSelectionContext context)
        {
            if (context.Catalog == null)
            {
                return MergeGameUnitType.Ranged;
            }

            var candidates = ListPool<MergeGameUnitDefinition>.Get();
            var definitions = context.Catalog.Definitions;
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null || definition.Grade != context.SpawnSpec.Grade)
                {
                    continue;
                }

                candidates.Add(definition);
            }

            if (candidates.Count == 0)
            {
                ListPool<MergeGameUnitDefinition>.Release(candidates);
                return MergeGameUnitType.Ranged;
            }

            var selected = candidates[Random.Range(0, candidates.Count)];
            ListPool<MergeGameUnitDefinition>.Release(candidates);
            return selected.UnitType;
        }
    }
}
