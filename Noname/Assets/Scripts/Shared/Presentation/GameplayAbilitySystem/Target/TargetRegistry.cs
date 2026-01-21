using System.Collections.Generic;
using UnityEngine;
using MyProject.GameplayAbilitySystem.Define;

namespace MyProject.GameplayAbilitySystem.Target
{
    /// <summary>
    /// 게임 내 모든 타겟(Targetable) 객체를 관리하는 레지스트리입니다.
    /// 싱글톤 패턴으로 접근하며, 그룹별로 타겟 리스트를 제공합니다.
    /// </summary>
    public sealed class TargetRegistry : MonoBehaviour
    {
        private static TargetRegistry _instance;

        private readonly HashSet<Targetable> _registered = new();
        private readonly List<Targetable> _playerUnits = new();
        private readonly List<Targetable> _opponentUnits = new();

        /// <summary>
        /// 레지스트리 인스턴스를 가져옵니다. 없으면 씬에서 찾습니다.
        /// </summary>
        public static bool TryGet(out TargetRegistry registry)
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TargetRegistry>();
            }

            registry = _instance;
            return registry != null;
        }

        /// <summary>
        /// 특정 그룹의 타겟 리스트를 반환합니다.
        /// </summary>
        public IReadOnlyList<Targetable> GetTargets(TargetGroup group)
        {
            switch (group)
            {
                case TargetGroup.Player:
                    return _playerUnits;
                case TargetGroup.Opponent:
                default:
                    return _opponentUnits;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }


        /// <summary>
        /// 타겟을 등록합니다.
        /// </summary>
        public void Register(Targetable target)
        {
            if (target == null || !_registered.Add(target))
            {
                return;
            }

            AddToGroup(target);
        }

        /// <summary>
        /// 타겟 등록을 해제합니다.
        /// </summary>
        public void Unregister(Targetable target)
        {
            if (target == null || !_registered.Remove(target))
            {
                return;
            }

            RemoveFromGroup(target);
        }

        /// <summary>
        /// 씬 내의 모든 타겟을 찾아 레지스트리를 재구성합니다.
        /// </summary>
        public void Rebuild()
        {
            _registered.Clear();
            _playerUnits.Clear();
            _opponentUnits.Clear();

            var targets = FindObjectsByType<Targetable>(FindObjectsSortMode.None);
            for (var i = 0; i < targets.Length; i++)
            {
                Register(targets[i]);
            }
        }

        private void AddToGroup(Targetable target)
        {
            switch (target.Group)
            {
                case TargetGroup.Player:
                    _playerUnits.Add(target);
                    break;
                case TargetGroup.Opponent:
                    _opponentUnits.Add(target);
                    break;
            }
        }

        private void RemoveFromGroup(Targetable target)
        {
            switch (target.Group)
            {
                case TargetGroup.Player:
                    _playerUnits.Remove(target);
                    break;
                case TargetGroup.Opponent:
                    _opponentUnits.Remove(target);
                    break;
            }
        }
    }

}
