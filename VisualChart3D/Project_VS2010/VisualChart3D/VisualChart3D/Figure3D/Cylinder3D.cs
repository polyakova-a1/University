// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Цилиндр 3D
    /// </summary>
    public class Cylinder3D : Mesh3D
    {
        /// <summary>
        /// Цилиндр 3D
        /// </summary>
        /// <param name="a">ширина</param>
        /// <param name="b">длина</param>
        /// <param name="h">высота</param>
        /// <param name="nRes">гладкость</param>
        public Cylinder3D(double a, double b, double h, int nRes)
        {
            SetMesh(nRes);
            SetData(a, b, h, nRes);
        }

        /// <summary>
        /// Задать сетчатую структуру
        /// </summary>
        /// <param name="nRes">кол-во треугольников</param>
        private void SetMesh(int nRes)
        {
            int nVertNo = 2 * nRes + 2;
            int nTriNo = 4 * nRes;
            SetSize(nVertNo, nTriNo);
            for (int i = 0; i < nRes; i++)
            {
                int n1 = i;
                int n2;
                if (i == (nRes - 1)) n2 = 0;
                else n2 = i + 1;
                SetTriangle(i * 4 + 0, n1, n2, nRes + n1); // side
                SetTriangle(i * 4 + 1, nRes + n1, n2, nRes + n2); // side
                SetTriangle(i * 4 + 2, n2, n1, 2 * nRes); // bottom
                SetTriangle(i * 4 + 3, nRes + n1, nRes + n2, 2 * nRes + 1); // top
            }
        }

        /// <summary>
        /// Установка пространсвернных координат
        /// </summary>
        /// <param name="a">ширина</param>
        /// <param name="b">длина</param>
        /// <param name="h">высота</param>
        /// <param name="nRes">кол-во треугольников</param>
        private void SetData(double a, double b, double h, int nRes)
        {
            double stepXandY = 2.0f * 3.1415926f / ((double)nRes);
            for (int i = 0; i < nRes; i++)
            {
                double aXandY = i * stepXandY;
                SetPoint(i, a * System.Math.Cos(aXandY), b * System.Math.Sin(aXandY), -h / 2);
            }

            for (int i = 0; i < nRes; i++)
            {
                double aXandY = i * stepXandY;
                SetPoint(nRes + i, a * System.Math.Cos(aXandY), b * System.Math.Sin(aXandY), h / 2);
            }

            SetPoint(2 * nRes, 0, 0, -h / 2);
            SetPoint(2 * nRes + 1, 0, 0, h / 2);

            MinPoint = new Point3D(-a, -b, -h / 2);
            MaxPoint = new Point3D(a, b, h / 2);
        }
    }
}
