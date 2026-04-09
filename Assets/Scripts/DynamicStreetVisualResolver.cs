using Cysharp.Threading.Tasks;
using UnityEngine;

public class DynamicStreetVisualResolver : MonoBehaviour
{
    public SpriteRenderer TargetRenderer;
    public DynamicStreetItemType ItemType;
    public string ItemLabel;
    public Color PrimaryColor;
    public Color AccentColor;
    public bool Decorative;

    private bool _initialized;

    private void Start()
    {
        if (!_initialized)
        {
            _initialized = true;
            ResolveAsync().Forget();
        }
    }

    private async UniTaskVoid ResolveAsync()
    {
        if (TargetRenderer == null)
        {
            return;
        }

        Sprite upgraded = await DynamicStreetSpriteFactory.TryGetEnhancedSpriteAsync(ItemType, ItemLabel, PrimaryColor, AccentColor, Decorative);
        if (upgraded != null && TargetRenderer != null)
        {
            TargetRenderer.sprite = upgraded;
        }
    }
}
