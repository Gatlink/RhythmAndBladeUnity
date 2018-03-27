using Gamelogic.Extensions;
using UnityEngine;

public class DefaultCoroutineHost : GLMonoBehaviour
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
                }
            }

            return _instance;
        }
    }
}