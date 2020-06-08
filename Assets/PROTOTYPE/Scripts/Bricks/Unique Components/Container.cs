using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]//Generic resource container for bot
    public class Container : MonoBehaviour
    {
        //Total resource capacity
        public float[] capacity;

        //Directional container
        public bool canCollect = true;
        public GameObject directionPrefab;
        public float startDirection = 0;
        float openDirection = 0;
        GameObject directionIcon;

        //Which direction does this container gather resources from?
        public float GetOpenDirection()
        {
            return openDirection;
        }

        //Init
        private void Awake()
        {
            GameController.Instance.bot.AddContainer(this);
            if (canCollect)
            {
                directionIcon = Instantiate(directionPrefab, transform);
                directionIcon.transform.localPosition = Vector3.zero;
                SetOpenDirection(startDirection, false);
            }
        }

        //Adjust open direction when bot rotates
        public void SetOpenDirection(float newDirection, bool overrideStart)
        {
            if (!canCollect)
                return;
            while (newDirection >= 360)
            {
                newDirection -= 360;
            }

            while (newDirection < 0)
            {
                newDirection += 360;
            }

            openDirection = newDirection;
            if (overrideStart)
                startDirection = openDirection;
            directionIcon.transform.localEulerAngles = new Vector3(0, 0, openDirection);
        }

        //Check if incoming resource is hitting the open side of the container
        public bool IsOpenDirection(Vector2Int hitDir)
        {
            if (!canCollect)
                return false;

            switch (openDirection)
            {
                case 0:
                    return hitDir.y < 0;
                case 90:
                    return hitDir.x > 0;
                case 180:
                    return hitDir.y > 0;
                case 270:
                    return hitDir.x < 0;
            }

            return false;
        }
    }
}