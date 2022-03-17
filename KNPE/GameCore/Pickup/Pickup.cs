using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    public enum PickupType
    {
        Health,
        Grenade,
        TimeBonus,
        Null
    }

    class Pickup
    {
        BoundingSphere Collision;
        public PickupType CurrentType = PickupType.Null;
        public bool Alive = false;
        int Cooldown = 700;

        public void Spawn(Vector3 Position, PickupType Type)
        {
            Collision.Center = Position;
            CurrentType = Type;
            Alive = true;
            Cooldown = 500;
        }

        public void Update()
        {
            if (Alive)
            {
                Cooldown--;
                if (Cooldown < 1)
                {
                    Alive = false;
                }
                Vector3 PlayerTarget = (Player.Collision.Center - Collision.Center);
                if (PlayerTarget.Length() < 50)
                {
                    ActivatePickup();
                }
                switch (CurrentType)
                {
                    case PickupType.Grenade:
                        RenderEntity3d_Manager.RenderModel(9, Collision.Center, Matrix.Identity);
                        break;
                    case PickupType.Health:
                        RenderEntity3d_Manager.RenderModel(10, Collision.Center, Matrix.Identity);
                        break;
                    case PickupType.TimeBonus:
                        RenderEntity3d_Manager.RenderModel(12, Collision.Center, Matrix.Identity);
                        break;
                }

            }
        }

        public void ActivatePickup()
        {
            Game1.AudManager.PlaySound("Pickup");
            switch (CurrentType)
            {
                case PickupType.Grenade:
                    Player.GrenadeCount = 3;
                    break;
                case PickupType.Health:
                    if (Player.Health < 70)
                    {
                        Player.Health += 30;
                    }
                    else
                    {
                        Player.Health = 100;
                    }
                    break;
                case PickupType.TimeBonus:
                    Game_Core.EndTime -= TimeSpan.FromSeconds(30);
                    break;
            }
            Alive = false;
        }
    }
}
