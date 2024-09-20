using System.Linq;

namespace GucUISystem
{
    /// <summary>
    /// 继承自GuccontainerControl，添加了GucBorderBox字段
    /// </summary>
	public class GucFrameBox : GucContainerControl
	{
		GucBorderBox box;

		public GucFrameBox(int borderSize = 0)
			: this(borderSize, borderSize) { }

		public GucFrameBox(int borderSize, int topBorderSize)
			: base(topBorderSize, borderSize, borderSize, borderSize)
		{
			box = new GucBorderBox(0, 0, borderSize);
			box.Initialize(Skin.BorderBackground, 1);
            //9个控制区域，只取前8个加入到本控件的“重绘区域列表”？
			CustomDrawRegions.AddRange(box.regions.Take(8));
		}

		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			if (box != null) box.Size = Size;
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucContainerControl（继承）、GucBorderBox
 * 2.在容器类中增加了GucBorderBox成员
***********************************/