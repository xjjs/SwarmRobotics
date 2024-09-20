using System;
using Microsoft.Xna.Framework;

namespace RobotDemo.Display
{
    /// <summary>
    /// A supporting Class for <see cref="Primitive"/> and <see cref="PrimitiveNormal"/> class.
    /// <para>Provides methods for inserting indexed vertex arrays into <see cref="Primitive"/> and <see cref="PrimitiveNormal"/> model.</para>
    /// </summary>
    /// <remarks><see cref="Geometrics"/> uses <see cref="Primitive.AddIndex(int)"/>, <see cref="Primitive.AddVertex(Vector3, Color, Vector2)"/>, 
    /// <see cref="PrimitiveNormal.AddIndex(int)"/> and <see cref="PrimitiveNormal.AddVertex"/> methods to set Index and Vertex Array Lists.
    /// </remarks>
    /// <seealso cref="Primitive"/>
    /// <seealso cref="PrimitiveNormal"/>
    public static class Geometrics
    {
        /// <summary>
        /// 扩展方法，Adds a 2-D panel in 3-D environment.
        /// </summary>
        /// <param name="primitive">The primitive model.</param>
        /// <param name="height">The height of panel.</param>
        /// <param name="width">The width of panel.</param>
        /// <param name="normal">The normal vector.</param>
        /// <param name="origin">The origin vector of panel.</param>
        /// <remarks>The panel's height and width direction are decided by normal vector using right-hand law.</remarks>
        public static void AddPanel(this PrimitiveNormal primitive, float height, float width, Vector3 normal, Vector3 origin)
        {
            int vertexcount = primitive.VertexCount;

            // Get two vectors perpendicular to the face normal and to each other.获取3个方向的单位向量。
            normal.Normalize();
			Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);	//y-axis
			Vector3 side2 = Vector3.Cross(side1, normal);				//x-axis
			side1 *= height / 2;
            side2 *= width / 2;

            // Six indices (two triangles) per face.
            //一个矩形分为两个三角形，涉及6个顶点（两个重复的），原型中已有vertexcount个顶点，故新添加的顶点编号为vertexcount
            primitive.AddIndex(vertexcount, vertexcount + 1, vertexcount + 2, vertexcount, vertexcount + 2, vertexcount + 3);

            // Four vertices per face.
            //origin用于指定图形的位置，这里origin为矩形对角线的中心，指定四个顶点的“空间位置”与“纹理坐标”
            primitive.AddVertex(origin - side1 - side2, normal, new Vector2(0, 1));
			primitive.AddVertex(origin + side1 - side2, normal, new Vector2(0, 0));
			primitive.AddVertex(origin + side1 + side2, normal, new Vector2(1, 0));
            primitive.AddVertex(origin - side1 + side2, normal, new Vector2(1, 1));
        }

        /// <summary>
        /// Adds a 2-D panel in 3-D environment.
        /// </summary>
        /// <param name="primitive">The primitive model.</param>
        /// <param name="height">The height of panel.</param>
        /// <param name="width">The width of panel.</param>
        /// <param name="normal">The normal vector.</param>
        /// <param name="origin">The origin vector of panel.</param>
        /// <remarks>The panel's height and width direction are decided by normal vector using right-hand law.</remarks>
        public static void AddPanel(this Primitive primitive, float height, float width, Vector3 normal, Vector3 origin)
        {
            int vertexcount = primitive.VertexCount;

            // Get two vectors perpendicular to the face normal and to each other.
            normal.Normalize();
			Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);	//y-axis
			Vector3 side2 = Vector3.Cross(side1, normal);				//x-axis
			side1 *= height / 2;
            side2 *= width / 2;

            // Six indices (two triangles) per face.
            primitive.AddIndex(vertexcount, vertexcount + 1, vertexcount + 2,
                               vertexcount, vertexcount + 2, vertexcount + 3);

