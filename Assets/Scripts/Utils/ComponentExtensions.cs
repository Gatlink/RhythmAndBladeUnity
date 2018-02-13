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
}