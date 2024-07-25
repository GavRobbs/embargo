using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour
{
    [SerializeField]
    GameObject buildingPrefab;

    [SerializeField]
    GameObject roadPrefab;

    [SerializeField]
    GameObject buildSpotPrefab;

    [SerializeField]
    int tiles_width;

    [SerializeField]
    int tiles_height;

    GameObject buildings_holder;
    GameObject tiles_holder;

    int[] building_position =
        { 1, 1, 1, 0, 1, 1, 1, 1,
          0, 0, 0, 0, 0, 0, 0, 0,
          1, 1, 1, 0, 1, 1, 1, 1,
          0, 0, 0, 0, 0, 0, 0, 0,
          1, 1, 1, 1, 1, 0, 1, 0,
          1, 0, 0, 0, 0, 0, 1, 0,
          1, 0, 1, 1, 0, 1, 1, 0,
          1, 0, 0, 0, 0, 0, 0, 0 };

    List<Pathpoint> map_as_pp = new List<Pathpoint>();

    List<Building> buildings = new List<Building>();

    Pathfinder pathfinder;

    void Start()
    {
        Regenerate();        
    }

    public void Regenerate()
    {
        //This function will only really be called from the editor anyway, its used by our custom GameMapEditor tool
        //I use DestroyImmediate because its the only thing that works in the editor
        if(buildings_holder != null)
        {
            GameObject.DestroyImmediate(buildings_holder);
        }

        if(tiles_holder != null)
        {
            GameObject.DestroyImmediate(tiles_holder);
        }

        buildings_holder = new GameObject("Buildings");
        tiles_holder = new GameObject("Tiles");

        buildings_holder.transform.SetParent(this.gameObject.transform, true);
        tiles_holder.transform.SetParent(this.gameObject.transform, true);


        RenderTiles();
        pathfinder = new Pathfinder(map_as_pp, tiles_width, tiles_height);
    }

    public List<Vector3> GetPath(Vector3 startPoint, Vector3 endPoint)
    {
        return pathfinder.FindPath(startPoint, endPoint);
    }

    private Vector3 GetTileDimensions(Mesh mesh)
    {
        //This function is important for positioning the tiles
        Vector3[] verts = mesh.vertices;
        Vector3 min = transform.TransformPoint(verts[0]);
        Vector3 max = transform.TransformPoint(verts[0]);

        foreach (Vector3 vertex in verts)
        {
            Vector3 wp = transform.TransformPoint(vertex);
            min = Vector3.Min(min, wp);
            max = Vector3.Max(max, wp);
        }

        return max - min;
    }

    private Vector3 GetFirstTileTopLeftCorner(int num_tiles_x, int num_tiles_z, Vector3 tile_dim)
    {
        //Calculates the top left corner of the first tile so we can position the rest of the map
        float left = (float)num_tiles_x / 2.0f;
        float top = (float)num_tiles_z / 2.0f;

        float top_edge = top * tile_dim.z;
        float left_edge = left * tile_dim.x;

        return new Vector3(-left_edge, 0.0f, top_edge);
    }

    private Pathpoint SpawnTileAtSpot(int tile_type, Vector3 target_position, Vector2Int array_positions)
    {
        //Spawns a tile of a specific type at a position
        //Also neatly files it away in the tile holder
        Pathpoint pp = null;
        if (tile_type == 1)
        {
            GameObject bto = GameObject.Instantiate(buildSpotPrefab, target_position, Quaternion.identity);
            pp = bto.GetComponent<Pathpoint>();
            pp.IsPassable = false;
            pp.transform.SetParent(tiles_holder.transform, true);

            GameObject building = GameObject.Instantiate(buildingPrefab, target_position, Quaternion.identity);
            Building b = building.GetComponent<Building>();
            b.tile_coordinates = array_positions;
            buildings.Add(b);
            b.transform.SetParent(buildings_holder.transform, true);

            //Gives the building a random rotation to generate a little visual interest
            MeshRenderer meshRenderer = b.GetComponentInChildren<MeshRenderer>();
            meshRenderer.transform.rotation = Quaternion.Euler(new Vector3(-89.98f, (float)Random.Range(1, 3) * 90.0f, 0.0f));

        }
        else if (tile_type == 0)
        {
            GameObject rto = GameObject.Instantiate(roadPrefab, target_position, Quaternion.identity);
            pp = rto.GetComponent<Pathpoint>();
            pp.IsPassable = true;
            pp.transform.SetParent(tiles_holder.transform, true);
        }

        pp.IndexedPosition = array_positions;
        return pp;
    }

    private void RenderTiles()
    {
        //This function is the workhorse and iterates through the building position array
        //to generate the map
        //TODO: Edit it to read from a text file so I can change the map on the fly

        Vector3 t_dim = GetTileDimensions(buildSpotPrefab.GetComponentInChildren<MeshFilter>().sharedMesh);
        Vector3 ft_topleft = GetFirstTileTopLeftCorner(tiles_width, tiles_height, t_dim);
        Vector3 halfstep_vector = new Vector3(t_dim.x / 2.0f, 0.0f, t_dim.z / -2.0f);

        for (int j = 0; j < tiles_height; j++)
        {
            for (int i = 0; i < tiles_width; i++)
            {
                int array_index = (tiles_width * j) + i;
                Vector3 current_center_position = ft_topleft + new Vector3(t_dim.x * (float)i, 0.0f, -t_dim.z * (float)j) + halfstep_vector;
                map_as_pp.Add(SpawnTileAtSpot(building_position[array_index], current_center_position, new Vector2Int(i, j)));
            }
        }

        //We populate the building data with the adjacent tiles for the drones or enemies to use
        foreach (Building b in buildings)
        {
            b.adjacent_empties = GetEmptyAdjacents(b.tile_coordinates.x, b.tile_coordinates.y);
        }
    }

    public List<Pathpoint> GetEmptyAdjacents(int x, int y)
    {
        List<Pathpoint> adjacents = new List<Pathpoint>();

        int single_value_index = y * tiles_width + x;
        int left_index = single_value_index - 1;
        int right_index = single_value_index + 1;
        int top_index = single_value_index - tiles_width;
        int bottom_index = single_value_index + tiles_width;

        if (left_index < 0 || left_index / tiles_width != single_value_index / tiles_width)
        {

        }
        else
        {
            if (map_as_pp[left_index].IsPassable)
            {
                adjacents.Add(map_as_pp[left_index]);
            }
        }

        if (right_index >= tiles_width * tiles_height || right_index / tiles_width != single_value_index / tiles_width)
        {

        }
        else
        {
            if (map_as_pp[right_index].IsPassable)
            {
                adjacents.Add(map_as_pp[right_index]);
            }
        }

        if (top_index < 0)
        {

        }
        else
        {
            if (map_as_pp[top_index].IsPassable)
            {
                adjacents.Add(map_as_pp[top_index]);
            }
        }

        if (bottom_index >= tiles_width * tiles_height)
        {

        }
        else
        {
            if (map_as_pp[bottom_index].IsPassable)
            {
                adjacents.Add(map_as_pp[bottom_index]);
            }
        }

        return adjacents;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
