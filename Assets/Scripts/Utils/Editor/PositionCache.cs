using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PositionCache : ScriptableObject
{
    [ Serializable ]
    private class PositionDictionary : SerializableDictionary<string, Vector2>
    {
    }

    [ SerializeField ]
    private PositionDictionary _cachedPositions = new PositionDictionary();

    public void CachePosition( string guid, Vector2 position )
    {
        _cachedPositions[ guid ] = position;
    }

    public Vector2 GetCachedPosition( string guid )
    {
        return _cachedPositions[ guid ];
    }

    public bool ValidateCache( IEnumerable<string> guids )
    {
        var loadFromCache = false;
        if ( _cachedPositions == null )
        {
//            Debug.Log( "no cache found" );
            _cachedPositions = new PositionDictionary();
        }
        else
        {
            var allNodes = guids.ToList();
            if ( allNodes.Any( guid => !_cachedPositions.ContainsKey( guid ) ) )
            {
//                Debug.Log( "invalid cache found" );
                _cachedPositions = new PositionDictionary();
            }
            else
            {
                loadFromCache = true;
                var toRemove = _cachedPositions.Keys.Except( allNodes ).ToArray();
                foreach ( var guid in toRemove )
                {
                    _cachedPositions.Remove( guid );
                }
            }
        }

        return loadFromCache;
    }

    public void InvalidateCache()
    {
        _cachedPositions = new PositionDictionary();
    }
}
