/// <summary>
/// RAJON — IInteractable
/// Tüm etkileşim objelerinin implement ettiği arayüz.
/// InteractionSystem bu interface üzerinden konuşur — obje tipini bilmez.
/// </summary>
public interface IInteractable
{
    /// <summary>Etkileşim türü. InteractionSystem radius seçimi için kullanır.</summary>
    InteractionType GetInteractionType();

    /// <summary>E tuşuna basılınca çağrılır.</summary>
    void Interact(PlayerController player);

    /// <summary>Toplandıktan sonra görsel efekt + Destroy. Interact içinden çağrılır.</summary>
    void OnCollected();
}

public enum InteractionType
{
    Pickup,      // geniş radius — sigara, bıçak, mermi vs.
    Interaction  // dar radius  — tavlacı dayı, nargile, araba vs.
}