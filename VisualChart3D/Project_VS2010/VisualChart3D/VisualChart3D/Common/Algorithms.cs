using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System;
using StarMathLib;

namespace VisualChart3D.Common
{
    public static class Algorithms
    {
        public struct SegmentInfo
        {
            public double Distance { get; }
            public int First { get; }
            public int Second { get; }

            public SegmentInfo(double distance, int first, int second)
            {
                Distance = distance;
                First = first;
                Second = second;
            }
        }

        public class BrokenLineStats
        {
            public int LineIndex { get; set; } = -1;
            public IList<int> BrokenLine { get; set; }
            public double Length { get; set; }
            public double TwoEndsDistance { get; set; }
            public double Difference { get; set; }
            public double SegmentAverageDistance { get; set; }
            public SegmentInfo MinSegment { get; set; }
            public SegmentInfo MaxSegment { get; set; }
        }
        
        public static List<List<int>> GetShortestOpenPath(double[,] distMatrix,
                                                          Func<double[,], (double value, int i, int j)> minFunc = null)
        {
            Contract.Requires(   distMatrix.GetLongLength(0) != 0
                              && distMatrix.GetLongLength(0) == distMatrix.GetLongLength(1));

            if (minFunc == null)
            {
                minFunc = (double[,] matr) =>
                {
                    Contract.Requires(matr.GetLength(0) == matr.GetLength(1) && matr.GetLength(0) != 0);
                    var minimum = (value: double.MaxValue, i: -1, j: -1);

                    // Ищем 2 наиболее близких вершины
                    for (var i = 0; i < matr.GetLength(0); ++i)
                    {
                        for (var j = i + 1; j < matr.GetLength(1); ++j)
                        {
                            if (matr[i, j] < minimum.value)
                            {
                                minimum = (matr[i, j], i, j);
                            }
                        }
                    }

                    Contract.Ensures(minimum.i != -1 && minimum.j != -1);
                    return minimum;
                };
            }

            var firstMin = minFunc(distMatrix);

            Contract.Assert(firstMin.i != -1 && firstMin.j != -1);

            // Формируем терминальные вершины
            var endings = (end1: firstMin.i, end2: firstMin.j);

            // Формируем список всех вершин
            var freeVertexes = new HashSet<int>(Enumerable.Range(0, distMatrix.GetLength(0)));

            // Граф, соответствующий кратчайшему незамкнутому пути
            // Граф кодируем списком смежности
            var graph = new List<List<int>>(distMatrix.GetLength(0));
            Enumerable.Range(0, distMatrix.GetLength(0)).ToList().ForEach(i => graph.Add(new List<int>()));

            // Добавляем 2 вершины с самым минимальным путем
            graph[firstMin.i].Add(firstMin.j);
            graph[firstMin.j].Add(firstMin.i);

            // Удаляем терминальные вершины из рассмотрения
            freeVertexes.Remove(firstMin.i);
            freeVertexes.Remove(firstMin.j);

            var cmp = Comparer<(int v, int e, double d)>.Create((l, r) => Comparer<double>.Default.Compare(l.d, r.d));

            // Пока есть неизолированные вершины
            while (freeVertexes.Any())
            {
                var (end1, end2) = endings;

                var dists = new (int v, int e, double d)[2 * freeVertexes.Count];
                foreach (var (v, i) in freeVertexes.Select((v, i) => (v, i)))
                {
                    dists[i] = (v, end1, distMatrix[end1, v]);
                    dists[freeVertexes.Count + i] = (v, end2, distMatrix[end2, v]);
                }

                var (vertex, end, _) = dists.Min(cmp);

                // Добавляем неизолированную вершину к графу,
                // помечаем ее как изолированную
                graph[end].Add(vertex);
                graph[vertex].Add(end);

                freeVertexes.Remove(vertex);
                if (endings.end1 == end)
                    endings.end1 = vertex;
                else
                    endings.end2 = vertex;
            }

            Contract.Ensures(graph.Count == distMatrix.GetLongLength(0));
            Contract.Ensures(graph.All(vertex => vertex.Count is > 0 and <= 2));
            return graph;
        }

