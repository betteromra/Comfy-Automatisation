using UnityEngine;

public class CarboardEffect : MonoBehaviour
{
    [SerializeField] SpriteRenderer _backSprite;
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.RegisterSpriteChangeCallback(OnSpriteChanged);
        OnSpriteChanged(spriteRenderer);
    }
    void OnSpriteChanged(SpriteRenderer changedSpriteRenderer)
    {
        _backSprite.sprite = changedSpriteRenderer.sprite;
    }
}
