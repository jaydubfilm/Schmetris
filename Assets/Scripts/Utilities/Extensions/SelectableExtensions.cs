using System.Collections.Generic;
using System.Linq;
using StarSalvager.Utilities.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.Extensions
{
    //TODO Add missing comments
    //TODO Consider the use of selection priorities in addition to the calculations below
    public static class SelectableExtensions
    {
        public static void FillNavigationOptions(this IEnumerable<Selectable> selectables)
        {
            selectables.FillNavigationOptions(null);
        }

        public static void FillNavigationOptions(this IEnumerable<Selectable> selectables,
            in NavigationRestriction[] navigationExceptions)
        {
            var array = selectables.ToArray();
            foreach (var selectable in array)
            {
                if (selectable == null)
                    continue;
                
                selectable.FillNavigationOptions(array, navigationExceptions, null);
            }
        }

        public static void FillNavigationOptions(this IEnumerable<Selectable> selectables,
            in IEnumerable<NavigationRestriction> navigationExceptions, in IEnumerable<NavigationOverride> navigationOverrides)
        {
            var array = selectables.ToArray();
            foreach (var selectable in array)
            {
                if (selectable == null)
                    continue;
                
                selectable.FillNavigationOptions(array, navigationExceptions?.ToArray(), navigationOverrides?.ToArray());
            }
        }

        public static void FillNavigationOptions(this Selectable selectable, in Selectable[] selectables,
            NavigationRestriction[] navigationExceptions, NavigationOverride[] navigationOverrides)
        {

            //--------------------------------------------------------------------------------------------------------//

            Selectable[] GetIgnoredSelectables(NavigationRestriction.DIRECTION navMode)
            {
                if (navigationExceptions.IsNullOrEmpty()) return new Selectable[0];

                return navigationExceptions
                    .Where(x => x.ContainsMode(navMode))
                    .Select(x => x.Selectable)
                    .ToArray();
            }

            void HasOverride(out NavigationOverride navigationOverride)
            {
                if (navigationOverrides.IsNullOrEmpty())
                {
                    navigationOverride = new NavigationOverride();
                    return;
                }

                navigationOverride = navigationOverrides.FirstOrDefault(x => x.FromSelectable == selectable);
            }

            //--------------------------------------------------------------------------------------------------------//
            var rotation = selectable.transform.rotation;

            HasOverride(out var overrides);

            //Using the overrides found (if any) assign the target to either the override, or find the best target
            var selectOnUp = overrides.UpTarget
                ? overrides.UpTarget
                : selectable.FindSelectable(selectables, rotation * Vector3.up,
                    GetIgnoredSelectables(NavigationRestriction.DIRECTION.UP));

            var selectOnDown = overrides.DownTarget
                ? overrides.DownTarget
                : selectable.FindSelectable(selectables, rotation * Vector3.down,
                    GetIgnoredSelectables(NavigationRestriction.DIRECTION.DOWN));

            var selectOnLeft = overrides.LeftTarget
                ? overrides.LeftTarget
                : selectable.FindSelectable(selectables, rotation * Vector3.left,
                    GetIgnoredSelectables(NavigationRestriction.DIRECTION.LEFT));

            var selectOnRight = overrides.RightTarget
                ? overrides.RightTarget
                : selectable.FindSelectable(selectables, rotation * Vector3.right,
                    GetIgnoredSelectables(NavigationRestriction.DIRECTION.RIGHT));

            //--------------------------------------------------------------------------------------------------------//

            selectable.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = selectOnUp,
                selectOnDown = selectOnDown,
                selectOnLeft = selectOnLeft,
                selectOnRight = selectOnRight,

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

        //A functionality override for https://docs.unity3d.com/2019.1/Documentation/ScriptReference/UI.Selectable.FindSelectable.html
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

                if (sel == null) continue;
                if (sel == selectable) continue;

                if (sel.enabled == false) continue;
                if (sel.gameObject.activeInHierarchy == false) continue;

                if (toIgnore.Contains(sel)) continue;




                if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None) continue;

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
                Vector3 selCenter = selRect != null ? (Vector3) selRect.rect.center : Vector3.zero;
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
