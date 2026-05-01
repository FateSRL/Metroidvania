using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCatchSwordState : PlayerState
{
    private const float CatchStateDuration = .25f;

    public PlayerCatchSwordState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();

        stateTimer = CatchStateDuration;
        player.ZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("BusyFor", .1f);
    }

    public override void Update()
    {
        base.Update();

        player.ZeroVelocity();

        if (triggerCalled || stateTimer < 0)
            stateMachine.ChangeState(player.idleState);
    }
}
