using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
	public class Guc3DGraphDisplay : GucControl
	{
		public Guc3DGraphDisplay()
		{
			Size = new Vector2(100);
		}

		public override bool Draw()
		{
			//foreach (var ctrl in AllControls)
			//    RequireRedraw |= ctrl.Draw();
			//if (RequireRedraw)
			//{
				//var render = graphicsDevice.GetRenderTargets();
				graphicsDevice.SetRenderTarget(renderBuffer);

                //原有的背景颜色为BackColor，此处设为透明
				graphicsDevice.Clear(Color.Transparent);

				if (Draw3DGraphic != null) Draw3DGraphic(this);
				//OnDraw();
				//this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
				//foreach (var r in CustomDrawRegions)
				//{
				//    if (r.Show) r.Draw(spriteBatch);
				//}
				//foreach (var ctrl in AllControls)
				//{
				//    //if (ctrl.Visible) ctrl.DrawRegion.Draw(spriteBatch);
				//    if (ctrl.Visible)
				//        spriteBatch.Draw(ctrl.renderBuffer, ctrl.Position, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, ctrl.IsActive ? DrawingDepth.ActiveControl : DrawingDepth.ChildrenControl);
				//}
				//this.spriteBatch.End();

				graphicsDevice.SetRenderTargets(null);
				//var stream = System.IO.File.Open(string.Format("{0}-{1}.jpg", width, height), System.IO.FileMode.Create);
				//renderTexture.SaveAsJpeg(stream, width, height);
				//stream.Close();
				//RequireRedraw = false;
				return true;
			//}
			//return false;
		}

		public override void BindGraphic(GraphicsDevice graphicsDevice)
		{
			base.BindGraphic(graphicsDevice);
			renderBuffer = new RenderTarget2D(graphicsDevice, Width, Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
		}

		protected override void OnSizeChange()
		{
			if(graphicsDevice!=null)
				renderBuffer = new RenderTarget2D(graphicsDevice, Width, Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
		}

		public event GucEventHandler Draw3DGraphic;
	}
}
/**********************************
 * 主要功能与实现
 * 1.不使用其他控件成员
 * 2.设置绘制缓冲区
***********************************/