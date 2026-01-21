using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 몬스터 이동 경로를 구성하는 컴포넌트입니다.
    /// </summary>
    public sealed class MergeGamePath : MonoBehaviour
    {
        [SerializeField] private List<Transform> _points = new();

        /// <summary>
        /// 경로 포인트 수입니다.
        /// </summary>
        public int Count => _points.Count;

        /// <summary>
        /// 지정한 인덱스의 경로 위치를 반환합니다.
        /// </summary>
        public Vector3 GetPoint(int index)
        {
            if (index < 0 || index >= _points.Count || _points[index] == null)
            {
                return transform.position;
            }

            return _points[index].position;
        }

        /// <summary>
        /// 랜덤 경로 위치를 반환합니다.
        /// </summary>
        public Vector3 GetRandomPoint()
        {
            if (_points.Count == 0)
            {
                return transform.position;
            }

            var point = _points[Random.Range(0, _points.Count)];
            return point != null ? point.position : transform.position;
        }

        /// <summary>
        /// 시작 지점 위치입니다.
        /// </summary>
        public Vector3 GetStartPoint()
        {
            return GetPoint(0);
        }

        /// <summary>
        /// 마지막 지점 위치입니다.
        /// </summary>
        public Vector3 GetEndPoint()
        {
            return GetPoint(_points.Count - 1);
        }

        private void OnDrawGizmos()
        {
            MergeGameGizmoUtility.DrawPath(_points, 0.2f);
        }
    }
}
