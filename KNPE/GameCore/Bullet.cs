using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace KNPE
{
    class Bullet
    {
        Vector3 Velocity = Vector3.Zero;
        BoundingSphere Collision = new BoundingSphere(new Vector3(0, 0, 1), 8);
        public bool Alive = false;
        int Damage = 2;
        Matrix Rotation = Matrix.Identity;
        bool GoodBullet = true;
        int RayCheck = 0;//Trace checks are expensive, don't do every frame
        public void Fire(Vector3 Position, Vector3 inVelocity, bool OwnerGood)
        {
            GoodBullet = OwnerGood;
            Collision.Center = Position;
            Velocity = inVelocity;
            Alive = true;
        }
        public void Update()
        {
            if (Alive == true)
            {
                Collision.Center += Velocity;
                Rotation = Matrix.CreateBillboard(Collision.Center, Collision.Center - Velocity, Camera.CameraUp, Velocity);
                Rotation.Translation = Vector3.Zero;
                if (GoodBullet)
                {
                    for (int i = 0; i < Game_Core.Enimies.Length; i++)
                    {
                        if (Collision.Intersects(Game_Core.Enimies[i].Collision) && Game_Core.Enimies[i].Alive)
                        {
                            Game_Core.Enimies[i].Damage(Collision.Center, Damage);
                            //ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                            Alive = false;
                        }
                    }
                    /*
                    for (int i = 0; i < Game_Core.Scanners.Length; i++)
                    {
                        if (Collision.Intersects(Game_Core.Scanners[i].Collision) && Game_Core.Scanners[i].Alive)
                        {
                            Game_Core.Scanners[i].Damage(Damage);
                            ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                            Alive = false;
                        }
                    }
                     */
                    RenderEntity3d_Manager.RenderModel(2, Collision.Center, Rotation);
                    //ParticleManger.RocketTrailEffect(Collision.Center);
                }
                else
                {
                    if (Collision.Intersects(Player.Collision))
                    {
                        Player.Health -= 10;
                        Alive = false;
                        ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                        Game1.AudManager.PlaySound("Explosion", Collision.Center);
                        RumbleCore.Rumble(0.5f, 500);
                    }
                    RenderEntity3d_Manager.RenderModel(4, Collision.Center, Rotation);
                    //ParticleManger.BulletEvilTrailEffect(Collision.Center);

                }
                if (RayCheck == 0)
                {
                    Ray Next = new Ray(Collision.Center, Velocity);
                    Vector3 CollisionPoint = Level.GetIntersectingPoly(Next, 1750);
                    if (CollisionPoint != Vector3.Zero)
                    {
                        if ((Collision.Center - CollisionPoint).Length() < 300)
                        {
                            if (GoodBullet)
                            {
                                //ParticleManger.GoodExplodeParticleEffect(Collision.Center);
                            }
                            else
                            {
                                ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                                Game1.AudManager.PlaySound("Explosion", Collision.Center);
                            }
                            Alive = false;
                        }
                        else
                        {
                            RayCheck = 0;
                            return;
                        }
                    }
                    RayCheck = 3;
                }
                else
                {
                    RayCheck--;
                }
            }
        }
    }
}
