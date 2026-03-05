using System.Collections;
using UnityEngine;

/// <summary>
/// RAJON — CigarettePickup
/// Yerden toplanabilir sigara objesi.
/// amount = 1 → tek dal, amount = 4 → paket
/// </summary>
public class CigarettePickup : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private int _amount = 1;

    [Header("Sorting")]
    [SerializeField] private string _sortingLayerName  = "Default";
    [SerializeField] private int    _sortingOrder      = 5; // world objeleri üstünde

    [Header("Collect Animasyonu")]
    [SerializeField] private float _riseAmount   = 0.6f;  // world unit
    [SerializeField] private float _riseDuration = 0.4f;

    private bool _collected = false;

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder     = _sortingOrder;
        }
    }

    // -------------------------------------------------------------------------
    // IInteractable
    // -------------------------------------------------------------------------
    public InteractionType GetInteractionType() => InteractionType.Pickup;

    public void Interact(PlayerController player)
    {
        if (_collected) return;
        _collected = true;

        var heal = player.GetComponent<HealSystem>();
        if (heal != null)
            heal.AddCigarettes(_amount);

        OnCollected();
    }

    public void OnCollected()
    {
        StartCoroutine(CollectRoutine());
    }

    // -------------------------------------------------------------------------
    // Yukarı uç, solar, yok ol
    // -------------------------------------------------------------------------
    private IEnumerator CollectRoutine()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        var sr       = GetComponent<SpriteRenderer>();
        Vector3 startPos = transform.position;
        Vector3 endPos   = startPos + Vector3.up * _riseAmount;
        float   elapsed  = 0f;

        while (elapsed < _riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _riseDuration);
            float e = EaseOutQuart(t);

            transform.position = Vector3.Lerp(startPos, endPos, e);
            if (sr) sr.color = new Color(1f, 1f, 1f, 1f - e);

            yield return null;
        }

        Destroy(gameObject);
    }

    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
}