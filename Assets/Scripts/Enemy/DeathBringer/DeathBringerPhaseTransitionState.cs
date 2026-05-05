public class DeathBringerPhaseTransitionState : EnemyState
{
    private Enemy_DeathBringer enemy;
    private int lasersSpawned;
    private float laserTimer;
    private float finishTimer;

    public DeathBringerPhaseTransitionState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_DeathBringer enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.BeginPhaseTwoTransition();
        enemy.ZeroVelocity();
        enemy.FacePlayer();

        lasersSpawned = 0;
        laserTimer = enemy.PhaseTwoTransitionStartDelay;
        finishTimer = -1;
    }

    public override void Update()
    {
        base.Update();

        enemy.ZeroVelocity();
        enemy.FacePlayer();

        if (lasersSpawned < enemy.PhaseTwoTransitionLaserCount)
        {
            laserTimer -= UnityEngine.Time.deltaTime;

            if (laserTimer <= 0)
            {
                enemy.SpawnPhaseTwoTransitionLaser(lasersSpawned);
                lasersSpawned++;
                laserTimer = enemy.PhaseTwoTransitionLaserInterval;

                if (lasersSpawned >= enemy.PhaseTwoTransitionLaserCount)
                    finishTimer = enemy.PhaseTwoTransitionLaserWarningTime + enemy.PhaseTwoTransitionLaserFireTime + .25f;
            }

            return;
        }

        finishTimer -= UnityEngine.Time.deltaTime;

        if (finishTimer <= 0)
        {
            enemy.ActivatePhaseTwo();
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
