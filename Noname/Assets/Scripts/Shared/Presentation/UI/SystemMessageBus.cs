using System;
using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// 시스템 메시지 전달을 위한 정적 이벤트 버스입니다.
    /// </summary>
    public static class SystemMessageBus
    {
        /// <summary>
        /// 메시지가 발행되었을 때 발생하는 이벤트입니다.
        /// </summary>
        public static event Action<SystemMessage> MessagePublished;

        /// <summary>
        /// 메시지를 발행합니다 (기본 색상).
        /// </summary>
        public static void Publish(string message)
        {
            Publish(message, new Color(0f, 0f, 0f, 0.6f));
        }

        /// <summary>
        /// 메시지를 발행합니다 (색상 지정).
        /// </summary>
        public static void Publish(string message, Color backgroundColor)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            MessagePublished?.Invoke(new SystemMessage
            {
                Text = message,
                BackgroundColor = backgroundColor
            });
        }
    }

    /// <summary>
    /// 시스템 메시지 데이터입니다.
    /// </summary>
    public struct SystemMessage
    {
        /// <summary>
        /// 메시지 텍스트입니다.
        /// </summary>
        public string Text;

        /// <summary>
        /// 배경 색상입니다.
        /// </summary>
        public Color BackgroundColor;
    }
}
