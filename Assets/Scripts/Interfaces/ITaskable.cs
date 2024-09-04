using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITaskable
{
    ITask CurrentTask { get; set; }
    bool Busy { get; }
    void ClearTask();

    void CancelTask();
}
