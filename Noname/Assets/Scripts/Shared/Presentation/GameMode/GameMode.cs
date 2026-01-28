using System;
using System.Collections.Generic;
using Noname.GameHost;
using Noname.GameHost.GameEvent;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// ?�네�??�?�을 ?�용?�는 게임 모드?�니??
    /// 구체?�인 게임 모드?????�래?��? ?�속받아 ?�???�전?�을 ?�보?????�습?�다.
    /// </summary>
    public abstract class GameMode<TCommand, TResult, TEvent, TSnapshot> : MonoBehaviour, IGameMode
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        [SerializeField] private List<MonoBehaviour> _modulePrefabs = new();

        private IHostCommandBus<TCommand, TResult, TEvent> _host;

        /// <summary>
        /// ?�???�전???�스???�근???�공?�니??
        /// </summary>
        protected IHostCommandBus<TCommand, TResult, TEvent> Host => _host;

        private readonly List<MonoBehaviour> _moduleInstances = new();
        private readonly List<IModule> _modules = new();
        private bool _initialized;
        private bool _started;

        /// <summary>
        /// ?�록??모듈 목록?�니??
        /// </summary>
        public IReadOnlyList<IModule> Modules => _modules;

        /// <summary>
        /// ??범위 ?�벤??버스?�니??
        /// </summary>
        public GameEventBus.Scope SceneBus => GameEventBus.Scene;

        /// <summary>
        /// ?�스?��? 주입?�고 초기?�합?�다.
        /// </summary>
        public void Initialize(IHostCommandBus<TCommand, TResult, TEvent> host)
        {
            if (_host != null)
            {
                return; // ?��? 초기?�됨
            }

            _host = host;
            _host.ResultProduced += OnHostResult;
            _host.EventRaised += OnHostEvent;

            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // 모듈 목록??빌드?�고 �?모듈??초기?�합?�다.
            BuildModuleList();
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Initialize(this);
            }

            StartupModule();
            OnInitialize();
        }

        /// <summary>
        /// ?�???�전???�스??결과 처리 메서?�입?�다.
        /// </summary>
        protected abstract void OnHostResult(TResult result);

        /// <summary>
        /// ?�???�전???�스???�벤??처리 메서?�입?�다.
        /// </summary>
        protected abstract void OnHostEvent(TEvent evt);


        /// <summary>
        /// 모듈 Startup???�출?�니??
        /// </summary>
        protected void StartupModule()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            // �?모듈??Startup???�출?�니??
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Startup();
            }

            OnStartup();
        }

        /// <summary>
        /// 모듈 Shutdown???�출?�니??
        /// </summary>
        protected void ShutdownModule()
        {
            if (!_started)
            {
                return;
            }

            _started = false;

            if (_host != null)
            {
                _host.ResultProduced -= OnHostResult;
                _host.EventRaised -= OnHostEvent;
                _host = null;
            }

            // �?모듈??Shutdown???�출?�니??
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Shutdown();
            }

            OnShutdown();
        }

        /// <summary>
        /// ?�정 ?�?�의 모듈??반환?�니??
        /// </summary>
        public T GetModule<T>() where T : class, IModule
        {
            for (var i = 0; i < _modules.Count; i++)
            {
                if (_modules[i] is T module)
                {
                    return module;
                }
            }

            return null;
        }

        public void Subscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext
        {
            GameEventBus.Subscribe(handler);
        }

        public void Unsubscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext
        {
            GameEventBus.Unsubscribe(handler);
        }

        public void Publish<TEventContext>(TEventContext context) where TEventContext : GameEventContext
        {
            GameEventBus.Publish(context);
        }

        private void BuildModuleList()
        {
            _modules.Clear();
            _moduleInstances.Clear();

            if (_modulePrefabs.Count == 0)
            {
                // ?�리??목록???�으�??�식 객체?�서 검?�합?�다.
                GetComponentsInChildren(true, _moduleInstances);
            }
            else
            {
                var parent = transform;
                for (var i = 0; i < _modulePrefabs.Count; i++)
                {
                    var prefab = _modulePrefabs[i];
                    if (prefab == null)
                    {
                        continue;
                    }

                    // 모듈 ?�리?�을 ?�식?�로 ?�성?�니??
                    var instance = Instantiate(prefab, parent);
                    _moduleInstances.Add(instance);
                }
            }

            for (var i = 0; i < _moduleInstances.Count; i++)
            {
                var behaviour = _moduleInstances[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour is not IModule module)
                {
                    continue;
                }

                if (_modules.Contains(module))
                {
                    continue;
                }

                _modules.Add(module);
            }
        }

        protected virtual void OnDestroy()
        {
            ShutdownModule();
        }

        /// <summary>
        /// 모듈 초기???�료 ???�출?�니??
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// 모듈 Startup 직후 ?�출?�니??
        /// </summary>
        protected virtual void OnStartup()
        {
        }

        /// <summary>
        /// 모듈 Shutdown 직후 ?�출?�니??
        /// </summary>
        protected virtual void OnShutdown()
        {
        }
    }
}