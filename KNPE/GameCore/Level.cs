using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ziggyware;

namespace KNPE
{
    public class Entity
    {
        public string Class = "";
        public Vector3 Position;
        public int DoorNumber = 0;
        public int TargetDoor = 0;
        public String TargetLevel;
    }
    public static class Level
    {
        static public Model LevelStoreModel;
        static OCTree LevelOct = new OCTree();
        static Model3d LevelDrawModel;
        static char[] Delimiter = new char[1];
        static char[] EntityDelimiter = new char[1];

        static Entity[] Entitys;

        static public bool LevelLoaded = false;

        public static Vector3 LoadLevel(String LevelModelName, int DoorTarget)
        {
            Delimiter[0] = ' ';
            EntityDelimiter[0] = '|';
            LevelStoreModel = Game1.ContentMan.Load<Model>(LevelModelName);
            OCTreeBuilder builder = new OCTreeBuilder(600, 15, true, 0.0001f, 0.05f);
            LevelOct = builder.Build(LevelStoreModel);
            LevelDrawModel = new Model3d();
            LevelDrawModel.ContentModel = LevelStoreModel;
            LevelDrawModel.InitShader(Game1.ContentMan);
            //Load the XML file as well
            string EntityFile = Game1.ContentMan.Load<string>(LevelModelName + "_Entity");
            string[] EntitySplit = EntityFile.Split(EntityDelimiter);
            Entitys = new Entity[EntitySplit.Length - 1];
            for (int i = 0; i < EntitySplit.Length - 1; i++)
            {
                string[] Temp = EntitySplit[i].Split(Delimiter);
                if (Temp.Length > 1)
                {
                    Entitys[i] = new Entity();
                    Entitys[i].Class = Temp[0];
                    Entitys[i].Position = new Vector3(int.Parse(Temp[1]), int.Parse(Temp[3]), -int.Parse(Temp[2]));
                    Entitys[i].DoorNumber = int.Parse(Temp[4]);
                    Entitys[i].TargetDoor = int.Parse(Temp[5]);
                    Entitys[i].TargetLevel = Temp[6];
                }
            }
            for (int i = 0; i < Entitys.Length; i++)
            {
                if (Entitys[i].Class == "enemy1")
                {
                    Game_Core.CreateEnemy(Entitys[i].Position);
                }
            }
            for (int i = 0; i < Entitys.Length; i++)
            {
                if (Entitys[i].Class == "scanner")
                {
                    Game_Core.CreateScanner(Entitys[i].Position,Entitys[i].DoorNumber);
                }
            }
            for (int i = 0; i < Entitys.Length; i++)
            {
                if (Entitys[i].Class == "creeper")
                {
                    Game_Core.CreateCreeper(Entitys[i].Position);
                }
            }
            for (int i = 0; i < Entitys.Length; i++)
            {
                if (Entitys[i].Class == "hoverblaster")
                {
                    Game_Core.CreateHoverBlaster(Entitys[i].Position);
                }
            }
            for (int i = 0; i < Entitys.Length; i++)
            {
                if (Entitys[i].Class == "checkpoint")
                {
                    Game_Core.CreateSpawner(Entitys[i].Position);
                }
            }
            LevelLoaded = true;
            if (DoorTarget > 0)
            {
                for (int i = 0; i < Entitys.Length; i++)
                {
                    if (Entitys[i].Class == "door")
                    {
                        if (Entitys[i].DoorNumber == DoorTarget)
                        {
                            return Entitys[i].Position;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Entitys.Length; i++)
                {
                    if (Entitys[i].Class == "startpoint")
                    {
                        return Entitys[i].Position;
                    }
                }

            }

            return new Vector3(0,0,0);
        }

        public static String NearDoor(Vector3 Position, out int DoorTarget)
        {
            for (int i = 0; i < Entitys.Length; i++)
            {
                if (Entitys[i].Class == "door")
                {
                    float test = (Position - Entitys[i].Position).Length();
                    if (test < 175)
                    {
                        DoorTarget = Entitys[i].TargetDoor;
                        return Entitys[i].TargetLevel;
                    }
                }
            }
            DoorTarget = 0;
            return "";
        }

        public static void Draw(GraphicsDevice Device)
        {
            Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            Device.SamplerStates[0].AddressW = TextureAddressMode.Wrap;

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                        (float)Device.Viewport.Width / (float)Device.Viewport.Height,
                                                        1,
                                                        100000);
            Matrix Rotation = Matrix.CreateFromYawPitchRoll(0, 0, 0);
            Matrix World = Matrix.CreateWorld(Vector3.Zero,Vector3.Forward,Vector3.Up);
            LevelDrawModel.DrawWithCull(Rotation * World, Camera.GetViewMatrix(), projection, 1, 1);
        }

        public static Vector3 GetIntersectingPoly(Ray Target, float length)
        {

            OCTreeIntersection Result;
            LevelOct.GetIntersectingPolygon(ref Target, out Result);
            if (Result.DistanceSquared < length)
            {
                if (Result.IntersectType != OCTreeIntersectionType.None)
                {
                    return Result.IntersectionPoint;
                }
            }
            return Vector3.Zero;
        }

        public static Vector3 GetIntersectingPolyUnits(Ray Target, float length)
        {
            OCTreeIntersection Result;
            LevelOct.GetIntersectingPolygon(ref Target, out Result);
            if (Result.DistanceSquared < length * length)
            {
                if (Result.IntersectType != OCTreeIntersectionType.None)
                {
                    return Result.IntersectionPoint;
                }
            }
            return Vector3.Zero;
        }

        public static void SphereColliding(ref BoundingSphere Sphere, ref Vector3 Velocity, out bool OnGround)
        {
            List<OCTreeIntersection> sphereColliders = new List<OCTreeIntersection>();
            LevelOct.MoveSphere(ref Sphere,
                ref Velocity,
                10000,
                ref sphereColliders);

            OnGround = false;
            for (int x = 0; x < sphereColliders.Count; x++)
            {
                if (Vector3.Dot(sphereColliders[x].IntersectionNormal,
                    Camera.CameraUp) > 0.5f)
                {
                    OnGround = true;
                    break;
                }
            }
        }
    }
}
