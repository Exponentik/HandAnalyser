using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandAnalyzer
{
    internal class EdgeOfBorder
    {

        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        public double k { get; set; }
        public double b { get; set; }


        public Point start;
        public Point end;
        int minX = 10000;
        int maxX = 0;
        int minY = 10000;
        int maxY = 0;

        public EdgeOfBorder() { }
        public EdgeOfBorder(Point start, Point end) {
            double x1 = start.X;
            double x2 = end.X;
            double y1 = start.Y;
            double y2 = end.Y;


            this.start = start;
            this.end = end;
            if (start.X < end.X)
            {
                minX = start.X;
                maxX = end.X;
            }
            else
            {
                minX = end.X;
                maxX = start.X;
            }

            if (start.Y < end.Y)
            {
                minY = start.Y;
                maxY = end.Y;
            }
            else
            {
                minY = end.Y;
                maxY = start.Y;
            }

            A = end.Y - start.Y;
            B = start.X - end.X;
            C = (start.X * end.Y) - (end.X * start.Y);

            b = (x2 * y1 - x1 * y2) / (x2 - x1);
            k = (y1 - b) / x1;
        }

        public double Evaluate(Point point)
        {
            return (A * point.X + B * point.Y + C);
        }

        public Point perm(Point startT, Point endT)
        {
            var E = 0.01;
            if(!(end.X - start.X != 0 && endT.X - startT.X!=0))
                return new Point(-1, -1);
            var m1 = (double) (end.Y - start.Y) / (end.X - start.X);
            var m2 = (double) (endT.Y - startT.Y) / (endT.X - startT.X);
            var b1 = end.Y - m1 * end.X;
            var b2 = endT.Y - m2 * endT.X;
            if (Math.Abs(m1 - m2) < E || (Math.Abs(m1 - m2) < E && Math.Abs(b1 - b2) < E))
                return new Point(-1, -1);
            var x = (double)(b2 - b1) / (m1 - m2);
            var y = (int) (m1* x+b1);
            x = (int) x;
            //var minXs = 1000;
            //var maxXs = 0;
            //if(startT.X < endT.X)
            //{
            //    minXs = startT.X;
            //    maxXs = endT.X;
            //}
            //else
            //{
            //    minXs = endT.X;
            //    maxXs = startT.X;
            //}

            if ((minX<=x && x<=maxX) && (minY<=y && y<=maxY))
                return new Point((int)x, y);
            else
                return new Point(-1, -1);
        }
    }
}
