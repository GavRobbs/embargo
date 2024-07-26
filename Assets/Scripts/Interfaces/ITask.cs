using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITask
{
    void OnTaskEnter();
    void OnTaskUpdate(float dt);
    void OnTaskExit();
}
