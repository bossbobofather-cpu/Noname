using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 몬스터 웨이브를 순차적으로 생성하는 스포너입니다.
    /// </summary>
    public sealed class MergeGameWaveSpawner : MonoBehaviour
    {
        [SerializeField] private MergeGamePath _path;
        [SerializeField] private List<MergeGameWaveEntry> _waves = new();
        [SerializeField] private float _startDelay = 1f;
        [SerializeField] private bool _autoStart = true;

        private Coroutine _routine;

        /// <summary>
        /// 몬스터가 스폰될 때 호출됩니다.
        /// </summary>
        public event Action<MergeGameMonster> MonsterSpawned;

        /// <summary>
        /// 스포너 설정을 갱신합니다.
        /// </summary>
        public void Configure(MergeGamePath path, IReadOnlyList<MergeGameWaveEntry> waves, float startDelay)
        {
            _autoStart = false;
            _path = path;
            _startDelay = Mathf.Max(0f, startDelay);

            _waves.Clear();
            if (waves == null)
            {
                return;
            }

            for (var i = 0; i < waves.Count; i++)
            {
                if (waves[i] == null)
                {
                    continue;
                }

                _waves.Add(waves[i]);
            }
        }

        private void Start()
        {
            if (_autoStart)
            {
                StartSpawning();
            }
        }

        /// <summary>
        /// 웨이브 스폰을 시작합니다.
        /// </summary>
        public void StartSpawning()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(SpawnRoutine());
        }

        /// <summary>
        /// 웨이브 스폰을 중지합니다.
        /// </summary>
        public void StopSpawning()
        {
            if (_routine == null)
            {
                return;
            }

            StopCoroutine(_routine);
            _routine = null;
        }

        private IEnumerator SpawnRoutine()
        {
            if (_startDelay > 0f)
            {
                // 시작 지연 시간을 적용한다.
                yield return new WaitForSeconds(_startDelay);
            }

            for (var waveIndex = 0; waveIndex < _waves.Count; waveIndex++)
            {
                var wave = _waves[waveIndex];
                if (wave == null || wave.Prefab == null)
                {
                    continue;
                }

                for (var i = 0; i < wave.Count; i++)
                {
                    SpawnMonster(wave.Prefab);

                    if (wave.Interval > 0f)
                    {
                        // 다음 스폰까지 대기한다.
                        yield return new WaitForSeconds(wave.Interval);
                    }
                }

                if (wave.WaveDelay > 0f)
                {
                    // 다음 웨이브 사이의 간격.
                    yield return new WaitForSeconds(wave.WaveDelay);
                }
            }

            _routine = null;
        }

        private void SpawnMonster(MergeGameMonster prefab)
        {
            if (prefab == null)
            {
                return;
            }

            // 몬스터를 생성하고 경로를 설정한다.
            var monster = Instantiate(prefab, transform.position, Quaternion.identity);
            monster.InitializePath(_path);
            MonsterSpawned?.Invoke(monster);
        }
    }

    /// <summary>
    /// 웨이브별 몬스터 스폰 정보입니다.
    /// </summary>
    [Serializable]
    public sealed class MergeGameWaveEntry
    {
        [SerializeField] private MergeGameMonster _prefab;
        [SerializeField] private int _count = 5;
        [SerializeField] private float _interval = 1f;
        [SerializeField] private float _waveDelay = 2f;

        /// <summary>
        /// 스폰할 몬스터 프리팹입니다.
        /// </summary>
        public MergeGameMonster Prefab => _prefab;

        /// <summary>
        /// 스폰 개수입니다.
        /// </summary>
        public int Count => Mathf.Max(0, _count);

        /// <summary>
        /// 개별 스폰 간격입니다.
        /// </summary>
        public float Interval => Mathf.Max(0f, _interval);

        /// <summary>
        /// 웨이브 사이 대기 시간입니다.
        /// </summary>
        public float WaveDelay => Mathf.Max(0f, _waveDelay);
    }
}
