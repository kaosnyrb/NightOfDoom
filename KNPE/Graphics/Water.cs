using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace KNPE
{
    //delegate that the water component calls to render the objects in the scene
    public delegate void RenderObjects(Matrix reflectionMatrix);

    /// <summary>
    /// Options that must be passed to the water component before Initialization
    /// </summary>
    public class WaterOptions
    {
        //the number of vertices in the x and z plane (must be of the form 2^n + 1)
        //and the amount of spacing between vertices
        public int Width = 257;
        public int Height = 257;
        public float CellSpacing = .5f;

        //how large to scale the wave map texture in the shader
        //higher than 1 and the texture will repeat providing finer detail normals
        public float WaveMapScale = 1.0f;

        //size of the reflection and refraction render targets' width and height
        public int RenderTargetSize = 512;

        //offsets for the texcoords of the wave maps updated every frame
        //these are used in combination with the velocities to scroll
        //the normal maps over the water plane. Resulting in the appearence
        //of moving ripples across the water plane
        public Vector2 WaveMapOffset0;
        public Vector2 WaveMapOffset1;

        //the direction to offset the texcoords of the wave maps
        public Vector2 WaveMapVelocity0;
        public Vector2 WaveMapVelocity1;

        //asset names for the normal/wave maps
        public string WaveMapAsset0;
        public string WaveMapAsset1;

        //water color and sun light properties
        public Vector4 WaterColor;
        public Vector4 SunColor;
        public Vector3 SunDirection;
        public float SunFactor; //the intensity of the sun specular term.
        public float SunPower;  //how shiny we want the sun specular term on the water to be.
    }

    /// <summary>
    /// Drawable game component for water rendering. Renders the scene to reflection and refraction
    /// maps that are projected onto the water plane and are distorted based on two scrolling normal
    /// maps.
    /// </summary>
    public class Water : DrawableGameComponent
    {
        #region Fields
        private RenderObjects mDrawFunc;

        //vertex and index buffers for the water plane
        private VertexBuffer mVertexBuffer;
        private VertexBuffer mVertexBuffer2;
        private bool Buffer1;

        //HieghtField
        public static float[][] CurrentGrid;
        private float[][] PastGrid;
        private IndexBuffer mIndexBuffer;
        private VertexDeclaration mDecl;

        //water shader
        private Effect mEffect;
        private string mEffectAsset;

        //camera properties
        private Vector3 mViewPos;
        private Matrix mViewProj;
        private Matrix mWorld;

        //maps to render the refraction/reflection to
        private RenderTarget2D mRefractionMap;
        private RenderTarget2D mReflectionMap;

        //scrolling normal maps that we will use as a
        //a normal for the water plane in the shader
        private Texture mWaveMap0;
        private Texture mWaveMap1;

        //user specified options to configure the water object
        private WaterOptions mOptions;

        //tells the water object if it needs to update the refraction
        //map itself or not. Since refraction just needs the scene drawn
        //regularly, we can:
        // --Draw the objects we want refracted
        // --Resolve the back buffer and send it to the water
        // --Skip computing the refraction map in the water object
        // This is useful if you are already drawing the scene to a render target
        // Prevents from rendering the scene objects multiple times
        private bool mGrabRefractionFromFB = false;

        private int mNumVertices;
        private int mNumTris;
        #endregion

        #region Properties

        public RenderObjects RenderObjects
        {
            set { mDrawFunc = value; }
        }

        /// <summary>
        /// Name of the asset for the Effect.
        /// </summary>
        public string EffectAsset
        {
            get { return mEffectAsset; }
            set { mEffectAsset = value; }
        }

        /// <summary>
        /// The render target that the refraction is rendered to.
        /// </summary>
        public RenderTarget2D RefractionMap
        {
            get { return mRefractionMap; }
            set { mRefractionMap = value; }
        }

        /// <summary>
        /// The render target that the reflection is rendered to.
        /// </summary>
        public RenderTarget2D ReflectionMap
        {
            get { return mReflectionMap; }
            set { mReflectionMap = value; }
        }

        /// <summary>
        /// Options to configure the water. Must be set before
        /// the water is initialized. Should be set immediately
        /// following the instantiation of the object.
        /// </summary>
        public WaterOptions Options
        {
            get { return mOptions; }
            set { mOptions = value; }
        }

        /// <summary>
        /// The world matrix of the water.
        /// </summary>
        public Matrix World
        {
            get { return mWorld; }
            set { mWorld = value; }
        }

        #endregion

        public Water(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            //build the water mesh
            mNumVertices = mOptions.Width * mOptions.Height;
            mNumTris = (mOptions.Width - 1) * (mOptions.Height - 1) * 2;
            VertexPositionTexture[] vertices = new VertexPositionTexture[mNumVertices];

            Vector3[] verts;
            int[] indices;

            //create the water vertex grid positions and indices
            GenTriGrid(mOptions.Height, mOptions.Width, mOptions.CellSpacing, mOptions.CellSpacing,
                        Vector3.Zero, out verts, out indices);

            //copy the verts into our PositionTextured array
            for (int i = 0; i < mOptions.Width; ++i)
            {
                for (int j = 0; j < mOptions.Height; ++j)
                {
                    int index = i * mOptions.Width + j;
                    vertices[index].Position = verts[index];
                    vertices[index].TextureCoordinate = new Vector2((float)j / mOptions.Width, (float)i / mOptions.Height);
                }
            }

            mVertexBuffer = new VertexBuffer(Game.GraphicsDevice,
                                             VertexPositionTexture.SizeInBytes * mOptions.Width * mOptions.Height,
                                             BufferUsage.WriteOnly);

            mVertexBuffer2 = new VertexBuffer(Game.GraphicsDevice,
                                             VertexPositionTexture.SizeInBytes * mOptions.Width * mOptions.Height,
                                             BufferUsage.WriteOnly);

            mVertexBuffer.SetData(vertices);
            mVertexBuffer2.SetData(vertices);

            mIndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            mIndexBuffer.SetData(indices);

            mDecl = new VertexDeclaration(Game.GraphicsDevice, VertexPositionTexture.VertexElements);

            //normalzie the sun direction in case the user didn't

            //Init the hieght grids
            Random Rand = new Random();
            CurrentGrid = new float[mOptions.Width][];
            for (int i = 0; i < mOptions.Width; i++)
            {
                CurrentGrid[i] = new float[mOptions.Height];
                for (int j = 0; j < mOptions.Height; j++)
                {
                    CurrentGrid[i][j] = 0;
                }
            }
            PastGrid = new float[mOptions.Width][];
            for (int i = 0; i < mOptions.Width; i++)
            {
                PastGrid[i] = new float[mOptions.Height];
                for (int j = 0; j < mOptions.Height; j++)
                {
                    PastGrid[i][j] = 0;
                }
            }

        }

        protected override void LoadContent()
        {
            base.LoadContent();

            //load the wave maps
            mWaveMap0 = Game.Content.Load<Texture2D>(mOptions.WaveMapAsset0);
            mWaveMap1 = Game.Content.Load<Texture2D>(mOptions.WaveMapAsset1);

            //get the attributes of the back buffer
            PresentationParameters pp = Game.GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;
            MultiSampleType msType = pp.MultiSampleType;
            int msQuality = pp.MultiSampleQuality;

            //create the reflection and refraction render targets
            //using the backbuffer attributes
            mRefractionMap = new RenderTarget2D(Game.GraphicsDevice, mOptions.RenderTargetSize, mOptions.RenderTargetSize,
                                                1, format, msType, msQuality);
            mReflectionMap = new RenderTarget2D(Game.GraphicsDevice, mOptions.RenderTargetSize, mOptions.RenderTargetSize,
                                                1, format, msType, msQuality);

            mEffect = Game.Content.Load<Effect>(mEffectAsset);

            //set the parameters that shouldn't change.
            //Some of these might need to change every once in awhile,
            //move them to updateEffectParams function if you need that functionality.
            if (mEffect != null)
            {
                mEffect.Parameters["WaveMap0"].SetValue(mWaveMap0);
                mEffect.Parameters["WaveMap1"].SetValue(mWaveMap1);

                mEffect.Parameters["TexScale"].SetValue(mOptions.WaveMapScale);

                mEffect.Parameters["WaterColor"].SetValue(mOptions.WaterColor);
                mEffect.Parameters["SunColor"].SetValue(mOptions.SunColor);
                mEffect.Parameters["SunDirection"].SetValue(Vector3.Normalize(mOptions.SunDirection));
                mEffect.Parameters["SunFactor"].SetValue(mOptions.SunFactor);
                mEffect.Parameters["SunPower"].SetValue(mOptions.SunPower);

                mEffect.Parameters["World"].SetValue(mWorld);
            }
        }

        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //update the wave map offsets so that they will scroll across the water
            mOptions.WaveMapOffset0 += mOptions.WaveMapVelocity0 * timeDelta;
            mOptions.WaveMapOffset1 += mOptions.WaveMapVelocity1 * timeDelta;

            if (mOptions.WaveMapOffset0.X >= 1.0f || mOptions.WaveMapOffset0.X <= -1.0f)
                mOptions.WaveMapOffset0.X = 0.0f;
            if (mOptions.WaveMapOffset1.X >= 1.0f || mOptions.WaveMapOffset1.X <= -1.0f)
                mOptions.WaveMapOffset1.X = 0.0f;
            if (mOptions.WaveMapOffset0.Y >= 1.0f || mOptions.WaveMapOffset0.Y <= -1.0f)
                mOptions.WaveMapOffset0.Y = 0.0f;
            if (mOptions.WaveMapOffset1.Y >= 1.0f || mOptions.WaveMapOffset1.Y <= -1.0f)
                mOptions.WaveMapOffset1.Y = 0.0f;

            //Update water ripple effect
            RippleFunction();
            VertexPositionTexture[] vertices = new VertexPositionTexture[mNumVertices];

            Vector3[] verts;
            int[] indices;

            //create the water vertex grid positions and indices
            GenTriGrid(mOptions.Height, mOptions.Width, mOptions.CellSpacing, mOptions.CellSpacing,
                        Vector3.Zero, out verts, out indices, CurrentGrid);

            //copy the verts into our PositionTextured array
            for (int i = 0; i < mOptions.Width; ++i)
            {
                for (int j = 0; j < mOptions.Height; ++j)
                {
                    int index = i * mOptions.Width + j;
                    vertices[index].Position = verts[index];
                    vertices[index].TextureCoordinate = new Vector2((float)j / mOptions.Width, (float)i / mOptions.Height);
                }
            }
            if (!Buffer1)
            {
                mVertexBuffer2.SetData(vertices);
                Buffer1 = false;
            }
            else
            {
                mVertexBuffer.SetData(vertices);
                Buffer1 = true;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //don't cull back facing triangles since we want the water to be visible
            //from beneath the water plane too
            Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            Game.GraphicsDevice.RenderState.AlphaTestEnable = false;
            Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            Game.GraphicsDevice.RenderState.CullMode = CullMode.None;

            UpdateEffectParams();

            Game.GraphicsDevice.Indices = mIndexBuffer;
            if (Buffer1)
            {
                Game.GraphicsDevice.Vertices[0].SetSource(mVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                Buffer1 = false;
            }
            else
            {
                Game.GraphicsDevice.Vertices[0].SetSource(mVertexBuffer2, 0, VertexPositionTexture.SizeInBytes);
                Buffer1 = true;
            }
            Game.GraphicsDevice.VertexDeclaration = mDecl;

            mEffect.Begin(SaveStateMode.None);

            foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mNumVertices, 0, mNumTris);
                pass.End();
            }

            mEffect.End();

            Game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

        }

        /// <summary>
        /// Set the ViewProjection matrix and position of the Camera.
        /// </summary>
        /// <param name="viewProj"></param>
        /// <param name="pos"></param>
        public void SetCamera(Matrix viewProj, Vector3 pos)
        {
            mViewProj = viewProj;
            mViewPos = pos;
        }

        /// <summary>
        /// Updates the reflection and refraction maps. Called
        /// on update.
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateWaterMaps(GameTime gameTime)
        {

            /*------------------------------------------------------------------------------------------
             * Render to the Reflection Map
             */
            //clip objects below the water line, and render the scene upside down
            GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;

            GraphicsDevice.SetRenderTarget(0, mReflectionMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, mOptions.WaterColor, 1.0f, 0);

            //reflection plane in local space
            Vector4 waterPlaneL = new Vector4(0.0f, -1.0f, 0.0f, 0.0f);

            Matrix wInvTrans = Matrix.Invert(mWorld);
            wInvTrans = Matrix.Transpose(wInvTrans);

            //reflection plane in world space
            Vector4 waterPlaneW = Vector4.Transform(waterPlaneL, wInvTrans);

            Matrix wvpInvTrans = Matrix.Invert(mWorld * mViewProj);
            wvpInvTrans = Matrix.Transpose(wvpInvTrans);

            //reflection plane in homogeneous space
            Vector4 waterPlaneH = Vector4.Transform(waterPlaneL, wvpInvTrans);

            GraphicsDevice.ClipPlanes[0].IsEnabled = true;
            GraphicsDevice.ClipPlanes[0].Plane = new Plane(waterPlaneH);

            Matrix reflectionMatrix = Matrix.CreateReflection(new Plane(waterPlaneW));

            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            if (mDrawFunc != null)
                mDrawFunc(reflectionMatrix);

            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.ClipPlanes[0].IsEnabled = false;

            GraphicsDevice.SetRenderTarget(0, null);


            /*------------------------------------------------------------------------------------------
             * Render to the Refraction Map
             */

            //if the application is going to send us the refraction map
            //exit early. The refraction map must be given to the water component
            //before it renders. 
            //***This option can be handy if you're already drawing your scene to a render target***
            if (mGrabRefractionFromFB)
            {
                return;
            }

            //update the refraction map, clip objects above the water line
            //so we don't get artifacts
            GraphicsDevice.SetRenderTarget(0, mRefractionMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, mOptions.WaterColor, 1.0f, 1);

            if (mDrawFunc != null)
                mDrawFunc(Matrix.Identity);

            GraphicsDevice.SetRenderTarget(0, null);
        }

        /// <summary>
        /// Updates effect parameters related to the water shader
        /// </summary>
        private void UpdateEffectParams()
        {
            //update the reflection and refraction textures
            mEffect.Parameters["ReflectMap"].SetValue(mReflectionMap.GetTexture());
            mEffect.Parameters["RefractMap"].SetValue(mRefractionMap.GetTexture());

            //normal map offsets
            mEffect.Parameters["WaveMapOffset0"].SetValue(mOptions.WaveMapOffset0);
            mEffect.Parameters["WaveMapOffset1"].SetValue(mOptions.WaveMapOffset1);

            mEffect.Parameters["WorldViewProj"].SetValue(mWorld * mViewProj);

            //pass the position of the camera to the shader
            mEffect.Parameters["EyePos"].SetValue(mViewPos);
        }

        /// <summary>
        /// Generates a grid of vertices to use for the water plane.
        /// </summary>
        /// <param name="numVertRows">Number of rows. Must be 2^n + 1. Ex. 129, 257, 513.</param>
        /// <param name="numVertCols">Number of columns. Must be 2^n + 1. Ex. 129, 257, 513.</param>
        /// <param name="dx">Cell spacing in the x dimension.</param>
        /// <param name="dz">Cell spacing in the y dimension.</param>
        /// <param name="center">Center of the plane.</param>
        /// <param name="verts">Outputs the constructed vertices for the plane.</param>
        /// <param name="indices">Outpus the constructed triangle indices for the plane.</param>
        private void GenTriGrid(int numVertRows, int numVertCols, float dx, float dz,
                                Vector3 center, out Vector3[] verts, out int[] indices)
        {
            int numVertices = numVertRows * numVertCols;
            int numCellRows = numVertRows - 1;
            int numCellCols = numVertCols - 1;

            int mNumTris = numCellRows * numCellCols * 2;

            float width = (float)numCellCols * dx;
            float depth = (float)numCellRows * dz;

            //===========================================
            // Build vertices.

            // We first build the grid geometry centered about the origin and on
            // the xz-plane, row-by-row and in a top-down fashion.  We then translate
            // the grid vertices so that they are centered about the specified 
            // parameter 'center'.

            //verts.resize(numVertices);
            verts = new Vector3[numVertices];

            // Offsets to translate grid from quadrant 4 to center of 
            // coordinate system.
            float xOffset = -width * 0.5f;
            float zOffset = depth * 0.5f;

            int k = 0;

            for (float i = 0; i < numVertRows; ++i)
            {
                for (float j = 0; j < numVertCols; ++j)
                {
                    // Negate the depth coordinate to put in quadrant four.  
                    // Then offset to center about coordinate system.
                    verts[k] = new Vector3(0, 0, 0);
                    verts[k].X = j * dx + xOffset;
                    verts[k].Z = -i * dz + zOffset;
                    verts[k].Y = 0.0f;
                    Matrix translation = Matrix.CreateTranslation(center);
                    verts[k] = Vector3.Transform(verts[k], translation);

                    ++k; // Next vertex
                }
            }

            //===========================================
            // Build indices.

            //indices.resize(mNumTris * 3);
            indices = new int[mNumTris * 3];

            // Generate indices for each quad.
            k = 0;
            for (int i = 0; i < numCellRows; ++i)
            {
                for (int j = 0; j < numCellCols; ++j)
                {
                    indices[k] = i * numVertCols + j;
                    indices[k + 1] = i * numVertCols + j + 1;
                    indices[k + 2] = (i + 1) * numVertCols + j;

                    indices[k + 3] = (i + 1) * numVertCols + j;
                    indices[k + 4] = i * numVertCols + j + 1;
                    indices[k + 5] = (i + 1) * numVertCols + j + 1;

                    // next quad
                    k += 6;
                }
            }
        }
        private void GenTriGrid(int numVertRows, int numVertCols, float dx, float dz,
                        Vector3 center, out Vector3[] verts, out int[] indices, float[][] InputGrid)
        {
            int numVertices = numVertRows * numVertCols;
            int numCellRows = numVertRows - 1;
            int numCellCols = numVertCols - 1;

            int mNumTris = numCellRows * numCellCols * 2;

            float width = (float)numCellCols * dx;
            float depth = (float)numCellRows * dz;

            //===========================================
            // Build vertices.

            // We first build the grid geometry centered about the origin and on
            // the xz-plane, row-by-row and in a top-down fashion.  We then translate
            // the grid vertices so that they are centered about the specified 
            // parameter 'center'.

            //verts.resize(numVertices);
            verts = new Vector3[numVertices];

            // Offsets to translate grid from quadrant 4 to center of 
            // coordinate system.
            float xOffset = -width * 0.5f;
            float zOffset = depth * 0.5f;

            int k = 0;

            for (float i = 0; i < numVertRows; ++i)
            {
                for (float j = 0; j < numVertCols; ++j)
                {
                    // Negate the depth coordinate to put in quadrant four.  
                    // Then offset to center about coordinate system.
                    verts[k] = new Vector3(0, 0, 0);
                    verts[k].X = j * dx + xOffset;
                    verts[k].Z = -i * dz + zOffset;
                    verts[k].Y = InputGrid[(int)i][(int)j];
                    Matrix translation = Matrix.CreateTranslation(center);
                    verts[k] = Vector3.Transform(verts[k], translation);

                    ++k; // Next vertex
                }
            }

            //===========================================
            // Build indices.

            //indices.resize(mNumTris * 3);
            indices = new int[mNumTris * 3];

            // Generate indices for each quad.
            k = 0;
            for (int i = 0; i < numCellRows; ++i)
            {
                for (int j = 0; j < numCellCols; ++j)
                {
                    indices[k] = i * numVertCols + j;
                    indices[k + 1] = i * numVertCols + j + 1;
                    indices[k + 2] = (i + 1) * numVertCols + j;

                    indices[k + 3] = (i + 1) * numVertCols + j;
                    indices[k + 4] = i * numVertCols + j + 1;
                    indices[k + 5] = (i + 1) * numVertCols + j + 1;

                    // next quad
                    k += 6;
                }
            }
        }
        public static void CreateRipple(Vector2 Pos, float Size)
        {
            if (Pos.X > 1 && Pos.X < 199 && Pos.Y > 1 && Pos.Y < 199)
            {
                CurrentGrid[(int)Pos.X][(int)Pos.Y] = Size;
                CurrentGrid[(int)Pos.X - 1][(int)Pos.Y] += Size / 10;
                CurrentGrid[(int)Pos.X + 1][(int)Pos.Y] += Size / 10;
                CurrentGrid[(int)Pos.X][(int)Pos.Y - 1] += Size / 10;
                CurrentGrid[(int)Pos.X][(int)Pos.Y + 1] += Size / 10;

            }
        }
        public static float GetHeight(Vector2 Pos)
        {
            return 0;
            //return CurrentGrid[(int)Pos.X][(int)Pos.Y];
        }
        private void RippleFunction()
        {

            for (int i = 1; i < mOptions.Width - 1; i++)
            {
                for (int j = 1; j < mOptions.Height - 1; j++)
                {
                    //For each point take the 8 sorrounding points smooth this one based on it
                    //Buffer2 holds what the current frame should look like, buffer1 is the last frame.
                    CurrentGrid[i][j] = (PastGrid[i - 1][j] + PastGrid[i + 1][j] +
                                     PastGrid[i][j + 1] + PastGrid[i][j - 1] +
                                     PastGrid[i - 1][j + 1] + PastGrid[i - 1][j - 1] +
                                     PastGrid[i + 1][j + 1] + PastGrid[i + 1][j - 1]) / 4 - CurrentGrid[i][j];//4 gives smoothest result
                    if (CurrentGrid[i][j] > 0)
                    {
                        CurrentGrid[i][j] -= 0.01f;//needed to prevent the waves lasting forever
                    }
                    if (CurrentGrid[i][j] < 0)
                    {
                        CurrentGrid[i][j] += 0.01f;//needed to prevent the waves lasting forever
                    }
                    if (CurrentGrid[i][j] > 100) CurrentGrid[i][j] = 100;
                    if (CurrentGrid[i][j] < -100) CurrentGrid[i][j] = -100;

                }
            }
            //Swap the buffers, making the current frame the previous frame.
            //We still need the frame before the previous for the smoothing effect, so store it in 2.
            float temp = 0;
            for (int i = 1; i < mOptions.Width; i++)
            {
                for (int j = 1; j < mOptions.Height; j++)
                {
                    temp = CurrentGrid[i][j];
                    CurrentGrid[i][j] = PastGrid[i][j];
                    PastGrid[i][j] = temp;
                }
            }
        }
    }
}
