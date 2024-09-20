using Microsoft.Xna.Framework;

namespace RobotDemo.Display
{
    /// <summary>
    /// Interface for Drawable Model.
    /// </summary>
    /// <remarks></remarks>
    public interface IDrawModel
    {
        /// <summary>
        /// Draws the model.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="color">The model color.</param>
        /// <remarks>The <paramref name="color"/> parameter may not take effect which depends on the model itself</remarks>
        void Draw(Matrix world, Matrix view, Matrix projection, Color color);
    }
}
