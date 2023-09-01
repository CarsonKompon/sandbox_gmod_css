using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace CarsonK;

public class GmodInventoryBar : Panel
{
	List<Panel> slots = new();
	List<Entity> inventory = new();
	int activeSlot = 0;
	int subSlot = 0;
	RealTimeSince lastChange = 2f;

	List<string[]> forcedSlots = new()
	{
		new string[] { "Fists", "Grav Gun", "Phys Gun", "Crowbar" },
		new string[] { "Pistol", "Magnum", "Revolver" },
		new string[] { "MP5", "SMG", "AK", "Assault", "Rifle" },
		new string[] { "Shotgun", "Barrel", "Crossbow", "Bolt" },
		new string[] { "Grenade", "RPG" },
		new string[] { "Flashlight", "Tool", "Camera" }
	};

	public GmodInventoryBar()
	{
		for ( int i = 0; i < 9; i++ )
		{
			var slot = Add.Panel( "slot" );
			slots.Add( slot );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var player = Game.LocalPawn as Player;
		if ( player == null ) return;
		if ( player.Inventory == null ) return;

		for(int i=0; i < player.Inventory.Count(); i++)
		{
			Entity item = player.Inventory.GetSlot(i);
			GmodInventoryIcon icon;
			if(!inventory.Contains(item))
			{
				int slotIndex = 0;
				bool forced = false;
				var di = DisplayInfo.For( item );
				foreach(var forcedSlot in forcedSlots)
				{
					foreach(var forcedName in forcedSlot)
					{
						if(di.Name.ToLower().Contains(forcedName.ToLower()))
						{
							slotIndex = forcedSlots.IndexOf(forcedSlot);
							forced = true;
							break;
						}
					}
				}
				if(!forced)
				{
					while(slotIndex < slots.Count - 1 && slots[slotIndex].ChildrenCount > 1)
					{
						slotIndex++;
					}
				}

				var slot = slots[slotIndex];
				icon = new GmodInventoryIcon( slotIndex, this );
				slot.AddChild( icon );
				inventory.Add(item);
			}
			else
			{
				icon = GetIcon(item);
			}
			UpdateIcon(item, icon, i);
		}

		for(int i=0; i<inventory.Count; i++)
		{
			var item = inventory[i];
			if(!player.Inventory.Contains(item))
			{
				var icon = GetIcon(item);
				icon.Delete();
				inventory.Remove(item);
			}
		}

		foreach(var slot in slots)
		{
			slot.SetClass("hidden", slot.ChildrenCount == 0);
		}

		SetClass("hidden", lastChange >= 2f);
	}

	private GmodInventoryIcon GetIcon(Entity ent)
	{
		foreach(var slot in slots)
		{
			foreach(var icon in slot.Children)
			{
				if(icon is GmodInventoryIcon gmodIcon)
				{
					if(gmodIcon.TargetEnt == ent)
					{
						return gmodIcon;
					}
				}
			}
		}
		return null;
	}

	private void UpdateIcon( Entity ent, GmodInventoryIcon inventoryIcon, int i )
	{
		var player = Game.LocalPawn as Player;

		if ( ent == null )
		{
			inventoryIcon.Clear();
			return;
		}

		var di = DisplayInfo.For( ent );

		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = di.Name;

		string icon = "";
		string name = di.Name.ToLower();
		if(name.Contains("phys gun")) icon = "h";
		else if(name.Contains("grav gun") || name.Contains("gravity")) icon = "m";
		else if(name.Contains("pistol")) icon = "d";
		else if(name.Contains("smg") || name.Contains("mp4") || name.Contains("mp5")) icon = "a";
		else if(name.Contains("crowbar") || name.Contains("fists")) icon = "c";
		else if(name.Contains("rpg") || name.Contains("rocket") || name.Contains("launcher")) icon = "i";
		else if(name.Contains("grenade")) icon = "k";
		else if(name.Contains("crossbow") || name.Contains("bolt")) icon = "g";
		else if(name.Contains("rifle") || name.Contains("ak ") || name.Contains("ak-")) icon = "l";
		else if(name.Contains("shotgun") || name.Contains("barrel")) icon = "b";
		else if(name.Contains("revolver") || name.Contains("magnum")) icon = "e";
		else if(name.Contains("bug")) icon = "j";
		else if(name.Contains("flashlight")) icon = "w";
		else if(name.Contains("tool")) icon = "|";
		inventoryIcon.Icon.Text = icon;

		bool isActive = player.ActiveChild == ent;
		int thisSubSlot = slots[inventoryIcon.Slot].GetChildIndex(inventoryIcon);
		inventoryIcon.SetClass( "active", isActive );
		inventoryIcon.SetClass( "top", thisSubSlot == 0);
		if(isActive)
		{
			SetActiveSlot(inventoryIcon.Slot);
			subSlot = thisSubSlot;
		}
	}

	[Event.Client.BuildInput]
	public void ProcessClientInput()
	{
		var player = Game.LocalPawn as Player;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}

		if ( Input.Pressed( "slot1" ) ) SwitchSlot( 0 );
		if ( Input.Pressed( "slot2" ) ) SwitchSlot( 1 );
		if ( Input.Pressed( "slot3" ) ) SwitchSlot( 2 );
		if ( Input.Pressed( "slot4" ) ) SwitchSlot( 3 );
		if ( Input.Pressed( "slot5" ) ) SwitchSlot( 4 );
		if ( Input.Pressed( "slot6" ) ) SwitchSlot( 5 );
		if ( Input.Pressed( "slot7" ) ) SwitchSlot( 6 );
		if ( Input.Pressed( "slot8" ) ) SwitchSlot( 7 );
		if ( Input.Pressed( "slot9" ) ) SwitchSlot( 8 );

		if ( Input.MouseWheel != 0 ) SwitchSlotDelta( -Input.MouseWheel );
	}

