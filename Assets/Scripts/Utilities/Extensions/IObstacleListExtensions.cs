using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class IObstacleListExtensions
    {
        public static T FindClosestObstacle<T>(this List<T> obstacles, Vector2 worldPosition) where T: IObstacle
        {
            if (obstacles.IsNullOrEmpty())
                return default;
            
            int index = -1;
            float closestDistance = 999f;
            
            
            for (var i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] is IRecycled recycled && recycled.IsRecycled)
                    continue;
                
                var dist = Vector2.Distance(worldPosition, obstacles[i].transform.position);

                if (dist >= closestDistance)
                    continue;

                closestDistance = dist;
                index = i;
            }

            return obstacles[index];
        }
        
        public static T FindClosestObstacleInRange<T>(this List<T> obstacles, Vector2 worldPosition, float range) where T: IObstacle
        {
            if (obstacles.IsNullOrEmpty())
                return default;
            
            int index = -1;
            float closestDistance = 999f;
            
            
            for (var i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] is IRecycled recycled && recycled.IsRecycled)
                    continue;
                
                var dist = Vector2.Distance(worldPosition, obstacles[i].transform.position);

                if (dist > range)
                    continue;

                if (dist >= closestDistance)
                    continue;

                closestDistance = dist;
                index = i;
            }

            return index < 0 ? default : obstacles[index];
        }


    }
    
    public static class AsteroidListExtensions
    {
        public static Asteroid FindClosestObstacle(this List<Asteroid> asteroids, Vector2 worldPosition)
        {
            if (asteroids.IsNullOrEmpty())
                return default;
            
            int index = -1;
            float closestDistance = 999f;
            
            
            for (var i = 0; i < asteroids.Count; i++)
            {
                var asteroid = asteroids[i];
                
                if (asteroid.IsRecycled)
                    continue;
                
                var dist = Vector2.Distance(worldPosition, asteroid.transform.position) - asteroid.Radius;

                if (dist >= closestDistance)
                    continue;

                closestDistance = dist;
                index = i;
            }

            return asteroids[index];
        }
        
        public static Asteroid FindClosestObstacleInRange(this List<Asteroid> asteroids, Vector2 worldPosition, float range)
        {
            if (asteroids.IsNullOrEmpty())
                return default;
            
            int index = -1;
            float closestDistance = 999f;
            
            
            for (var i = 0; i < asteroids.Count; i++)
            {
                var asteroid = asteroids[i];
                if (asteroid.IsRecycled)
                    continue;
                
                var dist = Vector2.Distance(worldPosition, asteroid.transform.position) - asteroid.Radius;

                if (dist > range)
                    continue;

                if (dist >= closestDistance)
                    continue;

                closestDistance = dist;
                index = i;
            }

            return index < 0 ? default : asteroids[index];
        }


    }
    
    public static class ListExtensions
    {
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count <= 0;
        }
        public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            return dictionary == null || dictionary.Count <= 0;
        }
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length <= 0;
        }
        
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> iEnumerable)
        {
            return iEnumerable == null || !iEnumerable.Any();
        }
    }

    public static class DictionaryExtensions
    {
        public static void AddRange<T1, T2>(this Dictionary<T1, T2> dictionary, IDictionary<T1, T2> collection)
        {
            foreach (var kvp in collection)
            {
                dictionary.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
