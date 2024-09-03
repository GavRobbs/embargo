using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder
{
    List<Pathpoint> map;
    int nRows;
    int nColumns;
    public Pathfinder(List<Pathpoint> level_map, int r, int c)
    {
        map = level_map;
        nRows = r;
        nColumns = c;
    }
    public List<Vector3> FindPath(Vector3 startPoint, Vector3 endPoint)
    {
        Dictionary<Pathpoint, Pathpoint> predecessors = new Dictionary<Pathpoint, Pathpoint>();
        List<float> distance_map = Enumerable.Repeat(float.MaxValue, map.Count).ToList();
        HashSet<Pathpoint> visited = new HashSet<Pathpoint>();

        //I'm using this as a priority queue for the A* algorithm
        SortedDictionary<float, List<Pathpoint>> path_pq = new SortedDictionary<float, List<Pathpoint>>();

        int start_index = PositionToPathPointIndex(startPoint);
        int end_index = PositionToPathPointIndex(endPoint);
        if (!map[start_index].IsPassable || !map[end_index].IsPassable)
        {
            return null;
        }

        distance_map[start_index] = 0.0f;


        //Kick off the algorithm here
        //Because of floating point imprecision, I round all calculations to 3 decimal places
        //to prevent key retrieval errors
        AddToPriorityQueue(0.0f + CalculateHeuristic(map[start_index].Position, map[end_index].Position),
            map[start_index],
            path_pq);

        while (path_pq.Count > 0)
        {
            /* Since its a sorted list (in ascending order), the first element always has
             * the lowest value, so this first section of code just plucks it out */

            var visiting = PopPriorityQueueTop(path_pq);
            int visiting_index = visiting.GetPositionAsLinearArrayIndex(nColumns);
            visited.Add(visiting);

            //Get the neighbouring nodes
            var neighbours = GetAdjacentNodes(visiting.IndexedPosition);

            //Iterate through each neighbour
            foreach (var neighbour in neighbours)
            {
                //If we've already visited the neighbour, ignore it
                if (visited.Contains(neighbour))
                {
                    continue;
                }

                //Check what the cost to the neighbour through the node that we're currently visiting is
                float tentative_cost = distance_map[visiting_index] + 1.0f;

                //Get the index of that neighbour as a 1D array coordinate
                int neighbour_index = neighbour.GetPositionAsLinearArrayIndex(nColumns);

                //If the cost we calculated is lower, update the distance map
                //and add this neighbour to the list of nodes to be visited in the priority queue
                if (tentative_cost < distance_map[neighbour_index])
                {
                    distance_map[neighbour_index] = tentative_cost;
                    predecessors[neighbour] = visiting;
                    AddToPriorityQueue(tentative_cost + CalculateHeuristic(neighbour.Position, endPoint),
                        neighbour,
                        path_pq);
                }
            }
        }

        //Read all the nodes positions in reverse order to get our path
        List<Vector3> final_path = new List<Vector3>();
        Pathpoint next_point = map[PositionToPathPointIndex(endPoint)];
        final_path.Add(next_point.Position);

        while (predecessors.TryGetValue(next_point, out next_point))
        {
            final_path.Add(next_point.Position);
        }

        final_path.Reverse();

        return final_path;
    }

    private void AddToPriorityQueue(float key, Pathpoint value, SortedDictionary<float, List<Pathpoint>> pq)
    {
        float rounded_key = (float)System.Math.Round(key, 3);
        if (!pq.ContainsKey(rounded_key))
        {
            pq[rounded_key] = new List<Pathpoint>();
        }

        pq[rounded_key].Add(value);
    }

    private Pathpoint PopPriorityQueueTop(SortedDictionary<float, List<Pathpoint>> pq)
    {
        Pathpoint first_value = null;
        var first_entry = pq.First();

        //Since the sorted dictionary sorts by key and the value is a list
        //Every value inside a list associated with a key is of equal priority, so we can do some randomization
        //so the paths aren't always the same

        int num_entries = pq[first_entry.Key].Count;

        if (num_entries == 1)
        {
            first_value = pq[first_entry.Key].First();
            pq[first_entry.Key].RemoveAt(0);
        }
        else
        {
            int rand_index = Random.Range(0, num_entries);
            first_value = pq[first_entry.Key][rand_index];
            pq[first_entry.Key].RemoveAt(rand_index);
        }

        if (pq[first_entry.Key].Count == 0)
        {
            pq.Remove(first_entry.Key);
        }

        return first_value;
    }

    private int PositionToPathPointIndex(Vector3 pos)
    {
        for (int i = 0; i < map.Count; ++i)
        {
            if (map[i].IsPointOnTile(pos))
            {
                return i;
            }
        }

        return -1;
    }

    private float CalculateHeuristic(Vector3 current_pair, Vector3 end_pair)
    {
        //I'm using the manhattan distance here to keep things simple
        return Mathf.Abs(end_pair.z = current_pair.z) + Mathf.Abs(end_pair.x - current_pair.x);
    }

    private List<Pathpoint> GetAdjacentNodes(Vector2Int ipos)
    {
        //We don't allow diagonal movement in the game, so the adjacent nodes are above, below, left, right
        //Above is y-1, left is x-1, right is x+1, below is y+1
        //If any of these yield an index that is out of array bounds, then that is excluded
        //If we get that index and check it and see that it is an impassable area, we also exclude it

        Vector2Int[] surrounding_deltas = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up
        };

        List<Vector2Int> adjacent_tile_indices = new List<Vector2Int>();
        Vector2Int np = Vector2Int.zero;

        //Could have done this more concisely with LINQ, but its only four objects
        //and I didn't want to introduce that overhead
        for (int i = 0; i < surrounding_deltas.Length; ++i)
        {
            np = surrounding_deltas[i] + ipos;
            if (np.x < 0 || np.y < 0 || np.x >= nColumns || np.y >= nRows)
            {
                continue;
            }
            else
            {
                adjacent_tile_indices.Add(np);
            }
        }

        List<Pathpoint> neighbours = new List<Pathpoint>();
        foreach (var pos_veci in adjacent_tile_indices)
        {
            int array_index = pos_veci.y * nColumns + pos_veci.x;
            if (map[array_index].IsPassable)
            {
                neighbours.Add(map[array_index]);
            }
        }

        return neighbours;

    }
}
