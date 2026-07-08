using UnityEngine;

public class PettableCat : Interactable
{
    public override void Interact(PlayerActions player)
    {
        player.TryPetCat(this);
    }
}
