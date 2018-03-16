using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions.Algorithms;

public class RandomBagGenerator<T> : Generator.AbstractGenerator<T>
{
    private int _current;
    private readonly List<T> _bag;

    public override T Current
    {
        get { return _bag[ _current ]; }
    }

    public RandomBagGenerator( IEnumerable<T> values )
    {
        _bag = values.ToList();
        Restart();
    }

    public override void MoveNext()
    {
        ++_current;
        if ( _current >= _bag.Count )
        {
            Restart();
        }
    }

    private void Restart()
    {
        _bag.Shuffle();
        _current = 0;
    }

    public override IGenerator<T> CloneAndRestart()
    {
        return new RandomBagGenerator<T>( _bag );
    }
}