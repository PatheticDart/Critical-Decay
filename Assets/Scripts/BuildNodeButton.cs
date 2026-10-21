using UnityEngine;
using UnityEngine.UI;

public class BuildNodeButton : MonoBehaviour
{
    [System.NonSerialized] public Vector2Int GridPosition;
    private BuildSystem buildSystem;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(Vector2Int gridPos, BuildSystem system)
    {
        GridPosition = gridPos;
        buildSystem = system;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void OnButtonClicked()
    {
        Debug.Log($"BuildNodeButton clicked at Grid Position: {GridPosition}");
        if (buildSystem != null)
        {
            buildSystem.OnNodeClicked(this);
        }
    }
}