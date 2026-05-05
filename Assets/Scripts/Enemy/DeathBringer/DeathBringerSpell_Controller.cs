using UnityEngine;

public class DeathBringerSpell_Controller : MonoBehaviour
{
    [SerializeField] private Transform check;
    [SerializeField] private Vector2 boxSize = new Vector2(2.2f, 1.4f);
    [SerializeField] private LayerMask whatIsPlayer;

    private CharacterStats ownerStats;
    private bool damageApplied;

    public void SetupSpell(CharacterStats stats)
    {
        ownerStats = stats;
    }

    private void AnimationTrigger()
    {
        if (damageApplied || check == null)
            return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(check.position, boxSize, 0, whatIsPlayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            Player player = colliders[i].GetComponent<Player>();
            if (player == null)
                continue;

            int damage = ownerStats != null ? ownerStats.GetDamage() : 1;
            player.Damage(transform.position, damage);
            damageApplied = true;
        }
    }

    private void SelfDestroy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (check != null)
            Gizmos.DrawWireCube(check.position, boxSize);
    }
}
