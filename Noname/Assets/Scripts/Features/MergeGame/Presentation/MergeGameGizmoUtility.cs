using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 배치용 기즈모를 그려주는 유틸리티입니다.
    /// </summary>
    public static class MergeGameGizmoUtility
    {
        private static readonly Color SpawnColor = new Color(0.2f, 0.9f, 0.6f, 1f);
        private static readonly Color PathPointColor = new Color(1f, 0.8f, 0.2f, 1f);
        private static readonly Color PathLineColor = new Color(1f, 0.6f, 0.1f, 1f);
        private static readonly Color SlotColor = new Color(0.2f, 0.6f, 1f, 1f);

        /// <summary>
        /// 스폰 포인트를 표시합니다.
        /// </summary>
        public static void DrawSpawnPoint(Vector3 position, float radius)
        {
            if (radius <= 0f)
            {
                return;
            }

            var previous = Gizmos.color;
            Gizmos.color = SpawnColor;
            Gizmos.DrawSphere(position, radius);
            Gizmos.color = previous;
        }

        /// <summary>
        /// 경로 포인트와 라인을 표시합니다.
        /// </summary>
        public static void DrawPath(IReadOnlyList<Transform> points, float radius)
        {
            if (points == null || points.Count == 0 || radius <= 0f)
            {
                return;
            }

            var previous = Gizmos.color;

            Gizmos.color = PathPointColor;
            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];
                if (point == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(point.position, radius);
            }

            Gizmos.color = PathLineColor;
            Transform lastPoint = null;
            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];
                if (point == null)
                {
                    continue;
                }

                if (lastPoint != null)
                {
                    Gizmos.DrawLine(lastPoint.position, point.position);
                }

                lastPoint = point;
            }

            Gizmos.color = previous;
        }

        /// <summary>
        /// 슬롯 위치를 박스로 표시합니다.
        /// </summary>
        public static void DrawSlot(Vector3 position, float size)
        {
            if (size <= 0f)
            {
                return;
            }

            var previous = Gizmos.color;
            Gizmos.color = SlotColor;
            Gizmos.DrawWireCube(position, new Vector3(size, 0.02f, size));
            Gizmos.color = previous;
        }

        /// <summary>
        /// 보드 그리드 슬롯 위치를 표시합니다.
        /// </summary>
        public static void DrawSlotGrid(Transform boardTransform, Vector3 origin, int columns, int rows, Vector2 spacing, float size)
        {
            if (boardTransform == null || columns <= 0 || rows <= 0 || size <= 0f)
            {
                return;
            }

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    var local = origin + new Vector3(col * spacing.x, 0f, row * spacing.y);
                    var world = boardTransform.TransformPoint(local);
                    DrawSlot(world, size);
                }
            }
        }
    }
}
