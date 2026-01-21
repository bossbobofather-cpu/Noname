using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 보드 상의 슬롯 정보를 담는 컴포넌트입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MergeGameSlot : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;
        [SerializeField] private CapsuleCollider _slotCollider;
        [SerializeField] private float _gizmoSize = 0.6f;
        private MergeGameUnit _occupant;

        /// <summary>
        /// 슬롯에 배치된 유닛입니다.
        /// </summary>
        public MergeGameUnit Occupant => _occupant;

        /// <summary>
        /// 슬롯이 비어있는지 여부입니다.
        /// </summary>
        public bool IsEmpty => _occupant == null;

        /// <summary>
        /// 유닛 배치 기준 위치입니다.
        /// </summary>
        public Vector3 Position => _anchor != null ? _anchor.position : transform.position;


        private void Awake()
        {
            if (_slotCollider != null)
            {
                //런타임에서 MeshRender 안보이도록하기 위해
                //배치 단계에서 위치잡는용으로만쓴다.
                var mr = _slotCollider.GetComponent<MeshRenderer>();
                if(mr) mr.enabled = false;
            }
        }

        private void OnDrawGizmos()
        {
            // 슬롯 위치를 배치 단계에서 확인하기 위한 표시.
            MergeGameGizmoUtility.DrawSlot(Position, _gizmoSize);
        }

        /// <summary>
        /// 슬롯에 유닛을 배치합니다.
        /// </summary>
        public void PlaceUnit(MergeGameUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            _occupant = unit;
            unit.SetSlot(this);
            unit.transform.position = Position;
        }

        /// <summary>
        /// 슬롯의 유닛 정보를 비웁니다.
        /// </summary>
        public void ClearUnit()
        {
            if (_occupant != null && _occupant.CurrentSlot == this)
            {
                _occupant.ClearSlot();
            }

            _occupant = null;
        }
    }
}
