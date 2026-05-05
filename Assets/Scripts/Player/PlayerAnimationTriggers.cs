using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
   private Player player=>GetComponentInParent<Player>();

    [SerializeField] private GameObject hitImpactPrefab;
    [SerializeField] private Vector2 hitImpactOffset = new Vector2(0, .75f);

    private void AnimationTrigger()
    {
        player.AnimationFinishTrigger();
    }
    private void AttackTrigger()
    {
        AudioManager.PlaySFX(SfxType.PlayerAttack, .8f);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
        foreach (var hit in colliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if(enemy!=null)
            {
                enemy.Damage(player.transform);
                CreateHitImpact(enemy.transform);
            }
        }
    }
    private void ThrowSword()
    {
        SkillManager.instance.sword.CreateSword();
    }

    private void CreateHitImpact(Transform target)
    {
        if (hitImpactPrefab == null || target == null)
            return;

        Vector3 offset = new Vector3(hitImpactOffset.x * player.facingDir, hitImpactOffset.y, 0);
        GameObject impact = Instantiate(hitImpactPrefab, target.position + offset, Quaternion.identity);

        if (target.position.x < player.transform.position.x)
            impact.transform.localScale = new Vector3(-Mathf.Abs(impact.transform.localScale.x), impact.transform.localScale.y, impact.transform.localScale.z);
    }
}
