using System.Collections.Generic;
using UnityEngine;
using MyProject.GameplayAbilitySystem.Define;

namespace MyProject.GameplayAbilitySystem.Target
{
    public sealed class TargetRegistry : MonoBehaviour
    {
        private static TargetRegistry _instance;

        private readonly HashSet<Targetable> _registered = new();
        private readonly List<Targetable> _playerUnits = new();
        private readonly List<Targetable> _opponentUnits = new();

        public static bool TryGet(out TargetRegistry registry)
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TargetRegistry>();
            }

            registry = _instance;
            return registry != null;
        }

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


        public void Register(Targetable target)
        {
            if (target == null || !_registered.Add(target))
            {
                return;
            }

            AddToGroup(target);
        }

        public void Unregister(Targetable target)
        {
            if (target == null || !_registered.Remove(target))
            {
                return;
            }

            RemoveFromGroup(target);
        }

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
