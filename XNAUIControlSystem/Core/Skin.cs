using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
    //皮肤选项类型
    public enum SkinItemType {
        Font, Texture, DisplayTexture
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    sealed class SkinItemAttribute : Attribute {
        SkinItemType type;

        public SkinItemAttribute(SkinItemType type) { this.type = type; }

        public SkinItemType Type { get { return type; } }
    }


	public class Skin
	{
        //属性名称，属性信息、特性类型，列表成员
		static Dictionary<string, Tuple<PropertyInfo, SkinItemType>> properties;

        //skin类的各种属性及其相应的特性
        public Texture2D Texture { get; set; }

        [SkinItem(SkinItemType.Font)]
        public SpriteFont TextFont { get; set; }

        [SkinItem(SkinItemType.Texture)]
        public Rectangle White1x1 { get; set; }

        [SkinItem(SkinItemType.Texture)]
        public Rectangle BorderBackground { get; set; }

        //复选框
        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture CheckBoxNormal { get; set; }

        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture CheckBoxChecked { get; set; }

        //单选框
        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture RadioBoxNormal { get; set; }

        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture RadioBoxChecked { get; set; }

        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture UpArrow { get; set; }

        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture DownArrow { get; set; }

        [SkinItem(SkinItemType.DisplayTexture)]
        public DisplayTexture Button { get; set; }

        //文本框
        [SkinItem(SkinItemType.Texture)]
        public Rectangle TextBox { get; set; }

        [SkinItem(SkinItemType.Texture)]
        public Rectangle TextBoxMargin { get; set; }


        //读取类的属性成员及其特性到列表成员
		static Skin() 
		{
            //获取属性成员
			PropertyInfo[] props = typeof(Skin).GetProperties(); 
            //设置属性字典
			properties = new Dictionary<string, Tuple<PropertyInfo, SkinItemType>>(props.Length);
			foreach (var prop in props)
			{
                    //从属性成员获取特性数组
				var atts = prop.GetCustomAttributes(typeof(SkinItemAttribute), false);   //返回属性成员的特性
				if (atts.Length > 0)
				{
                    //从特性数组，获取第一个特性的名称（枚举型Type成员）
					properties.Add(prop.Name, Tuple.Create(prop, (atts[0] as SkinItemAttribute).Type));   
				}
			}
		}

        //将字符串解析为矩形参数
        static Rectangle ParseRectangle(string input) {
            string[] split = input.Split(',');
            //Array.Resize(ref split, 4);
            return new Rectangle(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]));
        }

		private Skin() { }

		public static Skin LoadSkin(string assetName, ContentManager Content)
		{
            //创建<属性信息，特性>的项作为字典值
			Tuple<PropertyInfo, SkinItemType> item;  
            //定义Xml特性
			XmlAttribute att;
			var skin = new Skin();                                              //创建对象
            //纹理属性要单独载入
			skin.Texture = Content.Load<Texture2D>(assetName);                  
            //新建XML文档对象并载入文档
			XmlDocument doc = new XmlDocument();
			doc.Load(Path.Combine(Content.RootDirectory, assetName + ".xml"));  
            //获取文档根节点
			var Node = doc.SelectSingleNode("GucSkin");                         

            //遍历根节点的子节点
			foreach (XmlElement child in Node.ChildNodes)                        
			{
                //根据XML文件中的小节名（即Skin类的属性名）获取字典值
				if (properties.TryGetValue(child.LocalName, out item)) 
				{
                    //特性值（字典值的第二分量）决定了XML数据的项，根据特性值读取相应的XML项
                    //用该项的值（程序所需的设置数据）以重置字典值第一分量（属性信息）
                    //即XML是一个单独的设置文档，其中保存着设置相关的数据
					switch (item.Item2)     
					{
						case SkinItemType.Font:
							att = child.Attributes["Font"];                            
							item.Item1.SetValue(skin, Content.Load<SpriteFont>(att.Value), null);
							break;
						case SkinItemType.Texture:
							att = child.Attributes["Texture"];
							item.Item1.SetValue(skin, ParseRectangle(att.Value), null);
							break;
						//case SkinItemType.DisplayTexture:
						default:
							DisplayTexture dt = new DisplayTexture();            //创建对象

							att = child.Attributes["Normal"];                  //获取相应矩形设置
							dt.Normal = ParseRectangle(att.Value);

							att = child.Attributes["Press"];
							if (att == null)
								dt.Pressed = dt.Normal;
							else
								dt.Pressed = ParseRectangle(att.Value);

							att = child.Attributes["Hover"];
							if (att == null)
								dt.Hover = dt.Normal;
							else
								dt.Hover = ParseRectangle(att.Value);

							item.Item1.SetValue(skin, dt, null);                   
							break;
					}
				}
			}
			return skin;
		}

	}

}

/************************
 * 主要功能与实现
 * 1.Skin类的属性列表（不同的控件对应不同的属性），对应于XML文档的节点列表
 * 2.属性的特性，对应于XML文档节点的类型（不同类型的节点有不同的数据项）
 * 3.通过设置XML文档中数据项来设置属性值（即设置皮肤数据）
 * 4.遍历XML的节点以遍历Skin的属性，并获取相应的特性值
 * 5.对不同的特性值（不同的节点）分别编写设置代码——从XML获取数据项并赋值给属性的信息PropertyInfo
*************************/