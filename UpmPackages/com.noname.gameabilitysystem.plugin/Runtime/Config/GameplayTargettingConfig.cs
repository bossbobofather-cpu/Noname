using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTargettingConfig")]
    public class GameplayTargettingConfig : GameplayConfig
    {
        //타겟팅 기준점 설정. 하위 앵커 키(이름). 빈 문자열일 경우 오브젝트 중심점
        [SerializeField] protected string _anchorKey = string.Empty;

        //기준점으로부터 오프셋
        [SerializeField] protected Vector3 _centerOffset = Vector3.zero;

        //오너 자신을 타겟팅 대상에 포함할지 여부
        [SerializeField] protected bool _includeOwner = false;

        //최대 타겟 수. 0 이하일 경우 제한 없음
        [SerializeField] protected int _maxTargets = 1;

        //선정 방식이 속성 기반일 경우 사용할 속성
        [SerializeField] protected AttributeDefinition _selectionAttribute;


        protected Vector3 ResolveOrigin(AbilitySystemComponent owner)
        {
            if (owner == null)
            {
                return Vector3.zero;
            }

            if(string.IsNullOrEmpty(_anchorKey))
            {
                return owner.transform.TransformPoint(_centerOffset);
            }
            else
            {
                var anchor = owner.transform.Find(_anchorKey);
                if (anchor == null)
                {

                    Debug.LogWarning($"Anchor '{_anchorKey}' not found on owner {owner.name}");
                    return owner.transform.TransformPoint(_centerOffset);
                }

                return anchor.TransformPoint(_centerOffset);
            }
        }

        protected readonly struct TargetCandidate
        {
            public TargetCandidate(Transform target, AbilitySystemComponent abilitySystem, float score)
            {
                Target = target;
                AbilitySystem = abilitySystem;
                Score = score;
            }

            public Transform Target { get; }
            public AbilitySystemComponent AbilitySystem { get; }

            // 선정 점수
            public float Score { get; }
        }
    }
}
