using UnityEngine;

namespace Common.Interface
{
    public interface IMovement
    {
        public float GetMoveSpeed();
        public float GetJumpSpeed();
        public bool IsMoveBlocked();
        public void SetMoveSpeed(float moveSpeed);
        public void SetJumpSpeed(float jumpSpeed);
        public void SetMoveBlocked(bool isBlocked);

        public void RequestJump();
        public void SetMoveInput(Vector2 input);
    }
}