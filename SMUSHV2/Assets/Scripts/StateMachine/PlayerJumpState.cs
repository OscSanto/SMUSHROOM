using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState,IRootState {
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory) {
        IsRootState = true;
    }

    public override void CheckSwitchStates() {
        if (Ctx.CharacterController.isGrounded) {
            SwitchState(Factory.Grounded());
        }
    }

    public override void EnterState() {
        InitializeSubState();
        HandleJump();
    }

    public override void ExitState() {
        Ctx.Animator.SetBool("isJumping", false);
        if (Ctx.IsJumpPressed) {
            Ctx.RequireNewJumpPress = true;

        }

    }

    public override void InitializeSubState() {
        if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Idle());
        } else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Walk());
        } else {
            SetSubState(Factory.Run());
        }
    }

    public override void UpdateState() {
        HandleGravity();
        CheckSwitchStates(); //keep at bottom.

    }

    void HandleJump() {
        // set animator here
        Ctx.Animator.SetBool("isJumping", true);
        Ctx.IsJumping = true;
        Ctx.CurrentMovementY = Ctx.InitialJumpVelocity;
        Ctx.AppliedMovementY = Ctx.InitialJumpVelocity;
    }
    public void HandleGravity() {
        bool isfalling = Ctx.CurrentMovementY <= 0.0f || !Ctx.IsJumpPressed;
        float fallMultiplier = 2.0f;
        // apply gravity depending if the character is grounded or not     
        if (isfalling) {
            float previousYVelocity = Ctx.CurrentMovementY;
            Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.Gravity * fallMultiplier * Time.deltaTime);
            //clamp, so you don't fall at very fast speeds (a cliff fall would be like 200mph drop)
            Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY) * .5f, -20.0f);

        } else {
            //due to frame rates, different jumps can occur. This will average 60 and 30 fps differences.
            float previousYVelocity = Ctx.CurrentMovementY;
            Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.Gravity * Time.deltaTime);
            Ctx.AppliedMovementY = (previousYVelocity + Ctx.CurrentMovementY) * .5f;

            // old velocity + acceleration = new velocity
            // old position + new velocity = new position 'this applies inside Update function (isRunPressed) conditions'


        }
    }
}
