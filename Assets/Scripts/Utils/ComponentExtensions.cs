using System;
using System.Collections;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public static class ComponentExtensions
{
    public static TInterface GetInterfaceComponentInParent<TInterface>( this Component thisComponent )
        where TInterface : class
    {
        var component = thisComponent;
        TInterface found;
        do
        {
            found = component.GetInterfaceComponent<TInterface>();
            component = component.transform.parent;
        } while ( found == null && component != null );

        return found;
    }

    public static TInterface[] GetInterfaceComponents<TInterface>( this Component thisComponent )
        where TInterface : class
    {
        return thisComponent.GetComponents( typeof( TInterface ) ).Cast<TInterface>().ToArray();
    }

    public static Coroutine Then( this Coroutine self, Action action )
    {
        return DefaultCoroutineHost.Instance.StartCoroutine( ChainWithAction( self, action ) );
    }

    public static Coroutine Then( this Coroutine self, Coroutine coroutine )
    {
        return DefaultCoroutineHost.Instance.StartCoroutine( ChainWithCoroutine( self, coroutine ) );
    }

    public static Coroutine Then( this Coroutine self, IEnumerator coroutine )
    {
        return DefaultCoroutineHost.Instance.StartCoroutine( ChainWithCoroutine( self,
            DefaultCoroutineHost.Instance.StartCoroutine( coroutine ) ) );
    }

    private static IEnumerator ChainWithCoroutine( Coroutine first, Coroutine second )
    {
        yield return first;
        yield return second;
    }

    private static IEnumerator ChainWithAction( Coroutine coroutine, Action action )
    {
        yield return coroutine;
        action();
    }
}