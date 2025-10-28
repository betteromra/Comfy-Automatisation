using UnityEngine;

public struct Carrying
{
    public RessourceSO CurrenltyCarrying;
    public int Amount;

    public Carrying(RessourceSO carrying, int amount)
    {
        CurrenltyCarrying = carrying;
        Amount = amount;
    }
}

public class NPC : MonoBehaviour
{
    [SerializeField] private int _maxCarryingCapacity = 1;

    private Carrying _carrying;

    /// <summary>
    /// Tells the NPC to pick up a resource. Picks up one at a time.
    /// </summary>
    /// <param name="ressource">Resource that have been picked up</param>
    public void PickUp(RessourceSO ressource)
    {
        if (_carrying.CurrenltyCarrying == null)
            _carrying.CurrenltyCarrying = ressource;
        else if(_carrying.CurrenltyCarrying.actualName == ressource.actualName) //This is not a supersolid way of doing this, and if anyone have a good idea on a solution you're welcome to contact me regarding it.
        {
            if (_carrying.Amount < _maxCarryingCapacity)
                _carrying.Amount++;
        }
    }

    /// <summary>
    /// Calls the NPC to drop of resource. Drops of one resource at a time.
    /// </summary>
    /// <returns>Returns information about the dropped of resource.</returns>
    public Carrying DropOff()
    {
        RessourceSO currentCarry = _carrying.CurrenltyCarrying;

        if (_carrying.Amount > 0)
            _carrying.Amount--;

        if (_carrying.Amount == 0)
            _carrying.CurrenltyCarrying = null;

        return new Carrying(currentCarry, 1);
    }
}
