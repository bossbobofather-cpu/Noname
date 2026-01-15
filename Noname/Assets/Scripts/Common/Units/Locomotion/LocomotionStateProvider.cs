using System;

namespace MyProject.Common.Units.Locomotion
{
    public struct LocomotionState
    {
        public bool IsGrounded;
        public bool IsMoving;
        public bool IsAirMoving;
        public bool IsJumping;
        public bool IsFalling;
    }

    public interface ILocomotionStateProvider
    {
        LocomotionState CurrentState { get; }
        event Action<LocomotionState, LocomotionState> OnLocomotionStateChanged;
    }
}
