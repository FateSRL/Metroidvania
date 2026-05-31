using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCounterAttackState : PlayerState
{
    public PlayerCounterAttackState(Player _playerBase, PlayerStateMachine _stateMachine, string _animName) : base(_playerBase, _stateMachine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // player.SetVelocity(0,0);
        stateTimer = player.counterAttackDuration;
        player.anim.SetBool("SuccessfulCounterAttack", false);

    }
    public override void Update()
    {
        base.Update();
        player.ZeroVelocity();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                if(hit.GetComponent<Enemy>().CanBeStunned()){
                    stateTimer=10;
                    player.anim.SetBool("SuccessfulCounterAttack",true);
                    DamageCounteredEnemy(hit.GetComponent<Enemy>());
                    break;
                }
            }
        }

        if(stateTimer < 0||triggerCalled) 
        {       
            stateMachine.ChangeState(player.idleState);
        }
    }
    public override void Exit()
    {
        base.Exit();
    }

    private void DamageCounteredEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        int normalAttackDamage = player.stats != null ? player.stats.GetDamage() : 1;
        enemy.Damage(player.transform.position, Mathf.Max(1, normalAttackDamage * 2));
    }
}
