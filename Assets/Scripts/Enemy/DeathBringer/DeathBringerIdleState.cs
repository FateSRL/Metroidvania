public class DeathBringerIdleState : EnemyState
{
    private Enemy_DeathBringer enemy;

    public DeathBringerIdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_DeathBringer enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
        enemy.ZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        enemy.ZeroVelocity();
        enemy.FacePlayer();

        if (!enemy.bossFightBegun)
        {
            if (enemy.ShouldStartBossFight())
                stateMachine.ChangeState(enemy.battleState);

            return;
        }

        if (enemy.IsPlayerInAttackRange())
        {
            if (enemy.TryStartAttack())
                stateMachine.ChangeState(enemy.attackState);

            return;
        }

        if (enemy.CanDoSpellCast())
        {
            stateMachine.ChangeState(enemy.spellCastState);
            return;
        }

        if (!enemy.ShouldChasePlayer())
            return;

        if (stateTimer <= 0)
            stateMachine.ChangeState(enemy.battleState);
    }
}
