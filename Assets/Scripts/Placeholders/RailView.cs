using UnityEngine;

public class RailView : MonoBehaviour
{
    public float Width = 0.3f;

    private GameObject _segment;
    private Transform _container;

    private void Awake()
    {
        _segment = Resources.Load<GameObject>( "Rail Segment" );
        _container = new GameObject( "Rail Segment" ).transform;
        _container.SetParent( transform );
    }

    private void Start()
    {
        var rail = GetComponent<Rail>();
        foreach ( var segment in rail.EnumerateSegments() )
        {
            CreateSegment( segment.From, segment.To );
        }
    }

    private void CreateSegment( Vector3 from, Vector3 to )
    {
        var direction = to - from;
        var segment = Instantiate( _segment, _container ).transform;
        segment.position = from;
        segment.localScale = new Vector3( direction.magnitude, Width, 1 );
        segment.eulerAngles = new Vector3( 0, 0, Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg );
    }
}