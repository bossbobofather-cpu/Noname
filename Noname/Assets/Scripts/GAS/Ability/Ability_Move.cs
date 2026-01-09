using MergeGame.Unit;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Ability
{
    public sealed class Ability_Move : GameplayAbility
    {
        protected override void ActivateAbility(AbilityContext context)
        {
            if (!TryGetMoveInput(context.EventData.Payload, out var input))
            {
                return;
            }

            var controller = GetOrCreateMoveController();
            if (controller == null)
            {
                return;
            }

            controller.SetMoveInput(input);
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

        private static bool TryGetMoveInput(object payload, out Vector2 input)
        {
            switch (payload)
            {
                case Vector2 value2:
                    input = value2;
                    return true;
                case Vector3 value3:
                    input = new Vector2(value3.x, value3.z);
                    return true;
                case float value1:
                    input = new Vector2(value1, 0f);
                    return true;
                default:
                    input = Vector2.zero;
                    return payload == null;
            }
        }
    }
}
