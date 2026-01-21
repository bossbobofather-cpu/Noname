using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using MyProject.Common.GameEvent;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 공통 게임 모드 베이스입니다.
    /// </summary>
    public abstract class GameMode : MonoBehaviour
    {
        [FormerlySerializedAs("_moduleBehaviours")]
        [SerializeField] private List<MonoBehaviour> _modulePrefabs = new();

        private readonly List<MonoBehaviour> _moduleInstances = new();
        private readonly List<IModule> _modules = new();
        private bool _initialized;
        private bool _started;

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        public IReadOnlyList<IModule> Modules => _modules;

        /// <summary>
        /// 현재 씬에서 사용하는 이벤트 버스입니다.
        /// </summary>
        public GameEventBus.Scope SceneBus => GameEventBus.Scene;

        /// <summary>
        /// 모듈을 생성하고 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            // 모듈 목록을 구성한 뒤 초기화를 수행합니다.
            BuildModuleList();
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Initialize(this);
            }
        }

        /// <summary>
        /// 모듈의 Startup을 호출합니다.
        /// </summary>
        public void StartupModule()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            // 순서대로 모듈을 시작합니다.
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Startup();
            }
        }

        /// <summary>
        /// 모듈의 Shutdown을 호출합니다.
        /// </summary>
        public void ShutdownModule()
        {
            if (!_started)
            {
                return;
            }

            _started = false;
            // 순서대로 모듈을 정리합니다.
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Shutdown();
            }
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

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            GameEventBus.Subscribe(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            GameEventBus.Unsubscribe(handler);
        }

        public void Publish<TEvent>(TEvent context) where TEvent : GameEventContext
        {
            GameEventBus.Publish(context);
        }

        private void BuildModuleList()
        {
            _modules.Clear();
            _moduleInstances.Clear();

            if (_modulePrefabs.Count == 0)
            {
                // 프리팹 목록이 없으면 씬 내 배치된 모듈을 사용합니다.
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

                    // 모듈 프리팹을 자식으로 생성합니다.
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
        }
    }
}
