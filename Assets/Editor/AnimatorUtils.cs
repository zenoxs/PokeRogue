using UnityEditor.Animations;

public static class AnimatorControllerExtensions
{
    public static AnimatorState FindStateByName(this AnimatorStateMachine animatorStateMachine, string stateName)
    {
        foreach (var childState in animatorStateMachine.states)
        {
            if (childState.state.name == stateName)
            {
                return childState.state;
            }
        }

        // If the state was not found in the current state machine, search in sub-state machines
        foreach (var childStateMachine in animatorStateMachine.stateMachines)
        {
            AnimatorState foundState = childStateMachine.stateMachine.FindStateByName(stateName);
            if (foundState != null)
            {
                return foundState;
            }
        }

        return null;

    }
}