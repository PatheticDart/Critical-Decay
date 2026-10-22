using System.Collections.Generic;
using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem Instance { get; private set; }

    [Header("Grid Setup")]
    [SerializeField] private float tileSize = 1.0f;

    [Header("References")]
    [SerializeField] private Transform chuTransform;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private GameObject buildNodePrefab;
    [SerializeField] private GameObject nodeOptionPrefab;
    [SerializeField] private Camera buildCamera;

    [Header("Dynamic Shop UI")]
    [SerializeField] private GameObject moduleSelectionPanel;
    [SerializeField] private Transform shopContentContainer;
    [SerializeField] private GameObject moduleShopItemPrefab;
    [SerializeField] private ModuleScriptableObject[] allAvailableModules;

    [Header("Module Action Panel UI")]
    [SerializeField] private ModuleActionPanel actionPanel;

    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, GameObject> activeNodes = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, RectTransform> activeNodeRects = new Dictionary<Vector2Int, RectTransform>();
    private Dictionary<Vector2Int, GameObject> activeOptionNodes = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, RectTransform> activeOptionNodeRects = new Dictionary<Vector2Int, RectTransform>();

    private List<ModuleHealth> chronologicalModuleList = new List<ModuleHealth>();
    private BuildNodeButton selectedNode;

    public ModuleScriptableObject[] AllAvailableModules => allAvailableModules;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (buildCamera == null) buildCamera = Camera.main;
        if (moduleSelectionPanel != null) moduleSelectionPanel.SetActive(false);

        InitializeCHU();
    }

    private void Update()
    {
        UpdateNodeScreenPositions();
    }

    private void InitializeCHU()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                occupiedTiles.Add(new Vector2Int(x, y));
            }
        }

        if (chuTransform != null)
        {
            ModuleHealth chuHealth = chuTransform.GetComponent<ModuleHealth>();
            if (chuHealth != null)
            {
                chuHealth.GridPosition = Vector2Int.zero;
            }
        }

        RebuildAllNodes();
    }

    private void UpdateNodeScreenPositions()
    {
        if (buildCamera == null || chuTransform == null) return;

        foreach (var pair in activeNodeRects)
        {
            Vector3 worldPos = chuTransform.position + new Vector3(pair.Key.x * tileSize, pair.Key.y * tileSize, 0f);
            pair.Value.position = buildCamera.WorldToScreenPoint(worldPos);
        }

        foreach (var pair in activeOptionNodeRects)
        {
            Vector3 worldPos = chuTransform.position + new Vector3(pair.Key.x * tileSize, pair.Key.y * tileSize, 0f);
            pair.Value.position = buildCamera.WorldToScreenPoint(worldPos);
        }
    }

    public void OnNodeClicked(BuildNodeButton node)
    {
        selectedNode = node;
        RefreshShopUI();
        if (moduleSelectionPanel != null) moduleSelectionPanel.SetActive(true);
    }

    public void RefreshShopUI()
    {
        if (shopContentContainer == null || moduleShopItemPrefab == null) return;

        foreach (Transform child in shopContentContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ModuleScriptableObject data in allAvailableModules)
        {
            if (data == null) continue;

            string cleanedKey = data.requiredTechKey != null ? data.requiredTechKey.Replace(" ", "") : "";

            bool isUnlocked = string.IsNullOrEmpty(cleanedKey);
            if (!isUnlocked && ResearchHandler.Instance != null)
            {
                switch (cleanedKey)
                {
                    case "EnergyProduction": isUnlocked = ResearchHandler.Instance.unlockedEnergyProduction; break;
                    case "MoltenMetal": isUnlocked = ResearchHandler.Instance.unlockedMoltenMetal; break;
                    case "BoltAcquisition": isUnlocked = ResearchHandler.Instance.unlockedBoltAcquisition; break;
                }
            }

            if (isUnlocked)
            {
                GameObject itemObj = Instantiate(moduleShopItemPrefab, shopContentContainer);
                ModuleShopItemButton script = itemObj.GetComponent<ModuleShopItemButton>();
                if (script != null) script.Setup(data);
            }
        }
    }

    public void SelectModuleToBuild(ModuleScriptableObject moduleData)
    {
        if (selectedNode == null || moduleData == null) return;

        ResourceHandler res = ResourceHandler.Instance;
        if (res == null) return;

        bool hasCost = moduleData.costType == CurrencyType.Scrap ? res.Scrap >= moduleData.buildCost : res.MetalBars >= moduleData.buildCost;
        if (!hasCost) return;

        if (moduleData.costType == CurrencyType.Scrap) res.ConsumeScrap(moduleData.buildCost);
        else res.ConsumeMetalBars(moduleData.buildCost);

        Vector2Int buildGridPos = selectedNode.GridPosition;
        Vector3 worldPos = chuTransform.position + new Vector3(buildGridPos.x * tileSize, buildGridPos.y * tileSize, 0f);

        GameObject newModuleObj = Instantiate(moduleData.modulePrefab, worldPos, Quaternion.identity, chuTransform);
        ModuleHealth moduleHealth = newModuleObj.GetComponent<ModuleHealth>();

        if (moduleHealth != null)
        {
            moduleHealth.GridPosition = buildGridPos;
            chronologicalModuleList.Add(moduleHealth);
        }

        occupiedTiles.Add(buildGridPos);

        SpawnOptionNodeAt(buildGridPos, moduleHealth);

        RebuildAllNodes();
        CheckEnergyBalance();

        if (moduleSelectionPanel != null) moduleSelectionPanel.SetActive(false);
        selectedNode = null;
    }

    public void CheckEnergyBalance()
    {
        ResourceHandler res = ResourceHandler.Instance;
        if (res == null) return;

        int maxThreshold = res.MaxEnergyThreshold;

        int totalPotentialDrain = 0;
        foreach (ModuleHealth mod in chronologicalModuleList)
        {
            if (mod != null)
            {
                totalPotentialDrain += mod.EnergyDrain;
            }
        }

        if (totalPotentialDrain > maxThreshold)
        {
            int runningDrain = totalPotentialDrain;

            for (int i = chronologicalModuleList.Count - 1; i >= 0; i--)
            {
                ModuleHealth mod = chronologicalModuleList[i];
                if (mod == null) continue;

                if (runningDrain > maxThreshold)
                {
                    mod.SetEnergyDeactivated(true);
                    runningDrain -= mod.EnergyDrain;
                }
                else
                {
                    mod.SetEnergyDeactivated(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < chronologicalModuleList.Count; i++)
            {
                ModuleHealth mod = chronologicalModuleList[i];
                if (mod != null && mod.IsDeactivated)
                {
                    mod.SetEnergyDeactivated(false);
                }
            }
        }
    }

    public void OnModuleDestroyed(Vector2Int destroyedGridPos, ModuleHealth module)
    {
        occupiedTiles.Remove(destroyedGridPos);
        chronologicalModuleList.Remove(module);

        RemoveOptionNodeAt(destroyedGridPos);

        CheckAndDestroyDisconnectedModules();
        RebuildAllNodes();
        CheckEnergyBalance();
    }

    private void CheckAndDestroyDisconnectedModules()
    {
        ModuleHealth chuModule = ModuleHealth.ActivePlayerModules.Find(m => m != null && m.IsCHUModule);
        if (chuModule == null) return;

        HashSet<ModuleHealth> connectedModules = new HashSet<ModuleHealth> { chuModule };
        Queue<ModuleHealth> queue = new Queue<ModuleHealth>();
        queue.Enqueue(chuModule);

        while (queue.Count > 0)
        {
            ModuleHealth current = queue.Dequeue();

            foreach (ModuleHealth candidate in ModuleHealth.ActivePlayerModules)
            {
                if (candidate == null || connectedModules.Contains(candidate)) continue;

                int dx = Mathf.Abs(current.GridPosition.x - candidate.GridPosition.x);
                int dy = Mathf.Abs(current.GridPosition.y - candidate.GridPosition.y);

                bool isNeighbor = current.IsCHUModule ? (dx <= 2 && dy <= 1) || (dx <= 1 && dy <= 2) : (dx + dy == 1);

                if (isNeighbor)
                {
                    connectedModules.Add(candidate);
                    queue.Enqueue(candidate);
                }
            }
        }

        List<ModuleHealth> disconnected = new List<ModuleHealth>();
        foreach (ModuleHealth module in ModuleHealth.ActivePlayerModules)
        {
            if (module != null && !module.IsCHUModule && !connectedModules.Contains(module))
            {
                disconnected.Add(module);
            }
        }

        for (int i = 0; i < disconnected.Count; i++)
        {
            Vector2Int pos = disconnected[i].GridPosition;
            occupiedTiles.Remove(pos);
            chronologicalModuleList.Remove(disconnected[i]);
            RemoveOptionNodeAt(pos);
            disconnected[i].DieDirectly();
        }
    }

    private void RebuildAllNodes()
    {
        HashSet<Vector2Int> validNodePositions = new HashSet<Vector2Int>();
        Vector2Int[] offsets = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

        foreach (Vector2Int tile in occupiedTiles)
        {
            foreach (Vector2Int offset in offsets)
            {
                Vector2Int neighbor = tile + offset;
                if (!occupiedTiles.Contains(neighbor)) validNodePositions.Add(neighbor);
            }
        }

        List<Vector2Int> nodesToRemove = new List<Vector2Int>();
        foreach (var pair in activeNodes)
        {
            if (!validNodePositions.Contains(pair.Key)) nodesToRemove.Add(pair.Key);
        }

        foreach (Vector2Int pos in nodesToRemove) RemoveNodeAt(pos);

        foreach (Vector2Int pos in validNodePositions)
        {
            if (!activeNodes.ContainsKey(pos)) SpawnNodeAt(pos);
        }
    }

    private void SpawnNodeAt(Vector2Int gridPos)
    {
        GameObject nodeObj = Instantiate(buildNodePrefab, nodeContainer != null ? nodeContainer : transform);
        BuildNodeButton nodeScript = nodeObj.GetComponent<BuildNodeButton>();
        if (nodeScript != null) nodeScript.Initialize(gridPos, this);

        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        activeNodes.Add(gridPos, nodeObj);
        activeNodeRects.Add(gridPos, rectTransform);
    }

    private void RemoveNodeAt(Vector2Int gridPos)
    {
        if (activeNodes.TryGetValue(gridPos, out GameObject nodeObj))
        {
            Destroy(nodeObj);
            activeNodes.Remove(gridPos);
            activeNodeRects.Remove(gridPos);
        }
    }

    private void SpawnOptionNodeAt(Vector2Int gridPos, ModuleHealth module)
    {
        if (nodeOptionPrefab == null) return;

        GameObject optionObj = Instantiate(nodeOptionPrefab, nodeContainer != null ? nodeContainer : transform);
        NodeOptionButton optionScript = optionObj.GetComponent<NodeOptionButton>();
        if (optionScript != null) optionScript.Initialize(gridPos, module);

        RectTransform rectTransform = optionObj.GetComponent<RectTransform>();
        activeOptionNodes.Add(gridPos, optionObj);
        activeOptionNodeRects.Add(gridPos, rectTransform);
    }

    private void RemoveOptionNodeAt(Vector2Int gridPos)
    {
        if (activeOptionNodes.TryGetValue(gridPos, out GameObject optionObj))
        {
            Destroy(optionObj);
            activeOptionNodes.Remove(gridPos);
            activeOptionNodeRects.Remove(gridPos);
        }
    }
}