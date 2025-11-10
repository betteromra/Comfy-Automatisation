using System.Collections.Generic;
using UnityEngine;

public class BuildingNode : MonoBehaviour
{
  [SerializeField] protected Inventory _inventory;
  public Inventory inventory { get => _inventory; }
  Color _color;
  [SerializeField] Color _lastColour;
  [SerializeField] Color _selectedColour;
  [SerializeField] SpriteRenderer _spriteRenderer;
  private void Awake()
  {
    _color = _spriteRenderer.color;
  }
  public void HighlightSelected()
  {
    _spriteRenderer.color = _selectedColour;
  }

  public void HighlightLast()
  {
    _spriteRenderer.color = _lastColour;
  }

  public void RemoveHighlight()
  {
    _spriteRenderer.color = _color;
  }
}
