using System;
using System.Collections.Generic;
using Noname.GameHost;
using Noname.GameHost.GameEvent;
using UnityEngine;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 게임 모드 베이스 클래스입니다.
    /// 구체적인 게임 모드는 이 클래스를 상속해서 구현합니다.
    /// </summary>
    public abstract class GameMode<TCommand, TResult, TEvent, TSnapshot> : MonoBehaviour, IGameMode
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        /// <summary>
        /// 모듈 프리팹 목록입니다.
        /// </summary>
        [SerializeField] private List<MonoBehaviour> _modulePrefabs = new();

        /// <summary>
        /// 호스트 커맨드 버스입니다.
        /// </summary>
        private IGameHost<TCommand, TResult, TEvent, TSnapshot> _host;

        /// <summary>
        /// 호스트 커맨드 버스에 접근합니다.
        /// </summary>
        protected IGameHost<TCommand, TResult, TEvent, TSnapshot> Host => _host;

        /// <summary>
        /// 생성된 모듈 인스턴스 목록입니다.
        /// </summary>
        private readonly List<MonoBehaviour> _moduleInstances = new();

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        private readonly List<IModule> _modules = new();

        /// <summary>
        /// 초기화 완료 여부입니다.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Startup 완료 여부입니다.
        /// </summary>
        private bool _started;

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        public IReadOnlyList<IModule> Modules => _modules;

        /// <summary>
        /// 씬 스코프 이벤트 버스입니다.
        /// </summary>
        public GameEventBus.Scope SceneBus => GameEventBus.Scene;

        /// <summary>
        /// 호스트를 주입하고 모듈을 초기화합니다.
        /// </summary>
        public void Initialize(IGameHost<TCommand, TResult, TEvent, TSnapshot> host)
        {
            if (_host != null)
            {
                return; // 이미 초기화됨
            }

            _host = host;
            _host.ResultProduced += OnHostResult;
            _host.EventRaised += OnHostEvent;

            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // 모듈 목록을 구성하고 각 모듈을 초기화합니다.
            BuildModuleList();
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Initialize(this);
            }

            StartupModule();
            OnInitialize();
        }

        /// <summary>
        /// 호스트 결과를 처리합니다.
        /// </summary>
        protected abstract void OnHostResult(TResult result);

        /// <summary>
        /// 호스트 이벤트를 처리합니다.
        /// </summary>
        protected abstract void OnHostEvent(TEvent evt);


        /// <summary>
        /// 모듈 Startup을 호출합니다.
        /// </summary>
        protected void StartupModule()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            // 각 모듈의 Startup을 호출합니다.
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Startup();
            }

            OnStartup();
        }

        /// <summary>
        /// 모듈 Shutdown을 호출합니다.
        /// </summary>
        protected void ShutdownModule()
        {
            if (!_started)
            {
                return;
            }

            _started = false;

            // 각 모듈의 Shutdown을 호출합니다.
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Shutdown();
            }

            OnShutdown();
        }

        /// <summary>
        /// 지정한 타입의 모듈을 반환합니다.
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
                // 하위 객체에서 모듈을 찾아 목록을 구성합니다.
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

                    // 모듈 프리팹을 인스턴스로 생성합니다.
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

        protected virtual void Update()
        {
            _host?.FlushEvents();
        }

        protected virtual void OnDestroy()
        {
            if (_host != null)
            {
                _host?.StopSimulation();

                _host.ResultProduced -= OnHostResult;
                _host.EventRaised -= OnHostEvent;

                var discope = (IDisposable)_host;
                if(discope != null)
                {
                    discope.Dispose();
                }
                else
                {
                    Debug.LogError("Host가 Disposable 인터페이스를 구현하지 않았습니다.");
                }

                _host = null;
            }

            ShutdownModule();
        }

        /// <summary>
        /// 모듈 초기화 완료 후 호출됩니다.
        /// </summary>
        protected virtual void OnInitialize()
        {
            _host?.StartSimulation();
        }

        /// <summary>
        /// 모듈 Startup 직후 호출됩니다.
        /// </summary>
        protected virtual void OnStartup()
        {
        }

        /// <summary>
        /// 모듈 Shutdown 직후 호출됩니다.
        /// </summary>
        protected virtual void OnShutdown()
        {
            
        }
    }
}
