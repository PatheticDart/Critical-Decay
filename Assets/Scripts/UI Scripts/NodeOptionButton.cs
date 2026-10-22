using UnityEngine;
using UnityEngine.UI;

public class NodeOptionButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ModuleHealth targetModule;
    private Vector2Int gridPosition;

    public Vector2Int GridPosition => gridPosition;
    public ModuleHealth TargetModule => targetModule;

    public void Initialize(Vector2Int gridPos, ModuleHealth module)
    {
        gridPosition = gridPos;
        targetModule = module;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (targetModule == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(2);
        }

        ModuleActionPanel actionPanel = FindObjectOfType<ModuleActionPanel>(true);
        if (actionPanel != null)
        {
            actionPanel.OpenPanel(targetModule);
        }
    }
}