using UnityEngine;
using Unity6.LavosTrial.HUD;

namespace Code.Lavos.Core
{
    /// <summary>
    /// COLLECTIBLE — Objet ramassable (pièce, potion, bonus…)
    ///
    /// SETUP dans Unity :
    ///  1. Crée un GameObject (ex: une sphère)
    ///  2. Attache ce script dessus
    ///  3. Ajoute un Collider en mode "Is Trigger"
    ///  4. Choisis le type dans l'Inspector
    /// </summary>
    public class Collectible : MonoBehaviour
    {
    public enum CollectibleType { Score, Health, Mana, Stamina }

    [Header("Type et valeur")]
    [SerializeField] private CollectibleType type = CollectibleType.Score;
    [SerializeField] private float value = 50f;
    [SerializeField] private string popupMsg = "+50 pts";

    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Effets")]
    [SerializeField] private GameObject collectVFX;

    private Vector3 _startPosition;

    void Start() => _startPosition = transform.position;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect(other.gameObject);
    }

    private void Collect(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();

        switch (type)
        {
            case CollectibleType.Score:
                GameManager.Instance?.AddScore((int)value);
                break;

            case CollectibleType.Health:
                stats?.Heal(value);
                break;

            case CollectibleType.Mana:
                stats?.RestoreMana(value);
                break;

            case CollectibleType.Stamina:
                stats?.RestoreStamina(value);
                break;
        }

        if (!string.IsNullOrEmpty(popupMsg))
            HUDSystem.Instance?.ShowPopup(popupMsg);

        if (collectVFX != null)
            Instantiate(collectVFX, transform.position, Quaternion.identity);

        Debug.Log($"[Collectible] RamassÃ© : {type} +{value}");
        Destroy(gameObject);
    }
}
}
