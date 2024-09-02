using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandAnalyzer
{
    internal class Border
    {
        List<EdgeOfBorder> edges = new List<EdgeOfBorder>();

        public bool IsPointInBorder(Point point)
        {
            bool inside = true;

            for (int i = 0; i < edges.Count; i++)
            {
                double value = edges[i].Evaluate(point);
                if (value < 0)
                {
                    inside = false;
                    break;
                }
            }

            return inside;
        }

        public Point[] getAnglePoints()
        {
            List<Point> result = new List<Point>();
            if (edges.Count > 1)
            {
                List<PointR> sortArray = new List<PointR>();
                int x = 0, y = 0;
                for (int i = 0; i < edges.Count; i++)
                {
                    x += edges[i].end.X;
                    y += edges[i].end.Y;

                    x += edges[i].start.X;
                    y += edges[i].start.Y;
                }

                Point center = new Point(x / (edges.Count*2), y / (edges.Count * 2));

                for (int i = 0; i < edges.Count; i++)
                {
                    sortArray.Add(new PointR(edges[i].end, center));
                    sortArray.Add(new PointR(edges[i].start, center));
                }

                sortArray.Sort();
                for (int i = 0; i < sortArray.Count; i++)
                {
                    result.Add(sortArray[i].point);
                }
                //result[result.Length - 1] = result[0];
            }
                return result.ToArray();
        }

        static bool AreOtherPointsOnSameSide(Point start, Point end, List<Point> points)
        {
            int x1 = start.X;
            int y1 = start.Y;
            int x2 = end.X;
            int y2 = end.Y;

            int side = 0; // Сторона, на которой лежат остальные точки: 1 - слева, -1 - справа, 0 - на линии

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] != start && points[i] != end)
                {
                    int x0 = points[i].X;
                    int y0 = points[i].Y;

                    // Вычисляем знак векторного произведения (x2 - x1) * (y0 - y1) - (x0 - x1) * (y2 - y1)
                    int crossProduct = (x2 - x1) * (y0 - y1) - (x0 - x1) * (y2 - y1);

                    if (side == 0)
                    {
                        side = Math.Sign(crossProduct);
                    }
                    else if (Math.Sign(crossProduct) != side)
                    {
                        return false; // Остальные точки лежат по разные стороны от отрезка
                    }
                }
            }

            return true; // Остальные точки лежат с одной стороны от отрезка
        }
        public Border(List<Point> points) {
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    Point startPoint = points[i];
                    Point endPoint = points[j];

                    // Проверяем, что остальные две точки лежат с одной стороны от отрезка
                    if (AreOtherPointsOnSameSide(startPoint, endPoint, points))
                    {
                        edges.Add(new EdgeOfBorder(startPoint, endPoint));
                    }
                }
            }
            
        }

        public bool isPointInsideBorder(Point point)
        {
            EdgeOfBorder rightEdge = new EdgeOfBorder();
            EdgeOfBorder leftEdge = new EdgeOfBorder();
            EdgeOfBorder topEdge = new EdgeOfBorder();
            EdgeOfBorder bottomEdge = new EdgeOfBorder();

            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].k < 0)
                {
                    for(int j =0; j<edges.Count; j++)
                    {
                        if (j != i)
                        {
                            if (edges[j].k < 0)
                            {
                                if(edges[j].b > edges[i].b)
                                {
                                    rightEdge = edges[j];
                                    leftEdge = edges[i];
                                }
                                else
                                {
                                    leftEdge = edges[j];
                                    rightEdge = edges[i];

                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < edges.Count; j++)
                    {
                        if (j != i)
                        {
                            if (edges[j].k > 0)
                            {
                                if (edges[j].b > edges[i].b)
                                {
                                    topEdge = edges[j];
                                    bottomEdge = edges[i];
                                }
                                else
                                {
                                    bottomEdge = edges[j];
                                    topEdge = edges[i];
                                }
                            }
                        }
                    }
                }
            }
            bool first = point.Y <= topEdge.k * point.X + topEdge.b;
            bool second = point.Y >= leftEdge.k * point.X + leftEdge.b;
            bool third = point.Y <= rightEdge.k * point.X + rightEdge.b;
            bool fourth = point.Y >= bottomEdge.k * point.X + bottomEdge.b;
            return first && second && third && fourth;
        }

        public Point getNear(Point point)
        {
            double min = 100000;
            Point res = new Point();
            for(int i = 0; i < edges.Count; i++)
            {
                var c1 = Math.Sqrt(Math.Pow(edges[i].start.X - point.X,2) + Math.Pow(edges[i].start.Y - point.Y, 2));
                if (min >= c1)
                {
                    res = edges[i].start;
                    min = c1;
                }

                var c2 = Math.Sqrt(Math.Pow(edges[i].end.X - point.X, 2) + Math.Pow(edges[i].end.Y - point.Y, 2));
                if (min >= c2)
                {
                    res = edges[i].end;
                    min = c2;
                }
            }
            return res;
        }

        public Point[] perm(Point start, Point end)
        {
            Point[] res = new Point[2];

            List<Point> tmp = new List<Point>();
            for (int i = 0; i < edges.Count; i++)
            {
                var c = edges[i].perm(start, end);
                if(!(c.X == -1 && c.Y == -1))
                {
                    tmp.Add(c);
                }
            }

            if(tmp.Count > 1) {
                var minX = tmp[0].X < tmp[1].X ? tmp[0].X : tmp[1].X;
                var maxX = tmp[0].X > tmp[1].X ? tmp[0].X : tmp[1].X;
                var minY = tmp[0].Y < tmp[1].Y ? tmp[0].Y : tmp[1].Y;
                var maxY = tmp[0].Y > tmp[1].Y ? tmp[0].Y : tmp[1].Y;
                if(!(start.X>minX && start.X < maxX && start.Y >minY && start.Y < maxY))
                {
                    res[0] = Math.Abs(tmp[0].X - start.X) < Math.Abs(tmp[1].X - start.X) ? tmp[0] : tmp[1];
                }
                else
                {
                    res[0] = start;
                }

                if (!(end.X > minX && end.X < maxX && end.Y > minY && end.Y < maxY))
                {
                    res[1] = Math.Abs(tmp[0].X - end.X) < Math.Abs(tmp[1].X - end.X) ? tmp[0] : tmp[1];
                }
                else
                {
                    res[1] = end;
                }
            }
            else
            {
                res[0] = start;
                res[1] = end;
                return res;
            }

            return res;
        }

        public List<EdgeOfBorder> GetEdges()
        {
            return edges;
        }
    }
}
