using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool CompareTags(this GameObject gameObject, IEnumerable<string> tags)
        {
            return tags.Any(gameObject.CompareTag);
        }
        public static T FindObjectOfTypeInScene<T>(this GameObject gameObject, bool recursive = false) where T: MonoBehaviour
        {
            var toSearch = gameObject.scene.GetRootGameObjects();
            
            if (recursive)
            {
                foreach (var o in toSearch)
                {
                    if (o.GetComponent<T>() is T temp)
                        return temp;

                    if (o.GetComponentInChildren<T>() is T temp2)
                        return temp2;
                }
            }
            
            foreach (var o in toSearch)
            {
                if (o.GetComponent<T>() is T temp)
                    return temp;
            }

            return null;
        }
    }

}