using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using Diagnostics = System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
    /// 게임 호스트 기본 클래스입니다.
    /// 서버/호스트 시뮬레이션을 관리하며 스레드 기반 커맨드 처리,
    /// 이벤트 디스패치, 스냅샷 생성을 제공합니다.
    /// </summary>
    public abstract class GameHostBase<TCommand, TResult, TEvent, TSnapshot>
        : IGameHost<TCommand, TResult, TEvent, TSnapshot>,
          IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>,
          IDisposable
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        /// <summary>
        /// 처리 대기 중인 커맨드 큐입니다.
        /// </summary>
        private readonly ConcurrentQueue<TCommand> _pendingCommands = new();

        /// <summary>
        /// 메인 스레드로 전달할 디스패치 큐입니다.
        /// </summary>
        private readonly ConcurrentQueue<DispatchItem> _dispatchQueue = new();

        /// <summary>
        /// 스냅샷 큐입니다.
        /// </summary>
        private readonly ConcurrentQueue<TSnapshot> _snapshotQueue = new();

        /// <summary>
        /// 라이프사이클 동기화용 락입니다.
        /// </summary>
        private readonly object _lifecycleLock = new();

        /// <summary>
        /// 시뮬레이션 루프 스레드입니다.
        /// </summary>
        private Thread _loopThread;

        /// <summary>
        /// 실행 여부입니다.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Dispose 여부입니다.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 고정 스텝 시간입니다.
        /// </summary>
        private float _fixedStep = 1f / 30f;

        /// <summary>
        /// 틱당 최대 스텝 수입니다.
        /// </summary>
        private int _maxStepsPerTick = 8;

        /// <summary>
        /// 루프 대기 시간(ms)입니다.
        /// </summary>
        private int _sleepMilliseconds = 1;

        /// <summary>
        /// 종료 대기 제한(ms)입니다.
        /// </summary>
        private int _stopTimeoutMilliseconds = 5000;

        /// <summary>
        /// 스냅샷 생성 간격(초)입니다. 0이면 매 틱 생성합니다.
        /// </summary>
        private float _snapshotInterval = 0f;

        /// <summary>
        /// 스냅샷 누적 시간입니다.
        /// </summary>
        private double _snapshotAccumulator;

        /// <summary>
        /// 마지막으로 생성된 스냅샷입니다.
        /// </summary>
        private TSnapshot _latestSnapshot;

        /// <summary>
        /// 루프 예외 캐시입니다.
        /// </summary>
        private Exception _loopException;

        /// <summary>
        /// 현재 호스트 틱입니다.
        /// </summary>
        public long Tick { get; private set; }

        /// <summary>
        /// 실행 상태입니다.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 루프에서 발생한 예외입니다.
        /// </summary>
        public Exception LoopException => _loopException;

        /// <summary>
        /// 고정 스텝 시간입니다.
        /// </summary>
        public float FixedStep
        {
            get => _fixedStep;
            set => _fixedStep = value > 0f ? value : 1f / 30f;
        }

        /// <summary>
        /// 틱당 최대 스텝 수입니다. Spiral of Death 방지를 위한 제한입니다.
        /// </summary>
        public int MaxStepsPerTick
        {
            get => _maxStepsPerTick;
            set => _maxStepsPerTick = value > 0 ? value : 1;
        }

        /// <summary>
        /// 루프 대기 시간(ms)입니다.
        /// </summary>
        public int SleepMilliseconds
        {
            get => _sleepMilliseconds;
            set => _sleepMilliseconds = value < 0 ? 0 : value;
        }

        /// <summary>
        /// 종료 대기 제한(ms)입니다.
        /// </summary>
        public int StopTimeoutMilliseconds
        {
            get => _stopTimeoutMilliseconds;
            set => _stopTimeoutMilliseconds = value < 0 ? 5000 : value;
        }

        /// <summary>
        /// 스냅샷 생성 간격(초)입니다. 0이면 매 틱 생성합니다.
        /// </summary>
        public float SnapshotInterval
        {
            get => _snapshotInterval;
            set => _snapshotInterval = value < 0f ? 0f : value;
        }

        public event Action<TResult> ResultProduced;
        public event Action<TEvent> EventRaised;

        public void SendCommand(TCommand command)
        {
            if (command == null)
            {
                return;
            }

            _pendingCommands.Enqueue(command);
        }

        /// <summary>
        /// 백그라운드 스레드에서 시뮬레이션 루프를 시작합니다.
        /// </summary>
        public void StartSimulation()
        {
            ThrowIfDisposed();

            lock (_lifecycleLock)
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;
                _loopException = null;
                _loopThread = new Thread(RunLoop)
                {
                    IsBackground = true,
                    Name = $"{GetType().Name}-Loop"
                };
                _loopThread.Start();
            }
        }

        /// <summary>
        /// 시뮬레이션 루프를 중지하고 스레드 종료를 대기합니다.
        /// </summary>
        public void StopSimulation()
        {
            lock (_lifecycleLock)
            {
                if (!_isRunning)
                {
                    return;
                }

                _isRunning = false;
            }

            if (_loopThread != null)
            {
                var joined = _loopThread.Join(_stopTimeoutMilliseconds);
                if (!joined)
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] 시뮬레이션 스레드가 제한 시간 내 종료되지 않았습니다.({_stopTimeoutMilliseconds}ms)");
                }
                _loopThread = null;
            }
        }

        TSnapshot IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.BuildSnapshot()
        {
            var latest = default(TSnapshot);
            while (_snapshotQueue.TryDequeue(out var snapshot))
            {
                latest = snapshot;
            }

            // 최신 스냅샷이 있으면 반환하고, 없으면 캐시를 반환합니다.
            if (latest != null)
            {
                return latest;
            }

            return Volatile.Read(ref _latestSnapshot);
        }

        /// <summary>
        /// 커맨드를 처리하고 결과/이벤트를 반환합니다.
        /// </summary>
        protected abstract GameCommandOutcome<TResult, TEvent> HandleCommand(TCommand command);

        void IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.Submit(TCommand command)
        {
            SendCommand(command);
        }

        void IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.Advance(float deltaSeconds)
        {
            Tick++;

            // 대기 중인 커맨드를 처리하고 결과/이벤트를 큐에 등록합니다.
            while (_pendingCommands.TryDequeue(out var command))
            {
                try
                {
                    //커맨드에 대한 선행 이벤트가 있다면 발행합니다.
                    var outcome = HandleCommand(command);
                    if (outcome.PreEvents != null)
                    {
                        for (var i = 0; i < outcome.PreEvents.Count; i++)
                        {
                            PublishEvent(outcome.PreEvents[i]);
                        }
                    }

                    //커맨드에 대한 결과를 발행합니다.
                    if (outcome.Result != null)
                    {
                        PublishResult(outcome.Result);
                    }

                    //커맨드에 대한 후행 이벤트가 있다면 발행합니다.
                    if (outcome.PostEvents != null)
                    {
                        for (var i = 0; i < outcome.PostEvents.Count; i++)
                        {
                            PublishEvent(outcome.PostEvents[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{GetType().Name}] 커맨드 처리 오류 {command?.GetType().Name}: {ex}");
                    PublishEvent(CreateErrorEvent(command, ex));
                }
            }

            try
            {
                OnTick(deltaSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] OnTick 오류: {ex}");
            }

            TryBuildSnapshot(deltaSeconds);
        }

        /// <summary>
        /// 매 스텝 호출되어 게임 상태를 갱신합니다.
        /// </summary>
        protected abstract void OnTick(float deltaSeconds);

        /// <summary>
        /// 현재 게임 상태의 스냅샷을 생성합니다(루프 스레드에서 호출).
        /// </summary>
        protected abstract TSnapshot BuildSnapshotInternal();

        /// <summary>
        /// 커맨드 처리 실패 시 기본 에러 이벤트를 생성합니다.
        /// </summary>
        protected virtual TEvent CreateErrorEvent(TCommand command, Exception exception)
        {
            return default;
        }

        protected void PublishResult(TResult result)
        {
            if (result == null)
            {
                return;
            }

            _dispatchQueue.Enqueue(DispatchItem.ForResult(result));
        }

        protected virtual void HandleInternalEvent(TEvent eventData)
        {
        }

        protected void PublishEvent(TEvent eventData)
        {
            if (eventData == null)
            {
                return;
            }

            //내부에 쓸일있을수있으니 먼저 쏴주자.
            HandleInternalEvent(eventData);
            _dispatchQueue.Enqueue(DispatchItem.ForEvent(eventData));
        }

        /// <summary>
        /// 메인 스레드에서 결과/이벤트를 디스패치합니다.
        /// Unity Update에서 호출해야 합니다.
        /// </summary>
        public void FlushEvents()
        {
            while (_dispatchQueue.TryDequeue(out var item))
            {
                try
                {
                    if (item.IsResult)
                    {
                        ResultProduced?.Invoke(item.Result);
                    }
                    else
                    {
                        EventRaised?.Invoke(item.EventData);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{GetType().Name}] 이벤트 디스패치 오류: {ex}");
                }
            }
        }

        /// <summary>
        /// 최신 스냅샷을 반환합니다.
        /// Host 루프가 최신 스냅샷 캐시를 갱신합니다.
        /// </summary>
        public TSnapshot GetLatestSnapshot()
        {
            return Volatile.Read(ref _latestSnapshot);
        }

        private void TryBuildSnapshot(float deltaSeconds)
        {
            if (_snapshotInterval <= 0f)
            {
                EnqueueSnapshot();
                return;
            }

            _snapshotAccumulator += deltaSeconds;
            if (_snapshotAccumulator < _snapshotInterval)
            {
                return;
            }

            _snapshotAccumulator -= _snapshotInterval;
            EnqueueSnapshot();
        }

        private void EnqueueSnapshot()
        {
            try
            {
                //현재 상태를 복제한 스냅샷 생성
                var snapshot = BuildSnapshotInternal();
                if (snapshot == null)
                {
                    return;
                }

                //큐에 넣어서 메인 스레드가 소비할 수 있게 함
                _snapshotQueue.Enqueue(snapshot);

                //최신 스냅샷을 캐시에 저장
                Volatile.Write(ref _latestSnapshot, snapshot);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] 스냅샷 생성 오류: {ex}");
            }
        }

        private readonly struct DispatchItem
        {
            public bool IsResult { get; }
            public TResult Result { get; }
            public TEvent EventData { get; }

            private DispatchItem(TResult result, TEvent eventData, bool isResult)
            {
                Result = result;
                EventData = eventData;
                IsResult = isResult;
            }

            public static DispatchItem ForResult(TResult result)
            {
                return new DispatchItem(result, default, true);
            }

            public static DispatchItem ForEvent(TEvent eventData)
            {
                return new DispatchItem(default, eventData, false);
            }
        }

        private void RunLoop()
        {
            try
            {
                var stopwatch = Diagnostics.Stopwatch.StartNew();
                var lastTime = stopwatch.Elapsed;
                var accumulator = 0.0;
                var host = (IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>)this;

                while (_isRunning)
                {
                    //현재 시간 기준 deltaTime 계산
                    var now = stopwatch.Elapsed;
                    var deltaSeconds = (now - lastTime).TotalSeconds;
                    lastTime = now;

                    //✅ 안정성 제한 : 너무 큰 delta를 잘라서 폭주 방지
                    // 렉/정지 시 deltaSeconds가 갑자기 커지는 경우 
                    // 한번에 쌓인 많은 Tick을 처리하게 되는 이슈 방지
                    if (deltaSeconds > 0.25) deltaSeconds = 0.25;

                    accumulator += deltaSeconds;
                    var step = _fixedStep;
                    var steps = 0;

                    while (accumulator >= step && steps < _maxStepsPerTick)
                    {
                        host.Advance(step);
                        accumulator -= step;
                        steps++;
                    }

                    //과도한 지연 보정 (누적 시간 리셋)
                    //(_maxStepsPerTick(현재 8) 보다 더 많은 스탭이 쌓여있어도 버리고 현재 기준으로 재시작..
                    //아니면 다음 프레임에도 또 _maxStepsPerTick 만큼 advance 할거고
                    //게임이 계속 밀림
                    if (steps >= _maxStepsPerTick) accumulator = 0.0;

                    if (_sleepMilliseconds > 0) Thread.Sleep(_sleepMilliseconds);
                    else Thread.Yield();
                }
            }
            catch (Exception ex)
            {
                _loopException = ex;
                Debug.LogError($"[{GetType().Name}] 시뮬레이션 루프 오류: {ex}");
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _pendingCommands.Clear();
            _dispatchQueue.Clear();
            _snapshotQueue.Clear();
        }
    }
}
