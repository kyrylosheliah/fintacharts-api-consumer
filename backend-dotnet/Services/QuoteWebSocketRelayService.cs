using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BackendDotnet.Models;
using Serilog;

namespace BackendDotnet.Services;

public class QuoteWebSocketRelayService(
    AuthTokenService _tokenService,
    IConfiguration _config
)
{
    private const WebSocketMessageType text = WebSocketMessageType.Text;
    private readonly string _wssUri = _config["Wss:Uri"] ?? "";

    private readonly ConcurrentDictionary<string, SymbolRelay> _symbolRelays = new();

    public async Task HandleClientWebSocketAsync(
        WebSocket clientSocket,
        Instrument instrument
    )
    {
        var relay = _symbolRelays.GetOrAdd(instrument.Symbol, _ => new SymbolRelay(instrument, ConnectToBackendSocketAsync));
        await relay.AddClientAsync(clientSocket);
    }

    private async Task<ClientWebSocket> ConnectToBackendSocketAsync(Instrument instrument)
    {
        var ws = new ClientWebSocket();
        var token = await _tokenService.GetAccessTokenAsync();
        //ws.Options.SetRequestHeader("Authorization", token);
        var url = $"{_wssUri}/api/streaming/ws/v1/realtime?token={token}";
        Log.Warning($"url {url}");
        await ws.ConnectAsync(new Uri(url), CancellationToken.None);
        Log.Warning($"connect backend for {instrument.Id}");
        var message = new WebSocketMessage()
        {
            Type = "l1-subscription",
            InstrumentId = instrument.Id,
            Provider = "simulation",
        };
        await ws.SendAsync
        (
            Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message)),
            text,
            true,
            CancellationToken.None
        );
        return ws;
    }

    private class SymbolRelay(
        Instrument _instrument,
        Func<Instrument, Task<ClientWebSocket>> _backendConnector
    )
    {
        private readonly List<WebSocket> _clients = [];
        private readonly SemaphoreSlim _lock = new(1, 1);
        private ClientWebSocket? _backendSocket;
        private CancellationTokenSource? _cts;

        public async Task AddClientAsync(WebSocket clientSocket)
        {
            await _lock.WaitAsync();
            try
            {
                _clients.Add(clientSocket);

                Log.Warning($"clients count: {_clients.Count}");

                if (_backendSocket == null)
                {
                    _cts = new CancellationTokenSource();
                    _backendSocket = await _backendConnector(_instrument);
                    _ = Task.Run(() => RelayBackendToClientsAsync(_cts.Token));
                }
            }
            finally
            {
                _lock.Release();
            }

            await WaitClientDisconnectAsync(clientSocket);
        }

        private async Task RelayBackendToClientsAsync(CancellationToken token)
        {
            var buffer = new byte[8192];
            try
            {
                while (_backendSocket?.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _backendSocket.ReceiveAsync(buffer, token);
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var message = new ArraySegment<byte>(buffer, 0, result.Count);

                    await _lock.WaitAsync();
                    try
                    {
                        var sendTasks = _clients
                            .Where(ws => ws.State == WebSocketState.Open)
                            .Select(ws => ws.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None));
                        await Task.WhenAll(sendTasks);
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Relay Error: {_instrument.Symbol}] {ex.Message}");
            }
        }

        private async Task WaitClientDisconnectAsync(WebSocket clientSocket)
        {
            var buffer = new byte[1024];
            try
            {
                while (clientSocket.State == WebSocketState.Open)
                {
                    var result = await clientSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                }
            }
            finally
            {
                await _lock.WaitAsync();
                try
                {
                    _clients.Remove(clientSocket);
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", CancellationToken.None);

                    // if (_clients.Count == 0 && _backendSocket != null)
                    // {
                    //     _cts?.Cancel();
                    //     await _backendSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "No clients", CancellationToken.None);
                    //     _backendSocket.Dispose();
                    //     _backendSocket = null;
                    // }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }
}
