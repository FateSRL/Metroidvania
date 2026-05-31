using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Skeleton : Enemy
{
    [Header("Leash info")]
    [SerializeField] private bool useHorizontalLeash;
    [SerializeField] private float leftLeashDistance = 5;
    [SerializeField] private float rightLeashDistance = 5;
    [SerializeField] private float leashTurnPadding = .12f;
    [SerializeField] private float closeBattlePadding = .75f;

    private float leashOriginX;
    private bool leashOriginReady;

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
        leashOriginX = transform.position.x;
        leashOriginReady = true;

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

    public override void Damage(Vector2 damageSourcePosition, int damageAmount)
    {
        base.Damage(damageSourcePosition, damageAmount);
        RefreshBattleTime(PlayerManager.instance.player.transform);
    }

    public override void TakeBlackholeDamage()
    {
        base.TakeBlackholeDamage();
        RefreshBattleTime(PlayerManager.instance.player.transform);
    }

    public override RaycastHit2D IsPlayerDetected()
    {
        RaycastHit2D playerDetected = base.IsPlayerDetected();

        if (playerDetected.collider == null)
            return default;

        if (!IsPositionInsideLeash(playerDetected.collider.bounds.center.x))
            return default;

        return playerDetected;
    }

    public void RefreshBattleTime(Transform target)
    {
        RefreshBattleTime(target.position);
    }

    public void RefreshBattleTime(Vector2 targetPosition)
    {
        if (stats != null && stats.isDead)
            return;

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

    public bool IsFacingLeashBoundary()
    {
        if (!useHorizontalLeash)
            return false;

        float probeX = transform.position.x + facingDir * leashTurnPadding;
        return !IsPositionInsideLeash(probeX);
    }

    public Vector2 ClampPositionToLeash(Vector2 position)
    {
        if (!useHorizontalLeash)
            return position;

        position.x = Mathf.Clamp(position.x, GetLeftLeashX(), GetRightLeashX());
        return position;
    }

    public bool CanKeepBattleWith(Vector2 targetPosition)
    {
        if (!IsPositionInsideLeash(targetPosition.x))
            return false;

        return Vector2.Distance(transform.position, targetPosition) <= attackDistance + closeBattlePadding;
    }

    private bool IsPositionInsideLeash(float xPosition)
    {
        if (!useHorizontalLeash)
            return true;

        return xPosition >= GetLeftLeashX() && xPosition <= GetRightLeashX();
    }

    private float GetLeftLeashX()
    {
        return GetLeashOriginX() - Mathf.Max(0, leftLeashDistance);
    }

    private float GetRightLeashX()
    {
        return GetLeashOriginX() + Mathf.Max(0, rightLeashDistance);
    }

    private float GetLeashOriginX()
    {
        return leashOriginReady ? leashOriginX : transform.position.x;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!useHorizontalLeash)
            return;

        Gizmos.color = Color.cyan;

        Vector3 leftLineStart = new Vector3(GetLeftLeashX(), transform.position.y - 1.5f, transform.position.z);
        Vector3 rightLineStart = new Vector3(GetRightLeashX(), transform.position.y - 1.5f, transform.position.z);
        Gizmos.DrawLine(leftLineStart, leftLineStart + Vector3.up * 3);
        Gizmos.DrawLine(rightLineStart, rightLineStart + Vector3.up * 3);
    }
}
