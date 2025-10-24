#nullable enable

using Undercooked.Components.Interfaces;
using Undercooked.Components.Enums;

namespace Undercooked.Components;

[Icon( "room_service" )]
[Title( "Serving Station" )]
[Description( "A station where players can submit completed plates to fulfill orders" )]
public class ServingStation : Component, IInteractable
{
    public InteractionType InteractionType => InteractionType.Press;

    public InteractionType AlternateInteractionType => InteractionType.Press;

    [Rpc.Host]
    public void TryInteract( Player by )
    {
        var held = by.PlayerSlot.StoredPickable;

        // If player is holding a plate, try to submit it
        if ( held is PlateItem plate )
        {
            SubmitPlate( plate, by );
        }
    }

    [Rpc.Host]
    public void TryAlternateInteract( Player by )
    {
        // Alternate interaction is the same as primary interaction
        TryInteract( by );
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
