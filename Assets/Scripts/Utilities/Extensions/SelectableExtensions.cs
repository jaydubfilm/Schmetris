using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.Extensions
{
    public interface ICustomNavigation
    {
        Selectable[] currentSelectables { get; }
        void SetupNavigation(Selectable[] selectables, SelectableExtensions.NavigationException[] navigationExceptions);
    }
    
    public static class SelectableExtensions
    {
        public struct NavigationException
        {
            public DIRECTION Direction;
            public Selectable Selectable;
        }

        public static void FillNavigationOptions(this IEnumerable<Selectable> selectables)
        {
            selectables.FillNavigationOptions(null);
        }
        public static void FillNavigationOptions(this IEnumerable<Selectable> selectables, in NavigationException[] navigationExceptions)
        {
            var array = selectables.ToArray();
            foreach (var selectable in array)
            {
                selectable.FillNavigationOptions(array, navigationExceptions);
            }
        }

        public static void FillNavigationOptions(this Selectable selectable, in Selectable[] selectables, NavigationException[] navigationExceptions)
        {
            Selectable[] GetTrimmedList(DIRECTION direction)
            {
                if (navigationExceptions.IsNullOrEmpty()) 
                    return new Selectable[0];
                
                return navigationExceptions.Where(
                        x => x.Direction == direction)
                    .Select(x => x.Selectable)
                    .ToArray();
            }
            
            var rotation = selectable.transform.rotation;
            selectable.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = selectable.FindSelectable(selectables, rotation * Vector3.up, GetTrimmedList(DIRECTION.UP)),
                selectOnDown = selectable.FindSelectable(selectables, rotation * Vector3.down, GetTrimmedList(DIRECTION.DOWN)),
                selectOnLeft = selectable.FindSelectable(selectables, rotation * Vector3.left, GetTrimmedList(DIRECTION.LEFT)),
                selectOnRight = selectable.FindSelectable(selectables, rotation * Vector3.right, GetTrimmedList(DIRECTION.RIGHT)),

            };
        }

        public static void CleanNavigationOptions(this IEnumerable<Selectable> selectables)
        {
            var array = selectables.ToArray();
            foreach (var selectable in array)
            {
                selectable.CleanNavigationOptions();
            }
        }
        public static void CleanNavigationOptions(this Selectable selectable)
        {
            selectable.navigation = new Navigation
            {
                mode = Navigation.Mode.None,
                selectOnUp = null,
                selectOnDown = null,
                selectOnLeft = null,
                selectOnRight = null,

            };
        }

        public static Selectable FindSelectable(this Selectable selectable, 
            in Selectable[] selectables,
            Vector3 dir,
            in Selectable[] toIgnore)
        {

            //--------------------------------------------------------------------------------------------------------//

            Vector3 GetPointOnRectEdge(RectTransform rectTransform, Vector2 direction)
            {
                if (rectTransform == null)
                    return Vector3.zero;
                
                var rect = rectTransform.rect;

                
                if (direction != Vector2.zero)
                    direction /= Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(dir.y));
                direction = rect.center + Vector2.Scale(rect.size, direction * 0.5f);
                return direction;
            }

            //--------------------------------------------------------------------------------------------------------//

            var transform = selectable.transform;
            var count = selectables.Length;

            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            Selectable bestPick = null;

            for (int i = 0; i < count; ++i)
            {
                Selectable sel = selectables[i];

                if (toIgnore.Contains(sel)) continue;

                if (sel == selectable) continue;

                if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                    continue;

#if UNITY_EDITOR
                // Apart from runtime use, FindSelectable is used by custom editors to
                // draw arrows between different selectables. For scene view cameras,
                // only selectables in the same stage should be considered.
                if (Camera.current != null &&
                    !StageUtility.IsGameObjectRenderedByCamera(sel.gameObject,
                        Camera.current))
                    continue;
#endif

                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                float score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }

            return bestPick;
        }
    }
}
