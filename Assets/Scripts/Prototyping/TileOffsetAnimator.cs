using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Prototype
{
    public class TileOffsetAnimator : MonoBehaviour
    {
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int MaskTexture = Shader.PropertyToID("_Mask");
        private static readonly int MainColor = Shader.PropertyToID("_Color");

        //TileOffsetAnimator Properties
        //====================================================================================================================//
        
        [SerializeField, Required] private MeshRenderer meshRenderer;

        [SerializeField, Required, BoxGroup("Material Setup"), DisableInPlayMode]
        private Material templateMaterial;

        [SerializeField, BoxGroup("Material Setup"), DisableInPlayMode]
        private Texture2D mainTexture;

        [SerializeField, BoxGroup("Material Setup"), DisableInPlayMode]
        private Texture2D maskTexture;

        [SerializeField, BoxGroup("Material Setup"), DisableInPlayMode]
        private Vector2 startTiling = Vector2.one;

        //====================================================================================================================//
        

        [SerializeField, ToggleGroup("useColor", "Color")]
        private bool useColor;

        [SerializeField, Range(0.01f, 5f), ToggleGroup("useColor")]
        private float speed = 1f;

        [SerializeField, ToggleGroup("useColor")]
        private AnimationCurve colorCurve;

        [SerializeField, ToggleGroup("useColor")]
        private Color startColor = Color.white;

        [SerializeField, ToggleGroup("useColor")]
        private Color endColor = Color.white;

        private float colorTime;

        //====================================================================================================================//
        

        [SerializeField, ToggleGroup("useOffset", "Tile Offset")]
        private bool useOffset;
        
        [SerializeField, Range(0f, 10f), ToggleGroup("useOffset")]
        private float offsetSpeed;

        [SerializeField, ToggleGroup("useOffset")]
        private Vector2 offsetDirection;

        //Unity Functions
        //====================================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            meshRenderer.material = CreateMaterialInstance();
        }

        // Update is called once per frame
        private void Update()
        {
            if(useOffset)
                MoveOffset();
            
            if (useColor)
                ChangeColor();
        }

        //TileOffsetAnimator Functions
        //====================================================================================================================//

        private Material CreateMaterialInstance()
        {
            var material = new Material(templateMaterial)
            {
                mainTexture = mainTexture,
                mainTextureScale = startTiling
            };
            material.SetTexture(MaskTexture, maskTexture);

            return material;
        }

        private void MoveOffset()
        {
            meshRenderer.material.mainTextureOffset += offsetDirection * (offsetSpeed * Time.deltaTime);
        }

        private void ChangeColor()
        {
            colorTime += Time.deltaTime * speed;
            var color = Color.Lerp(startColor, endColor, colorCurve.Evaluate(colorTime));
            meshRenderer.material.SetColor(MainColor, color);
        }

        //====================================================================================================================//
        
    }
}
