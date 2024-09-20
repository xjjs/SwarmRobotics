using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RobotLib
{
    //类型，枚举元素为整数类型
	public enum ParameterType
	{
		Boolean, String, Float, Int, Enum, Array
	}


    //特性在实际应用的时候一般去掉作为后缀的Attribute
    /// <summary>
    ///可修饰属性、字段，不可继承的特性；
    ///特性类型的属性有：类型（枚举型）、描述串、列表名、方法名
    /// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
	public class ParameterAttribute : Attribute
	{
		public ParameterAttribute(ParameterType type)
		{
			Type = type;
			Description = "";
			StringFuncName = null;
		}

		public ParameterAttribute(string funcName)
			: this(ParameterType.Array)
		{
			ValueArrayName = funcName; 
		}

		//public object Default { get; set; }
		public ParameterType Type { get; private set; }
		public string Description { get; set; }
		public string ValueArrayName { get; set; }
		public string StringFuncName { get; set; }
	}

    /// <summary>
    /// 扩展方法类：定义了Type类型的扩展方法，用以获取“某类型”的(属性,特性)数组；
    /// 定义了IParameter类型的扩展方法，用以设置相应实例的某属性的值；
    /// </summary>
    public static class ExtensionUtility
    {
        ///////////////////////////////ParameterAttribute/////////////////////////+

        public static Tuple<PropertyInfo, ParameterAttribute>[] GetParameterAttributes(this Type classType)
        {
            //定义主数据list：(属性,特性)Tuple的数组
            List<Tuple<PropertyInfo, ParameterAttribute>> list = new List<Tuple<PropertyInfo, ParameterAttribute>>();
            
            //获取每个属性和第一个特性，并加入list数组
            foreach (var pi in classType.GetProperties())
            {
                //获取属性的自定义特性数组
                var att = pi.GetCustomAttributes(typeof(ParameterAttribute), false);
                if (att.Length > 0) list.Add(Tuple.Create(pi, att[0] as ParameterAttribute));
            }

            //将数组元素根据“特性的描述串”排序后返回
            return list.OrderBy(t => t.Item2.Description).ToArray();
            //return classType.GetProperties().SelectMany(pi => pi.GetCustomAttributes(typeof(ParameterAttribute), true).OfType<ParameterAttribute>()).ToArray();
        }

        //IParameter的扩展方法，设置某对象某属性的值
        public static void SetValue(this IParameter instance, string PropertyName, object value)
        {
            instance.GetType().GetProperty(PropertyName).SetValue(instance, value, null);
        }
    }
}
