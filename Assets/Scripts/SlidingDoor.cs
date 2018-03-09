using Gamelogic.Extensions;
using UnityEngine;

public class SlidingDoor : GLMonoBehaviour
{
    public bool StartsOpen = true;
    
    public Vector3 TransitionOffset;

    public float TransitionDuration;

    public Easing TransitionEasing;

    public bool IsOpen
    {
        get { return !enabled; }
        set { enabled = !value; }
    }
    
    private Vector3 _open;
    private Vector3 _closed;

    private Coroutine _transition;

    private float _refLength;
    
    private void Awake()
    {
        _closed = transform.position;
        _open = _closed + TransitionOffset;       
        _refLength = TransitionOffset.magnitude;

        if ( StartsOpen )
        {
            transform.position = _open;
        }
        
        IsOpen = StartsOpen;
    }

    private void OnEnable()
    {
        Transition( _closed );
    }

    private void OnDisable()
    {
        Transition( _open );
    }

    private void Transition( Vector3 to )
    {
        if ( !gameObject.activeInHierarchy ) return;
        
        if ( _transition != null )
        {
            StopCoroutine( _transition );
        }

        var from = transform.position;
        var duration = Vector3.Distance( from, to ) / _refLength * TransitionDuration;
        
        _transition = Tween( 0f, 1f, duration, TransitionEasing, 
            v => transform.position = Vector3.Lerp( from, to, v ) );
    }
}