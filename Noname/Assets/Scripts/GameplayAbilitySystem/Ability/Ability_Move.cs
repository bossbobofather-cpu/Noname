using UnityEngine;
using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.GameplayAbilitySystem.Ability
{
    public sealed class Ability_Move : GameplayAbility
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
            if (!TryGetMoveInput(context.EventData.Payload, out var input))
            {
                return;
            }

            _movement?.SetMoveInput(input);
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
