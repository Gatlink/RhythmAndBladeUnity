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

    private RenderTexture _renderTexture;

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
            var cam = Camera;

            cam.CopyFrom( Camera.main );

            cam.transform.position = _cameraConstraint.EvaluatePosition( _railPosition ).To3DXY( -10 );
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
            const int sliderSize = 20;
            GUI.DrawTexture( new Rect( 0.0f, 0.0f, position.width, position.height - sliderSize ), _renderTexture );
            _railPosition = GUI.HorizontalSlider(
                new Rect( new Vector2( 5, position.height - sliderSize ),
                    new Vector2( position.width - 10, sliderSize ) ),
                _railPosition,
                0, 1 );
        }
    }
}