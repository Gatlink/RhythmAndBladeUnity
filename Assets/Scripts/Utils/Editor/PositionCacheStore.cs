using System;
using UnityEditor;
using UnityEngine;

public class PositionCacheStore : ScriptableObject
{
    [ Serializable ]
    private class CacheStore : SerializableDictionary<string, PositionCache>
    {
    }

    [ SerializeField ]
    private CacheStore _cachedDictionnaries = new CacheStore();

    public PositionCache GetCache( string path )
    {
        AssetDatabase.Refresh();

        PositionCache cache;
        if ( !_cachedDictionnaries.TryGetValue( path, out cache ) )
        {
            Debug.Log( "Creating cache for " + path );

            cache = new PositionCache();
            _cachedDictionnaries[ path ] = cache;
            EditorUtility.SetDirty( this );
            AssetDatabase.SaveAssets();            
        }
        
        if ( cache == null )
        {
            Debug.Log( "cache is null !" + path );                
        }

        Debug.Log( "Got cache for " + path );

        return cache;
    }
}
