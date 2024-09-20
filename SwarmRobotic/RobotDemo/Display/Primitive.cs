using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotDemo.Display
{
    /// <summary>
    /// A container of an indexed <see cref="VertexPositionColorTexture"/> array containing Position, Color and Texture informations.
    /// <para>The model cantains two arrays. An array of <see cref="VertexPositionColorTexture"/> class containing the vertecies to use.
    /// Another array contains index of the previous array, determines the order of the vertecies.</para>
    /// </summary>
    /// <remarks>Texture information can be omitted.</remarks>
    public class Primitive : IDrawModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Primitive"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="texture">The 2D texture. Default is null.</param>
        /// <remarks>Texture is ignored if <paramref name="texture"/> is null. In this case, solid color will be used in <see cref="Draw"/> method.</remarks>
        public Primitive(GraphicsDevice graphicsDevice, Texture2D texture = null)
        {
            this.graphicsDevice = graphicsDevice;
            //顶点数组：位置、颜色、纹理坐标
            vertexlist = new List<VertexPositionColorTexture>();
            indexlist = new List<short>();
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.EnableDefaultLighting();
			basicEffect.LightingEnabled = false;
			Texture = texture;
        }

        #region Graphic Data
        /// <summary>
        /// After initialization of the <see cref="Primitive"/> model, vertex data is stored on the CPU in this managed list.
        /// <para>This data is sent to GPU in <see cref="Draw"/> method.</para>
        /// 模型初始化完成后顶点数组的存放数组
        /// </summary>
        protected VertexPositionColorTexture[] vertexarray;

        /// <summary>
        /// After initialization of the <see cref="Primitive"/> model, index data is stored on the CPU in this managed list.
        /// <para>This data is sent to GPU in <see cref="Draw"/> method.</para>
        /// </summary>
        protected short[] indexarray;

        /// <summary>
        /// During the initialization of the <see cref="Primitive"/> model, vertex data is stored in this dynamic list.
        /// <para>This data is destroyed in <see cref="EndInitArray"/> after initialization.</para>
        /// 模型初始化过程中顶点数据的存放数组
        /// </summary>
        protected List<VertexPositionColorTexture> vertexlist;

        /// <summary>
        /// During the initialization of the <see cref="Primitive"/> model, index data is stored in this dynamic list.
        /// <para>This data is destroyed in <see cref="EndInitArray"/> method after initialization.</para>
        /// 模型初始化过程中索引数组的存放数组
        /// </summary>
        protected List<short> indexlist;

        /// <summary>
        /// Stores the count of vertex array.
        /// <para>The value is calculated in <see cref="EndInitArray"/> method.</para>
        /// 存储顶点数组的元素个数
        /// </summary>
        int vertexcount;

        /// <summary>
        /// Stores the count of triangles to be rendered in <see cref="Draw"/> method.
        /// <para>The value is calculated in <see cref="EndInitArray"/> method.</para>
        /// <para>The value is calculated as <see cref="vertexcount"/> / 3.</para>
        /// 存储要渲染的三角形的个数
        /// </summary>
        int primitivecount;

        /// <summary>
        /// Determines whether a Texture is specified in the <see cref="Primitive.Primitive"/> contructor.
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
        /// Adds a new vertex with white color to the <see cref="Primitive"/> model.
        /// This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate of the vertex.</param>
        /// <remarks></remarks>
        public void AddVertex(Vector3 position, Vector2 textureCoordinate)
        {
            vertexlist.Add(new VertexPositionColorTexture(position, Color.White, textureCoordinate));
        } 
        
        /// <summary>
        /// Adds a new vertex to the <see cref="Primitive"/> model.
        /// This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">Color of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate of the vertex.</param>
        /// <remarks></remarks>
        public void AddVertex(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            vertexlist.Add(new VertexPositionColorTexture(position, color, textureCoordinate));
        }

        /// <summary>
        /// Adds a new index to the <see cref="Primitive"/> model.
        /// This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="index">Index of next vertex.</param>
        /// <remarks></remarks>
        /// <seealso cref="AddIndex(int[])"/>
		public void AddIndex(params int[] index)
		{
			//if (index > short.MaxValue)
			//    throw new ArgumentOutOfRangeException("index");
			indexlist.AddRange(index.Select(i => (short)i));
		}

        /// <summary>
        /// Adds indecies to the <see cref="Primitive"/> model.
        /// This should only be called when initializing, before calling <see cref="EndInitArray"/> method.
        /// </summary>
        /// <param name="indecies">The index array list.</param>
        /// <remarks></remarks>
        /// <seealso cref="AddIndex(int)"/>
        public void AddIndex(params short[] indecies)
        {
			indexlist.AddRange(indecies);
			//foreach (var i in indecies)
			//{
			//    AddIndex(i);
			//}
        }

        /// <summary>
        /// Gets the vertex count.
        /// </summary>
        /// <remarks></remarks>
        public int VertexCount
        {
            get { return vertexlist == null ? vertexcount : vertexlist.Count; }
        }

        /// <summary>
        /// Finishes the initialization of the model and prepares the model ready to be rendered.
        /// <para>Stores the vertex and index array in static array so as to use in <see cref="Draw"/> method.</para>
        /// </summary>
        /// <remarks>Calling <see cref="AddVertex(Vector3, Color, Vector2)"/>, <see cref="AddIndex(int[])"/> and <see cref="AddIndex(int[])"/> method
        /// after <see cref="EndInitArray"/> method is called will causs error. <para>Calling <see cref="Draw"/> method
        /// before <see cref="EndInitArray"/> method is called will also causs error.</para></remarks>
        public void EndInitArray()
        {
            vertexcount = vertexlist.Count;
            vertexarray = vertexlist.ToArray();
            vertexlist = null;

            primitivecount = indexlist.Count / 3;
            indexarray = indexlist.ToArray();
            indexlist = null;
        }

		public Texture2D Texture
		{
			get { return basicEffect.TextureEnabled ? basicEffect.Texture : null; }
			set
			{
				if (value == null)
				{
					basicEffect.TextureEnabled = false;
					HasTexture = false;
				}
				else
				{
					basicEffect.TextureEnabled = true;
					HasTexture = true;
					basicEffect.Texture = value;
				}
			}
		}

        /// <summary>
        /// Draws the <see cref="Primitive"/> model, using a <see cref="BasicEffect"/> shader with default lighting.
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
            //设置图形设备
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            //设置“效果”参数
            // Set BasicEffect parameters.
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;            
            if (!HasTexture)
            {
                basicEffect.DiffuseColor = color.ToVector3();
                basicEffect.Alpha = 1;
                basicEffect.VertexColorEnabled = true;
                //basicEffect.LightingEnabled = false;

                //// Set renderstates for alpha blended rendering.
                //BlendState bs = new BlendState();
                //bs.AlphaBlendFunction = BlendFunction.Add;
                ////bs.AlphaDestinationBlend = Blend.DestinationAlpha;
                ////bs.AlphaSourceBlend = Blend.InverseDestinationAlpha;
                //bs.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                //bs.AlphaSourceBlend = Blend.SourceAlpha;
                //bs.ColorBlendFunction = BlendFunction.Add;
                ////bs.ColorDestinationBlend = Blend.DestinationAlpha;
                ////bs.ColorSourceBlend = Blend.InverseDestinationAlpha;
                //bs.ColorDestinationBlend = Blend.InverseSourceAlpha;
                //bs.ColorSourceBlend = Blend.SourceAlpha;

                //graphicsDevice.BlendState = bs;
            }
            else
            {
                basicEffect.DiffuseColor = color.ToVector3();
                basicEffect.Alpha = 1; //以地图纹理为例，取0后就透明了，即地图没有颜色；这里设为0不影响真假目标的模型（无纹理）
                //graphicsDevice.BlendState = BlendState.Opaque;
            }

            //使用效果绘制模型，基元图形采用“三角形”
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
