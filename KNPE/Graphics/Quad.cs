using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KNPE
{
    public struct Quad
    {
        public Vector3 Origin;
        public Vector3 UpperLeft;
        public Vector3 LowerLeft;
        public Vector3 UpperRight;
        public Vector3 LowerRight;
        public Vector3 Normal;
        public Vector3 Up;
        public Vector3 Left;

        public VertexPositionNormalTexture[] Vertices;
        public int[] Indexes;

        public bool Active;
        public Matrix WorldMatrix;

        float Width;
        float Height;
        public int TextureID;

        public bool Billboard;

        public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            Active = false;
            WorldMatrix = Matrix.Identity;
            Vertices = new VertexPositionNormalTexture[4];
            Indexes = new int[6];
            Origin = origin;
            Normal = normal;
            Up = up;

            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);
            
            Width = width;
            Height = height;
            TextureID = 0;
            Billboard = false;

            FillVertices();
        }

        public void SetupQuad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            Origin = origin;
            Normal = normal;
            Up = up;
            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);
            FillVertices();
            Active = true;
            //Generate the world matrix
            //OMGWTFBBQFIXPLZHACKS!
            WorldMatrix = Matrix.CreateTranslation(Origin) * Matrix.CreateScale(0.5f);

            Width = width;
            Height = height;
        }
        public void Refresh(Vector3 normal)
        {
            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * Height / 2) + Origin;
            UpperLeft = uppercenter + (Left * Width / 2);
            UpperRight = uppercenter - (Left * Width / 2);
            LowerLeft = UpperLeft - (Up * Height);
            LowerRight = UpperRight - (Up * Height);
            FillVertices();
            Active = true;
            //OMGWTFBBQFIXPLZHACKS!
            WorldMatrix = Matrix.CreateTranslation(Origin) * Matrix.CreateScale(0.5f);
        }
        private void FillVertices()
        {
            //Check if we need to create the vertices or indices
            if (Vertices == null)
            {
                Vertices = new VertexPositionNormalTexture[4];
            }
            if (Indexes == null)
            {
                Indexes = new int[6];
            }
            // Fill in texture coordinates to display full texture
            // on quad
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

            // Provide a normal for each vertex
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Normal;
            }

            // Set the position and texture coordinate for each
            // vertex
            Vertices[0].Position = LowerLeft;
            Vertices[0].TextureCoordinate = textureLowerLeft;
            Vertices[1].Position = UpperLeft;
            Vertices[1].TextureCoordinate = textureUpperLeft;
            Vertices[2].Position = LowerRight;
            Vertices[2].TextureCoordinate = textureLowerRight;
            Vertices[3].Position = UpperRight;
            Vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            Indexes[0] = 0;
            Indexes[1] = 1;
            Indexes[2] = 2;
            Indexes[3] = 2;
            Indexes[4] = 1;
            Indexes[5] = 3;
        }
    }

    public static class Quadmanger
    {
        static Quad[] quadlist;
        static public Texture2D[] textures;
        static BasicEffect quadEffect;
        static VertexDeclaration quadVertexDecl;
        static int MAXQUADS = 700;

        static Quadmanger()
        {
            quadlist = new Quad[MAXQUADS];
            for (int i = 0; i < MAXQUADS; i++)
            {
                quadlist[i] = new Quad();
                quadlist[i].Active = false;
                quadlist[i].WorldMatrix = Matrix.Identity;
            }
            textures = new Texture2D[MAXQUADS];
        }
        static public void ClearQuadList()
        {
            for (int i = 0; i < MAXQUADS; i++)
            {
                quadlist[i].Active = false;
            }
        }
        static public void LoadContentQuad(int Quad, ContentManager Content, GraphicsDevice device, string TextureString)
        {
            textures[Quad] = Content.Load<Texture2D>(TextureString);
            quadEffect = new BasicEffect(device, null);
            quadEffect.LightingEnabled = false;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = textures[Quad];
            quadVertexDecl = new VertexDeclaration(device,VertexPositionNormalTexture.VertexElements);
        }

        static public void RenderQuad(int TextureID,Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            bool SetQuad = false;
            for (int i = 0; i < MAXQUADS & !SetQuad; i++)
            {
                if (!quadlist[i].Active )
                {
                    quadlist[i].SetupQuad(origin, normal, up, width, height);
                    SetQuad = true;
                }
            }
        }

        static public void RenderBillboard(int TextureID, Vector3 origin, Vector3 up, float width, float height)
        {
            bool SetQuad = false;
            for (int i = 0; i < MAXQUADS & !SetQuad; i++)
            {
                if (!quadlist[i].Active)
                {
                    quadlist[i].SetupQuad(origin, -Camera.CameraForward, up, width, height);
                    quadlist[i].Billboard = true;
                    quadlist[i].TextureID = TextureID;
                    SetQuad = true;
                }
            }
        }

        static public void Draw(GraphicsDevice device, GameTime Time)
        {
            // Calculate the camera matrices.
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    (float)device.Viewport.Width / (float)device.Viewport.Height,
                                                                    1,
                                                                    Graphics_Core.ViewDistance);


            if (quadEffect == null) return;

            quadEffect.View = Camera.GetViewMatrix();
            quadEffect.Projection = projection;
            quadEffect.Alpha = 0.5f;

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.AlphaBlendOperation = BlendFunction.Add;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.One;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = false; 
            
            device.VertexDeclaration = quadVertexDecl;
            
            //Render one at a time
            for (int i = 0; i < MAXQUADS; i++)
            {
                if (quadlist[i].Active)
                {
                    if (quadlist[i].Billboard)
                    {
                        quadlist[i].Refresh(-Camera.CameraForward);
                    }
                    quadEffect.World = quadlist[i].WorldMatrix;
                    
                    quadEffect.Texture = textures[quadlist[i].TextureID];

                    quadEffect.Begin();
                    foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();

                        device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                            PrimitiveType.TriangleList, quadlist[i].Vertices, 0, 4, quadlist[i].Indexes, 0, 2);

                        pass.End();
                    }
                    quadEffect.End();
                }
            }
            device.RenderState.DepthBufferWriteEnable = true;  
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

        }
    }
}
