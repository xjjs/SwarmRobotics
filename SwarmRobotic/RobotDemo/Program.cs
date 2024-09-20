//#define TEST

//using System;
//using System.Windows.Forms;

namespace RobotDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
//            ControlForm ctrlform;

//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//            ctrlform = new ControlForm();
//#if TEST
//            ctrlform.DefaultOperation();
//            ctrlform.game.Run();
//#else
//            while (true)
//            {
//                if (ctrlform.ShowDialog() == DialogResult.OK)
//                {
//                    ctrlform.game.Run();
//                    ctrlform.game.Dispose();
//                    ctrlform.RecreateInstance();
//                }
//                else
//                    break;
//            }
//#endif
			//using (TestGame game = new TestGame())
            //使用using(对象创建语句){对象处理语句}，可简化错误处理和资源清理(相当于try/catch+Dispose())
			using (RoboticGame game = new RoboticGame())
			{
				game.Run();
			}
        }
    }
#endif
}