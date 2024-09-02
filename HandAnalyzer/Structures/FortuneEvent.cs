using System;

namespace HandAnalyzer.Structures
{
    interface FortuneEvent : IComparable<FortuneEvent>
    {
        double X { get; }
        double Y { get; }
    }
}
//    interface FortuneEvent : IComparable<FortuneEvent>
//    {
//        double X { get; }
//        double Y { get; }
//    }
//}
