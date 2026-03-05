using UnityEngine;

/// <summary>
/// RAJON — InteractionSystem
/// Emmi'nin çevresindeki IInteractable objeleri bulur.
/// PlayerController E tuşunda Interact() çağırır.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Radius")]
    [SerializeField] private float   _pickupRadius      = 1.8f;
    [SerializeField] private float   _interactionRadius = 0.8f;
    [SerializeField] private Vector2 _centerOffset      = new Vector2(0f, 0.5f); // pivot bottom için yukarı kaydır

    [Header("Layer")]
    [SerializeField] private LayerMask _interactableLayer;

    public IInteractable CurrentTarget { get; private set; }

    private readonly Collider2D[] _buffer = new Collider2D[16];

    private Vector2 Center => (Vector2)transform.position + _centerOffset;

    private void Update()
    {
        CurrentTarget = FindClosest();
         Debug.Log($"CurrentTarget: {CurrentTarget}, Center: {Center}");
    }

    private IInteractable FindClosest()
    {
        IInteractable best     = null;
        float         bestDist = float.MaxValue;

        int count = Physics2D.OverlapCircleNonAlloc(
            Center, _pickupRadius, _buffer, _interactableLayer);
            Debug.Log($"OverlapCircle buldu: {count} obje");

        for (int i = 0; i < count; i++)
        {
            var interactable = _buffer[i].GetComponent<IInteractable>();
            if (interactable == null) continue;

            float radius = interactable.GetInteractionType() == InteractionType.Pickup
                ? _pickupRadius
                : _interactionRadius;

            float dist = Vector2.Distance(Center, _buffer[i].transform.position);
            if (dist > radius) continue;
            if (dist < bestDist)
            {
                bestDist = dist;
                best     = interactable;
            }
        }

        return best;
    }

    public bool TryInteract(PlayerController player)
    {
        if (CurrentTarget == null) return false;
        CurrentTarget.Interact(player);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + _centerOffset, _pickupRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((Vector2)transform.position + _centerOffset, _interactionRadius);
    }
}