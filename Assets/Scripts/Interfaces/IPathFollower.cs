using System.Collections.Generic;
using UnityEngine;

public interface IPathFollower {
    void FollowPath(List<Vector3> path_points, System.Action onComplete);
}