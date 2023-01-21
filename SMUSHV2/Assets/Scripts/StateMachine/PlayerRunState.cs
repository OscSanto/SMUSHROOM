using UnityEngine;

public class PlayerRunState : PlayerBaseState {
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory) {
    }

    public override void EnterState() {
        Ctx.Animator.SetBool("isWalking", true);
        Ctx.Animator.SetBool("isRunning", true);
    }

    public override void UpdateState() {
        Ctx.AppliedMovementX = Ctx.CurrentMovementInput.x * Ctx.RunMultiplier * Ctx.Speed;
        Ctx.AppliedMovementZ = Ctx.CurrentMovementInput.y * Ctx.RunMultiplier * Ctx.Speed;
        CheckSwitchStates(); //keep at bottom
    }

    public override void ExitState() {}
    public override void InitializeSubState() {}

    public override void CheckSwitchStates() {
        if (!Ctx.IsMovementPressed) {
            SwitchState(Factory.Idle());
        } else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SwitchState(Factory.Walk());
        }
    }

}
