/*
This code is released Open Source under the MIT license. Copyright 2009 UberGeekGames, 
All Rights Reserved. You may use it free of charge for any purposes, provided that 
UberGeekGames' copyright is reproduced in your use, and that you indemnify and hold 
harmless UberGeekGames from any claim arising out of any use (or lack of use or lack of 
ability of use) you make of it. This software is provided as-is, without any 
warranty or guarantee, including any implicit guarantee of merchantability or fitness 
for any particular purpose. Use at your own risk!

And if you use it for something cool, credit is always nice. :-)

Uses code from the Skinned Model Sample from the XNA website.
Special thanks go to Sowaz and csharp1024 for help and inspiration for several portions of the code!
Special thanks to Spear Darkness for the Collada importer!

If you have any questions/comments, or want to show off your project that uses this component,
then send me an email at contact@ubergeekgames.com!
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Xml.Serialization;
using SkinnedModel;

namespace KNPE
{
    /*///////////////////////////////////////////////////////////////////////////////////////////
     * __________________________________________________________________________________________
     * 
     * INSTRUCTIONS:
     * 
     * 1) Create an instance of Avatar.
     * 2) Start an animation (either a built in Preset, 
     *      or you can load your own SkinnedModel and pass that in)
     * 3) Call avatar.Update and avatar.Draw
     * 4) That's it! :-D
     * 
     * __________________________________________________________________________________________
     * 
     * Preparing your animations:
     * 
     * In order to use animation created yourself, you will need to use Maya or 3DS Max to
     * create an animation based on the included bone structures. You can then export
     * to either Collada (.DAE) or Autodesk FBX (.FBX) to use in the Avatar Wrapper component.
     * 
     * ____________________
     * 
     * If you are using Collada, then use the Avatar Collada Importer content processor.
     * You can load a Collada animation as easily as this:
     * 
     * AvatarColladaAnimation colladaAnimation = Content.Load<AvatarColladaAnimation>("your_collada_animation");
     * yourAvatar.StartAnimation(colladaAnimation, true);
     * 
     * ____________________
     * 
     * If you are using FBX, then use the Skinned Model Processor.
     * You can load an FBX animation as easily as this:
     * 
     * Model skinnedModel = Content.Load<Model>("your_skinned_model");
     * yourAvatar.StartAnimation(skinnedModel, true);
     * 
     * ____________________
     * 
     * A note about Gender Neutral Animations:
     * There are 31 built in animations that you can use on your Avatars. Some of them are
     * specific to the gender of your Avatar (i.e. FemaleLaugh and MaleLaugh). Normally
     * you have to make sure to use the correct type of animation for the gender of
     * your Avatar, but to make it even easier I have created the AvatarGenderNeutralAnimationPreset
     * enum. This enum contains almost all of the regular built in animations, but they are not
     * gender specific. If you pass in an AvatarGenderNeutralAnimationPreset instead of an
     * AvatarAnimationPreset, the wrapper will automatically choose the correct animation
     * based on the gender of this Avatar. 
     * Cool, huh? :-)
     * 
     * ____________________
     * 
     * A note about Blender:
     * Most of us can't afford awesome tools like 3DS Max or Maya. Unfortunately, to date 
     * we have not been able to successfully export an animation from Blender and get it
     * into the AvatarWrapper. :-( We are actively working to find a way around this.
     * 
     * ____________________
     * 
     * See the sample program for an example of what you can do with the wrapper!
    */
    /////////////////////////////////////////////////////////////////////////////////////////////

    #region Gender neutral animation presets enum

    public enum AvatarGenderNeutralAnimationPreset
    {
        Celebrate, Clap, Angry, Confused, Cry, CheckHand,
        LookAround, ShiftWeight,
        Laugh, Surpise, Yawn,
        Stand0, Stand1, Stand2, Stand3, Stand4, Stand5, Stand6, Stand7, Wave
    }

    #endregion

    public class Avatar
    {
        #region Animation Classes

        /// <summary>
        /// Represents a single frame of animation data.
        /// </summary>
        public class AnimationFrameData
        {
            //This is the list of bones for this frame of animation
            public List<Matrix> BoneTransforms = new List<Matrix>();

            //The time of this frame
            public TimeSpan FramePosition;
        }

        /// <summary>
        /// Represents a single animation, and holds the associated animation frame data
        /// </summary>
        public class AnimationData
        {
            //Total length of this animation
            public TimeSpan Length;

            //List of the frames of animation for this animation
            public List<AnimationFrameData> animationData = new List<AnimationFrameData>();

            //Title (i.e. Stand0, Wave, etc)
            public string Title;
        }

        /// <summary>
        /// Helper class for rendering Avatars on the PC
        /// </summary>
        public class AvatarPCAnimation
        {
            public AvatarAnimationPreset AnimationPreset;
            public TimeSpan CurrentPosition;
            public IList<Matrix> BoneTransforms;

            public AvatarPCAnimation(AvatarAnimationPreset preset)
            {
                this.AnimationPreset = preset;
            }
            public void Update(TimeSpan ElapsedGameTime, bool Loop)
            {
                this.CurrentPosition += ElapsedGameTime;
                if (this.CurrentPosition >= Avatar.GetLengthOfCurrentAnimation(AnimationPreset))
                {
                    if (Loop)
                        this.CurrentPosition = TimeSpan.Zero;
                    else
                        this.CurrentPosition = Avatar.GetLengthOfCurrentAnimation(AnimationPreset);
                }

                BoneTransforms = Avatar.GetBoneTransforms(AnimationPreset, CurrentPosition);
            }
        }

        #endregion

        #region Default poses

        //special thanks to csharp1024 for getting this data!
        private static int[] parentBones =   
        {  
            -1,  
            0,  
            0,  
            0,  
            0,  
            1,  
            2,  
            2,  
            3,  
            3,  
            1,  
            6,  
            5,  
            6,  
            5,  
            8,  
            5,  
            8,  
            5,  
            14,  
            12,  
            11,  
            16,  
            15,  
            14,  
            20,  
            20,  
            20,  
            22,  
            22,  
            22,  
            25,  
            25,  
            25,  
            28,  
            28,  
            28,  
            33,  
            33,  
            33,  
            33,  
            33,  
            33,  
            33,  
            36,  
            36,  
            36,  
            36,  
            36,  
            36,  
            36,  
            37,  
            38,  
            39,  
            40,  
            43,  
            44,  
            45,  
            46,  
            47,  
            50,  
            51,  
            52,  
            53,  
            54,  
            55,  
            56,  
            57,  
            58,  
            59,  
            60,  
        };

        private static Matrix[] bindPoses =   
        {  
            new Matrix(-1.06f, 4.517219E-16f, 1.263618E-07f, 0f, 4.560241E-16f, 1.05f, -2.059464E-16f, 0f, -1.263618E-07f, -2.040034E-16f, -1.06f, 0f, 1.00638E-06f, 0.7929635f, -0.00918531f, 1f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0f, 0f, -2.910383E-11f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.09005711f, -0.1072717f, 0.008671193f, 1f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, -0.09005711f, -0.1072717f, 0.008671193f, 0.9999999f),  
            new Matrix(0.9f, 0f, -3.929107E-25f, 0f, 0f, 1f, -1.964554E-25f, 0f, 3.722312E-25f, 2.067951E-25f, 0.95f, 0f, 0f, 0.03059953f, -2.910383E-11f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0f, 0.09179866f, -0.007715893f, 0.9999999f),  
            new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -7.070368E-06f, -0.2687335f, -0.01300271f, 0.9999999f),  
            new Matrix(0.85f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.85f, 0f, 0f, -0.1343725f, -0.006499312f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 4.135903E-25f, 0f, -4.135903E-25f, 1f, 0f, 0f, -4.135903E-25f, 0f, 1f, 0f, -7.070368E-06f, -0.2687335f, -0.01300271f, 0.9999999f),  
            new Matrix(0.85f, 4.135903E-25f, 3.515517E-25f, 0f, -3.515517E-25f, 1f, 0f, 0f, -3.515517E-25f, 0f, 0.85f, 0f, 0f, -0.1343725f, -0.006499312f, 0.9999999f),  
            new Matrix(0.8f, 0f, -3.308722E-25f, 0f, 0f, 1f, -1.654361E-25f, 0f, 3.308722E-25f, 2.067951E-25f, 0.8f, 0f, 0f, 0.06119912f, -0.005143928f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.005812414f, -0.2596922f, -0.02537671f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.007403408f, 0.1224214f, 0.01426828f, 0.9999999f),  
            new Matrix(0.85f, 0f, -3.515517E-25f, 0f, 0f, 1f, -1.757759E-25f, 0f, 3.515517E-25f, 2.067951E-25f, 0.85f, 0f, 0.002913281f, -0.1298461f, -0.01268019f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0f, 0.1737709f, -0.01882024f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.00579828f, -0.2596922f, -0.02537671f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, -0.007403407f, 0.1224214f, 0.01426828f, 0.9999999f),  
            new Matrix(0.85f, 4.135903E-25f, 0f, 0f, -3.515517E-25f, 1f, -1.757759E-25f, 0f, 0f, 2.067951E-25f, 0.85f, 0f, -0.00289914f, -0.1298461f, -0.01268019f, 0.9999999f),  
            new Matrix(0.95f, 0f, -3.722312E-25f, 0f, 0f, 1f, -1.861156E-25f, 0f, 3.929107E-25f, 2.067951E-25f, 0.9f, 0f, 0f, 0.04343986f, -0.004703019f, 0.9999999f),  
            new Matrix(0.95f, 0f, -3.929107E-25f, 0f, 0f, 0.95f, -1.964554E-25f, 0f, 3.929107E-25f, 1.964554E-25f, 0.95f, 0f, -7.071068E-06f, 0.1183337f, 0.02284149f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1176131f, 5.775504E-05f, -0.0279201f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.007855958f, -0.1010247f, 0.1342524f, 1f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.117613f, 5.775318E-05f, -0.0279201f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.007855952f, -0.1010247f, 0.1342524f, 0.9999999f),  
            new Matrix(0.85f, 0f, -3.515517E-25f, 0f, 0f, 1f, -1.757759E-25f, 0f, 3.515517E-25f, 2.067951E-25f, 0.85f, 0f, -7.071068E-06f, 0.03944456f, 0.007605664f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1696066f, -0.003995299f, 0.0005878786f, 0.9999999f),  
            new Matrix(1f, 0f, -3.515517E-25f, 0f, 0f, 0.85f, -1.757759E-25f, 0f, 4.135903E-25f, 1.757759E-25f, 0.85f, 0f, 0.0848033f, -0.00199765f, 0.0002898554f, 0.9999999f),  
            new Matrix(1f, 0f, -3.515517E-25f, 0f, 0f, 0.85f, -1.757759E-25f, 0f, 4.135903E-25f, 1.757759E-25f, 0.85f, 0f, 0f, 0f, -5.820766E-11f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1696066f, -0.003995301f, 0.0005878787f, 0.9999999f),  
            new Matrix(1f, 3.515517E-25f, 0f, 0f, -4.135903E-25f, 0.85f, -1.757759E-25f, 0f, 0f, 1.757759E-25f, 0.85f, 0f, -0.08480332f, -0.001997652f, 0.0002898555f, 0.9999999f),  
            new Matrix(1f, 3.515517E-25f, 0f, 0f, -4.135903E-25f, 0.85f, -1.757759E-25f, 0f, 0f, 1.757759E-25f, 0.85f, 0f, 0f, -1.862645E-09f, 0f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1027992f, -0.002424836f, 0.001567673f, 0.9999999f),  
            new Matrix(1f, 0f, -3.515517E-25f, 0f, 0f, 0.85f, -1.757759E-25f, 0f, 4.135903E-25f, 1.757759E-25f, 0.85f, 0f, 0.07710293f, -0.001824439f, 0.001175754f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1541988f, -0.003637314f, 0.002347426f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1027992f, -0.002424838f, 0.001567673f, 0.9999999f),  
            new Matrix(1f, 3.515517E-25f, 0f, 0f, -4.135903E-25f, 0.85f, -1.757759E-25f, 0f, 0f, 1.757759E-25f, 0.85f, 0f, -0.07710292f, -0.00182444f, 0.001175754f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1541988f, -0.003637316f, 0.002347426f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1083924f, -0.02864808f, 0.04242924f, 1f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1101885f, -0.02752805f, 0.01115742f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1034568f, -0.03065729f, -0.01861204f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.09029046f, -0.03503358f, -0.04665053f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.07399872f, -0.1849946f, 4.082802E-06f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.1110087f, -0.1110014f, 4.082802E-06f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0f, 0f, 0.009895937f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1083924f, -0.02864808f, 0.04242924f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1101884f, -0.02752805f, 0.01115742f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1034568f, -0.03065729f, -0.01861204f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.09029045f, -0.03503358f, -0.04665053f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.07399871f, -0.1849946f, 4.08286E-06f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.1110087f, -0.1110014f, 4.08286E-06f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, 0f, -1.862645E-09f, 0.009895937f, 0.9999999f),  
            new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.04690241f, -1.862645E-09f, 0.0003796703f, 1f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.04973078f, 0f, -1.224695E-05f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.04768014f, 0f, -0.000355173f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.04028386f, 0f, -0.0003551769f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.06541445f, -0.03722751f, 0.04949193f, 1f),  
            new Matrix(1f, 4.135903E-25f, 4.135903E-25f, 0f, -4.135903E-25f, 1f, 0f, 0f, -4.135903E-25f, 0f, 1f, 0f, -0.04690242f, 0f, 0.0003796704f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.04973083f, -1.862645E-09f, -1.224689E-05f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.04768026f, -1.862645E-09f, -0.0003551729f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.04028386f, -1.862645E-09f, -0.0003551766f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.06542858f, -0.03722751f, 0.04949194f, 0.9999999f),  
            new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.03380674f, -1.862645E-09f, 1.224864E-05f, 1f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.03515738f, 0f, -5.820766E-11f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.03452808f, 0f, 1.22448E-05f, 0.9999999f),  
            new Matrix(1f, 0f, -4.135903E-25f, 0f, 0f, 1f, -2.067951E-25f, 0f, 4.135903E-25f, 2.067951E-25f, 1f, 0f, 0.02958536f, 0f, -2.328306E-10f, 0.9999999f),  
            new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.03209555f, -0.01738984f, 0.01951019f, 1f),  
            new Matrix(1f, 4.135903E-25f, 4.135903E-25f, 0f, -4.135903E-25f, 1f, 0f, 0f, -4.135903E-25f, 0f, 1f, 0f, -0.03380674f, 0f, 1.22487E-05f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.03515732f, -1.862645E-09f, 0f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.03452795f, -1.862645E-09f, 1.224491E-05f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 0f, 0f, -4.135903E-25f, 1f, -2.067951E-25f, 0f, 0f, 2.067951E-25f, 1f, 0f, -0.02958536f, -1.862645E-09f, 0f, 0.9999999f),  
            new Matrix(1f, 4.135903E-25f, 4.135903E-25f, 0f, -4.135903E-25f, 1f, 0f, 0f, -4.135903E-25f, 0f, 1f, 0f, -0.03209561f, -0.01738983f, 0.01951019f, 0.9999999f),  
        };

        #endregion

        #region Animation data - loading and helpers

        private static List<AnimationData> AvatarPCAnimations;

        /// <summary>
        /// Gets the length of a built-in animation
        /// </summary>
        /// <param name="preset">The animation currently in use</param>
        /// <returns></returns>
        public static TimeSpan GetLengthOfCurrentAnimation(AvatarAnimationPreset preset)
        {
            AnimationData curData = GetAnimationFromPreset(preset);
            return curData.Length;
        }

        /// <summary>
        /// Gets the current bone transforms from a built-in animation
        /// </summary>
        /// <param name="preset">Animation currently in use</param>
        /// <param name="Position">The position in time of this animation to return</param>
        /// <returns></returns>
        public static List<Matrix> GetBoneTransforms(AvatarAnimationPreset preset, TimeSpan Position)
        {
            //get the data for this animation
            AnimationData curData = GetAnimationFromPreset(preset);

            //get the last closest frame and the next closest frame
            AnimationFrameData prevFrame = GetLastFrame(curData.animationData, Position);
            AnimationFrameData nextFrame = GetNextFrame(curData.animationData, Position);

            //normalize the times, so instead of it looking like
            // 3.1, 3.15, 3.2
            //it will be 
            // 0, .05, .1
            TimeSpan normalizedPrevFramePosition = TimeSpan.Zero;
            TimeSpan normalizedNextFramePosition = nextFrame.FramePosition - prevFrame.FramePosition;
            TimeSpan normalizedCurFramePosition = Position - prevFrame.FramePosition;

            //create a new bone matrix array so we don't inadvertantly corrupt
            //any of the animation data
            Matrix[] Bones = new Matrix[prevFrame.BoneTransforms.Count];
            prevFrame.BoneTransforms.CopyTo(Bones);

            //get the amount to interpolate by
            float amount = Position.Ticks / nextFrame.FramePosition.Ticks;

            //lerp each bone from the previous position to the next position, by the amount
            //of time inbetween them we are currently in.
            for (int i = 0; i < prevFrame.BoneTransforms.Count; i++)
            {
                Bones[i] = Matrix.Lerp(prevFrame.BoneTransforms[i], nextFrame.BoneTransforms[i], amount);
            }

            return Bones.ToList();
        }

        //Gets the last closest frame
        private static AnimationFrameData GetLastFrame(List<AnimationFrameData> data, TimeSpan Position)
        {
            AnimationFrameData curData = data[0];
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].FramePosition <= Position)
                    curData = data[i];
                else
                    break;
            }
            return curData;
        }

        //Gets the next closest frame
        private static AnimationFrameData GetNextFrame(List<AnimationFrameData> data, TimeSpan Position)
        {
            AnimationFrameData curData = data[0];
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].FramePosition <= Position)
                {
                    curData = data[i];
                }
                else
                {
                    curData = data[i];
                    break;
                }
            }
            return curData;
        }

        //Gets the AnimationData that matches the preset
        private static AnimationData GetAnimationFromPreset(AvatarAnimationPreset preset)
        {
            foreach (AnimationData ad in AvatarPCAnimations)
            {
                if (ad.Title == preset.ToString())
                    return ad;
            }
            return null;
        }

        //Loads all of the animation data for the built-in animations
        private static void LoadPCAnimations()
        {
            AvatarPCAnimations = new List<AnimationData>();

            //loop through the entire directory, loading data as we go
            ICollection<string> FileList = Directory.GetFiles(content.RootDirectory + "\\AnimationData");
            foreach (string filename in FileList)
            {
                LoadAnim(filename);
            }
        }

        //Loads a single animation from a file
        private static void LoadAnim(string filename)
        {
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = new BinaryReader(stream);

            AnimationData aData = new AnimationData();
            aData.Title = reader.ReadString();
            aData.Length = TimeSpan.FromTicks(reader.ReadInt64());

            int aDataCount = reader.ReadInt32();

            for (int i = 0; i < aDataCount; i++)
            {
                AnimationFrameData afd = new AnimationFrameData();

                afd.FramePosition = TimeSpan.FromTicks(reader.ReadInt64() * 6);

                int numAFDs = reader.ReadInt32();

                for (int b = 0; b < numAFDs; b++)
                {
                    Matrix m = new Matrix();
                    m.M11 = reader.ReadSingle();
                    m.M12 = reader.ReadSingle();
                    m.M13 = reader.ReadSingle();
                    m.M14 = reader.ReadSingle();
                    m.M21 = reader.ReadSingle();
                    m.M22 = reader.ReadSingle();
                    m.M23 = reader.ReadSingle();
                    m.M24 = reader.ReadSingle();
                    m.M31 = reader.ReadSingle();
                    m.M32 = reader.ReadSingle();
                    m.M33 = reader.ReadSingle();
                    m.M34 = reader.ReadSingle();
                    m.M41 = reader.ReadSingle();
                    m.M42 = reader.ReadSingle();
                    m.M43 = reader.ReadSingle();
                    m.M44 = reader.ReadSingle();
                    afd.BoneTransforms.Add(m);
                }
                aData.animationData.Add(afd);
            }
            AvatarPCAnimations.Add(aData);

            stream.Close();
            stream.Dispose();
        }

        #endregion

        #region Static vars

        //We need a ContentManager to load the data...
        private static ContentManager content;

        //...and a GraphicsDevice to draw the wireframes
        private static GraphicsDevice graphicsDevice;


        private static bool isPC = false;

        /// <summary>
        /// Checks if we are on the PC are not.
        /// </summary>
        public static bool IsPC
        {
            get
            {
                return isPC;
            }
        }

        #endregion

        #region Static class initialization

        /// <summary>
        /// This *MUST* be called before using the Avatar class in any way!
        /// </summary>
        /// <param name="gfxDevice">Non-null GraphicsDevice</param>
        /// <param name="cnt">A valid ContentManager</param>
        public static void Initialize(GraphicsDevice gfxDevice, ContentManager cnt)
        {
            content = cnt;
            graphicsDevice = gfxDevice;

            //are we on PC or Xbox?
#if !XBOX
            isPC = true;
#endif

            //load the animations if we're on the PC
            if (IsPC)
                LoadPCAnimations();
        }

        #endregion

        #region Variables and Properties

        /// <summary>
        /// Position of the Avatar
        /// </summary>
        public Vector3 Position = Vector3.Zero;

        /// <summary>
        /// Rotation of the Avatar
        /// </summary>
        public Vector3 Rotation = Vector3.Zero;

        /// <summary>
        /// Scale of the Avatar. It is a uniform scale, as anything else may be iffy with the Avatar TOU.
        /// </summary>
        public float Scale = 1;

        /// <summary>
        /// Gets the World matrix for the Avatar
        /// </summary>
        public Matrix World
        {
            get
            {
                return
                    Matrix.CreateScale(Scale) *
                    Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                    Matrix.CreateTranslation(Position);
            }
        }

        //private description
        private AvatarDescription description;

        /// <summary>
        /// Description of the Avatar. This can be changed mid-game for whatever reason, and everything else will be updated to reflect it
        /// </summary>
        public AvatarDescription Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                this.Renderer = new AvatarRenderer(description, true);
            }
        }

        //private expression
        private AvatarExpression expression;

        /// <summary>
        /// Current expression of the avatar
        /// </summary>
        public AvatarExpression Expression
        {
            get
            {
                return expression;
            }
            set
            {
                expression = value;
            }
        }


        /// <summary>
        /// Gets or sets the ambient light color used to draw the avatar on the Xbox
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get
            {
                return Renderer.AmbientLightColor;
            }
            set
            {
                Renderer.AmbientLightColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the light color used to draw the avatar on the Xbox
        /// </summary>
        public Vector3 LightColor
        {
            get
            {
                return Renderer.LightColor;
            }
            set
            {
                Renderer.LightColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the light direction used to draw the avatar on the Xbox
        /// </summary>
        public Vector3 LightDirection
        {
            get
            {
                return Renderer.LightDirection;
            }
            set
            {
                Renderer.LightDirection = value;
            }
        }

        /// <summary>
        /// Gets whether the avatar is loaded and ready to be rendered on the Xbox. 
        /// On PC it won't matter as it isn't drawn using AvatarRenderer, and on Xbox 
        /// an animation will be automatically played until the avatar is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return Renderer.IsLoaded;
            }
        }

        //private isAnimating flag
        private bool isAnimating = false;

        /// <summary>
        /// Checks if the Avatar is animating or not. This could be a built in animation or a user-defined animation
        /// </summary>
        public bool IsAnimating
        {
            get
            {
                return isAnimating;
            }
        }

        //private looping flag
        private bool isLoopingAnimation = true;

        /// <summary>
        /// Gets or sets whether to loop the animation or not.
        /// </summary>
        public bool IsLoopingAnimation
        {
            get
            {
                return isLoopingAnimation;
            }
            set
            {
                isLoopingAnimation = value;
            }
        }


        //view and projection matrices. These should be set elsewhere if you plan on using a camera! ;)
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 2, -1.5f), new Vector3(0, 1, 0),
                    Vector3.Up);
        public Matrix View
        {
            get
            {
                return view;
            }
            set
            {
                view = value;
            }
        }

        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    16 / 9, .01f, 200.0f);
        public Matrix Projection
        {
            get
            {
                return projection;
            }
            set
            {
                projection = value;
            }
        }

        //enum that we're using to keep the animation type in
        public enum AnimationType
        {
            BuiltIn, SkinnedModel, Collada
        }
        private AnimationType currentAnimationType = AnimationType.BuiltIn;

        /// <summary>
        /// Current animation type. Default means one of the 31 built in animations, and UserDefined means it's an animation loaded from a SkinnedModel
        /// </summary>
        public AnimationType CurrentAnimationType
        {
            get
            {
                return currentAnimationType;
            }
        }

        //current skinned model. This is needed for some drawing operations, so we keep a reference around when a new skinned model animation is set
        private Model currentSkinnedModel;

        //current Collada model. This is kept internally once you use StartAnimation and pass in a Collada model, so it can be updated.
        //private AvatarColladaAnimation currentColladaAnimation;

        //animation player for Skinned Models
        private AnimationPlayer skinnedAnimationPlayer;

        //avatar animation
        private AvatarAnimation Animation;

        //avatar animations - for the PC
        private AvatarPCAnimation AnimationPC;

        //avatar renderer - only used on the Xbox
        private AvatarRenderer Renderer;

        //effect used to draw the wireframe
        private BasicEffect effect;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts playing one of the built-in animations. Note that you will have to make check yourself to make sure it 
        /// matches the avatar's gender - for example playing the FemaleAngry animation on a male avatar.
        /// Use the other overload if you don't want to deal with that.
        /// </summary>
        /// <param name="avatarAnimationPreset">Animation to start</param>
        /// <param name="LoopAnimation">Loop the animation</param>
        public void StartAnimation(AvatarAnimationPreset avatarAnimationPreset, bool LoopAnimation)
        {
            //resets all of the animators to reflect the new animation
            currentAnimationType = AnimationType.BuiltIn;

            Animation = new AvatarAnimation(avatarAnimationPreset);
            AnimationPC = new AvatarPCAnimation(avatarAnimationPreset);

            isLoopingAnimation = LoopAnimation;
            isAnimating = true;
        }

        /// <summary>
        /// Starts playing one of the built in animations. From this enum you can pick one of the built in animations
        /// without having to worry about gender, and it will automatically start the correct one based on this avatar's current gender
        /// </summary>
        /// <param name="avatarAnimationPreset">Animation to start</param>
        /// <param name="LoopAnimation">Loop the animation</param>
        public void StartAnimation(AvatarGenderNeutralAnimationPreset avatarAnimationPreset, bool LoopAnimation)
        {
            currentAnimationType = AnimationType.BuiltIn;

            AvatarAnimationPreset preset = GetBuiltInPresetFromGenderNeutralPreset(avatarAnimationPreset);

            Animation = new AvatarAnimation(preset);
            AnimationPC = new AvatarPCAnimation(preset);

            isLoopingAnimation = LoopAnimation;
            isAnimating = true;
        }

        /// <summary>
        /// Starts playing a user-created animation. You will need to pass in a Model processed by the SkinnedModelPipeline and has a valid animation in it.
        /// </summary>
        /// <param name="skinnedModel">Skinned model to use</param>
        /// <param name="animationKey">Name of the animation to start, i.e. "Take 001". Case-sensitive, and will throw an exception if not valid</param>
        /// <param name="LoopAnimation">Loop the animation</param>
        public void StartAnimation(Model skinnedModel, string animationKey, bool LoopAnimation)
        {
            //set the new states
            currentSkinnedModel = skinnedModel;
            currentAnimationType = AnimationType.SkinnedModel;

            SkinningData skinningData = skinnedModel.Tag as SkinningData;

            //throw an error if there's no data
            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            //otherwise start the animation!
            skinnedAnimationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips[animationKey];

            skinnedAnimationPlayer.StartClip(clip);

            isLoopingAnimation = LoopAnimation;
            isAnimating = true;
        }

        /// <summary>
        /// Starts playing a user-created animation in the Collada format. You will need to pass in an AvatarColladaAnimation.
        /// </summary>
        /// <param name="avatarColladaAnimation">Animation to start playing</param>
        /// <param name="LoopAnimation">Loop the animation</param>
       // public void StartAnimation(AvatarColladaAnimation avatarColladaAnimation, bool LoopAnimation)
        //{
        //    currentColladaAnimation = avatarColladaAnimation;
        //    currentAnimationType = AnimationType.Collada;

