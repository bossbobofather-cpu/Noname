using TMPro;
using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// 월드 오브젝트 위에 이름이나 설명을 표시하는 라벨입니다.
    /// 에디터 모드에서도 동작하도록 ExecuteAlways가 설정되어 있습니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class WorldObjectLabel : MonoBehaviour
    {
        [Header("Text")]
        /// <summary>
        /// 표시할 제목 텍스트입니다.
        /// </summary>
        [SerializeField] private string _title;
        
        /// <summary>
        /// 표시할 설명 텍스트입니다.
        /// </summary>
        [SerializeField, TextArea(2, 4)] private string _description;

        [Header("Appearance")]
        /// <summary>
        /// 텍스트에 사용할 폰트 에셋입니다.
        /// </summary>
        [SerializeField] private TMP_FontAsset _font;
        
        /// <summary>
        /// 제목 색상입니다.
        /// </summary>
        [SerializeField] private Color _titleColor = Color.white;
        
        /// <summary>
        /// 설명 색상입니다.
        /// </summary>
        [SerializeField] private Color _descriptionColor = Color.white;
        
        /// <summary>
        /// 제목 크기입니다.
        /// </summary>
        [SerializeField] private float _titleSize = 1.6f;
        
        /// <summary>
        /// 설명 크기입니다.
        /// </summary>
        [SerializeField] private float _descriptionSize = 1.1f;
        
        /// <summary>
        /// 줄 간격입니다.
        /// </summary>
        [SerializeField] private float _lineSpacing;
        
        /// <summary>
        /// 텍스트 최대 너비입니다. 0보다 크면 자동 줄바꿈이 적용됩니다.
        /// </summary>
        [SerializeField] private float _maxWidth;
        
        /// <summary>
        /// 제목을 굵게 표시할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _boldTitle = true;

        [Header("Layout")]
        /// <summary>
        /// 라벨들을 담을 루트 트랜스폼입니다.
        /// </summary>
        [SerializeField] private Transform _labelRoot;
        
        /// <summary>
        /// 루트의 오프셋입니다.
        /// </summary>
        [SerializeField] private Vector3 _rootOffset = new Vector3(0f, 2f, 0f);
        
        /// <summary>
        /// 제목 라벨의 로컬 오프셋입니다.
        /// </summary>
        [SerializeField] private Vector3 _titleOffset = Vector3.zero;
        
        /// <summary>
        /// 설명 라벨의 로컬 오프셋입니다.
        /// </summary>
        [SerializeField] private Vector3 _descriptionOffset = new Vector3(0f, -0.4f, 0f);

        [Header("Facing")]
        /// <summary>
        /// 카메라를 바라볼지 여부입니다.
        /// </summary>
        [SerializeField] private bool _faceCamera = true;
        
        /// <summary>
        /// Y축 회전만 적용할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _lockYRotation = true;
        
        /// <summary>
        /// 바라볼 카메라입니다. 없으면 MainCamera를 사용합니다.
        /// </summary>
        [SerializeField] private Camera _camera;

        [Header("References")]
        [SerializeField] private TextMeshPro _titleLabel;
        [SerializeField] private TextMeshPro _descriptionLabel;

        private void Awake()
        {
            EnsureLabels();
            ApplyLabels();
        }

        private void OnValidate()
        {
            EnsureLabels();
            ApplyLabels();
        }

        private void LateUpdate()
        {
            if (_faceCamera)
            {
                FaceCamera();
            }
        }

        private void EnsureLabels()
        {
            if (_labelRoot == null)
            {
                var rootObject = new GameObject("WorldLabelRoot");
                rootObject.transform.SetParent(transform, false);
                _labelRoot = rootObject.transform;
            }

            if (_titleLabel == null)
            {
                _titleLabel = CreateLabel("TitleLabel", _labelRoot);
            }

            if (_descriptionLabel == null)
            {
                _descriptionLabel = CreateLabel("DescriptionLabel", _labelRoot);
            }
        }

        private TextMeshPro CreateLabel(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var label = obj.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.richText = true;
            return label;
        }

        private void ApplyLabels()
        {
            if (_labelRoot != null)
            {
                _labelRoot.localPosition = _rootOffset;
            }

            ApplyLabel(_titleLabel, _title, _titleSize, _titleColor, _titleOffset, _boldTitle);
            ApplyLabel(_descriptionLabel, _description, _descriptionSize, _descriptionColor, _descriptionOffset, false);
        }

        private void ApplyLabel(TextMeshPro label, string text, float size, Color color, Vector3 localOffset, bool bold)
        {
            if (label == null)
            {
                return;
            }

            label.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (_font != null)
            {
                label.font = _font;
            }

            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.lineSpacing = _lineSpacing;
            label.transform.localPosition = localOffset;

            if (_maxWidth > 0f)
            {
                label.textWrappingMode = TextWrappingModes.Normal;
                var rect = label.rectTransform;
                if (rect != null)
                {
                    var sizeDelta = rect.sizeDelta;
                    sizeDelta.x = _maxWidth;
                    rect.sizeDelta = sizeDelta;
                }
            }
            else
            {
                label.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        private void FaceCamera()
        {
            if (_labelRoot == null)
            {
                return;
            }

            var camera = _camera != null ? _camera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var direction = camera.transform.position - _labelRoot.position;
            if (_lockYRotation)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            _labelRoot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
