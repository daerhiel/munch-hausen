using System.Collections.Concurrent;

var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var context = new ConcurrentDictionary<Actor, Task>();
var volume = 5;
var min = 0.3;
var max = 0.8;
foreach (var index in Enumerable.Range(0, volume))
    context.TryAdd(new Actor(index, min + index * (max - min) / (volume - 1)), Task.CompletedTask);

Console.WriteLine(string.Join('\t', context.Keys.OrderBy(x => x.Speed).Select(x => x.Speed)));
do
{
    foreach (var (actor, task) in context)
    {
        if (task.IsCompleted)
            context[actor] = actor.Act(source.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                    Console.WriteLine($"{new string('\t', actor.Index)}{actor.State}");
            });
    }
    await Task.WhenAny(context.Values);
}
while (!source.IsCancellationRequested);

public class Actor
{
    private readonly int _index;
    private readonly double _speed;
    private int _state;

    public int Index => _index;
    public double Speed => _speed;
    public int State => _state;

    public Actor(int index, double speed)
    {
        _index = index;
        _speed = speed;
    }

    public async Task Act(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(_speed), cancellationToken);
        _state++;
    }

    public override string ToString() => $"Freq = {_speed}";
}