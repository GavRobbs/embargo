using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IHoverable, ISelectable
{
    public int building_id;
    public bool hasTurret = false;

    MeshRenderer meshRenderer;

    [SerializeField]
    Material defaultMaterial;

    [SerializeField]
    Material selectionMaterial;

    [SerializeField]
    GameObject turretLocationHolder;

    [SerializeField]
    AudioSource buildingTeleportSound;

    [SerializeField]
    GameObject teleportEffectPrefab;

    GameObject teleportEffect;

    GameObject previewTurret;

    public Vector2Int tile_coordinates;

    public List<Pathpoint> adjacent_empties;

    public bool building_turret = false;

    GameObject attached_turret = null;

    float build_duration = 0.0f;

    float fadeSpeed = 0.0f;

    public int hp = 30;

    public interface IBuildingTask
    {
        void StartTask();
        void UpdateTask(float dt);
        void CancelTask();

        bool IsDone { get; }
        bool IsCancelled { get; }
    }

    IBuildingTask current_task;

    public class BuildTurretTask : IBuildingTask
    {
        bool _done = false;
        bool _cancelled = false;
        public bool IsDone => _done;

        public bool IsCancelled => _cancelled;

        Building target_building;
        GameObject turretPrefab;
        float build_time;
        float fadeSpeed;

        System.Action<float> updateCallback;
        System.Action doneCallback;
        System.Action cancelCallback;

        public BuildTurretTask(Building b, GameObject prefab, float build_duration, System.Action<float> updateC, System.Action doneC, System.Action cancelC)
        {
            target_building = b;
            turretPrefab = prefab;
            build_time = build_duration;
            updateCallback = updateC;
            doneCallback = doneC;
            cancelCallback = cancelC;

            if (b.previewTurret)
            {
                GameObject.Destroy(b.previewTurret);
            }
        }

        public void CancelTask()
        {
            if (IsDone || IsCancelled)
            {
                return;
            }

            target_building.buildingTeleportSound.Stop();
            _cancelled = true;
            cancelCallback();
        }

        public void StartTask()
        {
            target_building.buildingTeleportSound.Play();
            target_building.building_turret = true;
            target_building.teleportEffect = GameObject.Instantiate(target_building.teleportEffectPrefab, target_building.turretLocationHolder.transform);
            target_building.attached_turret = GameObject.Instantiate(turretPrefab, target_building.turretLocationHolder.transform);

            SkinnedMeshRenderer[] mrs = target_building.attached_turret.GetComponentsInChildren<SkinnedMeshRenderer>();
            fadeSpeed = 0.9f / build_time;
            foreach (var renderer in mrs)
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; ++i)
                {
                    var col = materials[i].color;
                    col.a = 0.1f;
                    materials[i].color = col;
                }
            }
        }

        public void UpdateTask(float dt)
        {
            if (IsDone)
            {
                return;
            }

            updateCallback(dt);

            SkinnedMeshRenderer[] mrs = target_building.attached_turret.GetComponentsInChildren<SkinnedMeshRenderer>();

            bool fade_done = true;
            foreach (var renderer in mrs)
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; ++i)
                {
                    var col = materials[i].color;
                    col.a += dt * fadeSpeed;
                    materials[i].color = col;

                    if (col.a >= 1.0f)
                    {
                        col.a = 1.0f;
                        fade_done = fade_done && true;
                    }
                    else
                    {
                        fade_done = fade_done && false;
                    }
                }
            }

            if (fade_done)
            {
                doneCallback();
                target_building.buildingTeleportSound.Stop();
                _done = true;
                GameObject.Destroy(target_building.attached_turret);
                GameObject.Destroy(target_building.teleportEffect);
                target_building.building_turret = false;
                target_building.hasTurret = true;
                target_building.attached_turret = GameObject.Instantiate(turretPrefab, target_building.turretLocationHolder.transform);
                ITurret new_turret = target_building.attached_turret.GetComponent<ITurret>();
                new_turret.AttachedBuilding = target_building;
                new_turret.OnTurretSpawn();
            }           

        }
    }

    public Pathpoint RandomAdjacent
    {
        get
        {
            if (adjacent_empties == null || adjacent_empties.Count == 0)
            {
                return null;
            }

            return adjacent_empties[Random.Range(0, adjacent_empties.Count)];
        }
    }

    void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void SetTask(IBuildingTask task)
    {
        if(current_task != null)
        {
            return;
        }

        current_task = task;
        current_task.StartTask();
    }

    // Update is called once per frame
    void Update()
    {
        if(current_task != null)
        {
            current_task.UpdateTask(Time.deltaTime);

            if(current_task.IsCancelled || current_task.IsDone)
            {
                current_task = null;
            }
        }
    }

    public void Select()
    {
        meshRenderer.material = selectionMaterial;
    }

    public void Deselect()
    {
        meshRenderer.material = defaultMaterial;
    }

    void PreviewTurret(GameObject prefab_to_see)
    {
        if (hasTurret || previewTurret != null || building_turret)
        {
            return;
        }

        previewTurret = GameObject.Instantiate(prefab_to_see, turretLocationHolder.transform);
    }

    public Dictionary<string, string> GetHoverData()
    {
        Dictionary<string, string> result = new Dictionary<string, string>()
        {
            {"type", "building"},
            {"hp", hp.ToString()}
        };
        return result;
    }

    public void OnHoverOver(HoverInfo info)
    {

        if(info == null)
        {
            return;
        }


        if (info.mode == GameInputManager.HOVER_MODE.BUILD)
        {
            PreviewTurret((info as BuildHoverInfo).turretPrefab);
        }
    }

    public void OnHoverOff()
    {
        GameObject.Destroy(previewTurret);
    }
}
