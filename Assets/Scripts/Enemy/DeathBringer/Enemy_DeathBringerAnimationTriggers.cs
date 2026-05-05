using UnityEngine;

public class Enemy_DeathBringerAnimationTriggers : MonoBehaviour
{
    private Enemy_DeathBringer enemy => GetComponentInParent<Enemy_DeathBringer>();

    private void AnimationTrigger()
    {
        if (enemy != null)
            enemy.AnimationFinishTrigger();
    }

    private void AttackTrigger()
    {
        if (enemy == null || enemy.attackCheck == null)
            return;

        AudioManager.PlaySFX(SfxType.BossAttack, .75f);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackCheck.position, enemy.attackCheckRadius);
        foreach (var hit in colliders)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
                player.Damage(enemy.transform);
        }
    }

    private void MakeVisible()
    {
        if (enemy != null)
            enemy.MakeVisible(true);
    }

    private void MakeInvisible()
    {
        if (enemy != null)
            enemy.MakeVisible(false);
    }

    private void Relocate()
    {
        if (enemy != null)
            enemy.TeleportNearPlayer();
    }
}
