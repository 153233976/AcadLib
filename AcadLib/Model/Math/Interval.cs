﻿using System;
using JetBrains.Annotations;

namespace AcadLib
{
    /// <summary>
    /// Интервал
    /// </summary>
    /// <typeparam name="T">Тип значения интервала</typeparam>
    [PublicAPI]
    public class Interval<T> where T : struct, IComparable
    {
        public T? Start { get; set; }
        public T? End { get; set; }

        public Interval(T? start, T? end)
        {
            Start = start;
            End = end;
        }

        public bool InRange(T value)
        {
            return (!Start.HasValue || value.CompareTo(Start.Value) > 0) &&
                   (!End.HasValue || End.Value.CompareTo(value) > 0);
        }
    }
}
