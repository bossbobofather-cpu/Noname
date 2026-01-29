using System;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 자기 자신을 타겟으로 선택합니다.
    /// </summary>
    public sealed class SelfTargetingStrategy : ITargetingStrategy
    {
        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            var data = new TargetData(owner);
            data.AddTarget(owner);
            return data;
        }
    }

    /// <summary>
    /// 랜덤 적을 선택합니다.
    /// </summary>
    public sealed class RandomTargetingStrategy : ITargetingStrategy
    {
        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var index = context.Random.Next(0, enemies.Count);
            data.AddTarget(enemies[index]);
            return data;
        }
    }

    /// <summary>
    /// 가장 가까운 적을 선택합니다.
    /// </summary>
    public sealed class NearestEnemyTargetingStrategy : ITargetingStrategy
    {
        private readonly float _maxRange;

        public NearestEnemyTargetingStrategy(float maxRange = float.PositiveInfinity)
        {
            _maxRange = maxRange;
        }

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var origin = context.ResolvePosition(owner);
            var maxRangeSq = _maxRange * _maxRange;
            var bestDistance = float.PositiveInfinity;
            AbilitySystemComponent best = null;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var pos = context.ResolvePosition(enemy);
                var distance = Point2D.DistanceSquared(origin, pos);
                if (distance > maxRangeSq)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            data.AddTarget(best);
            return data;
        }
    }

    /// <summary>
    /// 체력이 가장 낮은 적을 선택합니다.
    /// </summary>
    public sealed class LowestHpTargetingStrategy : ITargetingStrategy
    {
        private readonly AttributeId _healthId;

        public LowestHpTargetingStrategy(AttributeId healthId)
        {
            _healthId = healthId;
        }

        public LowestHpTargetingStrategy() : this(AttributeId.Health) { }

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            AbilitySystemComponent best = null;
            var lowestHp = float.PositiveInfinity;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var hp = enemy.Get(_healthId);
                if (hp < lowestHp)
                {
                    lowestHp = hp;
                    best = enemy;
                }
            }

            data.AddTarget(best);
            return data;
        }
    }

    /// <summary>
    /// 가장 가까운 N명의 적을 선택합니다.
    /// </summary>
    public sealed class NearestNEnemiesTargetingStrategy : ITargetingStrategy
    {
        private readonly int _maxTargets;
        private readonly float _maxRange;

        public NearestNEnemiesTargetingStrategy(int maxTargets, float maxRange = float.PositiveInfinity)
        {
            _maxTargets = Math.Max(1, maxTargets);
            _maxRange = maxRange;
        }

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var origin = context.ResolvePosition(owner);
            var maxRangeSq = _maxRange * _maxRange;

            // 거리와 함께 저장
            var distances = new (AbilitySystemComponent enemy, float distSq)[enemies.Count];
            var validCount = 0;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var pos = context.ResolvePosition(enemy);
                var distSq = Point2D.DistanceSquared(origin, pos);
                if (distSq <= maxRangeSq)
                {
                    distances[validCount++] = (enemy, distSq);
                }
            }

            var addExtraTargetCount = Math.Min(10, _maxTargets + owner.Get(AttributeId.ExtraTargetCount));
            
            // 거리순 정렬 (간단한 선택 정렬)
            for (var i = 0; i < Math.Min(addExtraTargetCount, validCount); i++)
            {
                var minIdx = i;
                for (var j = i + 1; j < validCount; j++)
                {
                    if (distances[j].distSq < distances[minIdx].distSq)
                    {
                        minIdx = j;
                    }
                }
                if (minIdx != i)
                {
                    (distances[i], distances[minIdx]) = (distances[minIdx], distances[i]);
                }
                data.AddTarget(distances[i].enemy);
            }

            return data;
        }
    }

    /// <summary>
    /// 특정 반경 내의 모든 적을 선택합니다.
    /// </summary>
    public sealed class AreaTargetingStrategy : ITargetingStrategy
    {
        private readonly float _radius;

        public AreaTargetingStrategy(float radius)
        {
            _radius = Math.Max(0f, radius);
        }

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var origin = context.ResolvePosition(owner);
            var rangeSq = _radius * _radius;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var pos = context.ResolvePosition(enemy);
                var distance = Point2D.DistanceSquared(origin, pos);
                if (distance <= rangeSq)
                {
                    data.AddTarget(enemy);
                }
            }

            return data;
        }
    }
}
