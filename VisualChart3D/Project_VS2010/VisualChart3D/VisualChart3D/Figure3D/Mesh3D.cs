// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    public class Mesh3D
    {
        /// <summary>
        /// Координаты вершины
        /// </summary>
        protected Point3D[] Points;

        /// <summary>
        /// Индексы в массиве позиций 
        /// </summary>
        protected int[] VertIndices;

        /// <summary>
        /// Цвет
        /// </summary>
        protected Color Color;

        /// <summary>
        /// Треугольник
        /// </summary>
        protected Triangle3D[] Tris;

        /// <summary>
        /// минимальная точка
        /// </summary>
        public Point3D MinPoint = new Point3D();

        /// <summary>
        /// Максимальная точка
        /// </summary>
        public Point3D MaxPoint = new Point3D();

        /// <summary>
        /// получить кол-во вершин в этой сетке
        /// </summary>
        public int VertexNo
        {
            get
            {
                return Points == null ? 0 : Points.Length;
            }
        }

        /// <summary>
        /// получить кол-во треугольников в этой сетке
        /// </summary>
        public int TriangleNo
        {
            get
            {
                return Tris == null ? 0 : Tris.Length;
            }
        }

        /// <summary>
        /// задать количество вершин в этой сетке
        /// </summary>
        /// <param name="nSize">кол-во вершин</param>
        public virtual void SetVertexNo(int nSize)
        {
            Points = new Point3D[nSize];
            VertIndices = new int[nSize];
        }

        /// <summary>
        /// задать кол-во треугольников в этой сетке
        /// </summary>
        /// <param name="nSize">кол-во треугольников</param>
        public void SetTriangleNo(int nSize)
        {
            Tris = new Triangle3D[nSize];
        }

        /// <summary>
        /// установить количество вершины и треугольник в этом массиве
        /// </summary>
        /// <param name="nVertexNo">кол-во вершин</param>
        /// <param name="nTriangleNo">кол-во треугольников</param>
        public virtual void SetSize(int nVertexNo, int nTriangleNo)
        {
            SetVertexNo(nVertexNo);
            SetTriangleNo(nTriangleNo);
        }

        /// <summary>
        /// получить позицию с указанной вершиной в этой сетке
        /// </summary>
        /// <param name="n">индекс вершины</param>
        /// <returns>Позиция вершины</returns>
        public Point3D GetPoint(int n)
        {
            return Points[n];
        }

        /// <summary>
        /// задать позицию с указанной вершиной в этой сетке
        /// </summary>
        /// <param name="n">индекс вершины</param>
        /// <param name="pt">Позиция вершины</param>
        public void SetPoint(int n, Point3D pt)
        {
            Points[n] = pt;
        }

        /// <summary>
        /// задать позицию с указанной вершиной в этой сетке
        /// </summary>
        /// <param name="n">индекс вершины</param>
        /// <param name="x">координата x</param>
        /// <param name="y">координата y</param>
        /// <param name="z">координа z</param>
        public void SetPoint(int n, double x, double y, double z)
        {
            Points[n] = new Point3D(x, y, z);
        }

        /// <summary>
        /// Получить треугольник м указанным индексом
        /// </summary>
        /// <param name="n">индекс</param>
        /// <returns>Треугольник</returns>
        public Triangle3D GetTriangle(int n)
        {
            return Tris[n];
        }

        /// <summary>
        /// Задать треугольник
        /// </summary>
        /// <param name="n">индекс</param>
        /// <param name="triangle">треугольник</param>
        public void SetTriangle(int n, Triangle3D triangle)
        {
            Tris[n] = triangle;
        }

        /// <summary>
        /// Задать треугольник
        /// </summary>
        /// <param name="i">индекс</param>
        /// <param name="m0">вершина 1 треугольника</param>
        /// <param name="m1">вершина 2 треугольника</param>
        /// <param name="m2">вершина 3 треугольника</param>
        public void SetTriangle(int i, int m0, int m1, int m2)
        {
            Tris[i] = new Triangle3D(m0, m1, m2);
        }

        /// <summary>
        /// получить направление нормали треугольника
        /// </summary>
        /// <param name="n">индекс</param>
        /// <returns>нормаль</returns>
        public Vector3D GetTriangleNormal(int n)
        {
            Triangle3D tri = GetTriangle(n);
            Point3D pt0 = GetPoint(tri.N0);
            Point3D pt1 = GetPoint(tri.N1);
            Point3D pt2 = GetPoint(tri.N2);

            double dx1 = pt1.X - pt0.X;
            double dy1 = pt1.Y - pt0.Y;
            double dz1 = pt1.Z - pt0.Z;

            double dx2 = pt2.X - pt0.X;
            double dy2 = pt2.Y - pt0.Y;
            double dz2 = pt2.Z - pt0.Z;

            double vx = dy1 * dz2 - dz1 * dy2;
            double vy = dz1 * dx2 - dx1 * dz2;
            double vz = dx1 * dy2 - dy1 * dx2;

            double length = Math.Sqrt(vx * vx + vy * vy + vz * vz);

            return new Vector3D(vx / length, vy / length, vz / length);
        }

        /// <summary>
        /// Получить цвет
        /// </summary>
        /// <param name="nV"></param>
        /// <returns></returns>
        public virtual Color GetColor(int nV)
        {
            return Color;
        }

        /// <summary>
        /// Задать цвет
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        public void SetColor(Byte r, Byte g, Byte b)
        {
            Color = Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Задать цвет
        /// </summary>
        /// <param name="color">цвет</param>
        public void SetColor(Color color)
        {
            Color = color;
        }

        /// <summary>
        /// Обновить позицию
        /// </summary>
        /// <param name="meshGeometry"></param>
        /// <remarks>вызывать после смены расположения</remarks>
        public void UpdatePositions(MeshGeometry3D meshGeometry)
        {
            int nVertNo = VertexNo;
            for (int i = 0; i < nVertNo; i++)
            {
                meshGeometry.Positions[i] = Points[i];
            }
        }

        // Задать тестовую модель
        public virtual void SetTestModel()
        {
            const double size = 10;
            SetSize(3, 1);
            SetPoint(0, -0.5, 0, 0);
            SetPoint(1, 0.5, 0.5, 0.3);
            SetPoint(2, 0, 0.5, 0);
            SetTriangle(0, 0, 2, 1);
            MinPoint = new Point3D(0, 0, -size);
            MaxPoint = new Point3D(2 * size, size, size);
        }
    }
}
