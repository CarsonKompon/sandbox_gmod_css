using System.Runtime.InteropServices.ComTypes;
using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Editor;
using Sandbox.UI.Construct;

namespace CarsonK;

/// <summary>
/// An entity that applies gmod-styled CSS to connected clients when created
/// </summary>
[Library("carson_sandbox_gmod_css"), HammerEntity]
public partial class GmodCSSApplier : ModelEntity, IUse
{
    public virtual bool IsUsable( Entity user ) => true;

    ApplierNametag Nametag;

    public override void Spawn()
    {
        base.Spawn();

        Model = Cloud.Model("mdlresrc/facepunch_logo");
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        Nametag = new ApplierNametag(this);
    }

    public virtual bool OnUse(Entity user)
    {
        ApplyStylesheet(To.Single(user));

        return false;
    }

    [ClientRpc]
    void ApplyStylesheet()
    {
        if(Game.RootPanel != null)
        {
            Panel root = Game.RootPanel;

            foreach(var sheet in root.AllStyleSheets)
            {
                if(sheet.FileName == "/ui/gmod.scss")
                {
                    root.Delete(true);
                    _ = new SandboxHud();
                    return;
                }
            }

            root.StyleSheet.Load("/ui/gmod.scss");

            for(int z=0; z<root.ChildrenCount; z++)
            {
                var child = root.GetChild(z);
                switch(child.GetType().Name)
                {
                    case "Health":
                        child.Add.Label("HEALTH", "health-header");
                        break;
                    
                    case "Chat":

                        for(int i=0; i<child.ChildrenCount; i++)
                        {
                            var chatChild = child.GetChild(i);
                            if(chatChild is TextEntry chatEntry)
                            {
                                var entryPanel = child.Add.Panel("textentry-container");
                                entryPanel.Add.Label("Say :", "textentry-say");
                                entryPanel.AddChild(chatEntry);
                            }
                        }

                        break;
                    
                    case "InventoryBar":
                        var newInventoryBar = root.AddChild<GmodInventoryBar>();
                        root.SetChildIndex(newInventoryBar, root.GetChildIndex(child));
                        child.Delete(true);
                        
                        break;
                }
            }
        }

        Log.Info("gmod time");
    }
}