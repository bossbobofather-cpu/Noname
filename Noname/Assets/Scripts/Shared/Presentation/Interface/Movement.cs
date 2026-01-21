using UnityEngine;

namespace Common.Interface
{
    /// <summary>
    /// 이동 및 점프 기능을 정의하는 인터페이스입니다.
    /// </summary>
    public interface IMovement
    {
        /// <summary>
        /// 현재 이동 속도를 반환합니다.
        /// </summary>
        public float GetMoveSpeed();

        /// <summary>
        /// 현재 점프력을 반환합니다.
        /// </summary>
        public float GetJumpSpeed();

        /// <summary>
        /// 이동이 차단된 상태인지 여부를 반환합니다.
        /// </summary>
        public bool IsMoveBlocked();

        /// <summary>
        /// 이동 속도를 설정합니다.
        /// </summary>
        /// <param name="moveSpeed">설정할 이동 속도입니다.</param>
        public void SetMoveSpeed(float moveSpeed);

        /// <summary>
        /// 점프력을 설정합니다.
        /// </summary>
        /// <param name="jumpSpeed">설정할 점프력입니다.</param>
        public void SetJumpSpeed(float jumpSpeed);

        /// <summary>
        /// 이동 차단 여부를 설정합니다.
        /// </summary>
        /// <param name="isBlocked">true일 경우 이동을 차단합니다.</param>
        public void SetMoveBlocked(bool isBlocked);

        /// <summary>
        /// 점프 동작을 요청합니다.
        /// </summary>
        public void RequestJump();

        /// <summary>
        /// 이동 입력을 설정합니다.
        /// </summary>
        /// <param name="input">입력된 이동 벡터입니다 (보통 x, y 축 사용).</param>
        public void SetMoveInput(Vector2 input);
    }
}