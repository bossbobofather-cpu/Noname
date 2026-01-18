using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 게임플레이 능력의 기본 클래스입니다.
    /// </summary>
    public abstract class GameplayAbility
    {
        private AbilitySystemComponent _asc;
        private IReadOnlyList<GameplayConfig> _configs;
        private IAbilityTaskOwner _taskOwner;

        /// <summary>
        /// 능력 시스템 컴포넌트를 가져옵니다.
        /// </summary>
        protected AbilitySystemComponent ASC => _asc;

        /// <summary>
        /// 태스크 소유자를 가져옵니다.
        /// </summary>
        protected IAbilityTaskOwner TaskOwner => _taskOwner;

        /// <summary>
        /// 어빌리티에 연결된 구성 목록입니다.
        /// </summary>
        public IReadOnlyList<GameplayConfig> Configs => _configs;

        /// <summary>
        /// 능력 초기화 처리를 합니다.
        /// </summary>
        /// <param name="asc">능력 시스템 컴포넌트</param>
        /// <param name="configs">구성 목록</param>
        public void InitializeAbility(AbilitySystemComponent asc, IReadOnlyList<GameplayConfig> configs)
        {
            // 기본 참조와 구성을 먼저 보관한다.
            _asc = asc;
            _configs = configs;

            // 초기화 훅을 호출한다.
            OnInit();
        }

        /// <summary>
        /// 태스크 소유자를 바인딩합니다.
        /// </summary>
        /// <param name="owner">태스크 소유자</param>
        internal void BindTaskOwner(IAbilityTaskOwner owner)
        {
            // 태스크와 연결되는 소유자를 저장한다.
            _taskOwner = owner;
        }

        /// <summary>
        /// 특정 타입의 구성들을 가져옵니다.
        /// </summary>
        /// <typeparam name="T">요청할 구성 타입</typeparam>
        /// <param name="configs">찾아낸 구성 목록</param>
        /// <returns>하나 이상 찾았는지 여부</returns>
        public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig
        {
            // 결과 목록을 새로 만든다.
            configs = new List<T>();
            for (var i = 0; i < _configs.Count; i++)
            {
                if (_configs[i] is T typed)
                {
                    // 요청한 타입만 수집한다.
                    configs.Add(typed);
                }
            }

            return configs.Count > 0;
        }

        /// <summary>
        /// 특정 타입의 구성을 하나 가져옵니다.
        /// </summary>
        /// <typeparam name="T">요청할 구성 타입</typeparam>
        /// <param name="config">찾아낸 구성</param>
        /// <returns>찾았는지 여부</returns>
        public bool TryGetConfig<T>(out T config) where T : GameplayConfig
        {
            for (var i = 0; i < _configs.Count; i++)
            {
                if (_configs[i] is T typed)
                {
                    // 첫 번째로 발견한 항목을 반환한다.
                    config = typed;
                    return true;
                }
            }

            // 찾지 못하면 기본값을 넘긴다.
            config = null;
            return false;
        }

        /// <summary>
        /// 능력 활성화 가능 여부를 반환합니다.
        /// </summary>
        /// <returns>활성화 가능 여부</returns>
        public virtual bool CanActivateAbility()
        {
            // 기본값은 허용이다.
            return true;
        }

        /// <summary>
        /// 능력 취소 처리를 합니다.
        /// </summary>
        /// <param name="handle">능력 핸들</param>
        public virtual void CancelAbility(FGameplayAbilitySpecHandle handle)
        {
            // 기본 구현은 비워 둔다.
        }

        /// <summary>
        /// 능력 종료 처리를 합니다.
        /// </summary>
        /// <param name="handle">능력 핸들</param>
        public virtual void EndAbility(FGameplayAbilitySpecHandle handle)
        {
            // 기본 구현은 비워 둔다.
        }

        /// <summary>
        /// 능력 활성화를 호출합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        public void CallActivateAbility(AbilityContext context)
        {
            // 활성화 전 처리를 먼저 수행한다.
            PreActivate(context);
            ActivateAbility(context);
        }

        /// <summary>
        /// 능력 활성화 전 처리입니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        protected virtual void PreActivate(AbilityContext context)
        {
            // 필요 시 파생 클래스에서 오버라이드한다.
        }

        /// <summary>
        /// 능력 활성화 처리입니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        protected virtual void ActivateAbility(AbilityContext context)
        {
            // 필요 시 파생 클래스에서 오버라이드한다.
        }

        /// <summary>
        /// 어빌리티별 초기화 처리입니다.
        /// </summary>
        protected virtual void OnInit()
        {
            // 필요 시 파생 클래스에서 오버라이드한다.
        }
    }
}
