﻿using Serilog;
using SocketIOClient;
using System;
using System.Threading.Tasks;

namespace xyz.yewnyx.SubLink.StreamElements;

internal sealed class StreamElementsClient {
    private const string _socketUri = "https://realtime.streamelements.com";

    private ILogger _logger;
    private readonly SocketIOClient.SocketIO _socket;
    private string _token = string.Empty;

    public event EventHandler<TipEventArgs>? TipEvent;

    public StreamElementsClient(ILogger logger) {
        _logger = logger;

        _socket = new(_socketUri, new() {
            AutoUpgrade = true,
            ConnectionTimeout = TimeSpan.FromSeconds(30),
            EIO = SocketIO.Core.EngineIO.V3,
            Reconnection = true,
            ReconnectionAttempts = 3,
            ReconnectionDelay = 500,
            RandomizationFactor = 0.5,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        _socket.OnConnected += OnConnected;
        _socket.OnDisconnected += OnDisconnected;
        _socket.OnError += OnError;
        _socket.OnReconnectAttempt += OnReconnectAttempt;
        _socket.OnReconnected += OnReconnected;
        _socket.OnReconnectError += OnReconnectError;
        _socket.OnReconnectFailed += OnReconnectFailed;
        _socket.On("authenticated", OnAuthenticated);
        _socket.On("unauthorized", OnUnauthorized);
        _socket.On("event", OnEvent);
    }

    private async void OnConnected(object? sender, EventArgs e) {
        _logger.Information("[{TAG}] Connected to StreamElements", "StreamElements");
        await _socket.EmitAsync("authenticate", new SocketAuth("jwt", _token));
    }

    private void OnDisconnected(object? sender, string e) =>
        _logger.Information("[{TAG}] Disconnected from StreamElements", "StreamElements");

    private void OnError(object? sender, string e) =>
        _logger.Error("[{TAG}] StreamElements error: {ERROR}", "StreamElements", e);

    private void OnReconnectAttempt(object? sender, int e) =>
        _logger.Debug("[{TAG}] Socket reconnect attempt #{e}", "StreamElements", e);

    private void OnReconnected(object? sender, int e) =>
        _logger.Information("[{TAG}] Socket reconnected after {e} attempts", "StreamElements", e);

    private void OnReconnectError(object? sender, Exception e) =>
        _logger.Error("[{TAG}] Socket reconnect error:", "StreamElements", e);

    private void OnReconnectFailed(object? sender, EventArgs e) =>
        _logger.Error("[{TAG}] Socket reconnect failed", "StreamElements");

    private void OnAuthenticated(SocketIOResponse response) =>
        _logger.Information("[{TAG}] Authenticated with StreamElements", "StreamElements");

    private void OnUnauthorized(SocketIOResponse response) =>
        _logger.Information("[{TAG}] Not authorized to use the StreamElements Realtime API", "StreamElements");

    private void OnEvent(SocketIOResponse response) {
        SocketEvent sockEvent = response.GetValue<SocketEvent>();

        if (sockEvent == null) {
            _logger.Error("[{TAG}] Invalid event data recieved", "StreamElements");
            return;
        }

        switch (sockEvent.Type) {
            case "tip": {
                float amount = Convert.ToSingle(sockEvent.Data["amount"]);
                TipEvent?.Invoke(this, new() {
                    Name = (string?)sockEvent.Data["username"] ?? string.Empty,
                    Amount = amount,
                    CentAmount = (int)MathF.Round(amount * 100, 0, MidpointRounding.ToZero),
                    Message = (string?)sockEvent.Data["message"] ?? string.Empty,
                    UserCurrency = (string?)sockEvent.Data["currency"] ?? string.Empty
                });
                break;
            }
            default: {
                _logger.Debug("[{TAG}] Ignoring unsupported event of type: {TYPE}", "StreamElements", sockEvent.Type);
                break;
            }
        }
    }

    public async Task<bool> ConnectAsync(string token) {
        _token = token;

        try {
            await _socket.ConnectAsync();
            return true;
        } catch (Exception) {
            return false;
        }
    }

    public async Task DisconnectAsync() =>
        await _socket.DisconnectAsync();
}
