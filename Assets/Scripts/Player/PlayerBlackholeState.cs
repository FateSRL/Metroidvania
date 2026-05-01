using UnityEngine;

public class PlayerBlackholeState : PlayerState
{
    private readonly float flyTime = .25f;
    private bool skillUsed;
    private float defaultGravity;

    public PlayerBlackholeState(Player player, PlayerStateMachine stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        defaultGravity = player.rb.gravityScale;
        skillUsed = false;
        stateTimer = flyTime;
        rb.gravityScale = 0;
        player.ZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();

        rb.gravityScale = defaultGravity;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer > 0)
        {
            rb.velocity = new Vector2(0, 12);
            return;
        }

        rb.velocity = new Vector2(0, -.1f);

        if (!skillUsed)
        {
            if (SkillManager.instance.blackhole != null && SkillManager.instance.blackhole.CanUseSkill())
            {
                skillUsed = true;
            }
            else
            {
                stateMachine.ChangeState(player.airState);
            }
        }

        if (skillUsed && SkillManager.instance.blackhole.SkillCompleted())
            stateMachine.ChangeState(player.airState);
    }
}
