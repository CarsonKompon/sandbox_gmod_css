using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;

namespace CarsonK;

[Library]
public partial class GmodSpawnList : Panel
{
	VirtualScrollPanel Canvas;

	public GmodSpawnList()
	{
		AddClass( "spawnpage" );

		var searchContainer = Add.Panel("searchcontainer");
		var searchEntry = searchContainer.AddChild<TextEntry>();
		searchEntry.Placeholder = "Search...";
		var searchList = searchContainer.Add.Panel("searchlist");
		var allModelsEntry = searchList.Add.Panel("folder");
		allModelsEntry.Add.Panel( "icon all-models" );
		allModelsEntry.Add.Label( "All Models", "foldername" );
		allModelsEntry.AddClass("active");

		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemWidth = 100;
		Canvas.Layout.ItemHeight = 100;

		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var file = (string)data;
			var panel = cell.Add.Panel( "icon" );
			panel.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn", "models/" + file ) );
			panel.Style.BackgroundImage = Texture.Load( FileSystem.Mounted, $"/models/{file}_c.png", false );
			panel.Tooltip = file;
		};

        foreach( var folder in FileSystem.Mounted.FindDirectory( "" ) )
		{
			bool hasModels = false;

			foreach ( var file in FileSystem.Mounted.FindFile( folder, "*.vmdl_c.png", true ) )
			{
				if ( string.IsNullOrWhiteSpace( file ) ) continue;
				if ( file.Contains( "_lod0" ) ) continue;
				if ( file.Contains( "clothes" ) ) continue;

				Canvas.AddItem( file.Remove( file.Length - 6 ) );
				hasModels = true;
			}

			if(hasModels)
			{
				var folderEntry = searchList.Add.Panel("folder");
				folderEntry.Add.Panel( "icon" );
				folderEntry.Add.Label( folder, "foldername" );
			}

		}
	}
}
