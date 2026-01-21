using UnityEngine;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 폭발물을 던져 범위 공격을 수행하는 유닛 공격 로직입니다.
    /// </summary>
    public sealed class MergeGameUnitAttackBomb : MergeGameUnitAttackBase
    {
        [SerializeField] private MergeGameBomb _bombPrefab;
        [SerializeField] private MergeGamePath _path;
        [SerializeField] private GameplayEffectConfig _damageEffect;
        [SerializeField] private float _spawnHeight = 0.5f;

        protected override bool CanAttack()
        {
            // 폭발물 프리팹과 경로가 준비되어 있어야 한다.
            return _bombPrefab != null && _path != null && _damageEffect != null;
        }

        protected override void ExecuteAttack()
        {
            if (_bombPrefab == null || _path == null)
            {
                return;
            }

            var origin = _path.GetRandomPoint();
            origin.y += _spawnHeight;

            // 폭발물을 생성하고 효과를 전달한다.
            var bomb = Instantiate(_bombPrefab, origin, Quaternion.identity);
            bomb.Initialize(SourceAbility, _damageEffect);
        }
    }
}
