namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 도메인에서 사용하는 좌표 데이터입니다.
    /// Unity 의존성 없이 위치를 표현합니다.
    /// </summary>
    public readonly struct Point2D
    {
        private static Point2D zeroVector = new(0f, 0f);


        public float X { get; }
        public float Y { get; }

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static float DistanceSquared(in Point2D a, in Point2D b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static Point2D zero
        {
            get
            {
                return zeroVector;
            }
        }
    }
}
