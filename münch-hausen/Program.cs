using System.Collections.Concurrent;

var actor1 = new Actor(0.3);
var actor2 = new Actor(0.8);

var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var context = new ConcurrentDictionary<Actor, Task>();
context.TryAdd(actor1, Task.CompletedTask);
context.TryAdd(actor2, Task.CompletedTask);

Console.WriteLine("S\tF");
do
{
    foreach (var (actor, task) in context)
    {
        if (task.IsCompleted)
            context[actor] = actor.Act(source.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                    Console.WriteLine($"{(actor.Name != "fast" ? '\t' : string.Empty)}{actor.State}");
            });
    }
    await Task.WhenAny(context.Values);
}
while (!source.IsCancellationRequested);

public class Actor
{
    private readonly double _speed;
    private readonly string _name;
    private int _state;

    public string Name => _name;
    public int State => _state;

    public Actor(double speed)
    {
        _speed = speed;
        _name = speed > 0.5 ? "fast" : "slow";
    }

    public async Task Act(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(_speed), cancellationToken);
        _state++;
    }
}