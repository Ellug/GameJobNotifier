using System.Threading.Channels;
using GameJobNotifier.App.Services.Interfaces;

namespace GameJobNotifier.App.Services;

public sealed class CheckRequestQueue : ICheckRequestQueue
{
    private readonly Channel<byte> _channel = Channel.CreateUnbounded<byte>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public void RequestCheck()
    {
        _channel.Writer.TryWrite(0);
    }

    public async ValueTask WaitForCheckAsync(CancellationToken cancellationToken = default)
    {
        _ = await _channel.Reader.ReadAsync(cancellationToken);
    }

    public bool TryDequeue()
    {
        return _channel.Reader.TryRead(out _);
    }
}
