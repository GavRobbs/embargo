using UnityEngine;

// https://discussions.unity.com/t/delete-object-after-animation-2d/100143/6
public class DestroyOnEnter : StateMachineBehaviour
{
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Destroy(animator.gameObject);
    }
}
