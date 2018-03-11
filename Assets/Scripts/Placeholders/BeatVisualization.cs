using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class BeatVisualization : GLMonoBehaviour
{
    public float MinRadius;
    public float MaxRadius;

    [ Header( "Timings" ) ]
    public float PreviewTime = 1f;

    public float Adjustment;

    [ Range( 0, 1 ) ]
    [ Header( "On set effect" ) ]
    public float FinalEffectSize = 0.2f;

    public float FinalEffectDuration = 0.2f;
    public Easing FinalEffectMovementEasing = new Easing();
    public Easing FinalEffectFadeEasing = new Easing();

    private Material _circle;
    private float _maxCircleAlpha;

    private Dictionary<BeatManager.BeatAction, Sprite> _beatActionSprites;
    private Image _buttonImage;
    private CanvasGroup _beatActionGroup;

    private void Awake()
    {
        _beatActionSprites = new Dictionary<BeatManager.BeatAction, Sprite>()
        {
            { BeatManager.BeatAction.Up, Resources.Load<Sprite>( "Up" ) },
            { BeatManager.BeatAction.Down, Resources.Load<Sprite>( "Down" ) },
            { BeatManager.BeatAction.Left, Resources.Load<Sprite>( "Left" ) },
            { BeatManager.BeatAction.Right, Resources.Load<Sprite>( "Right" ) }
        };
    }

    private void Start()
    {
        var image = transform.Find( "Circle" ).GetComponentInChildrenAlways<Image>();
        image.enabled = true;
        _circle = image.material = Instantiate( image.material );
        _maxCircleAlpha = _circle.color.a;

        _beatActionGroup = transform.Find( "Background" ).GetComponent<CanvasGroup>();
        _buttonImage = _beatActionGroup.transform.Find( "Button" ).GetComponent<Image>();

        Debug.Log( _beatActionGroup.GetComponent<RectTransform>().rect.center);
        
        var beatManager = BeatManager.Instance;
        if ( beatManager == null )
        {
            gameObject.SetActive( false );
            return;
        }

        SetBeatActionAlpha( 0 );
        BeatManager.Instance.BeatEvent += OnBeatEvent;
    }

    private void OnBeatEvent( int beatCount, bool playerSuccess, float nextBeat, BeatManager.BeatAction requiredBeatAction )
    {
        if ( beatCount > 0 )
        {
            StartCoroutine( ShowResolveVizualization( playerSuccess ) );
        }
        
        var delay = Mathf.Max( 0, nextBeat - PreviewTime );
        var duration = Mathf.Min( PreviewTime, nextBeat );

        StartCoroutine( ShowVizualization( duration, delay, requiredBeatAction ) );
    }

    private IEnumerator ShowResolveVizualization( bool playerSuccess )
    {
        yield return new WaitForSeconds( Adjustment );
        if ( playerSuccess )
        {
            Tween( 0f, 1f, FinalEffectDuration, FinalEffectMovementEasing,
                    v => { SetProgressNormalized( 1 - v * FinalEffectSize ); } )
                .Then( () => { SetProgressNormalized( 0 ); } );

            Tween( 0f, 1f, FinalEffectDuration, FinalEffectFadeEasing,
                    v => { SetAlpha( ( 1 - v ) * _maxCircleAlpha ); } )
                .Then( () => { SetAlpha( _maxCircleAlpha ); } );
        }
        else
        {
            SetProgressNormalized( 0 );
            SetAlpha( _maxCircleAlpha );
        }

        Tween( 1f, 0f, 0.2f, EasingFunction.EaseOutQuad, SetBeatActionAlpha );
    }

    private IEnumerator ShowVizualization( float duration, float delay, BeatManager.BeatAction beatAction )
    {
        var startTime = Time.time;
        if ( delay > 0 )
        {
            yield return new WaitForSeconds( delay );
        }

        _buttonImage.sprite = _beatActionSprites[ beatAction ];

        var delta = Time.time - ( startTime + delay );
        Tween( 0f, 1f, 0.2f, EasingFunction.EaseOutQuad, SetBeatActionAlpha );

        Tween( 0f, 1f, duration - delta + Adjustment, Mathf.Lerp, SetProgressNormalized );
    }

    private void SetAlpha( float alpha )
    {
        _circle.color = _circle.color.WithAlpha( alpha );
    }

    private void SetProgressNormalized( float n )
    {
        _circle.SetFloat( "_Radius", Mathf.Lerp( MaxRadius, MinRadius, n ) );
    }

    private void SetBeatActionAlpha( float alpha )
    {
        _beatActionGroup.alpha = alpha;
    }
}