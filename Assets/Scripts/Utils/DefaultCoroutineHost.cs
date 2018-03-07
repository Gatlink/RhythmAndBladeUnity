using UnityEngine;

public class DefaultCoroutineHost : MonoBehaviour
{
    private static DefaultCoroutineHost _instance;

    public static DefaultCoroutineHost Instance
    {
        get
        {
            if ( _instance == null )
            {
                _instance = FindObjectOfType<DefaultCoroutineHost>();
                if ( _instance == null )
                {
                    _instance = new GameObject( "Default Coroutine Host", typeof( DefaultCoroutineHost ) )
                        .GetComponent<DefaultCoroutineHost>();
                    _instance.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            return _instance;
        }
    }
}