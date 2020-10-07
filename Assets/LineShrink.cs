using Recycling;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineShrink : MonoBehaviour, IRecycled, ICustomRecycle
{
    public bool IsRecycled { get; set; }

    
    [SerializeField]
    private new LineRenderer renderer;
    
    [SerializeField]
    private AnimationCurve shrinkCurve;

    [SerializeField]
    private float shrinkTime;

    private float _startWidth;
    private float _t;
    private bool _ready;

    //Unity Functions
    //====================================================================================================================//
    
    private void Update()
    {
        if (!_ready)
            return;

        if (_t >= shrinkTime)
        {
            _ready = false;
            Recycler.Recycle<LineShrink>(this);
            return;
        }

        renderer.widthMultiplier = _startWidth * shrinkCurve.Evaluate(_t / shrinkTime);

        _t += Time.deltaTime;
    }

    //LineShrink Functions
    //====================================================================================================================//
    
    public void Init(Vector3 startPosition, Vector3 endPosition, float startWidth = 0.5f)
    {
        _startWidth = startWidth;
        renderer.widthMultiplier = _startWidth;
        renderer.SetPositions(new []
        {
            startPosition,
            endPosition
        });
        
        _ready = true;
    }

    //ICustomRotate Functions
    //====================================================================================================================//
    
    public void CustomRecycle(params object[] args)
    {
        _ready = false;
        _t = 0f;
        renderer.SetPositions(new []
        {
            Vector3.zero,
            Vector3.zero
        });
    }
}
