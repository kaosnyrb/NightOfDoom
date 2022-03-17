using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    class BaseWeapon
    {
        public int Firecooldown = 0;
        public virtual void Update()
        {
            if ( Firecooldown > 0 ) Firecooldown--;
        }
        public virtual void PrimaryFire(Vector3 Position)
        {

        }
        public virtual void SecondaryFire(Vector3 Position)
        {

        }
    }
}
