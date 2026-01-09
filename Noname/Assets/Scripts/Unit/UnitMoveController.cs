using System;
using MergeGame.Provider;
using UnityEngine;

namespace MergeGame.Unit
{
    [DisallowMultipleComponent]
    public sealed class UnitMoveController : MonoBehaviour, ILocomotionStateProvider
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpSpeed = 5f;
        [SerializeField] private bool _useLocalSpace;
        [SerializeField] private bool _autoAddRigidbody = true;
        [SerializeField] private bool _freezeRotation = true;
        [SerializeField] private bool _faceMovement = true;
        [SerializeField] private bool _allowAirControl = false;
        [SerializeField] private float _rotationSpeed = 720f;
        [SerializeField] private float _inputDeadzone = 0.01f;
        [SerializeField] private float _speedDeadzone = 0.01f;
        [SerializeField] private LayerMask _groundLayers = ~0;
        [SerializeField] private Transform _groundCheckTr;

        private Vector2 _inputVector;
        private bool _jumpRequested;
        private Vector3 _velocity;
        private Vector3 _lastHorizontalVelocity;
        private LocomotionState _currentState;

        public LocomotionState CurrentState => _currentState;
        public Vector3 Velocity => _velocity;
        public bool HasMoveInput => _inputVector.sqrMagnitude > _inputDeadzone * _inputDeadzone;
        public event Action<LocomotionState, LocomotionState> OnLocomotionStateChanged;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            if (_body == null)
            {
                _body = GetComponent<Rigidbody>();
            }

            if (_body == null && _autoAddRigidbody)
            {
                _body = gameObject.AddComponent<Rigidbody>();
            }

            if (_body != null && _freezeRotation)
            {
                _body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        private void OnDisable()
        {
            _inputVector = Vector2.zero;
            _jumpRequested = false;
            if (_body != null)
            {
                var current = _body.linearVelocity;
                _body.linearVelocity = new Vector3(0f, current.y, 0f);
            }
        }

        public void SetMoveInput(Vector2 input)
        {
            _inputVector = input;
        }

        public void RequestJump()
        {
            _jumpRequested = true;
        }

        private void FixedUpdate()
        {
            if (_body == null)
            {
                return;
            }

            IsGrounded = CheckGrounded();

            var move = new Vector3(_inputVector.x, 0f, _inputVector.y);
            if (_useLocalSpace)
            {
                move = transform.TransformDirection(move);
            }

            var current = _body.linearVelocity;
            var targetVelocity = current;
            var canApplyHorizontal = IsGrounded || (_allowAirControl && HasMoveInput);
            if (canApplyHorizontal)
            {
                targetVelocity.x = move.x * _moveSpeed;
                targetVelocity.z = move.z * _moveSpeed;
                _lastHorizontalVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
            }
            else if (!IsGrounded)
            {
                targetVelocity.x = _lastHorizontalVelocity.x;
                targetVelocity.z = _lastHorizontalVelocity.z;
            }

            if (_jumpRequested && IsGrounded)
            {
                targetVelocity.y = _jumpSpeed;
                _jumpRequested = false;
            }

            _body.linearVelocity = targetVelocity;
            _velocity = targetVelocity;

            var faceDirection = canApplyHorizontal
                ? move
                : new Vector3(targetVelocity.x, 0f, targetVelocity.z);
            if (_faceMovement && faceDirection.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(faceDirection.normalized, Vector3.up);
                var nextRotation = _rotationSpeed > 0f
                    ? Quaternion.RotateTowards(_body.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime)
                    : targetRotation;
                _body.MoveRotation(nextRotation);
            }

            SyncState();
        }

        private bool CheckGrounded()
        {
            if (_groundCheckTr == null)
            {
                return false;
            }

            var origin = _groundCheckTr.position;

            var distanceToGround = 0.1f;

            return Physics.Raycast(origin
            , Vector3.down
            , out _
            , distanceToGround
            , _groundLayers);
        }

        private void SyncState()
        {
            var previous = _currentState;
            var horizontalSpeed = new Vector2(_velocity.x, _velocity.z).magnitude;
            var hasSpeed = horizontalSpeed > _speedDeadzone;
            var moveIntent = HasMoveInput;
            var grounded = IsGrounded;

            var moving = grounded && moveIntent && hasSpeed;
            var airMoving = !grounded && moveIntent && hasSpeed;
            var jumping = !grounded && _velocity.y > 0f;
            var falling = !grounded && _velocity.y <= 0f;

            var next = new LocomotionState
            {
                IsGrounded = grounded,
                IsMoving = moving,
                IsAirMoving = airMoving,
                IsJumping = jumping,
                IsFalling = falling
            };

            if (!LocomotionStateEquals(previous, next))
            {
                _currentState = next;
                OnLocomotionStateChanged?.Invoke(previous, next);
            }
            else
            {
                _currentState = next;
            }
        }

        private static bool LocomotionStateEquals(LocomotionState a, LocomotionState b)
        {
            return a.IsGrounded == b.IsGrounded
                && a.IsMoving == b.IsMoving
                && a.IsAirMoving == b.IsAirMoving
                && a.IsJumping == b.IsJumping
                && a.IsFalling == b.IsFalling;
        }
    }
}
