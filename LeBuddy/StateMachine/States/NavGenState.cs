using AOSharp.Core;
using AOSharp.Core.Misc;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AOSharp.Recast;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LeBuddy.IPCMessages;
using AOSharp.Common.GameData;
using org.critterai.nav;
using org.critterai.nmgen;
using AOSharp.Core.IPC;

namespace LeBuddy
{
    public class NavGenState : IState
    {
        private string filePath = $"{LeBuddy.PluginDir}\\Navmeshes\\{Playfield.ModelIdentity.Instance}.Navmesh";

        static double navGenWait;

        private bool NavmeshFileExists => File.Exists(filePath);

        public IState GetNextState()
        {
            if (LeBuddy._settings["Enable"].AsBool())
            {
                if (NavmeshFileExists)
                {
                    if (Playfield.IsDungeon)
                    {
                        LeBuddy.IPCChannel.Broadcast(new EnterMessage());
                        MovementController.Instance.SetMovement(MovementAction.BackwardStart);
                    }

                    if (!Playfield.IsDungeon)
                    {
                        return new EnterState();
                    }
                }
            
                if (!NavmeshFileExists && !Playfield.IsDungeon)
                {
                    return new IdleState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("NavGenState");

            if (!NavmeshFileExists && Playfield.IsDungeon)
            {
                if (Time.NormalTime > navGenWait + 20)
                {
                    navGenWait = Time.NormalTime;

                    NavGen();  
                }
            }
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit NavGenState");
            MovementController.Instance.Halt();
        }

        public void Tick()
        {
            if (Game.IsZoning)
                return;
        }

        public void NavGen()
        {
            NMGenParams NMGenParams = new NMGenParams
            {
                xzCellSize = 0.2f,
                yCellSize = 0.1f,
                walkableSlope = 45f,
                walkableHeight = 19,
                walkableStep = 3,
                walkableRadius = 2,
                maxEdgeLength = 20,
                edgeMaxDeviation = 4f,
                minRegionArea = 400,
                mergeRegionArea = 75,
                maxVertsPerPoly = 6,
                detailSampleDistance = 6f,
                detailMaxDeviation = 1f,
                contourOptions = ContourBuildFlags.TessellateWallEdges | ContourBuildFlags.TessellateAreaEdges,
                useMonotone = false,
                tileSize = 512,  //this.tileSize,
                borderSize = 16
            };

            NavmeshGenerator.BakeAsync(NMGenParams).ContinueWith(async (task) =>
            {
                if (task.IsFaulted)
                {
                    //string errorMsg = task.Exception.ToString();
                    //Chat.WriteLine(errorMsg);
                    Chat.WriteLine("NavGen Failed");
                    LeBuddy.IPCChannel.Broadcast(new ClearSelectedMemberMessage());
                    MovementController.Instance.SetMovement(MovementAction.BackwardStart);
                }
                else
                {
                    //Chat.WriteLine("NavGen Success");

                    if (!NavmeshFileExists)
                    {
                        Save(task.Result);
                        Chat.WriteLine("NavGen Saved");
                        EnterState.NavGenSuccessful = true; 
                    }
                }
            });
        }

        public void Save(Navmesh navmesh)
        {
            try
            {
                //Chat.WriteLine($"Saving navmesh to {filePath}");
                navmesh.Save(filePath);
                EnterState.NavGenSuccessful = true;

            }
            catch (Exception ex)
            {
                Chat.WriteLine($"Failed to save navmesh: {ex.Message}");
            }
        }
        public static void DeleteNavMesh()
        {
            string navMeshFolderPath = $"{LeBuddy.PluginDir}\\NavMeshes\\";
            string fileToKeep = "4364.Navmesh";

            DirectoryInfo di = new DirectoryInfo(navMeshFolderPath);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name.EndsWith(".Navmesh") && file.Name != fileToKeep)
                {
                    file.Delete();
                }
            }
        }
    }
}
