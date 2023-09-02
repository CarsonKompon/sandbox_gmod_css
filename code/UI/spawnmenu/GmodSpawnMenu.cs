using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CarsonK;

[Library]
public partial class GmodSpawnMenu : Panel
{
	public static GmodSpawnMenu Instance;
	readonly Panel toollist;

	private static ModelList modelList;
	private bool isSearching;

	public GmodSpawnMenu()
	{
		Instance = this;

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );

			{
                var props = body.AddChild<GmodSpawnList>();
				var btnProps = tabs.AddButtonActive( "#spawnmenu.props", ( b ) => props.SetClass( "active", b ) );
                btnProps.Add.Panel("icon spawnlist");		
				
                modelList = body.AddChild<ModelList>();
				var btnSpawnlist = tabs.AddButtonActive( "#spawnmenu.modellist", ( b ) => modelList.SetClass( "active", b ) );
                btnSpawnlist.Add.Panel("icon models");	

				var ents = body.AddChild<EntityList>();
				var btnEnts = tabs.AddButtonActive( "#spawnmenu.entities", ( b ) => ents.SetClass( "active", b ) );
                btnEnts.Add.Panel("icon entities");

				var npclist = body.AddChild<NpcList>();
				var btnNPCs = tabs.AddButtonActive( "#spawnmenu.npclist", ( b ) => npclist.SetClass( "active", b ) );
                btnNPCs.Add.Panel("icon npcs");
                
                

                tabs.SelectedButton = btnProps;
			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				var btnTools = tabs.Add.Button( "#spawnmenu.tools" );
                btnTools.AddClass( "active" );
                btnTools.Add.Panel("icon tools");
				var btnUtility = tabs.Add.Button( "#spawnmenu.utility" );
                btnUtility.Add.Panel("icon utility");
			}
			var body = right.Add.Panel( "body" );
			{
                var listcontainer = body.Add.Panel( "listcontainer" );
                var listTextEntry = listcontainer.AddChild<TextEntry>();
				listTextEntry.Placeholder = "Quick Filter...";
                toollist = listcontainer.Add.Panel( "toollist" );
				{
					RebuildToolList();
				}
				body.Add.Panel( "inspector" );
			}
		}

	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );

        bool isOdd = false;

        toollist.Add.Label("All Tools", "header");

		foreach ( var entry in TypeLibrary.GetTypes<BaseTool>() )
		{
			if ( entry.Name == "BaseTool" )
				continue;

			var button = toollist.Add.Button( entry.Title );
			button.SetClass( "active", entry.ClassName == ConsoleSystem.GetValue( "tool_current" ) );
            if(isOdd) button.AddClass("odd");

			button.AddEventListener( "onclick", () => 
			{
				SetActiveTool( entry.ClassName );

				foreach ( var child in toollist.Children )
					child.SetClass( "active", child == button );
			} );

            isOdd = !isOdd;
		}
	}

	void SetActiveTool( string className )
	{
		// setting a cvar
		ConsoleSystem.Run( "tool_current", className );

		// set the active weapon to the toolgun
		if ( Game.LocalPawn is not Player player ) return;
		if ( player.Inventory is null ) return;

		// why isn't inventory just an ienumurable wtf
		for ( int i = 0; i < player.Inventory.Count(); i++ )
		{
			var entity = player.Inventory.GetSlot( i );
			if ( !entity.IsValid() ) continue;
			if ( entity.ClassName != "weapon_tool" ) continue;

			player.ActiveChildInput = entity;
		}
	}

	public override void Tick()
	{
		base.Tick();
		
		if( modelList.SearchInput.HasFocus )
		{
			isSearching = true;		
		}
		else if (isSearching && Input.Pressed( "menu" ))
		{
			isSearching = false;
		}
		
		UpdateActiveTool();
		
		if ( isSearching )
			return;

		Parent.SetClass( "spawnmenuopen", Input.Down( "menu" ) );

	}

	void UpdateActiveTool()
	{
		var toolCurrent = ConsoleSystem.GetValue( "tool_current" );
		var tool = string.IsNullOrWhiteSpace( toolCurrent ) ? null : TypeLibrary.GetType<BaseTool>( toolCurrent );

		foreach ( var child in toollist.Children )
		{
			if ( child is Button button )
			{
				child.SetClass( "active", tool != null && button.Text == tool.Title );
			}
		}
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildToolList();
	}
}
