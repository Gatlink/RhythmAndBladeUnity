using Gamelogic.Extensions;
using UnityEngine;

[ CheckInspectorButtons ]
public class RailView : GLMonoBehaviour
{
    private const string ContainerName = "Rail View";
    public float Width = 0.3f;

    private GameObject _segment;
    private Transform _container;

    private GameObject Segment
    {
        get
        {
            if ( _segment == null )
            {
                _segment = Resources.Load<GameObject>( "Rail Segment" );
            }

            return _segment;
        }
    }

    private Transform Container
    {
        get
        {
            if ( _container == null )
            {
                _container = transform.Find( ContainerName );
                if ( _container == null )
                {
                    _container = new GameObject( ContainerName ).transform;
                    _container.SetParent( transform );
                }
            }

            return _container;
        }
    }

    private void Start()
    {
        if ( Container.childCount == 0 )
        {
            CreateView();
        }
    }

    private void CreateSegment( Vector3 from, Vector3 to )
    {
        var direction = to - from;
        var segment = Instantiate( Segment, Container ).transform;
        segment.position = from;
        segment.localScale = new Vector3( direction.magnitude, Width, 1 );
        segment.eulerAngles = new Vector3( 0, 0, Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg );
    }

    [ InspectorButton ]
    public void CreateView()
    {
        var rail = GetComponent<Rail>();
        foreach ( var segment in rail.EnumerateSegments() )
        {
            CreateSegment( segment.From, segment.To );
        }
    }

    [ InspectorButton ]
    public void ClearView()
    {
        Container.DestroyChildrenUniversal();
        DestroyUniversal( Container.gameObject );
        _container = null;
    }
}