using TMPro;
using UnityEngine;

namespace MyProject.Common.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class WorldObjectLabel : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private string _title;
        [SerializeField, TextArea(2, 4)] private string _description;

        [Header("Appearance")]
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private Color _titleColor = Color.white;
        [SerializeField] private Color _descriptionColor = Color.white;
        [SerializeField] private float _titleSize = 1.6f;
        [SerializeField] private float _descriptionSize = 1.1f;
        [SerializeField] private float _lineSpacing;
        [SerializeField] private float _maxWidth;
        [SerializeField] private bool _boldTitle = true;

        [Header("Layout")]
        [SerializeField] private Transform _labelRoot;
        [SerializeField] private Vector3 _rootOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private Vector3 _titleOffset = Vector3.zero;
        [SerializeField] private Vector3 _descriptionOffset = new Vector3(0f, -0.4f, 0f);

        [Header("Facing")]
        [SerializeField] private bool _faceCamera = true;
        [SerializeField] private bool _lockYRotation = true;
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
