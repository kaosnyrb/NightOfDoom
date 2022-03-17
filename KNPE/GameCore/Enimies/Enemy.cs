using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    public enum EnemyType
    {
        Creeper,
        Patroller,
        HoverBlaster,
        EnemySpawner,
        Orge,
        Null
    }
    class Enemy
    {
        protected Vector3 Velocity = new Vector3(0, 0, 0);
        public BoundingSphere Collision = new BoundingSphere(new Vector3(0, 0, 1), 10);
        public EnemyType Type = EnemyType.Null;
        protected int Health = 10;
        public bool Alive = false;
        protected Matrix Rotation = Matrix.Identity;

        public Enemy()
        {

        }

        public virtual void Init(Vector3 StartPos)
        {
            Alive = true;
            Collision.Center = StartPos;
        }
        public virtual void Update(GameTime Time)
        {
           
            
        }
        public virtual void Damage(Vector3 Position, int Amount)
        {

        }

    }
}
