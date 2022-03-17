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
    public class Graphics_Core
    {
        public static GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static BloomComponent bloom;

        //Camera Position

        public static float ViewDistance = 80000;
        public static Matrix view;
        public static Matrix projection;
        public static Game gameAccess;
        public static bool Quit = false;

        public static Vector3 LightDirection = Vector3.Forward;

        public Graphics_Core(Game game)
        {
            graphics = new GraphicsDeviceManager(game);
            
            game.Content.RootDirectory = "Content";
           graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
       //     graphics.PreferredBackBufferWidth = 1280;
        //    graphics.PreferredBackBufferHeight = 1024;
            //graphics.IsFullScreen = true;
           
            gameAccess = game;

            bloom = new BloomComponent(game);
            game.Components.Add(bloom);
            bloom.Visible = false;

        }
        internal void Initialize(Game game)
        {


        }
        internal void LoadContent(Game game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            ScreenTextManager.LoadFont(game.Content, "CreeperFont");
            LoadSprites(game);
            LoadModels(game);
            RenderEntity3d_Manager.Init(game.Content);

            graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            //AvatarFactory.Init();
            ParticleManger.Setup();

        }

        public static void LoadSprites(Game game)
        {
            SpriteManager.LoadSprite(0, game.Content, "xboxControllerButtonY");
            SpriteManager.LoadSprite(1, game.Content, "Textures/Splash");
            SpriteManager.LoadSprite(2, game.Content, "Textures/eggsprite");
            SpriteManager.LoadSprite(3, game.Content, "satchi");
            SpriteManager.LoadSprite(4, game.Content, "SciFi");
            SpriteManager.LoadSprite(5, game.Content, "DoomLaser");
            SpriteManager.LoadSprite(6, game.Content, "Textures/Titlepage");
            SpriteManager.LoadSprite(7, game.Content, "Textures/paused");
            SpriteManager.LoadSprite(8, game.Content, "MiniSkull");
            SpriteManager.LoadSprite(9, game.Content, "Advert");
            SpriteManager.LoadSprite(10, game.Content, "Crosshair");
            SpriteManager.LoadSprite(11, game.Content, "Textures/NadeTex");
            SpriteManager.LoadSprite(12, game.Content, "Textures/Healthbar");
            SpriteManager.LoadSprite(13, game.Content, "Textures/HealthBack");
            SpriteManager.LoadSprite(14, game.Content, "Unlock");
            SpriteManager.LoadSprite(15, game.Content, "HowToPlay");
        }

        public static void LoadModels(Game game)
        {
            RenderEntity3d_Manager.LoadModel(0, game.Content, "TestPlayer", 1);
            RenderEntity3d_Manager.LoadModel(1, game.Content, "Enemy1", 2);
            RenderEntity3d_Manager.LoadModel(2, game.Content, "Bullet", 1);
            RenderEntity3d_Manager.LoadModel(3, game.Content, "Scanner", 1);
            RenderEntity3d_Manager.LoadModel(4, game.Content, "Head", 1);
            RenderEntity3d_Manager.LoadModel(5, game.Content, "Zombie", 1);
            RenderEntity3d_Manager.LoadModel(6, game.Content, "HoverBlaster", 1);
            RenderEntity3d_Manager.LoadModel(7, game.Content, "Gun", 1);
            RenderEntity3d_Manager.LoadModel(8, game.Content, "Nade", 1);
            RenderEntity3d_Manager.LoadModel(9, game.Content, "Nadebox", 1);
            RenderEntity3d_Manager.LoadModel(10, game.Content, "Healthbox", 1);
            RenderEntity3d_Manager.LoadModel(11, game.Content, "MiniZombie", 1);
            RenderEntity3d_Manager.LoadModel(12, game.Content, "TimeBonus", 1);
            RenderEntity3d_Manager.LoadModel(13, game.Content, "Boss", 1);
            RenderEntity3d_Manager.LoadModel(14, game.Content, "Wizard", 1);
            RenderEntity3d_Manager.LoadModel(15, game.Content, "Pirate", 1);

            Quadmanger.LoadContentQuad(0, game.Content, graphics.GraphicsDevice, "Textures/eggsprite");
            Quadmanger.LoadContentQuad(1, game.Content, graphics.GraphicsDevice, "ExplodeFire");
        }
        internal void UnloadContent(Game game)
        {
            
        }

        internal void Update(Game game, GameTime gameTime)
        {
            if (Quit)
            {
               game.Exit();
            }

            RenderEntity3d_Manager.ClearRenderList();
            SpriteManager.ClearRenderSpriteList();
            Quadmanger.ClearQuadList();
            ParticleManger.Update();
        }



        internal void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(ClearOptions.Target,new Vector4(0,0.05f,0,255),1,0);
            graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Level.Draw(graphics.GraphicsDevice);
            RenderEntity3d_Manager.Draw(graphics.GraphicsDevice, gameTime);

            AvatarFactory.Draw(gameTime);

            ParticleManger.Draw();
            Quadmanger.Draw(graphics.GraphicsDevice, gameTime);
            SpriteManager.Draw(spriteBatch);
            
            ScreenTextManager.Draw(spriteBatch);
        }
    }



    public class BloomComponent : DrawableGameComponent
    {
        #region Fields

        SpriteBatch spriteBatch;

        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;

        ResolveTexture2D resolveTarget;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;


        // Choose what display settings the bloom should use.
        public BloomSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        BloomSettings settings = BloomSettings.PresetSettings[0];


        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;


        #endregion

        #region Initialization


        public BloomComponent(Game game)
            : base(game)
        {
            if (game == null)
                throw new ArgumentNullException("game");
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            bloomExtractEffect = Game.Content.Load<Effect>("BloomExtract");
            bloomCombineEffect = Game.Content.Load<Effect>("BloomCombine");
            gaussianBlurEffect = Game.Content.Load<Effect>("GaussianBlur");


            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the backbuffer contents.
            resolveTarget = new ResolveTexture2D(GraphicsDevice, width, height, 1,
                format);

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            renderTarget1 = new RenderTarget2D(GraphicsDevice, width, height, 1,
                format);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, 1,
                format);
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            resolveTarget.Dispose();
            renderTarget1.Dispose();
            renderTarget2.Dispose();
        }


        #endregion

        #region Draw


        /// <summary>
        /// This is where it all happens. Grabs a scene that has already been rendered,
        /// and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Resolve the scene into a texture, so we can
            // use it as input data for the bloom processing.
            GraphicsDevice.ResolveBackBuffer(resolveTarget);

            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(
                Settings.BloomThreshold);

            DrawFullscreenQuad(resolveTarget, renderTarget1,
                               bloomExtractEffect,
                               IntermediateBuffer.PreBloom);

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);

            DrawFullscreenQuad(renderTarget1.GetTexture(), renderTarget2,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

            DrawFullscreenQuad(renderTarget2.GetTexture(), renderTarget1,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            GraphicsDevice.SetRenderTarget(0, null);

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            GraphicsDevice.Textures[1] = resolveTarget;

            Viewport viewport = GraphicsDevice.Viewport;

            DrawFullscreenQuad(renderTarget1.GetTexture(),
                               viewport.Width, viewport.Height,
                               bloomCombineEffect,
                               IntermediateBuffer.FinalResult);
        }


        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            GraphicsDevice.SetRenderTarget(0, null);
        }


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, int width, int height,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            // Draw the quad.
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();

            // End the custom effect.
            if (showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }


        #endregion
    }
    public class BloomSettings
    {
        #region Fields


        // Name of a preset bloom setting, for display to the user.
        public readonly string Name;


        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public readonly float BloomThreshold;


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;


        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;


        #endregion


        /// <summary>
        /// Constructs a new bloom settings descriptor.
        /// </summary>
        public BloomSettings(string name, float bloomThreshold, float blurAmount,
                             float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }


        /// <summary>
        /// Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static BloomSettings[] PresetSettings =
        {
            //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
            //new BloomSettings("Default",     0.001f,  3,   1.55f, 0.9f,    1,1.0f),
            new BloomSettings("Default",     0.001f,  1,   0.4f, 0.6f,    0,1),
            new BloomSettings("Soft",        0,      3,   1,     1,    1,       1),
            new BloomSettings("Desaturated", 0.5f,   8,   2,     1,    0,       1),
            new BloomSettings("Saturated",   0.25f,  4,   2,     1,    2,       0),
            new BloomSettings("Blurry",      0,      2,   1,     0.1f, 1,       1),
            new BloomSettings("Subtle",      0.5f,   2,   1,     1,    1,       1),
        };
    }
}
