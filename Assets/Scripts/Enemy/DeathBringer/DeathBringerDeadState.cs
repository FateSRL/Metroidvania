public class DeathBringerDeadState : EnemyState
{
    private Enemy_DeathBringer enemy;

    public DeathBringerDeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_DeathBringer enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        triggerCalled = false;
        rb = enemy.rb;

        enemy.ZeroVelocity();
        enemy.PlayDeathAnimation();
    }

    public override void Update()
    {
        enemy.ZeroVelocity();
    }
}
