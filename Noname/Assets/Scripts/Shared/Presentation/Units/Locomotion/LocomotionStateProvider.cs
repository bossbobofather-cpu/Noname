using System;

namespace MyProject.Common.Units.Locomotion
{
    /// <summary>
    /// 유닛의 현재 이동 상태 정보를 담는 구조체입니다.
    /// </summary>
    public struct LocomotionState
    {
        /// <summary>
        /// 지면에 닿아있는지 여부입니다.
        /// </summary>
        public bool IsGrounded;
        
        /// <summary>
        /// 지면에서 이동 중인지 여부입니다.
        /// </summary>
        public bool IsMoving;
        
        /// <summary>
        /// 공중에서 이동 입력이 있는지 여부입니다.
        /// </summary>
        public bool IsAirMoving;
        
        /// <summary>
        /// 점프 상승 중인지 여부입니다.
        /// </summary>
        public bool IsJumping;
        
        /// <summary>
        /// 낙하 중인지 여부입니다.
        /// </summary>
        public bool IsFalling;
    }

    /// <summary>
    /// 이동 상태를 제공하고 상태 변경 이벤트를 알리는 인터페이스입니다.
    /// </summary>
    public interface ILocomotionStateProvider
    {
        /// <summary>
        /// 현재 이동 상태입니다.
        /// </summary>
        LocomotionState CurrentState { get; }
        
        /// <summary>
        /// 이동 상태가 변경되었을 때 발생하는 이벤트입니다. (이전 상태, 현재 상태)
        /// </summary>
        event Action<LocomotionState, LocomotionState> OnLocomotionStateChanged;
    }
}
