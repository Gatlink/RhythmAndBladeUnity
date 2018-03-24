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
        PositionCache cache;
        if ( !_cachedDictionnaries.TryGetValue( path, out cache ) )
        {
            cache = CreateInstance<PositionCache>();
            _cachedDictionnaries[ path ] = cache;
            AssetDatabase.SaveAssets();
        }

        return cache;
    }
}
