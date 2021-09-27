// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace VisualChart3D
{
    /// <summary>
    /// Треугольник в 3D
    /// </summary>
    public class Triangle3D
    {
        public Triangle3D(int m0, int m1, int m2)
        {
            N0 = m0; N1 = m1; N2 = m2;
        }
        /// <summary>
        /// Вершина 0
        /// </summary>
        public int N0 { get; private set; }

        /// <summary>
        /// Вершина 1
        /// </summary>
        public int N1 { get; private set; }

        /// <summary>
        /// Вершина 2
        /// </summary>
        public int N2 { get; private set; }
    }
}
