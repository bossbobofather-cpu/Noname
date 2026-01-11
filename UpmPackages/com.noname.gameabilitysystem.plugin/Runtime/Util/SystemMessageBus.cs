using System;

namespace Noname.GameAbilitySystem
{
    public static class SystemMessageBus
    {
        public static event Action<string> MessagePublished;

        public static void Publish(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            MessagePublished?.Invoke(message);
        }
    }
}