        private static List<List<int>> GetNonGreedyShortestOpenPath_Impl(double[,] distMatrix,
                                                                         List<List<int>> graph,
                                                                         (int end1, int end2) endings,
                                                                         HashSet<int> freeVertexes)
        {
            Contract.Requires(freeVertexes.Count != 0);

            var cmp = Comparer<(int vertex, double d)>.Create((l, r) => Comparer<double>.Default.Compare(l.d, r.d));

            // Берем изолированную вершину, ищем ближайшую к ней неизолированную
            while (freeVertexes.Any())
            {
                var (end1, end2) = endings;
    
                var dists1 = new (int v, double d)[freeVertexes.Count];
                var dists2 = new (int v, double d)[freeVertexes.Count];
                foreach (var (vertex, i) in freeVertexes.Select((v, i) => (v, i)))
                {
                    dists1[i] = (vertex, distMatrix[end1, vertex]);
                    dists2[i] = (vertex, distMatrix[end2, vertex]);
                }

                var dists = dists1.Concat(dists2);
                var min = dists.Min(cmp);
                var choices = Enumerable.Concat(dists1.Where(obj => Math.Abs(min.d - obj.d) <= double.Epsilon).Select(dist => (end: end1, dist.v, dist.d)),
                                                dists2.Where(obj => Math.Abs(min.d - obj.d) <= double.Epsilon).Select(dist => (end: end2, dist.v, dist.d)))
                                        .ToArray();
                if (choices.Length > 1)
                {
                    var graphs = new List<List<int>>[choices.Length];
                    foreach (var ((end, v, d), i) in choices.Select((choice, i) => (choice, i)))
                    {
                        var graphCopy = graph.Select(v => v.ToList()).ToList();
                        var endingsCopy = endings;
                        var freeVertexesCopy = freeVertexes.ToHashSet();

                        graphCopy[end].Add(v);
                        graphCopy[v].Add(end);

                        freeVertexesCopy.Remove(v);
                        if (endingsCopy.end1 == end)
                            endingsCopy.end1 = v;
                        else
                            endingsCopy.end2 = v;

                        graphs[i] = GetNonGreedyShortestOpenPath_Impl(distMatrix, graphCopy, endingsCopy, freeVertexesCopy);
                    }

                    var (_, idx) = graphs.Select(graph => GetBrokenLineLength(AdjacentListToBrokenLine(graph), distMatrix)).MinIndex();
                    return graphs[idx];
                }
                else
                {
                    var (end, v, d) = choices.First();

                    // Добавляем неизолированную вершину к графу,
                    // помечаем ее как изолированную
                    graph[end].Add(v);
                    graph[v].Add(end);

                    freeVertexes.Remove(v);
                    if (endings.end1 == end)
                        endings.end1 = v;
                    else
                        endings.end2 = v;
                }
            }

            return graph;
        }

