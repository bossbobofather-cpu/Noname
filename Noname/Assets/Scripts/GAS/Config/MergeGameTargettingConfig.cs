using System.Collections.Generic;
using MergeGame.Define;
using MergeGame.Target;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Config
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/MergeGameTargettingConfig")]
    public sealed class MergeGameTargettingConfig : GameplayTargettingConfig
    {
        //능력 오너의 관점에서 타겟팅 할 대상 그룹
        [SerializeField] private TargetGroup _targetGroup = TargetGroup.Opponent;

        //타겟 선정 방식
        [SerializeField] private TargetSelectionMode _selectionMode = TargetSelectionMode.All;

        //타겟 진영 선정 방식
        [SerializeField] private TargetFactionMode _factionMode = TargetFactionMode.Any;

        public AbilityTargetData AcquireTargets(AbilitySystemComponent owner, AbilityTargetRequest request)
        {
            var origin = request.UseOverrideOrigin ? request.OverrideOrigin : ResolveOrigin(owner);
            var data = new AbilityTargetData(origin);

            if (!TargetRegistry.TryGet(out var registry))
            {
                return data;
            }

            var targets = registry.GetTargets(_targetGroup);
            if (targets == null || targets.Count == 0)
            {
                return data;
            }

            var candidates = new List<TargetCandidate>(targets.Count);

            for (var i = 0; i < targets.Count; i++)
            {
                var targetable = targets[i];
                if (targetable == null)
                {
                    continue;
                }

                var abilitySystem = targetable.AbilitySystem;

                // 오너 자신 제외인 경우 타겟에서 자신은 제외
                if (!_includeOwner && abilitySystem == owner)
                {
                    continue;
                }

                // ... 진영 필터링 시작 ... //
                owner.TryGetComponent<Targetable>(out var ownerTargetable);
                if (ownerTargetable == null)
                {
                    continue;
                }

                //오너와 동일 진영모드일때
                if (_factionMode == TargetFactionMode.SameAsOwner)
                {
                    //타겟이 오너와 진영이 다르면 제외
                    if (targetable.Group != ownerTargetable.Group)
                    {
                        continue;
                    }
                }
                //오너와 다른 진영모드일때
                else if (_factionMode == TargetFactionMode.DifferentFromOwner)
                {
                    //타겟이 오너와 진영이 같으면 제외
                    if (targetable.Group == ownerTargetable.Group)
                    {
                        continue;
                    }
                }

                // ... 진영 필터 통과 ... //

                var targetTransform = targetable.GetTransform();
                if (targetTransform == null)
                {
                    continue;
                }

                if (!TryGetScore(targetTransform, abilitySystem, origin, out var score))
                {
                    continue;
                }

                candidates.Add(new TargetCandidate(targetTransform, abilitySystem, score));
            }

            if (candidates.Count == 0)
            {
                return data;
            }

            SortCandidates(candidates);

            var limit = _maxTargets > 0 ? Mathf.Min(_maxTargets, candidates.Count) : candidates.Count;
            for (var i = 0; i < limit; i++)
            {
                var candidate = candidates[i];
                if (candidate.AbilitySystem != null)
                {
                    data.AddAbilitySystem(candidate.AbilitySystem);
                }
                else
                {
                    data.AddTarget(candidate.Target);
                }
            }

            return data;
        }

        private bool TryGetScore(
            Transform target
            , AbilitySystemComponent abilitySystem
            , Vector3 origin
            , out float score)
        {
            score = 0f;
            if (target == null) return false;

            switch (_selectionMode)
            {
                case TargetSelectionMode.All:
                    return true;
                case TargetSelectionMode.ClosestToOrigin:
                    score = (target.position - origin).sqrMagnitude;
                    return true;
                case TargetSelectionMode.ClosestFromGoal:
                    score = -(target.position - origin).sqrMagnitude;
                    return true;
                case TargetSelectionMode.HighestAttribute:
                case TargetSelectionMode.LowestAttribute:
                    if (_selectionAttribute == null || abilitySystem == null)
                    {
                        return false;
                    }

                    if (!abilitySystem.Attributes.TryGet(_selectionAttribute, out var value))
                    {
                        return false;
                    }

                    score = value.CurrentValue;
                    return true;
                case TargetSelectionMode.Random:
                    score = Random.value;
                    return true;
                default:
                    return true;
            }
        }

        private void SortCandidates(List<TargetCandidate> candidates)
        {
            if (_selectionMode == TargetSelectionMode.All)
            {
                return;
            }

            var descending = _selectionMode == TargetSelectionMode.HighestAttribute;

            candidates.Sort((left, right) =>
                descending ? right.Score.CompareTo(left.Score) : left.Score.CompareTo(right.Score));
        }
    }
}
