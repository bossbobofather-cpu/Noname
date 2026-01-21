using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 유닛을 드래그하여 슬롯에 배치/머지하는 로직입니다.
    /// </summary>
    public sealed class MergeGameUnitDrag : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _unitLayer = ~0;
        [SerializeField] private LayerMask _slotLayer = ~0;
        [SerializeField] private float _dragPlaneHeight = 0f;

        private MergeGameUnit _unit;
        private MergeGameSlot _currentSlot;
        private Vector3 _startPosition;
        private Vector3 _dragOffset;
        private bool _dragging;
        private Plane _dragPlane;

        private static MergeGameUnitDrag _activeDrag;

        /// <summary>
        /// 드래그 대상 유닛을 초기화합니다.
        /// </summary>
        public void Initialize(MergeGameUnit unit)
        {
            _unit = unit;
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        /// <summary>
        /// 현재 슬롯 정보를 전달합니다.
        /// </summary>
        public void SetSlot(MergeGameSlot slot)
        {
            _currentSlot = slot;
        }

        private void Update()
        {
            if (_unit == null || _camera == null)
            {
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var pointerPos = mouse.position.ReadValue();

            if (!_dragging)
            {
                if (_activeDrag != null && _activeDrag != this)
                {
                    return;
                }

                if (mouse.leftButton.wasPressedThisFrame && IsPointerOnUnit(pointerPos))
                {
                    BeginDrag(pointerPos);
                }

                return;
            }

            if (mouse.leftButton.isPressed)
            {
                UpdateDrag(pointerPos);
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                EndDrag(pointerPos);
            }
        }

        private bool IsPointerOnUnit(Vector2 screenPosition)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, 200f, _unitLayer, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return hit.collider != null && hit.collider.transform.IsChildOf(transform);
        }

        private void BeginDrag(Vector2 screenPosition)
        {
            _activeDrag = this;
            _dragging = true;
            _startPosition = transform.position;

            // 드래그 평면을 유닛 높이 기준으로 설정한다.
            var planeY = _currentSlot != null ? _currentSlot.Position.y : _dragPlaneHeight;
            _dragPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

            // 오프셋을 계산해 자연스럽게 이동시킨다.
            _dragOffset = transform.position - GetPointerWorldPosition(screenPosition);

            // 드래그 중에는 슬롯을 비운다.
            if (_currentSlot != null)
            {
                _currentSlot.ClearUnit();
            }
        }

        private void UpdateDrag(Vector2 screenPosition)
        {
            var world = GetPointerWorldPosition(screenPosition);
            transform.position = world + _dragOffset;
        }

        private void EndDrag(Vector2 screenPosition)
        {
            _dragging = false;
            _activeDrag = null;

            // 드롭 지점을 확인한다.
            var slot = GetSlotFromPointer(screenPosition);
            if (slot != null && _unit.Board != null)
            {
                if (_unit.Board.TryPlaceOrMerge(_unit, slot))
                {
                    return;
                }
            }

            // 실패 시 원래 슬롯으로 되돌린다.
            if (_currentSlot != null)
            {
                _currentSlot.PlaceUnit(_unit);
            }
            else
            {
                transform.position = _startPosition;
            }
        }

        private MergeGameSlot GetSlotFromPointer(Vector2 screenPosition)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, 200f, _slotLayer, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider != null ? hit.collider.GetComponentInParent<MergeGameSlot>() : null;
        }

        private Vector3 GetPointerWorldPosition(Vector2 screenPosition)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            if (_dragPlane.Raycast(ray, out var distance))
            {
                return ray.GetPoint(distance);
            }

            return transform.position;
        }
    }
}
