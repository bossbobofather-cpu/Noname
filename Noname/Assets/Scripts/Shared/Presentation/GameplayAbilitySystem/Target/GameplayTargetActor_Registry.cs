using System.Collections.Generic;
using UnityEngine;
using Noname.GameAbilitySystem;
using MyProject.GameplayAbilitySystem.Config;
using MyProject.GameplayAbilitySystem.Define;

namespace MyProject.GameplayAbilitySystem.Target
{
    /// <summary>
    /// TargetRegistry에 등록된 Targetable 객체들을 대상으로 타겟을 탐색하는 액터입니다.
    /// 범위, 각도, 진영 등의 조건에 따라 대상을 필터링하고 정렬합니다.
    /// </summary>
    [DisallowMultipleComponent]
public sealed class GameplayTargetActor_Registry : GameplayTargetActor
    {
        public override AbilityTargetData AcquireTargetData(AbilitySystemComponent owner)
        {
            var origin = ResolveOrigin(owner);
            var data = new AbilityTargetData(origin);

            if (!TargetRegistry.TryGet(out var registry))
            {
                return data;
            }

            if (Config is not GameplayTargetActorRegistryConfig groupConfig)
            {
                Debug.LogWarning("TargetActor_Registry requires GameplayTargetActorRegistryConfig.");
                return data;
            }

            if (owner == null)
            {
                return data;
            }

            // 탐색 범위 및 각도 설정 가져오기
            var range = GetAttackRange(owner);
            var useRange = range > 0f;
            var rangeSqr = useRange ? range * range : 0f;
            var leftAngle = groupConfig.LeftAngle;
            var rightAngle = groupConfig.RightAngle;
            var useAngle = leftAngle > 0f || rightAngle > 0f;

            // 설정된 타겟 그룹 가져오기
            var targets = registry.GetTargets(groupConfig.TargetGroup);
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
                // 자기 자신 포함 여부 체크
                if (!Config.IncludeOwner && abilitySystem == owner)
                {
                    continue;
                }

                owner.TryGetComponent<Targetable>(out var ownerTargetable);
                if (ownerTargetable == null)
                {
                    continue;
                }

                // 진영 모드에 따른 필터링
                if (groupConfig.FactionMode == TargetFactionMode.SameAsOwner)
                {
                    if (targetable.Group != ownerTargetable.Group)
                    {
                        continue;
                    }
                }
                else if (groupConfig.FactionMode == TargetFactionMode.DifferentFromOwner)
                {
                    if (targetable.Group == ownerTargetable.Group)
                    {
                        continue;
                    }
                }

                var targetTransform = targetable.GetTransform();
                if (targetTransform == null)
                {
                    continue;
                }

                // 거리 체크
                if (useRange)
                {
                    var offset = targetTransform.position - origin;
                    if (offset.sqrMagnitude > rangeSqr)
                    {
                        continue;
                    }
                }

                // 각도 체크
                if (useAngle && !IsWithinAngle(owner.transform, origin, targetTransform.position, leftAngle, rightAngle))
                {
                    continue;
                }

                // 점수 계산 (정렬 기준)
                if (!TryGetScore(groupConfig, targetTransform, abilitySystem, origin, out var score))
                {
                    continue;
                }

                candidates.Add(new TargetCandidate(targetTransform, abilitySystem, score));
            }

            if (candidates.Count == 0)
            {
                return data;
            }

            // 점수에 따라 정렬
            SortCandidates(groupConfig, candidates);

            // 최대 타겟 수만큼 데이터 추가
            var limit = ClampMaxTargets(candidates.Count);
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

        private static float GetAttackRange(AbilitySystemComponent owner)
        {
            if (owner == null)
            {
                return 0f;
            }

            if (owner.Attributes.TryGet(AttributeId.AttackRange, out var value) && value != null)
            {
                return value.CurrentValue;
            }

            return 0f;
        }

        private static bool IsWithinAngle(Transform owner, Vector3 origin, Vector3 targetPosition, float leftAngle, float rightAngle)
        {
            if (owner == null)
            {
                return true;
            }

            var forward = owner.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            var toTarget = targetPosition - origin;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            forward.Normalize();
            toTarget.Normalize();

            // 전방 벡터와 타겟 방향 벡터 사이의 각도 계산
            var signedAngle = Vector3.SignedAngle(forward, toTarget, Vector3.up);
            return signedAngle >= -leftAngle && signedAngle <= rightAngle;
        }

        private bool TryGetScore(
            GameplayTargetActorRegistryConfig config,
            Transform target,
            AbilitySystemComponent abilitySystem,
            Vector3 origin,
            out float score)
        {
            score = 0f;
            if (target == null)
            {
                return false;
            }

            switch (config.SelectionMode)
            {
                case TargetSelectionMode.All:
                    return true;
                case TargetSelectionMode.ClosestToOrigin:
                    score = (target.position - origin).sqrMagnitude;
                    return true;
                case TargetSelectionMode.ClosestFromGoal:
                    score = -(target.position - origin).sqrMagnitude; // 거리가 멀수록 낮은 점수(가까운게 우선) -> 내림차순 정렬 가정 시 음수로 반전 필요? 로직 확인 필요하나 원본 유지
                    return true;
                case TargetSelectionMode.HighestAttribute:
                case TargetSelectionMode.LowestAttribute:
                    if (Config.SelectionAttribute == null || abilitySystem == null)
                    {
                        return false;
                    }

                    if (!abilitySystem.Attributes.TryGet(Config.SelectionAttribute, out var value))
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

        private static void SortCandidates(GameplayTargetActorRegistryConfig config, List<TargetCandidate> candidates)
        {
            if (config.SelectionMode == TargetSelectionMode.All)
            {
                return;
            }

            // HighestAttribute일 경우 내림차순, 그 외(거리 등)는 오름차순(점수가 작을수록 우선)
            // 원본 로직: descending ? right.CompareTo(left) : left.CompareTo(right)
            // ClosestToOrigin의 경우 거리가 작을수록(오름차순) 앞쪽에 와야 함.
            var descending = config.SelectionMode == TargetSelectionMode.HighestAttribute;
            candidates.Sort((left, right) =>
                descending ? right.Score.CompareTo(left.Score) : left.Score.CompareTo(right.Score));
        }

        private readonly struct TargetCandidate
        {
            public TargetCandidate(Transform target, AbilitySystemComponent abilitySystem, float score)
            {
                Target = target;
                AbilitySystem = abilitySystem;
                Score = score;
            }

            public Transform Target { get; }
            public AbilitySystemComponent AbilitySystem { get; }
            public float Score { get; }
        }
    }
}
