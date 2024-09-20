using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotDemo.Display
{
    /// <summary>
    /// Wraps a container for 3-D model.
    /// 实现IDrawModel接口
    /// </summary>
    /// <remarks></remarks>
    public class Model3D : IDrawModel
    {
        /// <summary>
        /// Stores the 3-D model.
        /// </summary>
        protected readonly Model model;

        /// <summary>
        /// The <see cref="GraphicsDevice"/> to be drawed on.
        /// <para>Used in <see cref="Draw"/> method.</para>
        /// </summary>
        protected GraphicsDevice graphicDevice;

        /// <summary>
        /// The absolute transforms of the <see cref="model"/>. Usually contains in the model file.
        /// <para>Used in <see cref="Draw"/> method.</para>
        /// </summary>
        protected readonly Matrix[] absoluteBoneTransforms;

        /// <summary>
        /// Initializes a new instance of the <see cref="Model3D"/> class.
        /// </summary>
        /// <param name="graphicDevice">The graphic device.</param>
        /// <param name="model">The 3-D model.</param>
        /// <param name="translation">The translation of model.</param>
        /// <remarks></remarks>
        public Model3D(GraphicsDevice graphicDevice, Model model, Matrix translation)
        {
            this.model = model;
            this.graphicDevice = graphicDevice;
            absoluteBoneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(absoluteBoneTransforms);
            //translation各网格相对移动、旋转、缩放效果的综合矩阵
            for (int i = 0; i < absoluteBoneTransforms.Length; i++)
            {
                absoluteBoneTransforms[i] *= translation;
            }
        }

        /// <summary>
        /// Draws the 3-D model.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="color">[Not Used]Ignored in <see cref="Model3D"/> class.</param>
        /// <remarks></remarks>
        public void Draw(Matrix world, Matrix view, Matrix projection, Color color)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * world;
                }
                mesh.Draw();
            }
            //model.Draw(world, view, projection);
        }
    }
}
