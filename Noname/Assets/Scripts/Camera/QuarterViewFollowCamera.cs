using UnityEngine;

namespace MergeGame.CameraSystem
{
    [DisallowMultipleComponent]
    public sealed class QuarterViewFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField] private float _distance = 8f;
        [SerializeField, Range(-89f, 89f)] private float _pitch = 35f;
        [SerializeField, Range(0f, 360f)] private float _yaw = 45f;
        [SerializeField] private float _positionSmoothTime = 0.1f;
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        [SerializeField] private bool _lookAtTarget = true;
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

            var anchor = _target.position + _targetOffset;
            var baseRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var desiredPosition = anchor - (baseRotation * Vector3.forward) * _distance;

            if (_positionSmoothTime <= 0f)
            {
                transform.position = desiredPosition;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref _positionVelocity,
                    _positionSmoothTime,
                    Mathf.Infinity,
                    deltaTime);
            }

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

            var t = 1f - Mathf.Exp(-deltaTime / _rotationSmoothTime);
            return Quaternion.Slerp(current, target, t);
        }
    }
}
