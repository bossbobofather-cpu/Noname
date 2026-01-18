using UnityEngine;

namespace MyProject.Common.CameraSystem
{
    /// <summary>
    /// 대상을 쿼터뷰 시점으로 추적하는 카메라 컨트롤러입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuarterViewFollowCamera : MonoBehaviour
    {
        /// <summary>
        /// 카메라가 추적할 대상의 트랜스폼입니다.
        /// </summary>
        [SerializeField] private Transform _target;

        /// <summary>
        /// 대상의 위치로부터 적용할 오프셋입니다.
        /// </summary>
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.6f, 0f);

        /// <summary>
        /// 대상과 카메라 사이의 거리입니다.
        /// </summary>
        [SerializeField] private float _distance = 8f;

        /// <summary>
        /// 카메라의 수직 각도(Pitch)입니다.
        /// </summary>
        [SerializeField, Range(-89f, 89f)] private float _pitch = 35f;

        /// <summary>
        /// 카메라의 수평 각도(Yaw)입니다.
        /// </summary>
        [SerializeField, Range(0f, 360f)] private float _yaw = 45f;

        /// <summary>
        /// 위치 이동 시 적용할 스무딩 시간입니다.
        /// </summary>
        [SerializeField] private float _positionSmoothTime = 0.1f;

        /// <summary>
        /// 회전 시 적용할 스무딩 시간입니다.
        /// </summary>
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        /// <summary>
        /// true일 경우 카메라가 항상 대상을 바라보도록 회전합니다.
        /// </summary>
        [SerializeField] private bool _lookAtTarget = true;

        /// <summary>
        /// true일 경우 LateUpdate에서, false일 경우 Update에서 로직을 처리합니다.
        /// </summary>
        [SerializeField] private bool _useLateUpdate = true;

        private Vector3 _positionVelocity;

        private void Reset()
        {
            if (_target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (_useLateUpdate)
            {
                UpdateCamera(Time.deltaTime);
            }
        }

        private void Update()
        {
            if (!_useLateUpdate)
            {
                UpdateCamera(Time.deltaTime);
            }
        }

        private void UpdateCamera(float deltaTime)
        {
            if (_target == null)
            {
                return;
            }

            // 대상의 위치에 오프셋을 더해 기준점(Anchor)을 계산
            var anchor = _target.position + _targetOffset;
            
            // Pitch와 Yaw를 기반으로 기본 회전값을 계산
            var baseRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            
            // 기준점으로부터 뒤쪽 방향으로 거리만큼 떨어진 위치를 목표 위치로 설정
            var desiredPosition = anchor - (baseRotation * Vector3.forward) * _distance;

            if (_positionSmoothTime <= 0f)
            {
                transform.position = desiredPosition;
            }
            else
            {
                // SmoothDamp를 사용하여 부드럽게 위치 이동
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref _positionVelocity,
                    _positionSmoothTime,
                    Mathf.Infinity,
                    deltaTime);
            }

            // _lookAtTarget 옵션에 따라 회전 목표 결정
            var desiredRotation = _lookAtTarget
                ? Quaternion.LookRotation(anchor - transform.position, Vector3.up)
                : baseRotation;

            transform.rotation = SmoothRotation(transform.rotation, desiredRotation, deltaTime);
        }

        private Quaternion SmoothRotation(Quaternion current, Quaternion target, float deltaTime)
        {
            if (_rotationSmoothTime <= 0f)
            {
                return target;
            }

            // 감쇠 계수를 사용한 구면 선형 보간으로 부드러운 회전 처리
            var t = 1f - Mathf.Exp(-deltaTime / _rotationSmoothTime);
            return Quaternion.Slerp(current, target, t);
        }
    }
}
