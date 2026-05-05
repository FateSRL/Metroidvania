public class DeathBringerTeleportState : EnemyState
{
    private Enemy_DeathBringer enemy;

    public DeathBringerTeleportState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_DeathBringer enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.ZeroVelocity();

        if (enemy.stats != null)
            enemy.stats.MakeInvincible(true);
    }

    public override void Update()
    {
        base.Update();

        enemy.ZeroVelocity();

        if (triggerCalled)
        {
            if (enemy.CanDoSpellCast())
                stateMachine.ChangeState(enemy.spellCastState);
            else
                stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        enemy.MakeVisible(true);

        if (enemy.stats != null)
            enemy.stats.MakeInvincible(false);
    }
}
