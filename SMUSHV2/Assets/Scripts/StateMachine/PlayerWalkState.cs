using UnityEngine;

public class PlayerWalkState : PlayerBaseState {
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory) {

    }
    public override void EnterState() {
        Ctx.Animator.SetBool("isWalking", true);
        Ctx.Animator.SetBool("isRunning", false);
    }

    public override void UpdateState() {


        Ctx.AppliedMovementX = Ctx.CurrentMovementInput.x;
        Ctx.AppliedMovementZ = Ctx.CurrentMovementInput.y;
        CheckSwitchStates(); // keep at bottom 
    }
    public override void ExitState() { }
    public override void InitializeSubState() { }

    public override void CheckSwitchStates() {
        if (!Ctx.IsMovementPressed) {
            SwitchState(Factory.Idle());
        } else if (Ctx.IsMovementPressed && Ctx.IsRunPressed) {
            SwitchState(Factory.Run());
        }
    }
}
