using System;
using UnityEngine;
using Common.Interface;
using MyProject.Common.Units.Locomotion;

namespace MyProject.Common.Units
{
    /// <summary>
    /// 유닛의 이동 및 물리 동작을 담당하는 partial 클래스입니다.
    /// Rigidbody 기반의 이동, 점프, 회전 로직을 처리합니다.
    /// </summary>
    public sealed partial class Unit : MonoBehaviour, IMovement, ILocomotionStateProvider, IAnimEventReceiver
    {
        [Header("Movement Settings")]
        [SerializeField] private Rigidbody _body;
        
        /// <summary>
        /// 기본 이동 속도입니다.
        /// </summary>
        [SerializeField] private float _moveSpeed = 5f;
        
        /// <summary>
        /// 점프 시 적용할 힘(속도)입니다.
        /// </summary>
        [SerializeField] private float _jumpSpeed = 5f;
        
        /// <summary>
        /// 이동 입력을 차단할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _moveBlock = false;
        
        /// <summary>
        /// 로컬 좌표계 기준으로 이동 입력을 처리할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _useLocalSpace;
        
        /// <summary>
        /// Rigidbody가 없을 경우 자동으로 추가할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _autoAddRigidbody = true;
        
        /// <summary>
        /// 물리 회전(X, Z축)을 고정할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _freezeRotation = true;
        
        /// <summary>
        /// 이동 방향을 바라보도록 회전할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _faceMovement = true;
        
        /// <summary>
        /// 공중에서 이동 제어를 허용할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _allowAirControl = false;
        
        /// <summary>
        /// 회전 속도(도/초)입니다.
        /// </summary>
        [SerializeField] private float _rotationSpeed = 720f;
        
        /// <summary>
        /// 입력 벡터의 데드존(최소 입력값)입니다.
        /// </summary>
        [SerializeField] private float _inputDeadzone = 0.01f;
        
        /// <summary>
        /// 이동 상태 판정을 위한 최소 속도입니다.
        /// </summary>
        [SerializeField] private float _speedDeadzone = 0.01f;
        
        /// <summary>
        /// 지면으로 인식할 레이어입니다.
        /// </summary>
        [SerializeField] private LayerMask _groundLayers = ~0;
        
        /// <summary>
        /// 지면 체크를 수행할 위치입니다.
        /// </summary>
        [SerializeField] private Transform _groundCheckTr;

        private Vector2 _inputVector;
        private bool _jumpRequested;
        private bool _jumpActive;
        private bool _jumpAppliedThisFrame;
        private Vector3 _velocity;
        private Vector3 _lastHorizontalVelocity;
        private LocomotionState _currentState;

        /// <inheritdoc />
        public LocomotionState CurrentState => _currentState;
        
        /// <summary>
        /// 현재 유닛의 속도입니다.
        /// </summary>
        public Vector3 Velocity => _velocity;
        
        /// <summary>
        /// 유효한 이동 입력이 있는지 여부입니다. (차단되지 않고 데드존 이상일 때)
        /// </summary>
        public bool HasMoveInput => !IsMoveBlocked() && _inputVector.sqrMagnitude > _inputDeadzone * _inputDeadzone;
        
        /// <inheritdoc />
        public event Action<LocomotionState, LocomotionState> OnLocomotionStateChanged;
        
        /// <summary>
        /// 현재 지면에 닿아있는지 여부입니다.
        /// </summary>
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
                // 비활성화 시 수평 속도 제거
                var current = _body.linearVelocity;
                _body.linearVelocity = new Vector3(0f, current.y, 0f);
            }
        }

        /// <inheritdoc />
        public void SetMoveInput(Vector2 input)
        {
            _inputVector = input;
        }

        /// <inheritdoc />
        public void RequestJump()
        {
            _jumpRequested = true;
            _jumpActive = true;
        }

        private void FixedUpdate()
        {
            if (_body == null)
            {
                return;
            }

            _jumpAppliedThisFrame = false;
            IsGrounded = CheckGrounded();

            var isMoveBlocked = IsMoveBlocked();
            if (isMoveBlocked)
            {
                _inputVector = Vector2.zero;
                _lastHorizontalVelocity = Vector3.zero;
            }

            // 입력 벡터 처리
            var moveInput = isMoveBlocked ? Vector2.zero : _inputVector;
            var move = new Vector3(moveInput.x, 0f, moveInput.y);
            if (_useLocalSpace)
            {
                move = transform.TransformDirection(move);
            }

            var current = _body.linearVelocity;
            var targetVelocity = current;
            
            // 수평 이동 적용 여부 판단 (지면이거나 공중 제어 허용 시)
            var canApplyHorizontal = IsGrounded || (_allowAirControl && !isMoveBlocked && moveInput.sqrMagnitude > _inputDeadzone * _inputDeadzone);
            if (canApplyHorizontal)
            {
                var moveSpeed = GetMoveSpeed();
                targetVelocity.x = move.x * moveSpeed;
                targetVelocity.z = move.z * moveSpeed;
                _lastHorizontalVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
            }
            else if (!IsGrounded)
            {
                // 공중 제어 불가 시 마지막 수평 속도 유지 (관성)
                if (isMoveBlocked)
                {
                    _lastHorizontalVelocity = Vector3.zero;
                }
                targetVelocity.x = _lastHorizontalVelocity.x;
                targetVelocity.z = _lastHorizontalVelocity.z;
            }

            // 점프 처리
            if (_jumpRequested && IsGrounded)
            {
                targetVelocity.y = GetJumpSpeed();
                _jumpRequested = false;
                _jumpActive = true;
                _jumpAppliedThisFrame = true;
            }

            _body.linearVelocity = targetVelocity;
            _velocity = targetVelocity;

            // 점프 중 하강 시작 시 점프 상태 해제
            if (IsGrounded && !_jumpAppliedThisFrame && _velocity.y <= 0f)
            {
                _jumpActive = false;
            }

            // 회전 처리
            var faceDirection = canApplyHorizontal
                ? move
                : new Vector3(targetVelocity.x, 0f, targetVelocity.z);
            
            // 이동 방향이 있을 때만 회전
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

            // 바닥 감지 레이캐스트
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
            var jumping = _jumpActive && _velocity.y > 0f;
            var falling = !grounded && _velocity.y <= 0f;

            var next = new LocomotionState
            {
                IsGrounded = grounded,
                IsMoving = moving,
                IsAirMoving = airMoving,
                IsJumping = jumping,
                IsFalling = falling
            };

            // 상태 변경 시 이벤트 호출
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

        /// <inheritdoc />
        public float GetMoveSpeed()
        {
            return _moveSpeed;
        }

        /// <inheritdoc />
        public float GetJumpSpeed()
        {
            return _jumpSpeed;
        }

        /// <inheritdoc />
        public void SetMoveSpeed(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        /// <inheritdoc />
        public void SetJumpSpeed(float jumpSpeed)
        {
            _jumpSpeed = jumpSpeed;
        }

        /// <inheritdoc />
        public void SetMoveBlocked(bool isBlocked)
        {
            _moveBlock = isBlocked;
        }

        /// <inheritdoc />
        public bool IsMoveBlocked()
        {
            return _moveBlock;
        }
    }
}
