using Gamelogic.Extensions;
using UnityEngine;
using UnityEditor;

public class CameraPreview : EditorWindow
{
    private const string PreviewCameraTag = "PreviewCamera";

    private Camera _camera;

    private Camera Camera
    {
        get
        {
            if ( _camera == null )
            {
                var cameraObject = GameObject.FindGameObjectWithTag( PreviewCameraTag );
                if ( cameraObject != null )
                {
                    _camera = cameraObject.GetComponent<Camera>();
                }

                if ( _camera == null )
                {
                    _camera = new GameObject( "Preview Camera", typeof( Camera ) ).GetComponent<Camera>();
                    _camera.gameObject.tag = PreviewCameraTag;
                }
            }

            return _camera;
        }
    }

    private Rail _cameraConstraint;

    private float _railPosition;
    private bool _displayOnGameView;

    private RenderTexture _renderTexture;
    private CameraZoomConstraint _zoomConstraint;

    [ MenuItem( "Camera/Preview Camera Constraint" ) ]
    private static void Init()
    {
        var editorWindow = (EditorWindow) GetWindow<CameraPreview>( typeof( CameraPreview ) );
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.titleContent = new GUIContent( "Camera Preview" );
        editorWindow.Show();
    }

    private void Update()
    {
        if ( _cameraConstraint != null )
        {
            EnsureRenderTexture();

//            // reuse main camera
//            var cam = Camera.main;
//            var pos = cam.transform.position;
//            var targetTex = cam.targetTexture;
//            cam.transform.position = _cameraConstraint.EvaluatePosition( _railPosition ).To3DXY( -10 );
//            cam.targetTexture = _renderTexture;
//            cam.Render();
//            cam.transform.position = pos;
//            cam.targetTexture = targetTex;

            var cam = Camera;

            cam.CopyFrom( Camera.main );

            cam.depth += _displayOnGameView ? 1 : -1;
            cam.transform.position = _cameraConstraint.EvaluatePosition( _railPosition ).To3DXY( -10 );

            if ( _zoomConstraint != null )
            {
                cam.orthographicSize *= _zoomConstraint.GetZoom( _railPosition );
            }
            
            cam.renderingPath = RenderingPath.UsePlayerSettings;
            cam.targetTexture = _renderTexture;
            cam.Render();
            cam.targetTexture = null;
        }
    }

    private void OnSelectionChange()
    {
        var obj = Selection.activeGameObject;
        if ( obj == null )
        {
            CleanUp();
            return;
        }

        if ( obj.layer != LayerMask.NameToLayer( Layers.CameraConstraint ) )
        {
            CleanUp();
            return;
        }

        _cameraConstraint = obj.GetComponent<Rail>();
        if ( _cameraConstraint == null )
        {
            CleanUp();
            return;
        }

        _zoomConstraint = obj.GetComponent<CameraZoomConstraint>();
    }

    private void CleanUp()
    {
        _cameraConstraint = null;
        if ( _camera != null )
        {
            DestroyImmediate( _camera.gameObject );
            _camera = null;
        }
    }

    private void EnsureRenderTexture()
    {
        if ( _renderTexture == null
             || (int) position.width != _renderTexture.width
             || (int) position.height != _renderTexture.height )
        {
            DestroyImmediate( _renderTexture );
            _renderTexture = new RenderTexture( (int) position.width, (int) position.height - 10, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB );
        }
    }

    private void OnGUI()
    {
        if ( _renderTexture != null )
        {
            var line = EditorGUIUtility.singleLineHeight + 1;
            const int paddingRight = 4;
            GUI.DrawTexture( new Rect( 0.0f, 0.0f, position.width, position.height - 2 * line ), _renderTexture );
            GUILayout.BeginArea( new Rect( new Vector2( 0, position.height - 2 * line + 1 ),
                new Vector2( position.width - paddingRight, 2 * line - 1 ) ) );

            _railPosition = EditorGUILayout.Slider( "Position on rail", _railPosition, 0, 1 );
            _displayOnGameView = EditorGUILayout.Toggle( "Display on game view", _displayOnGameView );

            GUILayout.EndArea();
        }
    }
}