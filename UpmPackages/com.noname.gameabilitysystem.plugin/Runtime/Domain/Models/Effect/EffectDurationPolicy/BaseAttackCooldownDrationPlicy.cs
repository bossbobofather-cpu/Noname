using System;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 기본 공격 쿨다운 이펙트 지속 시간 정책
    /// </summary>
    [Serializable]
    public sealed class BaseAttackCooldownDrationPlicy : IEffectDurationPolicy
    {
        public float CalculateDuration(AbilitySystemComponent asc, ref float duration)
        {
            if(asc == null) return duration;

            //아무리 공격속도가 빨라도 쿨다운 최소값 0.1 최대값 10 부여
            duration = Math.Clamp(duration / asc.Get(AttributeId.AttackSpeed), 0.1f, 10f);
            return duration;
        }
    }
}