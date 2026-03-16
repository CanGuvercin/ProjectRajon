using UnityEngine;

public class ItemGlow : MonoBehaviour
{
    [Header("Renk")]
    [SerializeField] private Color _colorA = new Color(0.6f, 0.5f, 0.8f, 0.8f);
    [SerializeField] private Color _colorB = new Color(0.4f, 0.4f, 0.5f, 0.3f);

    [Header("Hız")]
    [SerializeField] private float _pulseSpeed  = 1.8f;
    [SerializeField] private float _scaleSpeed  = 1.2f;
    [SerializeField] private float _scaleMin    = 0.85f;
    [SerializeField] private float _scaleMax    = 1.1f;

    [Header("Hareket Algılama")]
    [SerializeField] private float _moveThreshold = 0.01f;

    private SpriteRenderer _sr;
    private Vector3        _baseScale;
    private Vector3        _lastParentPos;
    private Transform      _parent;

    private void Awake()
    {
        _sr         = GetComponent<SpriteRenderer>();
        _baseScale  = transform.localScale;
        _parent     = transform.parent;
        
        if (_parent != null)
            _lastParentPos = _parent.position;
    }

    private void Update()
    {
        // Parent hareket ettiyse kapat
        if (_parent != null)
        {
            float dist = Vector3.Distance(_parent.position, _lastParentPos);
            if (dist > _moveThreshold)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;

        if (_sr) _sr.color = Color.Lerp(_colorB, _colorA, t);

        float s = Mathf.Lerp(_scaleMin, _scaleMax, t);
        transform.localScale = _baseScale * s;
    }
}