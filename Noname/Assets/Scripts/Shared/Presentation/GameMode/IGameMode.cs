using System;
using System.Collections.Generic;
using Noname.GameHost.GameEvent;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 게임 모드가 제공하는 공용 인터페이스입니다.
    /// </summary>
    public interface IGameMode
    {
        GameEventBus.Scope SceneBus { get; }
        IReadOnlyList<IModule> Modules { get; }

        T GetModule<T>() where T : class, IModule;
        void Subscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext;
        void Unsubscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext;
        void Publish<TEventContext>(TEventContext context) where TEventContext : GameEventContext;
    }
}
