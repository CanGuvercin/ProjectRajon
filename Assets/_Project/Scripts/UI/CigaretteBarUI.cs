using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CigaretteBarUI : MonoBehaviour
{
    [Header("Sistem")]
    [SerializeField] private HealSystem _healSystem;

    [Header("Sigara Slotları (soldan sağa: S1→S4)")]
    [SerializeField] private RectTransform[] _slots;

    [Header("CigaretteUI Pozisyonları")]
    [SerializeField] private Vector2 _defaultPos = new Vector2(68f, -10f);
    [SerializeField] private Vector2 _openPos    = new Vector2(68f,  59f);

    [Header("Sigara Çıkış")]
    [SerializeField] private float _cigaretteRiseAmount = 80f;
    [SerializeField] private float _riseDuration        = 0.45f;
    [SerializeField] private float _returnDuration      = 0.2f;

    [Header("Pickup Drop")]
    [SerializeField] private float _dropOffsetY  = 40f;
    [SerializeField] private float _dropDuration = 0.25f;

    private RectTransform _rt;
    private int           _currentCount;
    private Vector2[]     _slotHomePositions;
    private bool          _returnListenerRegistered = false; // birikme önleme

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _slotHomePositions = new Vector2[_slots.Length];
        for (int i = 0; i < _slots.Length; i++)
            _slotHomePositions[i] = _slots[i].anchoredPosition;

        _rt.anchoredPosition = _defaultPos;

        if (_healSystem == null) return;
        _currentCount = _healSystem.CurrentCigarettes;
        RefreshSlots();
    }

    private void OnEnable()
    {
        if (_healSystem == null) return;
        _healSystem.OnHealStarted       += OnHealStarted;
        _healSystem.OnCigarettesChanged += OnCigarettesChanged;
    }

    private void OnDisable()
    {
        if (_healSystem == null) return;
        _healSystem.OnHealStarted       -= OnHealStarted;
        _healSystem.OnCigarettesChanged -= OnCigarettesChanged;

        // Disable sırasında da temizle
        if (_returnListenerRegistered)
        {
            _healSystem.OnHealFinished -= ReturnToDefault;
            _returnListenerRegistered = false;
        }
    }

    private void OnHealStarted()
    {
        StartCoroutine(HealSequence());
    }

    private void OnCigarettesChanged(int current, int max)
    {
        bool pickedUp = current > _currentCount;
        _currentCount = current;
        if (pickedUp)
            StartCoroutine(PickupSequence(current));
    }

    private int GetExitSlotIndex()
    {
        for (int i = _slots.Length - 1; i >= 0; i--)
            if (_slots[i].gameObject.activeSelf) return i;
        return -1;
    }

    private IEnumerator HealSequence()
    {
        int exitIndex = GetExitSlotIndex();
        if (exitIndex < 0) yield break;

        StartCoroutine(ExitCigarette(exitIndex));
        yield return StartCoroutine(MovePacket(_openPos, _riseDuration));

        // Önceki listener varsa önce temizle — birikme önleme
        if (_returnListenerRegistered)
        {
            _healSystem.OnHealFinished -= ReturnToDefault;
            _returnListenerRegistered = false;
        }

        _healSystem.OnHealFinished += ReturnToDefault;
        _returnListenerRegistered = true;
    }

    private IEnumerator PickupSequence(int newCount)
    {
        yield return StartCoroutine(MovePacket(_openPos, _riseDuration));

        for (int i = 0; i < newCount && i < _slots.Length; i++)
        {
            if (_slots[i].gameObject.activeSelf) continue;

            var slot = _slots[i];
            var img  = slot.GetComponent<Image>();
            if (img == null) continue;

            Vector2 endPos   = _slotHomePositions[i];
            Vector2 startPos = endPos + Vector2.up * _dropOffsetY;

            slot.anchoredPosition = startPos;
            img.color = new Color(1f, 1f, 1f, 0f);
            slot.gameObject.SetActive(true);

            yield return StartCoroutine(DropSlot(slot, img, startPos, endPos, _dropDuration));
        }

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(MovePacket(_defaultPos, _returnDuration));
    }

    private void ReturnToDefault()
    {
        _healSystem.OnHealFinished -= ReturnToDefault;
        _returnListenerRegistered = false;
        StartCoroutine(MovePacket(_defaultPos, _returnDuration));
    }

    private IEnumerator MovePacket(Vector2 target, float duration)
    {
        Vector2 start   = _rt.anchoredPosition;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _rt.anchoredPosition = Vector2.Lerp(start, target, EaseOutQuart(t));
            yield return null;
        }

        _rt.anchoredPosition = target;
    }

    private IEnumerator ExitCigarette(int index)
    {
        if (_slotHomePositions == null) yield break;
        if (index < 0 || index >= _slots.Length) yield break;

        var rt  = _slots[index];
        var img = rt.GetComponent<Image>();
        if (img == null) yield break;

        rt.anchoredPosition = _slotHomePositions[index];
        img.color = Color.white;

        Vector2 startPos = _slotHomePositions[index];
        Vector2 endPos   = startPos + Vector2.up * _cigaretteRiseAmount;
        float   elapsed  = 0f;

        while (elapsed < _riseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _riseDuration);
            float e = EaseOutQuart(t);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, e);
            img.color = new Color(1f, 1f, 1f, 1f - e);
            yield return null;
        }

        rt.gameObject.SetActive(false);
        rt.anchoredPosition = startPos;
        img.color = Color.white;
    }

    private IEnumerator DropSlot(RectTransform rt, Image img, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = EaseOutQuart(t);
            rt.anchoredPosition = Vector2.Lerp(from, to, e);
            img.color = new Color(1f, 1f, 1f, e);
            yield return null;
        }
        rt.anchoredPosition = to;
        img.color = Color.white;
    }

    private void RefreshSlots()
    {
        if (_slotHomePositions == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            bool active = i < _currentCount;
            _slots[i].gameObject.SetActive(active);
            if (active)
            {
                _slots[i].anchoredPosition = _slotHomePositions[i];
                var img = _slots[i].GetComponent<Image>();
                if (img) img.color = Color.white;
            }
        }
    }

    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private static float EaseInQuart(float t)  => t * t * t * t;
}