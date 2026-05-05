using UnityEngine;

public class DeathBringerSpellCastState : EnemyState
{
    private Enemy_DeathBringer enemy;
    private int spellsLeft;
    private float spellTimer;

    public DeathBringerSpellCastState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_DeathBringer enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        spellsLeft = enemy.AmountOfSpells;
        spellTimer = .25f;
        enemy.ZeroVelocity();
        enemy.FacePlayer();
    }

    public override void Update()
    {
        base.Update();

        enemy.ZeroVelocity();
        enemy.FacePlayer();
        spellTimer -= Time.deltaTime;

        if (spellsLeft > 0 && spellTimer <= 0)
        {
            enemy.CastSpell();
            spellsLeft--;
            spellTimer = enemy.SpellCooldown;
        }

        if (spellsLeft <= 0)
            stateMachine.ChangeState(enemy.teleportState);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.MarkSpellCastFinished();
    }
}
