using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExtensions
{
    public static Coroutine Then( this Coroutine self, Action action )
    {
        return DefaultCoroutineHost.Instance.StartCoroutine( ChainWithAction( self, action ) );
    }

    public static Coroutine Then( this Coroutine self, IEnumerator coroutine )
    {
        return DefaultCoroutineHost.Instance.StartCoroutine( ChainWithCoroutine( self, coroutine ) );
    }

    private static IEnumerator ChainWithCoroutine( Coroutine first, IEnumerator second )
    {
        yield return first;
        yield return DefaultCoroutineHost.Instance.StartCoroutine( second );
    }

    private static IEnumerator ChainWithAction( Coroutine coroutine, Action action )
    {
        yield return coroutine;
        action();
    }
}