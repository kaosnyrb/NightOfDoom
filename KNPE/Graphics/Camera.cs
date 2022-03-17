using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace KNPE
{
    /*
    class Camera
    {
        
        public static Vector3 Camera_Position = new Vector3(0, 0, 0);
        public static Vector3 Camera_Rotation = new Vector3(0, 0, 0);
        public static Vector3 Camera_RotationRef;
        public static Vector3 CameraForward;
        public static Vector3 CameraRight;
        public static Vector3 CameraUp = new Vector3(0,1,0);
        public static Vector3 CameraTarget = new Vector3(0, 0, 1);
        public static float SafeZone = 5; //Vector units to keep the camera in
        private static Matrix View;

        public static void Translate(float forward, float right, float up)
        {
            // Move the camera position, and calculate a 
            // new target
            //Vector3 direction = Target - Pos;
            //direction.Normalize();
            // Shortcut to pull the above vector from the view matrix
            Vector3 direction = new Vector3(-View.M13, -View.M23, -View.M33);
            Vector3 Temp = Camera_Position;
            Camera_Position += direction * forward;
            Camera_Position += CameraRight * right;
            Camera_Position += CameraUp * up;

            CameraTarget = Camera_Position + direction;
            // Calculate the new view matrix
            View = Matrix.CreateLookAt(Camera_Position, CameraTarget, CameraUp);
        }
        public static void Translate(Vector3 Input)
        {
            Vector3 direction = new Vector3(-View.M13, -View.M23, -View.M33);
            Vector3 Temp = Camera_Position;
            Camera_Position += Input;
            CameraTarget = Camera_Position + direction;
            // Calculate the new view matrix
            View = Matrix.CreateLookAt(Camera_Position, CameraTarget, CameraUp);
        }
        public static Matrix GetViewMatrix()
        {
            Camera_RotationRef = Camera_Rotation;
            Camera_Rotation = new Vector3(0, 0, 0);
            View = GetViewMatrix(ref Camera_Position, ref CameraTarget, ref CameraUp, Camera_RotationRef.Y, Camera_RotationRef.X, Camera_RotationRef.Z);
            return View;
        }

        public static Quaternion yawpitch;
        public static Matrix GetViewMatrix(ref Vector3 position, ref Vector3 target,
                                           ref Vector3 up, float yaw, float pitch, float roll)
        {
            // The right vector can be inferred
            CameraForward = target - position;
            CameraRight = Vector3.Cross(CameraForward, up);

            // This quaternion is the total of all the
            // specified rotations
            yawpitch = CreateFromYawPitchRoll(up, yaw,
                CameraRight, pitch, CameraForward, roll);

            // Calculate the new target position, and the
            // new up vector by transforming the quaternion
            target = position + Vector3.Transform(CameraForward, yawpitch);
            up = Vector3.Transform(up, yawpitch);
            return Matrix.CreateLookAt(position, target, up);
        }
        public static Quaternion CreateFromYawPitchRoll(Vector3 up, float yaw, Vector3 right,
                                                        float pitch, Vector3 forward, float roll)
        {
            // Create a quaternion for each rotation, and multiply them
            // together.  We normalize them to avoid using the conjugate
            Quaternion qyaw = Quaternion.CreateFromAxisAngle(up, (float)yaw);
            qyaw.Normalize();
            Quaternion qtilt = Quaternion.CreateFromAxisAngle(right, (float)pitch);
            qtilt.Normalize();
            Quaternion qroll = Quaternion.CreateFromAxisAngle(forward, (float)roll);
            qroll.Normalize();
            Quaternion yawpitch = qyaw * qtilt * qroll;
            yawpitch.Normalize();

            return yawpitch;
        }
        public static void CreateLookAt(Vector3 Target, Vector3 Position)
        {
            Camera_Position = Position;
            CameraTarget = Target;
            CameraForward = Target - Position;
            CameraForward.Normalize();
            CameraRight = Vector3.Cross(CameraForward, CameraUp);
            CameraRight.Normalize();
            GetViewMatrix();
        }
    }*/

    class Camera
    {
        public static Vector3 CameraForward = new Vector3(0, 0, 0);
        public static Vector3 CameraRight = new Vector3(0, 0, 0);
        public static Vector3 CameraPosition = new Vector3(0, 0, 0);
        public static Vector3 CameraTarget = new Vector3(0, 0, 1);

        public static Matrix ViewMatrix;
        public static BoundingFrustum Frustum;

        public static Vector3 CameraUp = Vector3.Up;

        public static Matrix GetViewMatrix()
        {           
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    (float)1280 / (float)720,
                                                                    1,
                                                                    175000);
            Frustum = new BoundingFrustum(ViewMatrix * projection);
            ViewMatrix = Matrix.CreateLookAt(CameraPosition, CameraTarget, CameraUp);
            //CameraForward = CameraTarget - CameraPosition;
            //CameraForward.Normalize();
            //CameraRight = Vector3.Cross(CameraForward, Vector3.Up);
            //CameraRight.Normalize();
        

            return ViewMatrix;
        }

        public static void CreateLookAt(Vector3 Position, Vector3 Target)
        {
            CameraPosition = Position;
            CameraTarget = Target;
            CameraForward = CameraTarget - CameraPosition;
            CameraForward.Normalize();
            CameraRight = Vector3.Cross(CameraForward, CameraUp);
            CameraRight.Normalize();
            ViewMatrix = Matrix.CreateLookAt(CameraPosition, CameraTarget, CameraUp);
        }
    }
}