            // Four vertices per face.
            primitive.AddVertex(origin - side1 - side2, new Vector2(0, 1));
            primitive.AddVertex(origin + side1 - side2, new Vector2(0, 0));
            primitive.AddVertex(origin + side1 + side2, new Vector2(1, 0));
            primitive.AddVertex(origin - side1 + side2, new Vector2(1, 1));
		}

        /// <summary>
        /// Adds a cubic.
        /// </summary>
        /// <param name="primitive">The primitive model.</param>
        /// <param name="size">The edge length of the cubic.</param>
        /// <param name="origin">The origin vector of cubic.</param>
        /// <remarks>Normal of the cubic is (0,0,1). You can rotate the cubic in the <see cref="Primitive.Draw"/> method.</remarks>
        public static void AddCubic(this Primitive primitive, float size, Vector3 origin)
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            int vertexcount = primitive.VertexCount;
			float halfsize = size / 2;

            // Create each face in turn.
            foreach (Vector3 normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
				Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);	//y-axis
				Vector3 side2 = Vector3.Cross(side1, normal);				//x-axis

                // Six indices (two triangles) per face.
                primitive.AddIndex(vertexcount, vertexcount + 1, vertexcount + 2,
                                   vertexcount, vertexcount + 2, vertexcount + 3);

                // Four vertices per face.
                //normal既是每个面的法向量，也是该面的几何中心
				primitive.AddVertex((normal - side1 - side2) * halfsize + origin, new Vector2(0, 0));
				primitive.AddVertex((normal + side1 - side2) * halfsize + origin, new Vector2(0, 1));
				primitive.AddVertex((normal + side1 + side2) * halfsize + origin, new Vector2(1, 1));
				primitive.AddVertex((normal - side1 + side2) * halfsize + origin, new Vector2(1, 0));

                vertexcount += 4;
            }
        }

        /// <summary>
        /// Adds a cubic.
        /// </summary>
        /// <param name="primitive">The primitive model.</param>
        /// <param name="size">The edge length of the cubic.</param>
        /// <param name="origin">The origin vector of cubic.</param>
        /// <remarks>Normal of the cubic is (0,0,1). You can rotate the cubic in the <see cref="PrimitiveNormal.Draw"/> method.</remarks>
        public static void AddCubic(this PrimitiveNormal primitive, float size, Vector3 origin)
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            int vertexcount = primitive.VertexCount;
			float halfsize = size / 2;

            // Create each face in turn.
            foreach (Vector3 normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
				Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);	//y-axis
				Vector3 side2 = Vector3.Cross(side1, normal);				//x-axis

                // Six indices (two triangles) per face.
                primitive.AddIndex(vertexcount, vertexcount + 1, vertexcount + 2,
                                   vertexcount, vertexcount + 2, vertexcount + 3);

                // Four vertices per face.
				primitive.AddVertex((normal - side1 - side2) * halfsize + origin, normal, new Vector2(0, 0));
				primitive.AddVertex((normal + side1 - side2) * halfsize + origin, normal, new Vector2(0, 1));
				primitive.AddVertex((normal + side1 + side2) * halfsize + origin, normal, new Vector2(1, 1));
				primitive.AddVertex((normal - side1 + side2) * halfsize + origin, normal, new Vector2(1, 0));

                vertexcount += 4;
            }
        }

        /// <summary>
        /// 圆形也是通过三角形绘制的，整个圆分为points部分
        /// </summary>
		public static void AddCircle(this Primitive primitive, int points, float radius, Vector3 normal, Vector3 origin)
		{
			int vertexcount = primitive.VertexCount;

			// Get two vectors perpendicular to the face normal and to each other.
			normal.Normalize();
			Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);	//y-axis
			Vector3 side2 = Vector3.Cross(side1, normal);				//x-axis
			Vector3 pos;
			float angle = 0, fan = MathHelper.TwoPi / points;

            //origin为圆心，也是顶点vertexcount的位置，pos依次表示其他顶点的位置（即共有points+1个顶点），根据半径倍乘相应的比例因子
			primitive.AddVertex(origin, new Vector2(0.5f, 0.5f));
			for (int i = 0; i < points; i++, angle+=fan)
			{
				pos = side1 * (float)Math.Sin(angle) + side2 * (float)Math.Cos(angle);
				primitive.AddVertex(origin + pos * radius, new Vector2(pos.X + 1, pos.Y + 1) / 2);
				if (i == 0)
					primitive.AddIndex(vertexcount, vertexcount + 1, vertexcount + points);
				else
					primitive.AddIndex(vertexcount, vertexcount + i + 1, vertexcount + i);
			}
		}

		public static void AddCircle(this PrimitiveNormal primitive, int points, float radius, Vector3 normal, Vector3 origin)
		{
			int vertexcount = primitive.VertexCount;

			// Get two vectors perpendicular to the face normal and to each other.
			normal.Normalize();
			Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);	//y-axis
			Vector3 side2 = Vector3.Cross(side1, normal);				//x-axis
			Vector3 pos;
			float angle = 0, fan = MathHelper.TwoPi / points;

			primitive.AddVertex(origin, normal, new Vector2(0.5f, 0.5f));
			for (int i = 0; i < points; i++, angle += fan)
			{
				pos = side1 * (float)Math.Sin(angle) + side2 * (float)Math.Cos(angle);
				primitive.AddVertex(origin + pos * radius, normal, new Vector2(pos.X + 1, pos.Y + 1) / 2);

                //与上一个方法不同，此处为什么没有考虑最后一个三角形顶点编号的特殊性？
				primitive.AddIndex(vertexcount, vertexcount + i + 2, vertexcount + i + 1);
			}
		}

        /// <summary>
        /// Adds the simple craft.
        /// </summary>
        /// <param name="primitive">The primitive.</param>
        /// <param name="size">The size.</param>
        /// <param name="origin">The origin.</param>
        /// <remarks></remarks>
        public static void AddSimpleCraft(this Primitive primitive, float size, Vector3 origin)
        {
            int vertexcount = primitive.VertexCount;
            float halfsize = 0.5f * size, frontsize = 1.5f * size + size, wingsize = 1.5f * size + halfsize;
            Color WingColor = Color.Black, BodyColor = Color.White;

            primitive.AddVertex(origin + new Vector3(frontsize, 0, 0), WingColor, new Vector2(0.5f, 0.5f));  //0
            primitive.AddVertex(origin + new Vector3(0, -wingsize, 0), WingColor, new Vector2(0.5f, 0));  //1
            primitive.AddVertex(origin + new Vector3(-size, -wingsize, 0), WingColor, new Vector2(0.5f, 1));  //2
            primitive.AddVertex(origin + new Vector3(0, wingsize, 0), WingColor, new Vector2(0.5f, 1));  //3
            primitive.AddVertex(origin + new Vector3(-size, wingsize, 0), WingColor, new Vector2(0.5f, 0));  //4
            primitive.AddVertex(origin + new Vector3(size, -halfsize, -halfsize), BodyColor, new Vector2(0, 0));  //5
            primitive.AddVertex(origin + new Vector3(size, halfsize, -halfsize), BodyColor, new Vector2(0, 1));  //6
            primitive.AddVertex(origin + new Vector3(size, halfsize, halfsize), BodyColor, new Vector2(1, 1));  //7
            primitive.AddVertex(origin + new Vector3(size, -halfsize, halfsize), BodyColor, new Vector2(1, 0));  //8
            primitive.AddVertex(origin + new Vector3(-size, -halfsize, -halfsize), BodyColor, new Vector2(0, 1));  //9
            primitive.AddVertex(origin + new Vector3(-size, -halfsize, halfsize), BodyColor, new Vector2(1, 1));  //10
            primitive.AddVertex(origin + new Vector3(-size, halfsize, halfsize), BodyColor, new Vector2(1, 0));  //11
            primitive.AddVertex(origin + new Vector3(-size, halfsize, -halfsize), BodyColor, new Vector2(0, 0));  //12
            primitive.AddVertex(origin + new Vector3(0, -halfsize, -halfsize), BodyColor, new Vector2(0, 0.5f));  //13
            primitive.AddVertex(origin + new Vector3(0, halfsize, -halfsize), BodyColor, new Vector2(0, 0.5f));  //14
            primitive.AddVertex(origin + new Vector3(0, halfsize, halfsize), BodyColor, new Vector2(1, 0.5f));  //15
            primitive.AddVertex(origin + new Vector3(0, -halfsize, halfsize), BodyColor, new Vector2(1, 0.5f));  //16

            primitive.AddIndex(vertexcount, vertexcount + 5, vertexcount + 6,
                               vertexcount, vertexcount + 6, vertexcount + 7,
                               vertexcount, vertexcount + 7, vertexcount + 8,
                               vertexcount, vertexcount + 8, vertexcount + 5,
                               vertexcount + 5, vertexcount + 9, vertexcount + 12,
                               vertexcount + 5, vertexcount + 12, vertexcount + 6,
                               vertexcount + 8, vertexcount + 11, vertexcount + 10,
                               vertexcount + 8, vertexcount + 7, vertexcount + 11,
                               vertexcount + 9, vertexcount + 10, vertexcount + 11,
                               vertexcount + 9, vertexcount + 11, vertexcount + 12,
                               vertexcount + 1, vertexcount + 13, vertexcount + 5,
                               vertexcount + 1, vertexcount + 8, vertexcount + 16,
                               vertexcount + 1, vertexcount + 5, vertexcount + 8,
                               vertexcount + 1, vertexcount + 2, vertexcount + 9,
                               vertexcount + 1, vertexcount + 9, vertexcount + 13,
                               vertexcount + 1, vertexcount + 16, vertexcount + 10,
                               vertexcount + 1, vertexcount + 10, vertexcount + 2,
                               vertexcount + 2, vertexcount + 10, vertexcount + 9,
                               vertexcount + 3, vertexcount + 6, vertexcount + 14,
                               vertexcount + 3, vertexcount + 15, vertexcount + 7,
                               vertexcount + 3, vertexcount + 7, vertexcount + 6,
                               vertexcount + 3, vertexcount + 14, vertexcount + 12,
                               vertexcount + 3, vertexcount + 12, vertexcount + 4,
                               vertexcount + 3, vertexcount + 4, vertexcount + 11,
                               vertexcount + 3, vertexcount + 11, vertexcount + 15,
                               vertexcount + 4, vertexcount + 12, vertexcount + 11
                               );
        }

		public static void AddSphere(this Primitive primitive, float radius, int tessellation, Vector3 origin)
		{
            //tessellation通过嵌入顶点的方式细化曲面
			if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation");
			int vertexcount = primitive.VertexCount;

			int verticalSegments = tessellation;
			int horizontalSegments = tessellation * 2;

            //从球体的底部开始——下面的代码都是将相对坐标（未叠加origin）添加到了原型类中
			// Start with a single vertex at the bottom of the sphere.
			primitive.AddVertex(Vector3.Down * radius, new Vector2(0.5f, 1));

            //定位与创建不同的纬度（按纬度由低到高的顺序添加顶点）
			// Create rings of vertices at progressively higher latitudes.
			for (int i = 1; i < verticalSegments; i++)
			{
                //球心到各纬度的射线与赤道平面的夹角（上为正、下为负）
				float latitude = (i * MathHelper.Pi / verticalSegments) - MathHelper.PiOver2;

                //纬线圈的高度与半径
				float dy = (float)Math.Sin(latitude);
				float dxz = (float)Math.Cos(latitude);

                //在选定的维度上进行经度划分
				// Create a single ring of vertices at this latitude.
				for (int j = 0; j < horizontalSegments; j++)
				{
					float longitude = j * MathHelper.TwoPi / horizontalSegments;

					float dx = (float)Math.Cos(longitude) * dxz;
					float dz = (float)Math.Sin(longitude) * dxz;

					primitive.AddVertex(new Vector3(dx, dy, dz) * radius, new Vector2((float)j / horizontalSegments, (float)i / verticalSegments));
				}
			}

            //创建球体的顶点
			// Finish with a single vertex at the top of the sphere.
			primitive.AddVertex(Vector3.Up * radius, new Vector2(0.5f, 0));

            //创建球体底部到最低纬度部分的三角形（沿该纬度切下的球底部分）
			// Create a fan connecting the bottom vertex to the bottom latitude ring.
			for (int i = 0; i < horizontalSegments; i++)
			{
				primitive.AddIndex(vertexcount);
				primitive.AddIndex(vertexcount + 1 + (i + 1) % horizontalSegments);
				primitive.AddIndex(vertexcount + 1 + i);
			}

            //创建纬线圈之间的三角形
			// Fill the sphere body with triangles joining each pair of latitude rings.
			for (int i = 0; i < verticalSegments - 2; i++)
			{
				for (int j = 0; j < horizontalSegments; j++)
				{
					int nextI = i + 1;
					int nextJ = (j + 1) % horizontalSegments;

					primitive.AddIndex(vertexcount + 1 + i * horizontalSegments + j);
					primitive.AddIndex(vertexcount + 1 + i * horizontalSegments + nextJ);
					primitive.AddIndex(vertexcount + 1 + nextI * horizontalSegments + j);

					primitive.AddIndex(vertexcount + 1 + i * horizontalSegments + nextJ);
					primitive.AddIndex(vertexcount + 1 + nextI * horizontalSegments + nextJ);
					primitive.AddIndex(vertexcount + 1 + nextI * horizontalSegments + j);
				}
			}

			vertexcount = primitive.VertexCount;
			// Create a fan connecting the top vertex to the top latitude ring.
			for (int i = 0; i < horizontalSegments; i++)
			{
				primitive.AddIndex(vertexcount - 1);
				primitive.AddIndex(vertexcount - 2 - (i + 1) % horizontalSegments);
				primitive.AddIndex(vertexcount - 2 - i);
			}
		}

		public static void AddSphere(this PrimitiveNormal primitive, float radius, int tessellation, Vector3 origin)
		{
			if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation");
			int vertexcount = primitive.VertexCount;

			int verticalSegments = tessellation;
			int horizontalSegments = tessellation * 2;

			// Start with a single vertex at the bottom of the sphere.
			primitive.AddVertex(Vector3.Down * radius, Vector3.Down, new Vector2(0.5f, 1));

			// Create rings of vertices at progressively higher latitudes.
			for (int i = 1; i < verticalSegments; i++)
			{
				float latitude = (i * MathHelper.Pi / verticalSegments) - MathHelper.PiOver2;

				float dy = (float)Math.Sin(latitude);
				float dxz = (float)Math.Cos(latitude);

				// Create a single ring of vertices at this latitude.
				for (int j = 0; j < horizontalSegments; j++)
				{
					float longitude = j * MathHelper.TwoPi / horizontalSegments;

					float dx = (float)Math.Cos(longitude) * dxz;
					float dz = (float)Math.Sin(longitude) * dxz;

					Vector3 normal = new Vector3(dx, dy, dz);

					primitive.AddVertex(normal * radius, normal, new Vector2((float)j / horizontalSegments, (float)i / verticalSegments));
				}
			}

			// Finish with a single vertex at the top of the sphere.
			primitive.AddVertex(Vector3.Up * radius, Vector3.Up, new Vector2(0.5f, 0));

			// Create a fan connecting the bottom vertex to the bottom latitude ring.
			for (int i = 0; i < horizontalSegments; i++)
			{
				primitive.AddIndex(vertexcount);
				primitive.AddIndex(vertexcount + 1 + (i + 1) % horizontalSegments);
				primitive.AddIndex(vertexcount + 1 + i);
			}

			// Fill the sphere body with triangles joining each pair of latitude rings.
			for (int i = 0; i < verticalSegments - 2; i++)
			{
				for (int j = 0; j < horizontalSegments; j++)
				{
					int nextI = i + 1;
					int nextJ = (j + 1) % horizontalSegments;

					primitive.AddIndex(vertexcount + 1 + i * horizontalSegments + j);
					primitive.AddIndex(vertexcount + 1 + i * horizontalSegments + nextJ);
					primitive.AddIndex(vertexcount + 1 + nextI * horizontalSegments + j);

					primitive.AddIndex(vertexcount + 1 + i * horizontalSegments + nextJ);
					primitive.AddIndex(vertexcount + 1 + nextI * horizontalSegments + nextJ);
					primitive.AddIndex(vertexcount + 1 + nextI * horizontalSegments + j);
				}
			}

			vertexcount = primitive.VertexCount;
			// Create a fan connecting the top vertex to the top latitude ring.
			for (int i = 0; i < horizontalSegments; i++)
			{
				primitive.AddIndex(vertexcount - 1);
				primitive.AddIndex(vertexcount - 2 - (i + 1) % horizontalSegments);
				primitive.AddIndex(vertexcount - 2 - i);
			}
		}
    }
}
