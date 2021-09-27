// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Создание класса <see cref="ModelVisual3D"/> из <see cref="Mesh3D"/>
    /// </summary>
    public class Model3D : ModelVisual3D
    {
        /// <summary>
        /// Отображение
        /// </summary>
        private readonly TextureMapping _mapping = new TextureMapping();


        /// <summary>
        /// получить объект MeshGeometry3D от Viewport3D
        /// </summary>
        /// <param name="viewport3D">область просмотра</param>
        /// <param name="nModelIndex">индекс моеди</param>
        /// <returns></returns>
        public static MeshGeometry3D GetGeometry(Viewport3D viewport3D, int nModelIndex)
        {
            if (nModelIndex == -1) return null;
            ModelVisual3D visual3D = (ModelVisual3D)(viewport3D.Children[nModelIndex]);
            if (visual3D.Content == null) return null;
            GeometryModel3D triangleModel = (GeometryModel3D)(visual3D.Content);
            return (MeshGeometry3D)triangleModel.Geometry;
        }


        /// <summary>
        /// обновить объект <see cref="ModelVisual3D" /> с помощью <paramref name="meshs"/>
        /// </summary>
        /// <param name="meshs">список сеток</param>
        /// <param name="backMaterial">материал</param>
        /// <param name="nModelIndex">индекс модели</param>
        /// <param name="viewport3D">область просмотра</param>
        /// <returns>кол-во объектов</returns>
        public int UpdateModel(List<Mesh3D> meshs, Material backMaterial, int nModelIndex, Viewport3D viewport3D)
        {
            if (nModelIndex >= 0)
            {
                ModelVisual3D m = (ModelVisual3D)viewport3D.Children[nModelIndex];
                viewport3D.Children.Remove(m);
            }

            if (backMaterial == null)
                SetRGBColor();
            else
                SetPsedoColor();

            SetModel(meshs, backMaterial);

            int nModelNo = viewport3D.Children.Count;
            viewport3D.Children.Add(this);

            return nModelNo;
        }

        /// <summary>
        /// Задать RGB цвет
        /// </summary>
        private void SetRGBColor()
        {
            _mapping.SetRGBMaping();
        }

        /// <summary>
        /// Задать псевдо цвет
        /// </summary>
        private void SetPsedoColor()
        {
            _mapping.SetPseudoMaping();
        }

        /// <summary>
        /// Задать модель <see cref="ModelVisual3D"/> из <paramref name="meshs"/>
        /// </summary>
        /// <param name="meshs">список сеток</param>
        /// <param name="backMaterial">Материал</param>
        private void SetModel(IList<Mesh3D> meshs, Material backMaterial)
        {
            int nMeshNo = meshs.Count;
            if (nMeshNo == 0) return;

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            int nTotalVertNo = 0;
            for (int j = 0; j < nMeshNo; j++)
            {
                Mesh3D mesh = meshs[j];
                int nVertNo = mesh.VertexNo;
                int nTriNo = mesh.TriangleNo;
                if ((nVertNo <= 0) || (nTriNo <= 0)) continue;

                double[] vx = new double[nVertNo];
                double[] vy = new double[nVertNo];
                double[] vz = new double[nVertNo];
                for (int i = 0; i < nVertNo; i++)
                {
                    vx[i] = vy[i] = vz[i] = 0;
                }

                // получить нормаль каждой вершины
                for (int i = 0; i < nTriNo; i++)
                {
                    Triangle3D tri = mesh.GetTriangle(i);
                    Vector3D vN = mesh.GetTriangleNormal(i);
                    int n0 = tri.N0;
                    int n1 = tri.N1;
                    int n2 = tri.N2;

                    vx[n0] += vN.X;
                    vy[n0] += vN.Y;
                    vz[n0] += vN.Z;
                    vx[n1] += vN.X;
                    vy[n1] += vN.Y;
                    vz[n1] += vN.Z;
                    vx[n2] += vN.X;
                    vy[n2] += vN.Y;
                    vz[n2] += vN.Z;
                }
                for (int i = 0; i < nVertNo; i++)
                {
                    double length = Math.Sqrt(vx[i] * vx[i] + vy[i] * vy[i] + vz[i] * vz[i]);
                    if (length > 1e-20)
                    {
                        vx[i] /= length;
                        vy[i] /= length;
                        vz[i] /= length;
                    }
                    triangleMesh.Positions.Add(mesh.GetPoint(i));
                    Color color = mesh.GetColor(i);
                    Point mapPt = _mapping.GetMappingPosition(color);
                    triangleMesh.TextureCoordinates.Add(new Point(mapPt.X, mapPt.Y));
                    triangleMesh.Normals.Add(new Vector3D(vx[i], vy[i], vz[i]));
                }

                for (int i = 0; i < nTriNo; i++)
                {
                    Triangle3D tri = mesh.GetTriangle(i);
                    int n0 = tri.N0;
                    int n1 = tri.N1;
                    int n2 = tri.N2;

                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n0);
                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n1);
                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n2);
                }
                nTotalVertNo += nVertNo;
            }
            Material material = _mapping.Material;

            GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material)
            {
                Transform = new Transform3DGroup()
            };
            if (backMaterial != null) triangleModel.BackMaterial = backMaterial;

            Content = triangleModel;
        }

    }
}
