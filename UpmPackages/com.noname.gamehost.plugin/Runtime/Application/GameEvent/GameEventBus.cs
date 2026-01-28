using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 전역/씬 스코프를 함께 제공하는 게임 이벤트 버스입니다.
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
        /// 현재 Active Scene에 해당하는 스코프를 반환합니다.
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
        /// 이벤트 타입에 맞는 스코프로 구독을 등록합니다.
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Subscribe(handler);
        }

        /// <summary>
        /// 이벤트 타입에 맞는 스코프에서 구독을 해제합니다.
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Unsubscribe(handler);
        }

        /// <summary>
        /// 이벤트 타입에 맞는 스코프로 이벤트를 발행합니다.
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
        /// Active Scene이 아직 설정되지 않았을 때만 설정합니다.
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
            UnityEngine.Debug.LogWarning("Active Scene이 설정되지 않아 Scene Event Bus를 사용할 수 없습니다.");
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
                $"{eventType.Name} 이벤트가 스코프를 지정하지 않았습니다. SceneGameEventContext 또는 GlobalGameEventContext를 사용하세요.");
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
