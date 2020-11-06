using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Prototype
{
    public class ScaleColorSpriteAnimation : MonoBehaviour
    {
        
        [Serializable]
        public class Data
        {
            [Required, FoldoutGroup("$GetName")]
            public SpriteRenderer spriteRenderer;

            [FoldoutGroup("$GetName"), Range(1f,100f)] 
            public float speed = 1f;

            [ToggleGroup("$GetName/useColor", "Color", CollapseOthersOnExpand = false)]
            public bool useColor;
            [ToggleGroup("$GetName/useColor")]
            public AnimationCurve colorCurve;
            [ToggleGroup("$GetName/useColor")]
            public Color startColor = Color.white;
            [ToggleGroup("$GetName/useColor")]
            public Color endColor = Color.white;

            
            [ToggleGroup("$GetName/useScale", "Scale", CollapseOthersOnExpand = false)]
            public bool useScale;
            [ToggleGroup("$GetName/useScale")]
            public AnimationCurve scaleCurve;
            [ToggleGroup("$GetName/useScale")]
            public Vector2 startScale = Vector2.one;
            [ToggleGroup("$GetName/useScale")]
            public Vector2 endScale = Vector2.one;
            

#if UNITY_EDITOR

            private string GetName()
            {
                return spriteRenderer ? spriteRenderer.gameObject.name : string.Empty;
            }
            
#endif

        }

        //====================================================================================================================//
        
        public float AnimationTime => animationTime;
        
        [SerializeField, Range(0.01f, 30f)]
        private float animationTime;

        [SerializeField] private bool looping;

        [SerializeField, ToggleGroup("useGlobalScale", "Animate Global Scale")]
        private bool useGlobalScale;
        [SerializeField,ToggleGroup("useGlobalScale"), Range(1f,100f)]
        private float globalScaleSpeed = 1f;
        [SerializeField,ToggleGroup("useGlobalScale")]
        private AnimationCurve globalScaleCurve;
        [SerializeField,ToggleGroup("useGlobalScale")]
        private Vector2 globalScaleStart;
        [SerializeField,ToggleGroup("useGlobalScale")]
        private Vector2 globalScaleEnd;
        

        [SerializeField, DisableInPlayMode]
        private Data[] effectors;

        //====================================================================================================================//

        private Coroutine _coroutine;
        
        private void Start()
        {
            if(effectors.IsNullOrEmpty())
                throw new Exception("No animation information setup");


            TestPlay();
        }


        //====================================================================================================================//

        [Button, PropertyOrder(-1000), DisableInEditorMode]
        private void TestPlay()
        {
            if(_coroutine != null)
                StopCoroutine(_coroutine);
            
            _coroutine = StartCoroutine(AnimateCoroutine());
        }


        

        private IEnumerator AnimateCoroutine()
        {
            do
            {
                if (useGlobalScale)
                {
                    transform.localScale = globalScaleStart;
                }
                
                
                //Setting up the effectors
                foreach (var effector in effectors)
                {
                    if (!effector.useColor && !effector.useScale)
                        continue;

                    if (effector.useColor)
                        effector.spriteRenderer.color = effector.startColor;

                    if (effector.useScale)
                        effector.spriteRenderer.transform.localScale = effector.startScale;
                }

                //Begin the animation
                float t = 0f;
                while (t / animationTime < 1f)
                {
                    var td = t / animationTime;

                    
                    
                    if (useGlobalScale)
                    {
                        transform.localScale =
                            Vector2.Lerp(globalScaleStart, globalScaleEnd, globalScaleCurve.Evaluate(td * globalScaleSpeed));
                    }
                    

                    foreach (var effector in effectors)
                    {
                        if (!effector.useColor && !effector.useScale)
                            continue;

                        if (effector.useColor)
                        {
                            effector.spriteRenderer.color = Color.Lerp(effector.startColor, effector.endColor,
                                effector.colorCurve.Evaluate(td * effector.speed));
                        }

                        if (effector.useScale)
                        {
                            var trans = effector.spriteRenderer.transform;

                            trans.localScale = Vector2.Lerp(effector.startScale, effector.endScale,
                                effector.scaleCurve.Evaluate(td * effector.speed));
                        }

                    }

                    t += Time.deltaTime;
                    yield return null;
                }
                
            } while (looping);
            
            _coroutine = null;
        }

#if UNITY_EDITOR
        [Button, PropertyOrder(-900), DisableInPlayMode, InfoBox("Fill Renderers will create a new list, clearing data. Use with caution", InfoMessageType.Warning)]
        private void FillRenderers()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();

            if (renderers.IsNullOrEmpty())
            {
                Debug.LogError("No sprite renderers found in children");
                return;
            }
            
            effectors = new Data[renderers.Length];
            
            
            for (var i = 0; i < renderers.Length; i++)
            {
                effectors[i] = new Data
                {
                    spriteRenderer = renderers[i]
                };
            }
        }
#endif

    }
}
