using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 타겟팅에 필요한 월드 조회 함수를 제공하는 컨텍스트입니다.
    /// Host/서버 환경에서 주입하여 사용합니다.
    /// </summary>
    public sealed class TargetContext
    {
        public TargetContext(
            Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> getEnemies,
            Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> getAllies,
            Func<AbilitySystemComponent, Point2D> getPosition,
            Random random = null)
        {
            GetEnemies = getEnemies;
            GetAllies = getAllies;
            GetPosition = getPosition;
            Random = random ?? new Random();
        }

        /// <summary>
        /// 특정 주체 기준으로 적 목록을 반환합니다.
        /// </summary>
        public Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> GetEnemies { get; }

        /// <summary>
        /// 특정 주체 기준으로 아군 목록을 반환합니다.
        /// </summary>
        public Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> GetAllies { get; }

        /// <summary>
        /// 주체의 위치 정보를 반환합니다.
        /// </summary>
        public Func<AbilitySystemComponent, Point2D> GetPosition { get; }

        /// <summary>
        /// 랜덤 선택을 위한 RNG입니다.
        /// </summary>
        public Random Random { get; }

        public IReadOnlyList<AbilitySystemComponent> ResolveEnemies(AbilitySystemComponent owner)
        {
            return GetEnemies != null ? GetEnemies(owner) : Array.Empty<AbilitySystemComponent>();
        }

        public IReadOnlyList<AbilitySystemComponent> ResolveAllies(AbilitySystemComponent owner)
        {
            return GetAllies != null ? GetAllies(owner) : Array.Empty<AbilitySystemComponent>();
        }

        public Point2D ResolvePosition(AbilitySystemComponent owner)
        {
            return GetPosition != null ? GetPosition(owner) : default;
        }
    }
}
