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

            //현재 공격 속도를 기반해서 쿨다운 지속 시간을 계산한다. AttackSpeed가 높을 수록 쿨다운은 줄어두는 형태
            return duration *= asc.Get(AttributeId.AttackSpeed);
        }
    }
}