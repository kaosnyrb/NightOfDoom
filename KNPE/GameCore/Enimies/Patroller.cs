using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE.GameCore.Enimies
{
    class Patroller : Enemy
    {

        int MoveCheck = 0;

        public override void Init(Vector3 StartPos)
        {
            base.Init(StartPos);
        }

        public override void Update(GameTime Time)
        {
            if (Alive)
            {
                //Movement, Erratic
                if (MoveCheck == 0)
                {
                    Ray TestMovement = new Ray(Collision.Center, Velocity);
                    Vector3 Test = Collision.Center + new Vector3(0, 30, 0);
                    Ray TestMovement2 = new Ray(Test, Velocity);

                    if (Level.GetIntersectingPoly(TestMovement, 2000) != Vector3.Zero)// || Level.GetIntersectingPoly(TestMovement2, 2000) != Vector3.Zero)
                    {
                        RandomDirection();
                    }
                    MoveCheck = 5;

                    Rotation = Matrix.CreateBillboard(Collision.Center, Collision.Center - Velocity, Camera.CameraUp, Velocity);
                    Rotation.Translation = Vector3.Zero;

                }
                if (MoveCheck > 0) MoveCheck--;
                Collision.Center += Velocity;
                if (Health < 1)
                {
                    Alive = false;
                    ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                }
                RenderEntity3d_Manager.RenderModel(1, Collision.Center, Rotation);
            }
        }
        public override void Damage(Vector3 Position, int Amount)
        {
            Health -= Amount;
            if (Health < 1)
            {
                Alive = false;
                ParticleManger.ExplodeParticleEffect(Collision.Center);
            }
        }
        private void RandomDirection()
        {
            Random TempRand = new Random();
            switch (TempRand.Next(4))
            {
                case 0:
                    Velocity = new Vector3(1, 0, 0);
                    break;
                case 1:
                    Velocity = new Vector3(0, 0, 1);
                    break;
                case 2:
                    Velocity = new Vector3(-1, 0, 0);
                    break;
                case 3:
                    Velocity = new Vector3(0, 0, -1);
                    break;
            }
        }
    }
}
