using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandAnalyzer
{
    internal class CalculatingEdge 
    {
        public Point firstPoint;
        public Point secondPoint;
        
        public CalculatingEdge(Point first, Point second)
        {
            this.firstPoint = first;
            this.secondPoint = second;
        }

        public CalculatingEdge() { }

    }
}
