using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameMap : MonoBehaviour {
    [SerializeField] private GameObject buildingPrefab;

    [SerializeField] private GameObject roadPrefab;

    [SerializeField] private GameObject buildSpotPrefab;

    [FormerlySerializedAs("tiles_width")] [SerializeField]
    private int tilesWidth;

    [FormerlySerializedAs("tiles_height")] [SerializeField]

    private int tilesHeight;
    private GameObject _buildingsHolder;
    private GameObject _tilesHolder;

    [SerializeField] private TextAsset rawMapData;

    private int[] _buildingPosition;

    [FormerlySerializedAs("map_as_pp")] [SerializeField]
    private List<Pathpoint> mapAsPp = new List<Pathpoint>();

    [SerializeField] private List<Building> buildings = new List<Building>();

    private Pathfinder _pathfinder;

    [FormerlySerializedAs("target_grid_positions")] [SerializeField]
    private List<Vector3> targetGridPositions = new List<Vector3>();

    /* The use of SerializeField for the map_as_pp, buildings and target_grid_position fields is tricky. I'm not using them here because I want to expose them to the editor.
     I'm using them here because I want the data to persist or be serialized between edit and play mode. The Regenerate function is called by my GameMapEditor tool,
    so it populates everything in edit mode, but for some reason, even though I mark it as dirty, it doesn't persist the data unless its a SerializeField. This is also
    related to why I had to initialize the pathfinder in the start method, its not a monobehaviour and does not seem to hold on to its initization unless I do it at runtime.*/

    private void Start() {
        /* This takes a bit of lateral thinking, but the idea is that the regenerate method is called in the editor, so those other fields are already populated.*/
        _pathfinder = new Pathfinder(mapAsPp, tilesWidth, tilesHeight);
        Debug.Log("Pathfinder created");
    }

    public void Regenerate() {
        //This function will only really be called from the editor anyway, its used by our custom GameMapEditor tool
        //I use DestroyImmediate because its the only thing that works in the editor
        if (_buildingsHolder) {
            for (var i = 0; i < _buildingsHolder.transform.childCount; ++i) {
                DestroyImmediate(_buildingsHolder.transform.GetChild(i).gameObject);
            }

            DestroyImmediate(_buildingsHolder);
        }

        if (_tilesHolder) {
            for (var i = 0; i < _tilesHolder.transform.childCount; ++i) {
                DestroyImmediate(_tilesHolder.transform.GetChild(i).gameObject);
            }

            DestroyImmediate(_tilesHolder);
        }

        targetGridPositions = new List<Vector3>();

        var map_string_raw = rawMapData.text;
        var cleaned_map_string = map_string_raw.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");

        _buildingPosition = new int[tilesWidth * tilesHeight];
        for (var i = 0; i < tilesWidth * tilesHeight; ++i) {
            //This was a very painful bug until I looked it up
            //Converting directly to an int, gives me the ASCII code, not the actual number
            _buildingPosition[i] = cleaned_map_string[i] - '0';
        }

        _buildingsHolder = new GameObject("Buildings");
        _tilesHolder = new GameObject("Tiles");

        _buildingsHolder.transform.SetParent(gameObject.transform, true);
        _tilesHolder.transform.SetParent(gameObject.transform, true);


        RenderTiles();
        _pathfinder = new Pathfinder(mapAsPp, tilesWidth, tilesHeight);
    }

    public List<Vector3> GetPath(Vector3 startPoint, Vector3 endPoint) {
        if (_pathfinder != null) return _pathfinder.FindPath(startPoint, endPoint);

        Debug.LogError("No pathfinder found");
        Debug.Break();

        return null;
    }

    private Vector3 GetTileDimensions(Mesh mesh) {
        //This function is important for positioning the tiles
        var verts = mesh.vertices;
        var min = transform.TransformPoint(verts[0]);
        var max = transform.TransformPoint(verts[0]);

        foreach (var vertex in verts) {
            var wp = transform.TransformPoint(vertex);
            min = Vector3.Min(min, wp);
            max = Vector3.Max(max, wp);
        }

        return max - min;
    }

    private Vector3 GetFirstTileTopLeftCorner(int numTilesX, int numTilesZ, Vector3 tileDim) {
        //Calculates the top left corner of the first tile so we can position the rest of the map
        var left = numTilesX / 2.0f;
        var top = numTilesZ / 2.0f;

        var top_edge = top * tileDim.z;
        var left_edge = left * tileDim.x;

        return new Vector3(-left_edge, 0.0f, top_edge);
    }

    private Pathpoint SpawnTileAtSpot(int tileType, Vector3 targetPosition, Vector2Int arrayPositions) {
        //Spawns a tile of a specific type at a position
        //Also neatly files it away in the tile holder
        Pathpoint pp = null;
        switch (tileType) {
            case 0: {
                var rto = Instantiate(roadPrefab, targetPosition, Quaternion.identity);
                pp = rto.GetComponent<Pathpoint>();
                pp.IsPassable = true;
                pp.transform.SetParent(_tilesHolder.transform, true);
                break;
            }
            case 1: {
                var bto = Instantiate(buildSpotPrefab, targetPosition, Quaternion.identity);
                pp = bto.GetComponent<Pathpoint>();
                pp.IsPassable = false;
                pp.transform.SetParent(_tilesHolder.transform, true);

                var building = Instantiate(buildingPrefab, targetPosition, Quaternion.identity);
                var b = building.GetComponent<Building>();
                b.tile_coordinates = arrayPositions;
                buildings.Add(b);
                b.transform.SetParent(_buildingsHolder.transform, true);

                //Rotate the building at a random angle to get some visual interest
                var rot_angle = Random.Range(1, 3) * 90.0f;

                var meshRenderer = b.regularMesh.GetComponentInChildren<MeshRenderer>();
                meshRenderer.transform.rotation = Quaternion.Euler(new Vector3(-89.98f, rot_angle, 0.0f));

                var mr2 = b.shatteredMesh.GetComponentInChildren<MeshRenderer>();
                mr2.transform.rotation = Quaternion.Euler(new Vector3(0.0f, rot_angle, 0.0f));
                break;
            }
            case 2: {
                var rto = Instantiate(roadPrefab, targetPosition, Quaternion.identity);
                pp = rto.GetComponent<Pathpoint>();
                pp.IsPassable = true;
                pp.transform.SetParent(_tilesHolder.transform, true);
                Debug.Log("Adding to target grid positions");
                targetGridPositions.Add(targetPosition);
                break;
            }
        }

        if (!pp) {
            Debug.LogError("No pathpoint found");
            Debug.Break();
            return null;
        }

        pp.IndexedPosition = arrayPositions;
        return pp;
    }

    private void RenderTiles() {
        //This function is the workhorse and iterates through the building position array
        //to generate the map
        Debug.Log("Creating tiles");

        var t_dim = GetTileDimensions(buildSpotPrefab.GetComponentInChildren<MeshFilter>().sharedMesh);
        var ft_topleft = GetFirstTileTopLeftCorner(tilesWidth, tilesHeight, t_dim);
        var halfstep_vector = new Vector3(t_dim.x / 2.0f, 0.0f, t_dim.z / -2.0f);

        for (var j = 0; j < tilesHeight; j++) {
            for (var i = 0; i < tilesWidth; i++) {
                var array_index = (tilesWidth * j) + i;

                var current_center_position =
                    ft_topleft + new Vector3(t_dim.x * i, 0.0f, -t_dim.z * j) + halfstep_vector;

                mapAsPp.Add(SpawnTileAtSpot(_buildingPosition[array_index], current_center_position,
                    new Vector2Int(i, j)));
            }
        }

        //We populate the building data with the adjacent tiles for the drones or enemies to use
        foreach (var b in buildings) {
            b.adjacent_empties = GetEmptyAdjacents(b.tile_coordinates.x, b.tile_coordinates.y);
        }
    }

    private List<Pathpoint> GetEmptyAdjacents(int x, int y) {
        var adjacents = new List<Pathpoint>();

        var single_value_index = y * tilesWidth + x;
        var left_index = single_value_index - 1;
        var right_index = single_value_index + 1;
        var top_index = single_value_index - tilesWidth;
        var bottom_index = single_value_index + tilesWidth;

        if (left_index >= 0 && left_index / tilesWidth == single_value_index / tilesWidth) {
            if (mapAsPp[left_index].IsPassable) {
                adjacents.Add(mapAsPp[left_index]);
            }
        }

        if (right_index < tilesWidth * tilesHeight &&
            right_index / tilesWidth == single_value_index / tilesWidth) {
            if (mapAsPp[right_index].IsPassable) {
                adjacents.Add(mapAsPp[right_index]);
            }
        }

        if (top_index >= 0) {
            if (mapAsPp[top_index].IsPassable) {
                adjacents.Add(mapAsPp[top_index]);
            }
        }

        if (bottom_index >= tilesWidth * tilesHeight) return adjacents;

        if (mapAsPp[bottom_index].IsPassable) {
            adjacents.Add(mapAsPp[bottom_index]);
        }

        return adjacents;
    }

    public Vector3 GetRandomTargetPosition() {
        return targetGridPositions[Random.Range(0, targetGridPositions.Count)];
    }
}