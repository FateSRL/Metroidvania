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
        enemy.ZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        enemy.ZeroVelocity();
        enemy.FacePlayer();

        if (enemy.ShouldStartBossFight())
            stateMachine.ChangeState(enemy.battleState);
    }
}
