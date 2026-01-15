using System;

namespace Common.Interface
{
    public interface IAnimEventReceiver
    {
        public event Action<string> OnAnimEventReceived;

        public void OnAnimationEventReceive(string eventData);
    
    }
}