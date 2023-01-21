using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState {
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(
        currentContext,
        playerStateFactory){
        IsRootState = true;
    }

    public override void EnterState(){
        InitializeSubState();

        // enable this line when animation is ready
        //Ctx.Animator.Setbool ("isFalling",true);
    }
    public override void UpdateState(){
        // checkSwitchStates should be called every single frame
        HandleGravity();
        CheckSwitchStates(); // keep at bottom 
    }
    public override void ExitState(){
        //enable this line when animation is ready.
        //Ctx.Animator.SetBool("isFalling", false);
    }
    public override void CheckSwitchStates(){
        //Switch back to ground state when Character is grounded
        if (Ctx.CharacterController.isGrounded) {
            SwitchState(Factory.Grounded());
        }

        //do not implement the jump switch, since you can't jump while falling
    }
    public override void InitializeSubState(){
        if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Idle());
        } else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Walk());
        } else {
            SetSubState(Factory.Run());
        }
    }
    public void HandleGravity(){
        float previousYVelocity = Ctx.CurrentMovementY;
        Ctx.CurrentMovementY = Ctx.CurrentMovementY + Ctx.Gravity * Time.deltaTime;
        Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY) * .5f, -20.0f);
    }
}