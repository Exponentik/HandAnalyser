using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandAnalyzer
{
    internal class PointR : IComparable<PointR>
    {
        public double angle;
        public Point point;

        public PointR(Point point, Point center)
        {
            this.point = point;
            angle = Math.Atan2(point.Y- center.Y, point.X - center.X);
        }

        public int CompareTo(PointR other)
        {
            if(other == null) return -1;

            if (other != null)
                return this.angle.CompareTo(other.angle);
            else
                throw new ArgumentException("Object is not a PointR");
        }
    }
}
