using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Diagnostics = System.Diagnostics;

namespace MyProject.Common.Host
{
    /// <summary>
    /// 게임 호스트 기본 클래스입니다.
    /// 서버 측 게임 시뮬레이션을 관리하며, 스레드 안전 커맨드 처리, 이벤트 디스패칭, 스냅샷 생성을 제공합니다.
    /// </summary>
    public abstract class GameHostBase<TCommand, TResult, TEvent, TSnapshot>
        : IGameHost<TCommand, TResult, TEvent, TSnapshot>, IDisposable
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        private readonly ConcurrentQueue<TCommand> _pendingCommands = new();
        private readonly ConcurrentQueue<DispatchItem> _dispatchQueue = new();
        private readonly ConcurrentQueue<TSnapshot> _snapshotQueue = new();
        private readonly object _lifecycleLock = new();

        private Thread _loopThread;
        private bool _isRunning;
        private bool _disposed;
        private float _fixedStep = 1f / 30f;
        private int _maxStepsPerTick = 8;
        private int _sleepMilliseconds = 1;
        private int _stopTimeoutMilliseconds = 5000;
        private float _snapshotInterval = 0f;
        private double _snapshotAccumulator;
        private TSnapshot _latestSnapshot;
        private Exception _loopException;

        public long Tick { get; private set; }
        public bool IsRunning => _isRunning;
        public Exception LoopException => _loopException;

        /// <summary>
        /// 고정 시뮬레이션 타임스텝(초)입니다.
        /// </summary>
        public float FixedStep
        {
            get => _fixedStep;
            set => _fixedStep = value > 0f ? value : 1f / 30f;
        }

        /// <summary>
        /// 틱당 최대 시뮬레이션 스텝 수입니다. Spiral of Death를 방지합니다.
        /// </summary>
        public int MaxStepsPerTick
        {
            get => _maxStepsPerTick;
            set => _maxStepsPerTick = value > 0 ? value : 1;
        }

        /// <summary>
        /// 시뮬레이션 틱 간 슬립 시간(밀리초)입니다.
        /// </summary>
        public int SleepMilliseconds
        {
            get => _sleepMilliseconds;
            set => _sleepMilliseconds = value < 0 ? 0 : value;
        }

        /// <summary>
        /// 시뮬레이션 스레드 정지 시 타임아웃(밀리초)입니다.
        /// </summary>
        public int StopTimeoutMilliseconds
        {
            get => _stopTimeoutMilliseconds;
            set => _stopTimeoutMilliseconds = value < 0 ? 5000 : value;
        }

        /// <summary>
        /// 스냅샷 생성 간격(초)입니다. 0이면 매 틱마다 생성합니다.
        /// </summary>
        public float SnapshotInterval
        {
            get => _snapshotInterval;
            set => _snapshotInterval = value < 0f ? 0f : value;
        }

        public event Action<TResult> ResultProduced;
        public event Action<TEvent> EventRaised;

        public void Submit(TCommand command)
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
        /// 시뮬레이션 루프를 정지하고 스레드가 종료될 때까지 대기합니다.
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
                    Debug.LogWarning($"[{GetType().Name}] 시뮬레이션 스레드가 타임아웃 내에 정지하지 않았습니다 ({_stopTimeoutMilliseconds}ms)");
                }
                _loopThread = null;
            }
        }

        public void Advance(float deltaSeconds)
        {
            Tick++;

            // 대기 중인 모든 커맨드를 처리하고 결과/이벤트를 큐에 추가합니다.
            while (_pendingCommands.TryDequeue(out var command))
            {
                try
                {
                    var outcome = HandleCommand(command);
                    if (outcome.PreEvents != null)
                    {
                        for (var i = 0; i < outcome.PreEvents.Count; i++)
                        {
                            PublishEvent(outcome.PreEvents[i]);
                        }
                    }

                    if (outcome.Result != null)
                    {
                        PublishResult(outcome.Result);
                    }

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
                    Debug.LogError($"[{GetType().Name}] 커맨드 처리 중 오류 {command?.GetType().Name}: {ex}");
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

        public TSnapshot BuildSnapshot()
        {
            var latest = default(TSnapshot);
            while (_snapshotQueue.TryDequeue(out var snapshot))
            {
                latest = snapshot;
            }

            // 사용 가능한 최신 스냅샷을 반환하거나, 없으면 캐시된 스냅샷을 반환합니다.
            if (latest != null)
            {
                return latest;
            }

            return Volatile.Read(ref _latestSnapshot);
        }

        /// <summary>
        /// 커맨드를 처리하고 결과 및 이벤트가 포함된 결과를 반환합니다.
        /// </summary>
        protected abstract GameCommandOutcome<TResult, TEvent> HandleCommand(TCommand command);

        /// <summary>
        /// 매 시뮬레이션 틱마다 호출되어 게임 상태를 업데이트합니다.
        /// </summary>
        protected abstract void OnTick(float deltaSeconds);

        /// <summary>
        /// 현재 게임 상태의 스냅샷을 생성합니다 (시뮬레이션 스레드에서 호출됨).
        /// </summary>
        protected abstract TSnapshot BuildSnapshotInternal();

        /// <summary>
        /// 커맨드 처리 실패 시 에러 이벤트를 생성합니다.
        /// 커스텀 에러 이벤트 생성을 위해 오버라이드할 수 있습니다.
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

        protected void PublishEvent(TEvent eventData)
        {
            if (eventData == null)
            {
                return;
            }

            _dispatchQueue.Enqueue(DispatchItem.ForEvent(eventData));
        }

        /// <summary>
        /// 메인 스레드에서 대기 중인 모든 결과와 이벤트를 디스패치합니다.
        /// Unity의 Update 루프에서 정기적으로 호출해야 합니다.
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
                var snapshot = BuildSnapshotInternal();
                if (snapshot == null)
                {
                    return;
                }

                _snapshotQueue.Enqueue(snapshot);
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

                while (_isRunning)
                {
                    var now = stopwatch.Elapsed;
                    var deltaSeconds = (now - lastTime).TotalSeconds;
                    lastTime = now;

                    // 과도하게 큰 델타는 불안정성을 방지하기 위해 제한합니다.
                    if (deltaSeconds > 0.25)
                    {
                        deltaSeconds = 0.25;
                    }

                    accumulator += deltaSeconds;
                    var step = _fixedStep;
                    var steps = 0;

                    while (accumulator >= step && steps < _maxStepsPerTick)
                    {
                        Advance(step);
                        accumulator -= step;
                        steps++;
                    }

                    if (steps >= _maxStepsPerTick)
                    {
                        // 최대 스텝에 도달하면 누적기를 리셋합니다 (Spiral of Death 방지).
                        accumulator = 0.0;
                    }

                    if (_sleepMilliseconds > 0)
                    {
                        Thread.Sleep(_sleepMilliseconds);
                    }
                    else
                    {
                        Thread.Yield();
                    }
                }
            }
            catch (Exception ex)
            {
                _loopException = ex;
                Debug.LogError($"[{GetType().Name}] 시뮬레이션 루프에서 치명적 오류 발생: {ex}");
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopSimulation();
            _disposed = true;
        }
    }
}
