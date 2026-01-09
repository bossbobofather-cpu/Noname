using MergeGame.Unit;
using Noname.GameAbilitySystem;

namespace MergeGame.Ability
{
    public sealed class Ability_Jump : GameplayAbility
    {
        protected override void ActivateAbility(AbilityContext context)
        {
            var controller = GetOrCreateMoveController();
            if (controller == null)
            {
                return;
            }

            controller.RequestJump();
        }

        private UnitMoveController _moveController;

        private UnitMoveController GetOrCreateMoveController()
        {
            if (_moveController != null)
            {
                return _moveController;
            }

            if (ASC == null)
            {
                return null;
            }

            _moveController = ASC.GetComponentInParent<UnitMoveController>();
            if (_moveController != null)
            {
                return _moveController;
            }

            _moveController = ASC.gameObject.AddComponent<UnitMoveController>();
            return _moveController;
        }
    }
}
