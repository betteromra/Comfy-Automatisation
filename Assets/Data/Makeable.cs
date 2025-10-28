using UnityEngine;

public interface Makeable
{
  public Ingredient[] ingredients { get; }
  void Make(Inventory inventory)
  {
    
  }
}
