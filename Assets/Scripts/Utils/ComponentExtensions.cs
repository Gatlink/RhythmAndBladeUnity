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

    public static bool IsAncestorOf( this Transform self, Transform other )
    {
        while ( other != null )
        {
            if ( other == self ) return true;
            other = other.parent;
        }

        return false;
    }
}