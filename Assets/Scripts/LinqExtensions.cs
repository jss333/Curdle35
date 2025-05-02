using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public static class LinqExtensions
{
    public static (List<T> Matches, List<T> NonMatches) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var matches = new List<T>();
        var nonMatches = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                matches.Add(item);
            else
                nonMatches.Add(item);
        }

        return (matches, nonMatches);
    }

    public static IEnumerable<TComponent> ChildrenWithComponent<TComponent>(this Transform parent, bool activeOnly) where TComponent : Component
    {
        return parent.Cast<Transform>()
            .Where(child => child.gameObject.activeInHierarchy)
            .Select(child => child.GetComponent<TComponent>())
            .Where(component => component != null && (!activeOnly || component.gameObject.activeInHierarchy))
            .ToList();
    }

    public static T TakeRandom<T>(this IEnumerable<T> source, System.Random random = null)
    {
        if (source == null || !source.Any())
            throw new ArgumentException("Source collection is null or empty.");

        return source.ElementAt(UnityEngine.Random.Range(0, source.Count())); 
    }

    public static IEnumerable<TSource> WhereMin<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : IComparable<TKey>
    {
        var minValue = source.Min(selector);
        return source.Where(item => selector(item).CompareTo(minValue) == 0);
    }
}
