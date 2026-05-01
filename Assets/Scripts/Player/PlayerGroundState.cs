using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : PlayerState
{
    public PlayerGroundState(Player player, PlayerStateMachine stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.R) && SkillManager.instance.blackhole != null)
        {
            stateMachine.ChangeState(player.blackholeState);
            return;
        }

        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (player.sword == null)
            {
                stateMachine.ChangeState(player.aimSwordState);
            }
            else
            {
                Sword_Skill_Controller swordController = player.sword.GetComponent<Sword_Skill_Controller>();

                if (swordController != null)
                    swordController.ReturnSword();
            }
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            stateMachine.ChangeState(player.counterAttackState);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            stateMachine.ChangeState(player.primaryAttackState);
        }
        if (!player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }

        if (Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.jumpState);
        }

    }
}
