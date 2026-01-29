using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Noname.GameHost.GameEvent;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 공통 부트스트래퍼의 기본 동작을 제공하는 베이스입니다.
    /// 런타임 이벤트 바인딩과 매니저 생성 흐름을 담당합니다.
    /// </summary>
    public abstract class BootstrapperBase : MonoBehaviour
    {
        /// <summary>
        /// 초기화 시 생성할 매니저 프리팹 목록입니다.
        /// </summary>
        [SerializeField] private List<MonoBehaviour> _managerPrefabs = new();

        private static bool _initialized;
        private bool _didInit;

        /// <summary>
        /// 정적 상태와 이벤트 바인딩을 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _initialized = false;
            Application.quitting -= OnQuit;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            GameEventBus.ResetAll();
        }

        /// <summary>
        /// 런타임 시작 시 공용 이벤트를 연결합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            Application.quitting += OnQuit;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            GameEventBus.TrySetActiveScene(SceneManager.GetActiveScene());
        }

        private void Start()
        {
            if (_didInit)
            {
                return;
            }

            // 씬 로드 이후 한 번만 초기화를 수행합니다.
            _didInit = true;
            OnInit();
        }

        /// <summary>
        /// 자식 클래스 초기화 훅입니다.
        /// </summary>
        protected virtual void OnInit()
        {
            for (var i = 0; i < _managerPrefabs.Count; i++)
            {
                // 등록된 매니저 프리팹을 순서대로 생성합니다.
                CreateManager(_managerPrefabs[i]);
            }
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            GameEventBus.ClearScene(scene);
        }

        private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (oldScene.handle == newScene.handle)
            {
                return;
            }

            GameEventBus.SetActiveScene(newScene);
        }

        private static void OnQuit()
        {
            Application.quitting -= OnQuit;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            GameEventBus.ResetAll();
        }

        /// <summary>
        /// 매니저 프리팹을 생성하거나 기존 인스턴스를 재사용합니다.
        /// </summary>
        protected IManager CreateManager(MonoBehaviour prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            if (prefab is not IManager)
            {
                Debug.LogWarning($"IManager를 구현하지 않은 매니저 프리팹입니다: {prefab.name}");
                return null;
            }

            var managerType = prefab.GetType();
            var existing = Object.FindFirstObjectByType(managerType);
            if (existing != null)
            {
                // 이미 존재하면 재사용하고 초기화만 보장합니다.
                var manager = existing as IManager;
                manager?.Initialize();
                return manager;
            }

            var instance = Instantiate(prefab);
            if (instance == null)
            {
                return null;
            }

            // 생성된 매니저는 씬 전환에서도 유지합니다.
            DontDestroyOnLoad(instance.gameObject);
            var createdManager = instance as IManager;
            createdManager?.Initialize();
            return createdManager;
        }

        protected virtual void OnDestroy()
        {
            // 필요 시 자식 클래스에서 정리 로직을 추가합니다.
        }
    }
}
