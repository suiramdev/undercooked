#nullable enable

namespace Undercooked;

[Icon( "room_service" )]
[Title( "Serving Station" )]
[Description( "A station where players can submit completed plates to fulfill orders" )]
public class ServingStation : Component, IInteractable
{
    public InteractionType InteractionType => InteractionType.Press;

    public InteractionType AlternateInteractionType => InteractionType.Press;

    public string InteractionText => "Submit";

    public string? AlternateInteractionText => null;

    public bool CanInteract( Player by )
    {
        return by.StoredPickable is PlateItem;
    }

    [Rpc.Host]
    public void TryInteract( Player by )
    {
        if ( !CanInteract( by ) ) return;

        var held = by.StoredPickable;

        // If player is holding a plate, try to submit it
        if ( held is PlateItem plate )
        {
            SubmitPlate( plate, by );
        }
    }

    public bool CanAlternateInteract( Player by )
    {
        return false;
    }

    [Rpc.Host]
    public void TryAlternateInteract( Player by )
    {
        return;
    }

    [Rpc.Host]
    private static void SubmitPlate( PlateItem plate, Player _ )
    {
        // Check if the plate has a valid recipe
        if ( plate.Recipe is null )
        {
            Log.Warning( "Plate does not contain a valid recipe" );
            // TODO: Add negative feedback (sound, visual effect, etc.)
            return;
        }

        OrderManager.Instance.CompleteOrder( plate.Recipe );
    }
}
