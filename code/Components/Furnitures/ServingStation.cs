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

    public bool TryInteract( Player player )
    {
        var held = player.PlayerSlot.GetPickable();

        // If player is holding a plate, try to submit it
        if ( held is PlateItem plate )
        {
            return TrySubmitPlate( plate, player );
        }

        return false;
    }

    public bool TryAlternateInteract( Player player )
    {
        return TryInteract( player );
    }

    private static bool TrySubmitPlate( PlateItem plate, Player player )
    {
        // Check if the plate has a valid recipe
        if ( plate.Recipe is null )
        {
            Log.Warning( "Plate does not contain a valid recipe" );
            // TODO: Add negative feedback (sound, visual effect, etc.)
            return false;
        }

        // Try to complete the order
        if ( OrderManager.Instance.TryCompleteOrder( plate.Recipe ) )
        {
            Log.Info( $"Successfully submitted order: {plate.Recipe}" );

            // Remove the plate from player's hands and destroy it
            player.PlayerSlot.TakePickable();
            plate.GameObject.Destroy();

            // TODO: Add positive feedback (sound, visual effect, particle effects, etc.)
            // TODO: Add score/money to the player

            return true;
        }
        else
        {
            Log.Warning( $"No matching order for recipe: {plate.Recipe}" );
            // TODO: Add negative feedback (wrong order sound, etc.)
            return false;
        }
    }
}
