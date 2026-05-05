using UnityEngine;

public class DeathBringerBattleState : EnemyState
{
    private Enemy_DeathBringer enemy;
    private Transform player;

    public DeathBringerBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_DeathBringer enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            player = PlayerManager.instance.player.transform;
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
        {
            enemy.ZeroVelocity();
            return;
        }

        enemy.FacePlayer();

        float xDistanceToPlayer = player.position.x - enemy.transform.position.x;
        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);

        if (enemy.CanDoSpellCast())
        {
            stateMachine.ChangeState(enemy.spellCastState);
            return;
        }

        if (distanceToPlayer <= enemy.attackDistance && CanAttack())
        {
            stateMachine.ChangeState(enemy.attackState);
            return;
        }

        if (Mathf.Abs(xDistanceToPlayer) <= enemy.targetOverlapEpsilon)
        {
            enemy.ZeroVelocity();
            return;
        }

        int moveDir = xDistanceToPlayer > 0 ? 1 : -1;
        enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }

    private bool CanAttack()
    {
        return Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown;
    }
}
