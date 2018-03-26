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
            cache = new PositionCache();
            _cachedDictionnaries[ path ] = cache;
            EditorUtility.SetDirty( this );
            AssetDatabase.SaveAssets();            
        }
        
        return cache;
    }
}
