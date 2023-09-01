
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CarsonK;

public class GmodInventoryIcon : Panel
{
	public Entity TargetEnt;
	public Label Label;
	public Label Number;
	public Label Icon;
	public int Slot;

	public GmodInventoryIcon( int i, Panel parent )
	{
		Parent = parent;
		Slot = i;
		Label = Add.Label( "empty", "item-name" );
		Number = Add.Label( $"{i + 1}", "slot-number" );
		Icon = Add.Label( "", "item-icon" );
	}

	public void Clear()
	{
		Label.Text = "";
		SetClass( "active", false );
	}
}
