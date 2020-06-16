using StarSalvager.Factories;
using UnityEngine;

namespace StarSalvager.Prototype
{
    public class ShapeGenerator : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CreateShape();
            }
        
        }
        
        
        private GameObject temp;
        private void CreateShape()
        {
            if(temp != null)
                DestroyImmediate(temp);
        
            var newShape = FactoryManager.Instance
                .GetFactory<ShapeFactory>()
                .CreateObject<Shape>(
                    (BIT_TYPE) Random.Range(0, 7),
                    Random.Range(1, 10));

            temp = newShape.gameObject;
        }

    }
}