        public static List<List<int>> GetNonGreedyShortestOpenPath(double[,] distMatrix,
                                                                   Func<double[,], HashSet<(double value, int i, int j)>> minFunc = null)
        {
            Contract.Requires(   distMatrix.GetLongLength(0) != 0
                              && distMatrix.GetLongLength(0) == distMatrix.GetLongLength(1));

            if (minFunc == null)
            {
                minFunc = (double[,] matr) =>
                {
                    var minimum = (value: double.MaxValue, i: -1, j: -1);

                    // Ищем 2 наиболее близких вершины
                    for (var i = 0; i < matr.GetLength(0); ++i)
                    {
                        for (var j = i + 1; j < matr.GetLength(1); ++j)
                        {
                            if (matr[i, j] < minimum.value)
                            {
                                minimum = (matr[i, j], i, j);
                            }
                        }
                    }

                    if (minimum.i < 0 || minimum.j < 0)
                    {
                        return new HashSet<(double value, int i, int j)>();
                    }

                    var minimums = new HashSet<(double value, int i, int j)> { minimum };

                    // Проверим еще раз. Возможно, есть несколько начальных решений
                    for (var i = 0; i < matr.GetLength(0); ++i)
                    {
                        for (var j = i + 1; j < matr.GetLength(1); ++j)
                        {
                            if (Math.Abs(matr[i, j] - minimum.value) <= double.Epsilon)
                            {
                                minimums.Add((matr[i, j], i, j));
                            }
                        }
                    }

                    return minimums;
                };
            }

            var minimums = minFunc(distMatrix);

            Contract.Assert(minimums.Any());

            var graphs = new List<List<int>>[minimums.Count];
            foreach (var ((value, i, j), it) in minimums.Select((choice, i) => (choice, i)))
            {
                // Формируем список всех вершин
                var freeVertexes = new HashSet<int>(Enumerable.Range(0, distMatrix.GetLength(0)));

                // Граф, соответствующий кратчайшему незамкнутому пути
                // Граф кодируем списком смежности
                var graph = new List<List<int>>(distMatrix.GetLength(0));
                freeVertexes.ToList().ForEach(i => graph.Add(new List<int>()));
                var endings = (end1: i, end2: j);
                
                graph[i].Add(j);
                graph[j].Add(i);
                freeVertexes.Remove(i);
                freeVertexes.Remove(j);

                graphs[it] = GetNonGreedyShortestOpenPath_Impl(distMatrix, graph, endings, freeVertexes);
            }

            var (_, idx) = graphs.Select(graph => GetBrokenLineLength(AdjacentListToBrokenLine(graph), distMatrix)).MinIndex();
            var resGraph = graphs[idx];

            Contract.Ensures(resGraph.Count == distMatrix.GetLongLength(0));
            Contract.Ensures(resGraph.All(vertex => vertex.Count is > 0 and <= 2));

            return resGraph;
        }

        public static List<int> AdjacentListToBrokenLine(List<List<int>> graph)
        {
            Contract.Requires(graph.All(adjList => adjList.Count <= 2));

            var start = graph.FindIndex(adj => adj.Count == 1);
            if (start < 0)
            {
                throw new ArgumentException("Graph wasn't open.");
            }

            var brokenLine = new List<int>(2 * (graph.Count - 2) + 2)
            {
                start
            };

            var vertex = graph[start].First();
            var queue = new Queue<int>();
            queue.Enqueue(vertex);
            
            while (queue.Count != 0)
            {
                vertex = queue.Dequeue();
                brokenLine.Add(vertex);

                foreach (var adjVertex in graph[vertex].Except(brokenLine))
                {
                    queue.Enqueue(adjVertex);
                }
            }

            return brokenLine;
        }

        public static double GetBrokenLineLength(int[] brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Length >= 2);
            double res = 0.0;
            for (var i = 1; i < brokenLine.Length; ++i)
            {
                res += distMatrix[brokenLine[i - 1], brokenLine[i]];
            }

            return res;
        }

        public static double GetBrokenLineLength(IList<int> brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Count >= 2);
            double res = 0.0;
            for (var i = 1; i < brokenLine.Count; ++i)
            {
                res += distMatrix[brokenLine[i - 1], brokenLine[i]];
            }

