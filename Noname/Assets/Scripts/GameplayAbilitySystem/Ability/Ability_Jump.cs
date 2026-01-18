using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.GameplayAbilitySystem.Ability
{
    /// <summary>
    /// 점프 능력을 정의하는 클래스입니다.
    /// IMovement 인터페이스를 통해 점프를 요청합니다.
    /// </summary>
    public sealed class Ability_Jump : GameplayAbility
    {
        private IMovement _movement;

        protected override void OnInit()
        {
            _movement = ASC?.Owner.GetComponent<IMovement>();
        }

        public override bool CanActivateAbility()
        {
            return _movement != null;
        }
        protected override void ActivateAbility(AbilityContext context)
        {
            _movement?.RequestJump();
        }
    }
}
