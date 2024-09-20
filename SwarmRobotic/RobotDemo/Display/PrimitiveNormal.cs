using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotDemo.Display
{
    /// <summary>
    /// A container of an indexed <see cref="VertexPositionNormalTexture"/> array containing Position, Normal and Texture informations.
    /// <para>The model cantains two arrays. An array of <see cref="VertexPositionNormalTexture"/> class containing the vertecies to use.
    /// Another array contains index of the previous array, determines the order of the vertecies.</para>
    /// </summary>
    /// <remarks>Texture information can be omitted.</remarks>
    public class PrimitiveNormal : IDrawModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveNormal"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="texture">The 2D texture. Default is null.</param>
        /// <remarks>Texture is ignored if <paramref name="texture"/> is null. In this case, solid color will be used in <see cref="Draw"/> method.</remarks>
        public PrimitiveNormal(GraphicsDevice graphicsDevice, Texture2D texture = null)
        {
            this.graphicsDevice = graphicsDevice;
            vertexlist = new List<VertexPositionNormalTexture>();
            indexlist = new List<short>();

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.EnableDefaultLighting();
            if (texture == null)
            {
                basicEffect.TextureEnabled = false;
                HasTexture = false;
            }
            else
            {
                basicEffect.TextureEnabled = true;
                HasTexture = true;
                basicEffect.Texture = texture;
            }
        }

        #region Graphic Data
        /// <summary>
        /// After initialization of the <see cref="PrimitiveNormal"/> model, vertex data is stored on the CPU in this managed list.
        /// <para>This data is sent to GPU in <see cref="Draw"/> method.</para>
        /// </summary>
        protected VertexPositionNormalTexture[] vertexarray;

        /// <summary>
        /// After initialization of the <see cref="PrimitiveNormal"/> model, index data is stored on the CPU in this managed list.
        /// <para>This data is sent to GPU in <see cref="Draw"/> method.</para>
        /// </summary>
        protected short[] indexarray;

        /// <summary>
        /// During the initialization of the <see cref="PrimitiveNormal"/> model, vertex data is stored in this dynamic list.
        /// <para>This data is destroyed in <see cref="EndInitArray"/> after initialization.</para>
        /// </summary>
        protected List<VertexPositionNormalTexture> vertexlist;

        /// <summary>
        /// During the initialization of the <see cref="PrimitiveNormal"/> model, index data is stored in this dynamic list.
        /// <para>This data is destroyed in <see cref="EndInitArray"/> method after initialization.</para>
        /// </summary>
        protected List<short> indexlist;

        /// <summary>
        /// Stores the count of vertex array.
        /// <para>The value is calculated in <see cref="EndInitArray"/> method.</para>
        /// </summary>
        int vertexcount;

        /// <summary>
        /// Stores the count of triangles to be rendered in <see cref="Draw"/> method.
        /// <para>The value is calculated in <see cref="EndInitArray"/> method.</para>
        /// <para>The value is calculated as <see cref="vertexcount"/> / 3.</para>
        /// </summary>
        int primitivecount;

        /// <summary>
        /// Determines whether a Texture is specified in the <see cref="PrimitiveNormal.PrimitiveNormal"/> contructor.
        /// </summary>
        public bool HasTexture
        {
            get;
            protected set;
        }

        /// <summary>
        /// The <see cref="BasicEffect"/> of the model.
        /// <para>Used in <see cref="Draw"/> method.</para>
        /// </summary>
        protected BasicEffect basicEffect;

        /// <summary>
        /// The <see cref="GraphicsDevice"/> to be drawed on.
        /// <para>Used in <see cref="Draw"/> method.</para>
        /// </summary>
        protected GraphicsDevice graphicsDevice;

        #endregion

        /// <summary>
        /// Adds a new vertex to the <see cref="PrimitiveNormal"/> model. This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate of the vertex.</param>
        /// <remarks></remarks>
        public void AddVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            vertexlist.Add(new VertexPositionNormalTexture(position, normal, textureCoordinate));
        }

        /// <summary>
        /// Adds a new index to the <see cref="PrimitiveNormal"/> model. This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="index">Index of next vertex.</param>
        /// <remarks></remarks>
        /// <seealso cref="AddIndex(int[])"/>
        public void AddIndex(int index)
        {
            if (index > short.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indexlist.Add((short)index);
        }
        
        /// <summary>
        /// Adds indecies to the <see cref="PrimitiveNormal"/> model. This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="indecies">The index array list.</param>
        /// <remarks></remarks>
        /// <seealso cref="AddIndex(int)"/>
        public void AddIndex(params int[] indecies)
        {
            foreach (var i in indecies)
            {
                AddIndex(i);
            }
        }

        /// <summary>
        /// Gets the vertex count.
        /// </summary>
        /// <remarks></remarks>
        public int VertexCount
        {
            get { return vertexlist.Count; }
        }

        /// <summary>
        /// Finishes the initialization of the model and prepares the model ready to be rendered.
        /// <para>Stores the vertex and index array in static array so as to use in <see cref="Draw"/> method.</para>
        /// </summary>
        /// <remarks>Calling <see cref="AddVertex"/>, <see cref="AddIndex(int)"/> and <see cref="AddIndex(int[])"/> method after <see cref="EndInitArray"/> method is called will causs error.
        /// <para>Calling <see cref="Draw"/> method before <see cref="EndInitArray"/> method is called will also causs error.</para></remarks>
        public void EndInitArray()
        {
            vertexcount = vertexlist.Count;
            vertexarray = vertexlist.ToArray();
            vertexlist = null;

            primitivecount = indexlist.Count / 3;
            indexarray = indexlist.ToArray();
            indexlist = null;
        }

        /// <summary>
        /// Draws the <see cref="PrimitiveNormal"/> model, using a <see cref="BasicEffect"/> shader with default lighting.
        /// This method sets important renderstates to sensible values for 3D model rendering,
        /// so you do not need to set these states before you call it.
        /// <para>If <see cref="HasTexture"/> is true, <paramref name="color"/> is ignored and texture will be rendered.</para>
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="color">The model color.</param>
        /// <remarks></remarks>
        public void Draw(Matrix world, Matrix view, Matrix projection, Color color)
        {
            // Set BasicEffect parameters.
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            if (!HasTexture)
            {
                basicEffect.DiffuseColor = color.ToVector3();
                basicEffect.Alpha = color.A / 255.0f;
                if (color.A < 255)
                {
                    // Set renderstates for alpha blended rendering.
                    graphicsDevice.BlendState = BlendState.AlphaBlend;
                }
                else
                {
                    // Set renderstates for opaque rendering.
                    graphicsDevice.BlendState = BlendState.Opaque;
                }
            }
            else
            {
                basicEffect.DiffuseColor = Color.White.ToVector3();
                basicEffect.Alpha = 1;
                graphicsDevice.BlendState = BlendState.Opaque;
            }


            // Draw the model, using BasicEffect.
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    vertexarray, 0, vertexcount, indexarray, 0, primitivecount);
            }

        }
    }
}
