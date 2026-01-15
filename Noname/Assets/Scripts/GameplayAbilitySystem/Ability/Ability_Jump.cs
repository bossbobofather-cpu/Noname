using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.GameplayAbilitySystem.Ability
{
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
