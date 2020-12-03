using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    public class InvertedMaskUI : Image
    {
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

        public override Material materialForRendering => GetMaterial();
        private Material _overrideMaterial;


        private Material GetMaterial()
        {
            if (_overrideMaterial != null)
                return _overrideMaterial;

            var newMat = new Material(base.materialForRendering);
            newMat.SetInt(StencilComp, (int) CompareFunction.NotEqual);

            _overrideMaterial = newMat;

            return _overrideMaterial;
        }
    }
}
