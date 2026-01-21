using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// UI 타입과 프리팹을 연결하는 레지스트리입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "UI/UI Registry")]
    public sealed class UIRegistry : ScriptableObject
    {
        [SerializeField] private List<Entry> _entries = new();

        /// <summary>
        /// 타입으로 UI 프리팹을 찾습니다.
        /// </summary>
        public bool TryGetPrefab<T>(out T prefab) where T : UIBase
        {
            if (TryGetPrefab(typeof(T), out var ui))
            {
                prefab = ui as T;
                return prefab != null;
            }

            prefab = null;
            return false;
        }

        /// <summary>
        /// 타입으로 UI 프리팹을 찾습니다.
        /// </summary>
        public bool TryGetPrefab(Type type, out UIBase prefab)
        {
            if (type == null)
            {
                prefab = null;
                return false;
            }

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry == null || entry.Prefab == null)
                {
                    continue;
                }

                if (entry.Matches(type))
                {
                    prefab = entry.Prefab;
                    return true;
                }
            }

            prefab = null;
            return false;
        }

        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private UIBase _prefab;
            [SerializeField] private string _typeName;

            public UIBase Prefab => _prefab;

            public bool Matches(Type type)
            {
                if (type == null)
                {
                    return false;
                }

                var target = _prefab != null ? _prefab.GetType() : ResolveType();
                return target == type;
            }

            public void SyncTypeName()
            {
                if (_prefab == null)
                {
                    return;
                }

                _typeName = _prefab.GetType().AssemblyQualifiedName;
            }

            private Type ResolveType()
            {
                if (string.IsNullOrEmpty(_typeName))
                {
                    return null;
                }

                return Type.GetType(_typeName);
            }
        }

        private void OnValidate()
        {
            RemoveNullEntries();
            RemoveDuplicateTypes();

            for (var i = 0; i < _entries.Count; i++)
            {
                _entries[i]?.SyncTypeName();
            }
        }

        private void RemoveNullEntries()
        {
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i] == null || _entries[i].Prefab == null)
                {
                    _entries.RemoveAt(i);
                }
            }
        }

        private void RemoveDuplicateTypes()
        {
            var seen = new HashSet<string>();
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry == null || entry.Prefab == null)
                {
                    continue;
                }

                var typeName = entry.Prefab.GetType().AssemblyQualifiedName;
                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                if (seen.Contains(typeName))
                {
                    _entries.RemoveAt(i);
                    continue;
                }

                seen.Add(typeName);
            }
        }
    }
}
