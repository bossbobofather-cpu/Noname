using System;
using System.Collections.Generic;

namespace MyProject.Common.Host
{
    /// <summary>
    /// 게임 세션을 구성하는 기본 설명서입니다.
    /// </summary>
    public sealed class GameSessionDescriptor
    {
        public string ModeKey { get; }
        public IReadOnlyList<GameModuleDescriptor> Modules { get; }

        public GameSessionDescriptor(string modeKey, IReadOnlyList<GameModuleDescriptor> modules)
        {
            ModeKey = modeKey ?? string.Empty;
            Modules = modules ?? Array.Empty<GameModuleDescriptor>();
        }

        public bool TryGetModule(string moduleKey, out GameModuleDescriptor descriptor)
        {
            descriptor = null;

            if (string.IsNullOrWhiteSpace(moduleKey))
            {
                return false;
            }

            for (var i = 0; i < Modules.Count; i++)
            {
                var current = Modules[i];
                if (current == null)
                {
                    continue;
                }

                if (string.Equals(current.ModuleKey, moduleKey, StringComparison.Ordinal))
                {
                    descriptor = current;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 게임 모듈 구성을 전달하는 단위입니다.
    /// </summary>
    public sealed class GameModuleDescriptor
    {
        public string ModuleKey { get; }
        public GameModuleConfig Config { get; }

        public GameModuleDescriptor(string moduleKey, GameModuleConfig config = null)
        {
            ModuleKey = moduleKey ?? string.Empty;
            Config = config;
        }
    }

    /// <summary>
    /// 모듈별 설정 데이터의 기본 타입입니다.
    /// </summary>
    public abstract class GameModuleConfig
    {
    }
}
