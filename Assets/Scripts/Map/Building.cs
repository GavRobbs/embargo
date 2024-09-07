using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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
    GameObject explosionPrefab;

    [SerializeField]
    GameObject turretLocationHolder;

    [SerializeField]
    AudioSource buildingTeleportSound;

    [SerializeField]
    AudioSource buildingScrapSound;

    [SerializeField]
    GameObject teleportEffectPrefab;

    [SerializeField]
    GameObject scrapEffectPrefab;

    [SerializeField]
    public GameObject shatteredMesh;

    [SerializeField]
    AudioSource collapseSound;

    [SerializeField]
    public GameObject regularMesh;

    [SerializeField]
    GameObject upgradeArrow;

    [SerializeField]
    GameObject scrapIcon;

    [SerializeField]
    AudioSource explosionSound;

    [SerializeField]
    Transform buildingTextSpawnPoint;

    [SerializeField]
    GameObject hurtTextPrefab;

    [SerializeField]
    GameObject healTextPrefab;

    GameObject teleportEffect;

    GameObject previewTurret;

    public Vector2Int tile_coordinates;

    public List<Pathpoint> adjacent_empties;

    public bool building_turret = false;
    public bool upgrading_turret = false;
    public bool scrapping_turret = false;

    GameObject attached_turret = null;

    public int hp = 12;

    public bool isDestroyed = false;

    public ITurret OccupyingTurret
    {
        get
        {
            return attached_turret.GetComponent<ITurret>();
        }
    }

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

            if(target_building.teleportEffect != null)
            {
                GameObject.Destroy(target_building.teleportEffect);
            }

            if(target_building.attached_turret != null)
            {
                GameObject.Destroy(target_building.attached_turret);
            }

            target_building.building_turret = false;
            target_building.hasTurret = false;

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

    public class UpgradeTurretTask : IBuildingTask
    {
        bool _cancelled = false;
        int upgradeCost;
        float upgradeTime;
        System.Action doneCallback;
        System.Action cancelCallback;
        System.Action<float> updateCallback;
        Building target_building;
        bool _done = false;
        public UpgradeTurretTask(Building b, int cost, float utime, System.Action<float> updateC, System.Action doneC, System.Action cancelC)
        {
            upgradeCost = cost;
            upgradeTime = utime;
            doneCallback = doneC;
            cancelCallback = cancelC;
            updateCallback = updateC;
            target_building = b;
            
        }

        public bool IsDone => _done;

        public bool IsCancelled => _cancelled;

        public void CancelTask()
        {
            if(IsDone || IsCancelled)
            {
                return;
            }

            _cancelled = true;
            target_building.buildingTeleportSound.Stop();
            if (target_building.teleportEffect != null)
            {
                GameObject.Destroy(target_building.teleportEffect);
            }
            target_building.upgrading_turret = false;
            cancelCallback();

        }

        public void StartTask()
        {
            target_building.buildingTeleportSound.Play();
            target_building.upgrading_turret = true;
            target_building.teleportEffect = GameObject.Instantiate(target_building.teleportEffectPrefab, target_building.turretLocationHolder.transform);
        }

        public void UpdateTask(float dt)
        {
            if (IsDone)
            {
                return;
            }

            updateCallback(dt);

            upgradeTime -= dt;
            if(upgradeTime <= 0.0f)
            {
                _done = true;
                target_building.attached_turret.GetComponent<ITurret>().Level += 1;

            }

            if (_done)
            {
                doneCallback();
                target_building.attached_turret.GetComponent<ITurret>().OnTurretUpgrade();
                target_building.buildingTeleportSound.Stop();
                _done = true;
                target_building.upgrading_turret = false;
                GameObject.Destroy(target_building.teleportEffect);
            }
        }
    }

    public class ScrapTurretTask : IBuildingTask
    {
        bool _cancelled = false;
        System.Action doneCallback;
        System.Action cancelCallback;
        System.Action<float> updateCallback;
        Building target_building;
        bool _done = false;
        float fadeSpeed = 0.0f;
        float scrapTime = 8.0f;
        float refund_amount = 0.0f;
        public ScrapTurretTask(Building b, float stime, System.Action<float> updateC, System.Action doneC, System.Action cancelC)
        {
            doneCallback = doneC;
            cancelCallback = cancelC;
            updateCallback = updateC;
            target_building = b;
            scrapTime = stime;
            ITurret t = b.attached_turret.GetComponent<ITurret>();
            refund_amount = (t.Level * t.Cost) / 2.0f;
        }

        public bool IsDone => _done;

        public bool IsCancelled => _cancelled;

        public void CancelTask()
        {
            if (IsDone || IsCancelled)
            {
                return;
            }

            _cancelled = true;
            target_building.buildingScrapSound.Stop();
            if (target_building.teleportEffect != null)
            {
                GameObject.Destroy(target_building.teleportEffect);
            }
            target_building.scrapping_turret = false;
            cancelCallback();

        }

        public void StartTask()
        {
            target_building.buildingScrapSound.Play();
            target_building.scrapping_turret = true;
            target_building.teleportEffect = GameObject.Instantiate(target_building.scrapEffectPrefab, target_building.turretLocationHolder.transform);


            SkinnedMeshRenderer[] mrs = target_building.attached_turret.GetComponentsInChildren<SkinnedMeshRenderer>();
            fadeSpeed = 1.0f / scrapTime;
            foreach (var renderer in mrs)
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; ++i)
                {
                    var col = materials[i].color;
                    col.a = 1.0f;
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
                    col.a -= dt * fadeSpeed;
                    materials[i].color = col;

                    if (col.a <= 0.0f)
                    {
                        col.a = 0.0f;
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
                GameObject.Destroy(target_building.attached_turret);
                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.AddScrap, (int)refund_amount));
                target_building.hasTurret = false;
                target_building.building_turret = false;
                target_building.scrapping_turret = false;
                target_building.upgrading_turret = false;

                target_building.buildingScrapSound.Stop();
                _done = true;
                if (target_building.teleportEffect != null)
                {
                    GameObject.Destroy(target_building.teleportEffect);
                }
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
        current_task?.StartTask();
    }

    public void CancelCurrentTask()
    {
        current_task?.CancelTask();
        current_task = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDestroyed)
        {
            return;
        }

        if (hp <= 0)
        {
            CancelCurrentTask();
            DestroyBuilding();
            return;
        }

        if (current_task != null)
        {
            current_task.UpdateTask(Time.deltaTime);

            if(current_task.IsCancelled || current_task.IsDone)
            {
                current_task = null;
            }
        }
    }

    public void Damage(float damage)
    {
        if (isDestroyed)
        {
            return;
        }

        hp -= (int)damage;

        TextMeshProUGUI dtext = GameObject.Instantiate(hurtTextPrefab, buildingTextSpawnPoint).GetComponent<TextMeshProUGUI>();
        dtext.text = "-" + ((int)damage).ToString();
        GameObject.Destroy(dtext.gameObject, 4.0f);
    }

    public void IncreaseHP()
    {
        hp = Mathf.Min(hp + 1, 12);

        GameObject hText = GameObject.Instantiate(healTextPrefab, buildingTextSpawnPoint);
        GameObject.Destroy(hText, 4.0f);
    }

    void DestroyBuilding()
    {
        if(attached_turret != null)
        {
            GameObject.Destroy(attached_turret);
            GameObject.Instantiate(explosionPrefab, turretLocationHolder.transform);
            explosionSound.Play();
        }

        if(teleportEffect != null)
        {
            GameObject.Destroy(teleportEffect);
        }

        collapseSound.Play();
        GameObject.Destroy(this.gameObject, 4.0f);
        shatteredMesh.SetActive(true);
        regularMesh.SetActive(false);
        isDestroyed = true;
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
        if (hasTurret || previewTurret != null || building_turret || isDestroyed || upgrading_turret || scrapping_turret)
        {
            return;
        }

        previewTurret = GameObject.Instantiate(prefab_to_see, turretLocationHolder.transform);
    }

    public Dictionary<string, string> GetHoverData()
    {
        if (isDestroyed)
        {
            return null;
        }

        Dictionary<string, string> result = new Dictionary<string, string>()
        {
            {"type", "building"},
            {"hp", hp.ToString()}
        };
        return result;
    }

    public void OnHoverOver(HoverInfo info)
    {
        if (isDestroyed)
        {
            return;
        }

        if(info == null)
        {
            return;
        }


        if (info.mode == GameInputManager.HOVER_MODE.BUILD && !upgrading_turret && !scrapping_turret && !building_turret && !hasTurret)
        {
            PreviewTurret((info as BuildHoverInfo).turretPrefab);
        }
        else if(info.mode == GameInputManager.HOVER_MODE.UPGRADE && !building_turret && !scrapping_turret && !upgrading_turret && hasTurret)
        {
            if(attached_turret != null)
            {
                ActivateArrow();
            }

        }
        else if (info.mode == GameInputManager.HOVER_MODE.SCRAP && !building_turret && !scrapping_turret && !upgrading_turret && hasTurret)
        {
            if (attached_turret != null)
            {
                ActivateScrapIcon();
            }

        }
    }

    public void OnHoverOff()
    {
        if (isDestroyed)
        {
            return;
        }

        if(previewTurret != null)
        {
            GameObject.Destroy(previewTurret);
        }

        DeactivateArrow();
        DeactivateScrapIcon();
    }

    public void ActivateArrow()
    {
        if (!upgradeArrow.activeSelf)
        {
            upgradeArrow.SetActive(true);
        }
    }

    public void DeactivateArrow()
    {
        upgradeArrow.SetActive(false);
    }

    public void ActivateScrapIcon()
    {
        if (!scrapIcon.activeSelf)
        {
            scrapIcon.SetActive(true);
        }
    }

    public void DeactivateScrapIcon()
    {
        scrapIcon.SetActive(false);
    }
}
