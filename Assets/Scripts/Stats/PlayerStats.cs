using System.Collections;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    [SerializeField] private float invincibilityDuration = 1f;

    private Player player;
    private Coroutine invincibilityCoroutine;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    public override bool TakeDamage(int damageAmount)
    {
        bool damageTaken = base.TakeDamage(damageAmount);

        if (damageTaken && !isDead)
        {
            if (invincibilityCoroutine != null)
                StopCoroutine(invincibilityCoroutine);

            invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
        }

        return damageTaken;
    }

    protected override void Die()
    {
        base.Die();

        if (player != null)
            player.Die();
    }

    private IEnumerator InvincibilityCoroutine()
    {
        MakeInvincible(true);
        yield return new WaitForSeconds(invincibilityDuration);
        MakeInvincible(false);
        invincibilityCoroutine = null;
    }
}
