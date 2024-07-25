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
    GameObject turretPrefab;

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

    bool building_turret = false;

    GameObject attached_turret = null;

    float build_duration = 0.0f;

    float fadeSpeed = 0.0f;

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

    // Update is called once per frame
    void Update()
    {
        if (building_turret && !hasTurret)
        {
            bool doneMaking = false;
            SkinnedMeshRenderer[] mrs = attached_turret.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in mrs)
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; ++i)
                {
                    var col = materials[i].color;
                    col.a += Time.deltaTime * fadeSpeed;
                    materials[i].color = col;

                    if (col.a >= 1.0f)
                    {
                        col.a = 1.0f;
                        doneMaking = true;
                    }
                }
            }

            if (doneMaking)
            {
                GameObject.Destroy(attached_turret);
                building_turret = false;
                buildingTeleportSound.Stop();
                GameObject.Destroy(teleportEffect);
                AddTurret();
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

    public void BuildTurret(float duration)
    {
        if (hasTurret || building_turret)
        {
            return;
        }

        buildingTeleportSound.Play();
        build_duration = duration;
        building_turret = true;
        attached_turret = GameObject.Instantiate(turretPrefab, turretLocationHolder.transform);
        teleportEffect = GameObject.Instantiate(teleportEffectPrefab, turretLocationHolder.transform);

        SkinnedMeshRenderer[] mrs = attached_turret.GetComponentsInChildren<SkinnedMeshRenderer>();
        fadeSpeed = 0.5f / duration;
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

    public void AddTurret()
    {
        if (hasTurret)
        {
            return;
        }

        hasTurret = true;
        attached_turret = GameObject.Instantiate(turretPrefab, turretLocationHolder.transform);
    }

    void PreviewTurret()
    {
        if (hasTurret || previewTurret != null || building_turret)
        {
            return;
        }

        previewTurret = GameObject.Instantiate(turretPrefab, turretLocationHolder.transform);

    }

    public PopupContent GetHoverData()
    {
        return new PopupContent("Building", 30, 30, hasTurret ? "Turret" : null, hasTurret ? 1 : 0);
    }

    public void OnHoverOver()
    {
        PreviewTurret();
    }

    public void OnHoverOff()
    {
        GameObject.Destroy(previewTurret);
    }
}
