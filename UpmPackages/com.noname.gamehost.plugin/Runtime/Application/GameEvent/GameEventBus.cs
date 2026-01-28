using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ����/�� �������� ���� ���� �̺�Ʈ �����Դϴ�.
    /// </summary>
    public static class GameEventBus
    {
        private static readonly Scope GlobalScope = new();
        private static readonly Dictionary<int, Scope> SceneScopes = new();
        private static Scene _activeScene;
        private static bool _missingActiveSceneLogged;
        private static readonly HashSet<Type> UnknownScopeLogged = new();

        public static Scope Global => GlobalScope;

        /// <summary>
        /// ���� Active ���� �ش��ϴ� �����Դϴ�.
        /// </summary>
        public static Scope Scene
        {
            get
            {
                if (!_activeScene.IsValid())
                {
                    LogMissingActiveScene();
                    return null;
                }

                if (!SceneScopes.TryGetValue(_activeScene.handle, out var scope))
                {
                    LogMissingActiveScene();
                    return null;
                }

                return scope;
            }
        }

        /// <summary>
        /// �̺�Ʈ Ÿ���� �������� ���� �����մϴ�.
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Subscribe(handler);
        }

        /// <summary>
        /// �̺�Ʈ Ÿ���� �������� ���� ������ �����մϴ�.
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Unsubscribe(handler);
        }

        /// <summary>
        /// �̺�Ʈ Ÿ���� �������� ���� �����մϴ�.
        /// </summary>
        public static void Publish<TEvent>(TEvent context) where TEvent : GameEventContext
        {
            if (context == null)
            {
                return;
            }

            var scope = ResolveScope(context.GetType());
            scope?.Publish(context);
        }

        public static Scope ForScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                LogMissingActiveScene();
                return null;
            }

            return ForScene(scene.handle);
        }

        public static Scope ForScene(int handle)
        {
            var scene = FindLoadedSceneByHandle(handle);
            if (!scene.HasValue)
            {
                LogMissingActiveScene();
                return null;
            }

            if (!SceneScopes.TryGetValue(handle, out var scope))
            {
                scope = new Scope();
                SceneScopes.Add(handle, scope);
            }

            return scope;
        }

        public static void SetActiveScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                _activeScene = default;
                return;
            }

            _activeScene = scene;
            _missingActiveSceneLogged = false;
            ForScene(scene);
        }

        /// <summary>
        /// Active ���� ������� ���� �����մϴ�.
        /// </summary>
        public static bool TrySetActiveScene(Scene scene)
        {
            if (_activeScene.IsValid())
            {
                return false;
            }

            if (!scene.IsValid())
            {
                return false;
            }

            SetActiveScene(scene);
            return true;
        }

        public static void ClearGlobal()
        {
            GlobalScope.Clear();
        }

        public static void ResetAll()
        {
            GlobalScope.Clear();
            SceneScopes.Clear();
            _activeScene = default;
            _missingActiveSceneLogged = false;
            UnknownScopeLogged.Clear();
        }

        public static void ClearScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            ClearScene(scene.handle);
        }

        public static void ClearScene(int handle)
        {
            if (SceneScopes.TryGetValue(handle, out var scope))
            {
                scope.Clear();
                SceneScopes.Remove(handle);
            }

            if (_activeScene.IsValid() && _activeScene.handle == handle)
            {
                _activeScene = default;
            }
        }

        private static void LogMissingActiveScene()
        {
            if (_missingActiveSceneLogged)
            {
                return;
            }

            _missingActiveSceneLogged = true;
            UnityEngine.Debug.LogWarning("Active Scene�� �������� �ʾ� Scene Event Bus�� ����� �� �����ϴ�.");
        }

        private static Scope ResolveScope(Type eventType)
        {
            if (eventType == null)
            {
                return null;
            }

            if (typeof(GlobalGameEventContext).IsAssignableFrom(eventType))
            {
                return GlobalScope;
            }

            if (typeof(SceneGameEventContext).IsAssignableFrom(eventType))
            {
                return Scene;
            }

            LogUnknownScope(eventType);
            return Scene;
        }

        private static void LogUnknownScope(Type eventType)
        {
            if (!UnknownScopeLogged.Add(eventType))
            {
                return;
            }

            UnityEngine.Debug.LogWarning(
                $"{eventType.Name} �̺�Ʈ�� �������� �������� �ʾҽ��ϴ�. SceneGameEventContext �Ǵ� GlobalGameEventContext�� ����ϼ���.");
        }

        private static Scene? FindLoadedSceneByHandle(int handle)
        {
            if (handle == 0 && SceneManager.sceneCount == 0)
            {
                return null;
            }

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.handle == handle)
                {
                    return scene;
                }
            }

            return null;
        }

        public sealed class Scope : IEventBus<GameEventContext>
        {
            private readonly Dictionary<Type, List<Delegate>> _handlers = new();

            public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
            {
                if (handler == null)
                {
                    return;
                }

                var key = typeof(TEvent);
                if (!_handlers.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _handlers.Add(key, handlers);
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                }
            }

            public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
            {
                if (handler == null)
                {
                    return;
                }

                var key = typeof(TEvent);
                if (!_handlers.TryGetValue(key, out var handlers))
                {
                    return;
                }

                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _handlers.Remove(key);
                }
            }

            public void Publish<TEvent>(TEvent context) where TEvent : GameEventContext
            {
                if (context == null)
                {
                    return;
                }

                var key = typeof(TEvent);
                if (!_handlers.TryGetValue(key, out var handlers))
                {
                    return;
                }

                var snapshot = handlers.ToArray();
                for (var i = 0; i < snapshot.Length; i++)
                {
                    if (snapshot[i] is Action<TEvent> handler)
                    {
                        handler(context);
                    }
                }
            }

            public void Clear()
            {
                _handlers.Clear();
            }
        }
    }
}