//            isLoopingAnimation = LoopAnimation;
 //           isAnimating = true;
  //      }

        /// <summary>
        /// Resets the avatar to a default standing pose, and halts all animation.
        /// </summary>
        public void ResetAnimation()
        {
            currentAnimationType = AnimationType.BuiltIn;

            Animation = new AvatarAnimation(AvatarAnimationPreset.Stand0);
            AnimationPC = new AvatarPCAnimation(AvatarAnimationPreset.Stand0);

            isAnimating = false;
        }

        /// <summary>
        /// Stops the avatar animating at whatever frame he happens to be at
        /// </summary>
        public void StopAnimation()
        {
            isAnimating = false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts a gender-neutral animation to it's gender-specific animation based on this avatar's gender
        /// </summary>
        /// <param name="avatarAnimationPreset">gender neutral animation to convert</param>
        /// <returns>gender specific animation</returns>
        private AvatarAnimationPreset GetBuiltInPresetFromGenderNeutralPreset(AvatarGenderNeutralAnimationPreset avatarAnimationPreset)
        {
            AvatarAnimationPreset preset = AvatarAnimationPreset.Celebrate;

            bool isMale = true;
            if (Description.BodyType == AvatarBodyType.Female)
                isMale = false;

            switch (avatarAnimationPreset)
            {
                case AvatarGenderNeutralAnimationPreset.Angry:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleAngry;
                    else
                        preset = AvatarAnimationPreset.FemaleAngry;
                    break;
                case AvatarGenderNeutralAnimationPreset.Celebrate:
                    preset = AvatarAnimationPreset.Celebrate;
                    break;
                case AvatarGenderNeutralAnimationPreset.CheckHand:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleIdleCheckHand;
                    else
                        preset = AvatarAnimationPreset.FemaleIdleCheckNails;
                    break;
                case AvatarGenderNeutralAnimationPreset.Clap:
                    preset = AvatarAnimationPreset.Clap;
                    break;
                case AvatarGenderNeutralAnimationPreset.Confused:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleConfused;
                    else
                        preset = AvatarAnimationPreset.FemaleConfused;
                    break;
                case AvatarGenderNeutralAnimationPreset.Cry:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleCry;
                    else
                        preset = AvatarAnimationPreset.FemaleCry;
                    break;
                case AvatarGenderNeutralAnimationPreset.Laugh:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleLaugh;
                    else
                        preset = AvatarAnimationPreset.FemaleLaugh;
                    break;
                case AvatarGenderNeutralAnimationPreset.LookAround:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleIdleLookAround;
                    else
                        preset = AvatarAnimationPreset.MaleIdleLookAround;
                    break;
                case AvatarGenderNeutralAnimationPreset.ShiftWeight:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleIdleShiftWeight;
                    else
                        preset = AvatarAnimationPreset.MaleIdleShiftWeight;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand0:
                    preset = AvatarAnimationPreset.Stand0;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand1:
                    preset = AvatarAnimationPreset.Stand1;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand2:
                    preset = AvatarAnimationPreset.Stand2;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand3:
                    preset = AvatarAnimationPreset.Stand3;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand4:
                    preset = AvatarAnimationPreset.Stand4;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand5:
                    preset = AvatarAnimationPreset.Stand5;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand6:
                    preset = AvatarAnimationPreset.Stand6;
                    break;
                case AvatarGenderNeutralAnimationPreset.Stand7:
                    preset = AvatarAnimationPreset.Stand7;
                    break;
                case AvatarGenderNeutralAnimationPreset.Surpise:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleSurprised;
                    else
                        preset = AvatarAnimationPreset.FemaleShocked;
                    break;
                case AvatarGenderNeutralAnimationPreset.Wave:
                    preset = AvatarAnimationPreset.Wave;
                    break;
                case AvatarGenderNeutralAnimationPreset.Yawn:
                    if (isMale)
                        preset = AvatarAnimationPreset.MaleYawn;
                    else
                        preset = AvatarAnimationPreset.FemaleYawn;
                    break;
            }

            return preset;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Avatar object
        /// </summary>
        /// <param name="desc">Description of the Avatar. Could be AvatarDescription.CreateRandom() or a gamer's Avatar</param>
        public Avatar(AvatarDescription desc)
        {
            //creates a new effect - this will be needed to draw the wireframe bones
            this.effect = new BasicEffect(graphicsDevice, null);

            //sets up some default values
            //defaults to the Stand0 animation, but without any animation playing
            this.Description = desc;
            this.Animation = new AvatarAnimation(AvatarAnimationPreset.Stand0);
            this.Renderer = new AvatarRenderer(desc, true);
            this.Renderer.View = View;
            this.Renderer.Projection = Projection;
            this.AnimationPC = new AvatarPCAnimation(AvatarAnimationPreset.Stand0);
            this.isAnimating = false;
        }

        #endregion

        #region Update

        /// <summary>
        /// Should be called once per frame
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            //get the elapsed time to start with
            TimeSpan UpdateTime = gameTime.ElapsedGameTime;

            //if we're not animating, then we won't increase the time for the animations
            if (!isAnimating)
                UpdateTime = TimeSpan.Zero;

            //updates the animations based on the current type
            switch (currentAnimationType)
            {
                case AnimationType.BuiltIn:
                    Animation.Update(UpdateTime, IsLoopingAnimation);
                    if (IsPC)
                        AnimationPC.Update(UpdateTime, IsLoopingAnimation);
                    break;

                case AnimationType.SkinnedModel:
                    skinnedAnimationPlayer.Update(UpdateTime, true, World);
                    break;

        //        case AnimationType.Collada:
          //          currentColladaAnimation.Update(UpdateTime, IsLoopingAnimation);
            //        break;
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the Avatar. If we're on the PC then it will draw a wireframe where the Avatar's bones would be.
        /// </summary>
        public void Draw()
        {
            //setup the AvatarRenderer camera if we're on the Xbox
            if (!IsPC)
            {
                Renderer.View = View;
                Renderer.Projection = Projection;
                Renderer.World = World;
            }

            switch (currentAnimationType)
            {
                case AnimationType.BuiltIn:
                    if (!IsPC)
                    {
                        Renderer.Draw(Animation.BoneTransforms, expression);
                    }
                    else
                    {
                        DrawWireframeBones(AnimationPC.BoneTransforms);
                    }
                    break;

                //the process is a bit more involved for user-defined animations from skinned models
                case AnimationType.SkinnedModel:

                    //skinnedAnimationPlayer.UpdateBoneTransforms(TimeSpan.Zero, true);
                    //Special thanks to Sowaz for this code that organizes the bones for drawing!
                    Matrix[] organizedBoneTransforms = new Matrix[71];
                    Matrix[] boneTransforms = skinnedAnimationPlayer.GetBoneTransforms();

                    IEnumerable<ModelBone> interestBones =
                        from bone in currentSkinnedModel.Bones where bone.Name.Contains("joint") select bone;

                    int boneIdx = -1;
                    foreach (ModelBone bone in interestBones)
                    {
                        boneIdx++;
                        string number = bone.Name.Replace("joint", "");
                        int index = int.Parse(number) - 1;
                        if (index < 71)
                        {
                            Vector3 scale, translation;
                            Quaternion rotationQuat;

                            boneTransforms[boneIdx].Decompose(out scale, out rotationQuat, out translation);
                            Matrix rotationMat = Matrix.CreateFromQuaternion(rotationQuat);
                            organizedBoneTransforms[index] = rotationMat;
                        }
                    }

                    //draw our new list of bones!
                    if (!IsPC)
                    {
                        Renderer.Draw(organizedBoneTransforms, expression);
                    }
                    else
                    {
                        DrawWireframeBones(organizedBoneTransforms);
                    }
                    break;

                //Bone transformation is taken care of for us in the collada classes, so this is relatively simple
//                case AnimationType.Collada:
                    //
                   // Matrix[] Bones = new Matrix[currentColladaAnimation.BoneTransforms.Count];
//                    currentColladaAnimation.BoneTransforms.CopyTo(Bones, 0);

  //                  if (!IsPC)
    //                {
      //                  Renderer.Draw(Bones, Expression);
        //            }
          //          else
            //        {
              //          DrawWireframeBones(Bones);
                //    }

                  //  break;
            }
        }

        #endregion

        #region Draw Wireframe

        private void DrawWireframeBones(IList<Matrix> Bones)
        {
            //transforms the bone positions by their binding poses.
            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i] *= bindPoses[i];
            }

            //Transforms the bone positions by the world position of the Avatar, 
            //and the world positions of each individual bone
            //if we didn't do this then the bones would be in a tiny jumbled pile
            Matrix[] worldTransforms = new Matrix[Bones.Count];

            worldTransforms[0] = Bones[0];
            worldTransforms[0] *= World;
            for (int i = 1; i < Bones.Count; i++)
            {
                worldTransforms[i] =
                    Bones[i] *
                    worldTransforms[parentBones[i]];
            }

            //the actual drawing of the wireframe bones
            //Thanks to csharp1024 for this code!
            VertexPositionColor[] verts = new VertexPositionColor[Bones.Count * 2];
            verts[0].Color = Color.Blue;
            verts[0].Position = worldTransforms[0].Translation;
            verts[1] = verts[0];
            for (int i = 2; i < Bones.Count * 2; i += 2)
            {
                verts[i].Position = worldTransforms[i / 2].Translation;
                verts[i].Color = Color.Red;
                verts[i + 1].Position = worldTransforms[parentBones[i / 2]].Translation;
                verts[i + 1].Color = Color.Green;
            }

            graphicsDevice.VertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);
            graphicsDevice.RenderState.PointSize = 4;
            effect.LightingEnabled = false;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.Projection = Projection;
            effect.View = View;
            effect.World = Matrix.Identity;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, Bones.Count);
                pass.End();
            }
            effect.End();
        }

        #endregion
    }
}
