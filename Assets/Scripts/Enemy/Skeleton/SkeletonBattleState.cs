using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonBattleState : EnemyState 
{
    private Enemy_Skeleton enemy;
    private Transform player;
    private Vector2 lastKnownPlayerPosition;
    private int moveDir;
    public SkeletonBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }
    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.instance.player.transform;
        RefreshBattleTime(player.position);
    }
    public override void Update()
    {
        base.Update();

        RaycastHit2D playerDetected = enemy.IsPlayerDetected();

        if(playerDetected)
        {
            RefreshBattleTime(player.position);

            if(playerDetected.distance < enemy.attackDistance)
            {
                if(CanAttack())
                {
                    stateMachine.ChangeState(enemy.attackState); 
                    return;
                }
            }
        }

        if(stateTimer < 0)
        {
            enemy.ZeroVelocity();
            stateMachine.ChangeState(enemy.idleState); 
            return;
        }

        if(Vector2.Distance(lastKnownPlayerPosition, enemy.transform.position) < .2f && !playerDetected)
        {
            enemy.ZeroVelocity();
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if(lastKnownPlayerPosition.x > enemy.transform.position.x)
        {
            moveDir = 1;
        }
        else if(lastKnownPlayerPosition.x < enemy.transform.position.x)
        {
            moveDir = -1;
        }

        enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
        
    }
    public override void Exit()
    {
        base.Exit();
    }

    public void RefreshBattleTime(Vector2 targetPosition)
    {
        stateTimer = enemy.battleTime;
        lastKnownPlayerPosition = targetPosition;
    }

    private bool CanAttack()
    {
        if (Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown)
        {
            return true;
        }
        return false;
    }
}
