using System;

namespace Common.Interface
{
    /// <summary>
    /// 애니메이션 이벤트를 수신하고 처리하기 위한 인터페이스입니다.
    /// </summary>
    public interface IAnimEventReceiver
    {
        /// <summary>
        /// 애니메이션 이벤트가 수신되었을 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<string> OnAnimEventReceived;

        /// <summary>
        /// 애니메이션 이벤트를 수신했을 때 호출되는 메서드입니다.
        /// </summary>
        /// <param name="eventData">이벤트와 함께 전달된 데이터 문자열입니다.</param>
        public void OnAnimationEventReceive(string eventData);
    
    }
}