	private void SetActiveSlot(int ind)
	{
		for(int i=0; i<slots.Count; i++)
		{
			slots[i].SetClass("active", i == ind);
		}
		activeSlot = ind;
	}

	private void SwitchSlot(int ind)
	{
		if(ind == activeSlot)
		{
			int newIndex = (subSlot + 1) % slots[activeSlot].ChildrenCount;
			SetActiveEntityFromSlot(activeSlot, newIndex);
		}
		else
		{
			SetActiveEntityFromSlot(ind, 0);
		}
	}

	private void SwitchSlotDelta(int delta)
	{
		subSlot += delta;
		if(subSlot < 0)
		{
			do { activeSlot--; if(activeSlot < 0) activeSlot = slots.Count - 1; } while ( slots[activeSlot].ChildrenCount == 0 );
			subSlot = slots[activeSlot].ChildrenCount - 1;
			SetActiveEntityFromSlot(activeSlot, subSlot);
		}
		else if(subSlot >= slots[activeSlot].ChildrenCount)
		{
			do { activeSlot++; if(activeSlot >= slots.Count) activeSlot = 0; } while ( slots[activeSlot].ChildrenCount == 0 );
			subSlot = 0;
			SetActiveEntityFromSlot(activeSlot, subSlot);
		}
		else
		{
			SetActiveEntityFromSlot(activeSlot, subSlot);
		}
	}

	private void SetActiveEntity(Entity ent)
	{
		var player = Game.LocalPawn as Player;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( player.ActiveChild == ent )
			return;

		if ( ent == null )
			return;

		player.ActiveChildInput = ent;
		lastChange = 0f;
	}

	private void SetActiveEntityFromSlot(int slot, int sub)
	{
		var player = Game.LocalPawn as Player;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		var child = slots[slot].GetChild(sub);
		if(child is GmodInventoryIcon icon)
		{
			SetActiveEntity(icon.TargetEnt);
		}
	}
}
