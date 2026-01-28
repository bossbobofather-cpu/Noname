using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using Diagnostics = System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
    /// 게임 ?�스??기본 ?�래?�입?�다.
    /// ?�버 �?게임 ?��??�이?�을 관리하�? ?�레???�전 커맨??처리, ?�벤???�스?�칭, ?�냅???�성???�공?�니??
    /// </summary>
    public abstract class GameHostBase<TCommand, TResult, TEvent, TSnapshot>
    : IHostCommandBus<TCommand, TResult, TEvent>,
    IGameHost<TCommand, TResult, TEvent, TSnapshot>,
    IDisposable
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
        /// 고정 ?��??�이???�?�스??�??�니??
        /// </summary>
        public float FixedStep
        {
            get => _fixedStep;
            set => _fixedStep = value > 0f ? value : 1f / 30f;
        }

        /// <summary>
        /// ?�당 최�? ?��??�이???�텝 ?�입?�다. Spiral of Death�?방�??�니??
        /// </summary>
        public int MaxStepsPerTick
        {
            get => _maxStepsPerTick;
            set => _maxStepsPerTick = value > 0 ? value : 1;
        }

        /// <summary>
        /// ?��??�이????�??�립 ?�간(밀리초)?�니??
        /// </summary>
        public int SleepMilliseconds
        {
            get => _sleepMilliseconds;
            set => _sleepMilliseconds = value < 0 ? 0 : value;
        }

        /// <summary>
        /// ?��??�이???�레???��? ???�?�아??밀리초)?�니??
        /// </summary>
        public int StopTimeoutMilliseconds
        {
            get => _stopTimeoutMilliseconds;
            set => _stopTimeoutMilliseconds = value < 0 ? 5000 : value;
        }

        /// <summary>
        /// ?�냅???�성 간격(�??�니?? 0?�면 �??�마???�성?�니??
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
        /// 백그?�운???�레?�에???��??�이??루프�??�작?�니??
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
        /// ?��??�이??루프�??��??�고 ?�레?��? 종료???�까지 ?�기??�다.
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
                    Debug.LogWarning($"[{GetType().Name}] ?��??�이???�레?��? ?�?�아???�에 ?��??��? ?�았?�니??({_stopTimeoutMilliseconds}ms)");
                }
                _loopThread = null;
            }
        }
        TSnapshot IGameHost<TCommand, TResult, TEvent, TSnapshot>.BuildSnapshot()
        {
            var latest = default(TSnapshot);
            while (_snapshotQueue.TryDequeue(out var snapshot))
            {
                latest = snapshot;
            }

            // ?�용 가?�한 최신 ?�냅?�을 반환?�거?? ?�으�?캐시???�냅?�을 반환?�니??
            if (latest != null)
            {
                return latest;
            }

            return Volatile.Read(ref _latestSnapshot);
        }

        /// <summary>
        /// 커맨?��? 처리?�고 결과 �??�벤?��? ?�함??결과�?반환?�니??
        /// </summary>
        protected abstract GameCommandOutcome<TResult, TEvent> HandleCommand(TCommand command);

        void IGameHost<TCommand, TResult, TEvent, TSnapshot>.Submit(TCommand command)
        {
            SendCommand(command);
        }

        void IGameHost<TCommand, TResult, TEvent, TSnapshot>.Advance(float deltaSeconds)
        {
            Tick++;

            // ?��?중인 모든 커맨?��? 처리?�고 결과/?�벤?��? ?�에 추�??�니??
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
                    Debug.LogError($"[{GetType().Name}] 커맨??처리 �??�류 {command?.GetType().Name}: {ex}");
                    PublishEvent(CreateErrorEvent(command, ex));
                }
            }

            try
            {
                OnTick(deltaSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] OnTick ?�류: {ex}");
            }

            TryBuildSnapshot(deltaSeconds);
        }


        /// <summary>
        /// �??��??�이???�마???�출?�어 게임 ?�태�??�데?�트?�니??
        /// </summary>
        protected abstract void OnTick(float deltaSeconds);

        /// <summary>
        /// ?�재 게임 ?�태???�냅?�을 ?�성?�니??(?��??�이???�레?�에???�출??.
        /// </summary>
        protected abstract TSnapshot BuildSnapshotInternal();

        /// <summary>
        /// 커맨??처리 ?�패 ???�러 ?�벤?��? ?�성?�니??
        /// 커스?�??�러 ?�벤???�성???�해 ?�버?�이?�할 ???�습?�다.
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
        /// 메인 ?�레?�에???��?중인 모든 결과?�??�벤?��? ?�스?�치?�니??
        /// Unity??Update 루프?�서 ?�기?�으�??�출?�야 ?�니??
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
                    Debug.LogError($"[{GetType().Name}] ?�벤???�스?�치 ?�류: {ex}");
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
                Debug.LogError($"[{GetType().Name}] ?�냅???�성 ?�류: {ex}");
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

                var host = (IGameHost<TCommand, TResult, TEvent, TSnapshot>)this;

                while (_isRunning)
                {
                    var now = stopwatch.Elapsed;
                    var deltaSeconds = (now - lastTime).TotalSeconds;
                    lastTime = now;

                    // 과도?�게 ???��???불안?�성??방�??�기 ?�해 ?�한?�니??
                    if (deltaSeconds > 0.25)
                    {
                        deltaSeconds = 0.25;
                    }

                    accumulator += deltaSeconds;
                    var step = _fixedStep;
                    var steps = 0;

                    while (accumulator >= step && steps < _maxStepsPerTick)
                    {
                        host.Advance(step);
                        accumulator -= step;
                        steps++;
                    }

                    if (steps >= _maxStepsPerTick)
                    {
                        // 최�? ?�텝???�달?�면 ?�적기�? 리셋?�니?? //?�레??밀?�을???�번??많이 ?�라?��? ?�게?�자
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
                Debug.LogError($"[{GetType().Name}] ?��??�이??루프?�서 치명???�류 발생: {ex}");
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
