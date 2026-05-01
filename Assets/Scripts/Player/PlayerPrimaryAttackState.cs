using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{

    private int comboCounter;
    private float lastTimeAttacked;
    private float comboWindow = 2;
    public PlayerPrimaryAttackState(Player player, PlayerStateMachine stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // player.SetVelocity(0, 0);
        xInput=0;
        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }

        player.anim.SetInteger("ComboCounter", comboCounter);
        // player.anim.speed = 1.2f;
        #region Choose attack direction
        float attackDir=player.facingDir;
        if(xInput!=0){
            attackDir=xInput;
        }   
        #endregion
       
        player.SetVelocity(player.attackMovement[comboCounter].x * attackDir, player.attackMovement[comboCounter].y);


        stateTimer = .1f;
    }
    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine("BusyFor", .1f);
        // player.anim.speed = 1;
        lastTimeAttacked = Time.time;
        comboCounter++;

    }
    public override void Update()
    {
        base.Update();
        if (stateTimer < 0)
        {
            player.ZeroVelocity();
        }
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

}
