using UnityEngine;
using UnityEngine.UI;

public class ModuleSelectItem : MonoBehaviour
{
    [Tooltip("The index corresponding to the module in BuildSystem's Available Module Prefabs array.")]
    [SerializeField] private int moduleIndex;
    [SerializeField] private BuildSystem buildSystem;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (buildSystem == null)
        {
            buildSystem = FindFirstObjectByType<BuildSystem>();
        }
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnSelectModule);
        }
    }

    private void OnSelectModule()
    {
        if (buildSystem != null)
        {
            //buildSystem.SelectModuleToBuild(moduleIndex);
        }
    }
}