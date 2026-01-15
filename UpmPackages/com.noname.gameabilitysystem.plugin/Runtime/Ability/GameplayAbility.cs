using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 게임플레이 능력의 기본 클래스
    /// </summary>
    public abstract class GameplayAbility
    {
        private AbilitySystemComponent _asc;
        private IReadOnlyList<GameplayConfig> _configs;
        private IAbilityTaskOwner _taskOwner;

        protected AbilitySystemComponent ASC => _asc;
        protected IAbilityTaskOwner TaskOwner => _taskOwner;
        public IReadOnlyList<GameplayConfig> Configs => _configs;

        /// <summary>
        /// 능력 초기화
        /// </summary>
        /// <param name="asc"></param>
        /// <param name="configs"></param>
        public void InitializeAbility(AbilitySystemComponent asc, IReadOnlyList<GameplayConfig> configs)
        {
            _asc = asc;
            _configs = configs;

            OnInit();
        }

        internal void BindTaskOwner(IAbilityTaskOwner owner)
        {
            _taskOwner = owner;
        }

        /// <summary>
        /// 특정 타입의 구성들을 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configs"></param>
        /// <returns></returns>
        public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig
        {
            configs = new List<T>();
            for (var i = 0; i < _configs.Count; i++)
            {
                if (_configs[i] is T typed)
                {
                    configs.Add(typed);
                }
            }

            return configs.Count > 0;
        }

        /// <summary>
        /// 특정 타입의 구성을 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>

        public bool TryGetConfig<T>(out T config) where T : GameplayConfig
        {
            for (var i = 0; i < _configs.Count; i++)
            {
                if (_configs[i] is T typed)
                {
                    config = typed;
                    return true;
                }
            }

            config = null;
            return false;
        }

        /// <summary>
        /// 능력 활성화 가능 여부
        /// </summary>
        /// <returns></returns>
        public virtual bool CanActivateAbility()
        {
            return true;
        }

        /// <summary>
        /// 능력 활성화
        /// </summary>
        /// </summary>
        /// <param name="handle"></param>
        public virtual void CancelAbility(FGameplayAbilitySpecHandle handle)
        {
        }

        /// <summary>
        /// 능력 종료
        /// </summary>
        /// <param name="handle"></param>

        public virtual void EndAbility(FGameplayAbilitySpecHandle handle)
        {
        }

        /// <summary>
        /// 능력 활성화
        /// </summary>
        /// </summary>
        /// <param name="context"></param>
        public void CallActivateAbility(AbilityContext context)
        {
            PreActivate(context);
            ActivateAbility(context);
        }

        /// <summary>
        /// 능력 활성화 전 처리
        /// </summary>
        /// <param name="context"></param>

        protected virtual void PreActivate(AbilityContext context)
        {
        }

        /// <summary>
        /// 능력 활성화 처리
        /// </summary>
        /// <param name="context"></param>

        protected virtual void ActivateAbility(AbilityContext context)
        {

        }

        /// <summary>
        /// Ability 별 초기화
        /// </summary>
        protected virtual void OnInit()
        {

        }
    }
}
