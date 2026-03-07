// Ennemi.cs
// Basic enemy behavior - deals damage on collision
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Simple enemy that attaches to any GameObject with a Collider
// Deals damage when colliding with the player

using UnityEngine;

namespace Code.Lavos.Core
{
    public class Ennemi : MonoBehaviour
    {
    [Header("Combat")]
    [SerializeField] private float damage = 20f;

    private void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        var stats = col.gameObject.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning($"[Ennemi] {gameObject.name} : pas de PlayerStats sur le joueur.");
            return;
        }
        stats.TakeDamage(damage);
    }
    }
}