            return res;
            // return brokenLine.AdjacentAggregate(0.0, (seed, left, right) => seed + distMatrix[left, right]);
        }

        public static double GetDistBetweenTwoEnds(int[] brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Length >= 2);
            return distMatrix[brokenLine[0], brokenLine[^1]];
        }

        public static double GetDistBetweenTwoEnds(IList<int> brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Count >= 2);
            return distMatrix[brokenLine[0], brokenLine[^1]];
        }

        public static void SwapRowColumn(double[,] distMatrix, int first, int second)
        {
            var col1 = distMatrix.GetColumn(first);
            var col2 = distMatrix.GetColumn(second);

            distMatrix.SetColumn(first, col2);
            distMatrix.SetColumn(second, col1);

            var row1 = distMatrix.GetRow(first);
            var row2 = distMatrix.GetRow(second);

            distMatrix.SetRow(first, row2);
            distMatrix.SetRow(second, row1);
        }

        public static void SwapRowColumn((int, int)[,] names, int first, int second)
        {
            for (var j = 0; j < names.GetLength(1); ++j)
            {
                var tmp = names[j, first];
                names[j, first] = names[j, second];
                names[j, second] = tmp;
            }

            for (var i = 0; i < names.GetLength(0); ++i)
            {
                var tmp = names[first, i];
                names[first, i] = names[second, i];
                names[second, i] = tmp;
            }
        }

        public static (double[,], (int, int)[,]) OptimizeDistMatrix(double[,] distMatrix)
        {
            double[,] ret = (double[,]) distMatrix.Clone();
            var names = new (int, int)[distMatrix.GetLength(0), distMatrix.GetLength(1)];
            for (int i = 0; i < names.GetLength(0); ++i)
            {
                for (int j = 0; j < names.GetLength(1); ++j)
                {
                    names[i, j] = (i, j);
                }
            }
            
            for (var i = 0; i < ret.GetLength(0) - 1; ++i)
            {
                var (_, idx) = ret.GetRow(i).Skip(i + 1).MinIndex();
                idx += i + 1;
                if (idx != i + 1)
                {
                    var col1 = ret.GetColumn(i + 1);
                    var col2 = ret.GetColumn(idx);

                    ret.SetColumn(i + 1, col2);
                    ret.SetColumn(idx, col1);

                    for (int j = 0; j < names.GetLength(0); ++j)
                    {
                        var tmp = names[j, idx];
                        names[j, idx] = names[j, i + 1];
                        names[j, i + 1] = tmp;
                    }

                    var row1 = ret.GetRow(i + 1);
                    var row2 = ret.GetRow(idx);

                    ret.SetRow(i + 1, row2);
                    ret.SetRow(idx, row1);

                    for (int j = 0; j < names.GetLength(0); ++j)
                    {
                        var tmp = names[idx, j];
                        names[idx, j] = names[i + 1, j];
                        names[i + 1, j] = tmp;
                    }
                }
            }

            return (ret, names);
        }

        public static double GetTraceForDiagonalAboveMain(double[,] distMatrix)
        {
            double res = 0.0;
            for (var i = 0; i < distMatrix.GetLength(0) - 1; ++i)
            {
                res += distMatrix[i, i + 1];
            }

            return res;
        }

        public static (SegmentInfo, SegmentInfo) GetMinMaxSegmentLength(IList<int> brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Count >= 2, "Broken line must contain at least 2 elements.");

            var segmentLengths = new List<double>(brokenLine.Count - 1);
            for (var i = 1; i < brokenLine.Count; ++i)
            {
                segmentLengths.Add(distMatrix[brokenLine[i - 1], brokenLine[i]]);
            }

            var (segmentMin, segmentMax) = segmentLengths.MinMaxIndex();
            return (new SegmentInfo(segmentMin.Item1, brokenLine[segmentMin.Item2], brokenLine[segmentMin.Item2 + 1]),
                    new SegmentInfo(segmentMax.Item1, brokenLine[segmentMax.Item2], brokenLine[segmentMax.Item2 + 1]));
        }

        public static SegmentInfo GetMinimumSegmentLength(List<int> brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Count >= 2, "Broken line must contain at least 2 elements.");

            var segmentLengths = new List<double>(brokenLine.Count - 1);
            for (var i = 1; i < brokenLine.Count; ++i)
            {
                segmentLengths.Add(distMatrix[brokenLine[i - 1], brokenLine[i]]);
            }

            var (segmentMin, segmentIdx) = segmentLengths.MinIndex();
            return new SegmentInfo(segmentMin, brokenLine[segmentIdx], brokenLine[segmentIdx + 1]);
        }

        public static SegmentInfo GetMaximumSegmentLength(List<int> brokenLine, double[,] distMatrix)
        {
            Contract.Requires(brokenLine.Count >= 2, "Broken line must contain at least 2 elements.");
            
            var segmentLengths = new List<double>(brokenLine.Count - 1);
            for (var i = 1; i < brokenLine.Count; ++i)
            {
                segmentLengths.Add(distMatrix[brokenLine[i - 1], brokenLine[i]]);
            }

            var (segmentMax, segmentIdx) = segmentLengths.MaxIndex();
            return new SegmentInfo(segmentMax, brokenLine[segmentIdx], brokenLine[segmentIdx + 1]);
        }

        public static void PermutateDistMatrix(double[,] res, IList<int> order, IList<int> originOrder)
        {
            PermutateDistMatrix(res, null, order, originOrder);
        }

        public static void PermutateDistMatrix(double[,] res, (int, int)[,] names, IList<int> order, IList<int> originOrder)
        {
            if (order.Count == 0 || order.Count != originOrder.Count)
            {
                throw new ArgumentException($"'{nameof(order)}' and '${nameof(originOrder)}' " +
                                             "contain different number of elements.");
            }

            for (var i = 0; i < order.Count; ++i)
            {
                if (order[i] != originOrder[i])
                {
                    var idxToPerm = originOrder.IndexOf(order[i]);
                    SwapRowColumn(res, originOrder[i], originOrder[idxToPerm]);

                    if (names != null)
                    {
                        SwapRowColumn(names, originOrder[i], originOrder[idxToPerm]);
                    }    
                    
                    var tmp = originOrder[i];
                    originOrder[i] = originOrder[idxToPerm];
                    originOrder[idxToPerm] = tmp;
                }
            }
        }

        public struct WithNamesT
        {
            public static WithNamesT WithNames; 
        }

        public static IEnumerable<IList<int>> SelectDistMatrixPermutations(double[,] distMatrix)
        {
            // bool CombinationsEqual(List<int> lhs, List<int> rhs)
            // {
            //     if (lhs == null)
            //     {
            //         return rhs == null;
            //     }
            //     else
            //     {
            //         if (rhs == null)
            //         {
            //             return false;
            //         }
            // 
            //         if (lhs.Count != rhs.Count)
            //         {
            //             return false;
            //         }
            // 
            //         var directElemEq = true;
            //         for (int i = 0; i < lhs.Count; ++i)
            //         {
            //             if (lhs[i] != rhs[i])
            //             {
            //                 directElemEq = false;
            //                 break;
            //             }
            //         }
            // 
            //         if (!directElemEq)
            //         {
            //             for (int i = 0; i < lhs.Count; ++i)
            //             {
            //                 if (lhs[i] != rhs[rhs.Count - i - 1])
            //                 {
            //                     return false;
            //                 }
            //             }
            // 
            //             return true;
            //         }
            //         else
            //         {
            //             return true;
            //         }
            //     }
            // }

            // var perms = new HashSet<List<int>>(new FuncEqualityComparer<List<int>>(CombinationsEqual, list => list.GetHashCode()));
            // var perm = Enumerable.Range(0, distMatrix.GetLength(0)).ToArray();
            // yield return perm.ToList();
            // 
            // while (NextPermutation(perm))
            // {
            //     yield return perm.ToList();
            // }

            foreach (var perm in new Combinatorics.Collections.Permutations<int>(Enumerable.Range(0, distMatrix.GetLength(0)).ToArray(),
                                                                                 Combinatorics.Collections.GenerateOption.WithoutRepetition))
            {
                yield return perm;
            }
        }

        public static IEnumerable<int[]> GetStartPermutationsForParallel(double[,] distMatrix)
        {
            var perm = Enumerable.Range(0, distMatrix.GetLength(0)).ToArray();
            yield return (int[]) perm.Clone();
            for (var i = 1; i < perm.Length; ++i)
            {
                Swap(ref perm[0], ref perm[i]);
                yield return (int[]) perm.Clone();
                Swap(ref perm[0], ref perm[i]);
            }
        }

        public static IEnumerable<(List<int> perm, (int, int)[,] names)> SelectDistMatrixPermutations(double[,] distMatrix,
                                                                                                      WithNamesT withNames)
        {
            var names = new (int, int)[distMatrix.GetLength(0), distMatrix.GetLength(1)];
            for (int i = 0; i < names.GetLength(0); ++i)
            {
                for (int j = 0; j < names.GetLength(1); ++j)
                {
                    names[i, j] = (i, j);
                }
            }

            // bool CombinationsEqual(List<int> lhs, List<int> rhs)
            // {
            //     if (lhs == null)
            //     {
            //         return rhs == null;
            //     }
            //     else
            //     {
            //         if (rhs == null)
            //         {
            //             return false;
            //         }
            // 
            //         if (lhs.Count != rhs.Count)
            //         {
            //             return false;
            //         }
            // 
            //         var directElemEq = true;
            //         for (int i = 0; i < lhs.Count; ++i)
            //         {
            //             if (lhs[i] != rhs[i])
            //             {
            //                 directElemEq = false;
            //                 break;
            //             }
            //         }
            // 
            //         if (!directElemEq)
            //         {
            //             for (int i = 0; i < lhs.Count; ++i)
            //             {
            //                 if (lhs[i] != rhs[rhs.Count - i - 1])
            //                 {
            //                     return false;
            //                 }
            //             }
            // 
            //             return true;
            //         }
            //         else
            //         {
            //             return true;
            //         }
            //     }
            // }

            // var perms = new HashSet<List<int>>(new FuncEqualityComparer<List<int>>(CombinationsEqual, list => list.GetHashCode()));
            foreach (var perm in new Combinatorics.Collections.Permutations<int>(Enumerable.Range(0, distMatrix.GetLength(0)).ToArray(),
                                                                                 Combinatorics.Collections.GenerateOption.WithoutRepetition))
            {
                var permList = perm.ToList();
                // if (perms.Add(permList))
                {
                    var permNames = ((int, int)[,])names.Clone();
                    yield return (permList, permNames);
                }
            }
        }

        public static T[,] RearrangeMatrix<T>(T[,] matrix, IList<int> order)
        {
            var res = (T[,]) new T[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                for (int j = i + 1; j < matrix.GetLength(1); ++j)
                {
                    res[j, i] = res[i, j] = matrix[order[i], order[j]];
                }
            }

            return res;
        }

        public static double GetTraceForDiagonalAboveMain(double[,] origDistMatrix, IList<int> order)
        {
            Contract.Requires(origDistMatrix.GetLength(0) == order.Count);

            double res = 0.0;
            for (var i = 0; i < origDistMatrix.GetLength(0) - 1; ++i)
            {
                res += origDistMatrix[order[i], order[i + 1]];
            }

            return res;
        }

        public static (double Distance, double SegmentAverageDistance, double TwoEndsDistance, double Difference, SegmentInfo MinSegment, SegmentInfo MaxSegment) GetBrokenLineStats(double[,] distMatrix, IList<int> brokenLine)
        {
            Contract.Requires(distMatrix.GetLength(0) == brokenLine.Count);

            var distance = 0.0;
            var minSegment = new SegmentInfo(double.MaxValue, -1, -1);
            var maxSegment = new SegmentInfo(double.MinValue, -1, -1);

            for (var i = 1; i < brokenLine.Count; ++i)
            {
                var segmentDist = distMatrix[brokenLine[i - 1], brokenLine[i]];

                distance += segmentDist;
                
                if (segmentDist < minSegment.Distance)
                {
                    minSegment = new SegmentInfo(segmentDist, brokenLine[i - 1], brokenLine[i]);
                }

                if (maxSegment.Distance < segmentDist)
                {
                    maxSegment = new SegmentInfo(segmentDist, brokenLine[i - 1], brokenLine[i]);
                }
            }

            var twoEndsDistance = GetDistBetweenTwoEnds(brokenLine, distMatrix);
            var averageDistance = distance / (brokenLine.Count - 1);
            var difference = Math.Abs(distance - twoEndsDistance);

            return (distance, averageDistance, twoEndsDistance, difference, minSegment, maxSegment);
        }

        /// <summary>
        /// Performs an in-place permutation of <paramref name="values"/>, and returns if there 
        /// are any more permutations remaining.
        /// </summary>
        public static bool NextPermutation<T>(ArraySegment<T> values) where T : IComparable<T>
        {
            int j = values.Count - 2;
            while (j >= 0 && values[j].CompareTo(values[j + 1]) >= 0)
            {
                --j;
            }
            
            if (j < 0)
            {
                return false;
            }

            int k = values.Count - 1;
            while (values[j].CompareTo(values[k]) >= 0)
            {
                --k;
            }

            Swap(ref values.Array[values.Offset + j], ref values.Array[values.Offset + k]);
            
            int l = j + 1;
            int r = values.Count - 1;
            while (l < r)
            {
                Swap(ref values.Array[values.Offset + l++], ref values.Array[values.Offset + r--]);
            }

            return true;
        }

        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
    }
}
