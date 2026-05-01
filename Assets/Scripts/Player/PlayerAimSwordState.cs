using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimSwordState : PlayerState
{
    public PlayerAimSwordState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (SkillManager.instance != null && SkillManager.instance.sword != null)
            SkillManager.instance.sword.DotsActive(true);
    }

    public override void Exit()
    {
        base.Exit();

        if (SkillManager.instance != null && SkillManager.instance.sword != null)
            SkillManager.instance.sword.DotsActive(false);
    }

    public override void Update()
    {
        base.Update();

        player.ZeroVelocity();
        FaceMousePosition();

        if (Input.GetKeyUp(KeyCode.Mouse1))
            stateMachine.ChangeState(player.idleState);
    }

    private void FaceMousePosition()
    {
        if (Camera.main == null)
            return;

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z - player.transform.position.z);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        if (player.transform.position.x > mousePosition.x && player.facingDir == 1)
            player.Flip();
        else if (player.transform.position.x < mousePosition.x && player.facingDir == -1)
            player.Flip();
    }
}
