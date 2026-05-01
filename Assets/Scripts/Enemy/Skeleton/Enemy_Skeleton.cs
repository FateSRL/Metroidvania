using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Skeleton : Enemy
{

    #region State
    public SkeletonIdleState idleState { get; private set; }
    public SkeletonMoveState moveState { get; private set; }
    public SkeletonBattleState battleState { get; private set; }
    public SkeletonAttackState attackState { get; private set; }
    public SkeletonStunnedState stunnedState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        idleState = new SkeletonIdleState(this, stateMachine, "Idle", this);
        moveState = new SkeletonMoveState(this, stateMachine, "Move", this);
        battleState = new SkeletonBattleState(this, stateMachine, "Move", this);
        attackState = new SkeletonAttackState(this, stateMachine, "Attack", this);
        stunnedState = new SkeletonStunnedState(this, stateMachine, "Stun", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        // if(Input.GetKeyDown(KeyCode.U)){
        //     stateMachine.ChangeState(stunnedState);
        // }
    }

    public override void Damage(Transform damageSource)
    {
        base.Damage(damageSource);
        RefreshBattleTime(PlayerManager.instance.player.transform);
    }

    public override void Damage(Vector2 damageSourcePosition)
    {
        base.Damage(damageSourcePosition);
        RefreshBattleTime(PlayerManager.instance.player.transform);
    }

    public override void TakeBlackholeDamage()
    {
        base.TakeBlackholeDamage();
        RefreshBattleTime(PlayerManager.instance.player.transform);
    }

    public void RefreshBattleTime(Transform target)
    {
        RefreshBattleTime(target.position);
    }

    public void RefreshBattleTime(Vector2 targetPosition)
    {
        battleState.RefreshBattleTime(targetPosition);

        if (stateMachine.currentState == idleState || stateMachine.currentState == moveState)
            stateMachine.ChangeState(battleState);
    }

    public override bool CanBeStunned()
    {
        if(base.CanBeStunned()){
            stateMachine.ChangeState(stunnedState);
            return true;
        }
        return false;
    }
}
