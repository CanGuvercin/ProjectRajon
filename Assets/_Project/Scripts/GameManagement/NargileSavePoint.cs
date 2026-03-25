using UnityEngine;

/// <summary>
/// RAJON — NargileSavePoint
/// Nargile noktasına Y basınca oyun kaydedilir.
/// Her nargile noktasının unique ID'si olmalı.
/// </summary>
public class NargileSavePoint : MonoBehaviour, IInteractable
{
    [Header("Checkpoint Ayarları")]
    [SerializeField] private string _checkpointID;
    [SerializeField] private Transform _respawnPoint;
    
    [Header("Görsel Feedback")]
    [SerializeField] private GameObject _smokeEffect;
    [SerializeField] private SpriteRenderer _nargileSpriteRenderer;
    [SerializeField] private Color _activeColor = Color.yellow;
    
    [Header("Ses")]
    [SerializeField] private AudioClip _saveSound;
    
    private bool _isActive = false;
    private Color _originalColor;
    
    private void Awake()
    {
        // Checkpoint ID otomatik oluştur (eğer boşsa)
        if (string.IsNullOrEmpty(_checkpointID))
        {
            _checkpointID = $"nargile_{transform.position.x:F0}_{transform.position.y:F0}";
        }
        
        if (_nargileSpriteRenderer != null)
        {
            _originalColor = _nargileSpriteRenderer.color;
        }
        
        // Respawn point yoksa kendi pozisyonunu kullan
        if (_respawnPoint == null)
        {
            _respawnPoint = transform;
        }
    }
    
    private void Start()
    {
        // Bu checkpoint son kullanılan mı?
        if (GameManager.Instance != null && 
            GameManager.Instance.Data.lastCheckpointID == _checkpointID)
        {
            ActivateVisuals();
        }
    }
    
    // -------------------------------------------------------------------------
    // IInteractable Implementation
    // -------------------------------------------------------------------------
    public void Interact(PlayerController player)
    {
        SaveGame();
    }
    
    public InteractionType GetInteractionType()
    {
        return InteractionType.Interaction; // Dar radius - nargileye yakın olmalı
    }
    
    public void OnCollected()
    {
        // Nargile toplanmaz, sadece etkileşilir - boş bırak
    }
    
    // -------------------------------------------------------------------------
    // Kaydetme
    // -------------------------------------------------------------------------
    private void SaveGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[NargileSavePoint] GameManager bulunamadı!");
            return;
        }
        
        // Checkpoint kaydet
        Vector2 respawnPos = _respawnPoint.position;
        GameManager.Instance.SaveCheckpoint(_checkpointID, respawnPos);
        
        // Görsel feedback
        ActivateVisuals();
        
        // Ses
        if (_saveSound != null)
        {
            AudioSource.PlayClipAtPoint(_saveSound, transform.position);
        }
        
        // Duman efekti
        if (_smokeEffect != null)
        {
            _smokeEffect.SetActive(true);
            // 2 saniye sonra kapat
            Invoke(nameof(DisableSmoke), 2f);
        }
        
        Debug.Log($"[NargileSavePoint] Oyun kaydedildi: {_checkpointID}");
    }
    
    private void ActivateVisuals()
    {
        _isActive = true;
        
        if (_nargileSpriteRenderer != null)
        {
            _nargileSpriteRenderer.color = _activeColor;
        }
    }
    
    private void DisableSmoke()
    {
        if (_smokeEffect != null)
        {
            _smokeEffect.SetActive(false);
        }
    }
    
    // -------------------------------------------------------------------------
    // Gizmos (Editor'da görselleştirme)
    // -------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (_respawnPoint != null && _respawnPoint != transform)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _respawnPoint.position);
            Gizmos.DrawWireSphere(_respawnPoint.position, 0.3f);
        }
    }
}