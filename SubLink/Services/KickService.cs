﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using xyz.yewnyx.SubLink.Kick;

namespace xyz.yewnyx.SubLink;

[UsedImplicitly]
internal sealed partial class KickService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IOptionsMonitor<KickSettings> _settingsMonitor;
    private KickSettings _settings;

    private readonly KickPusherClient _kick;

    private readonly IRules _rules;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Shhh")]
    private IServiceScope? _kickLoggedInScope;

    public KickService(
        ILogger logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<KickSettings> settingsMonitor,
        IServiceProvider serviceProvider,
        KickPusherClient kickPusherClient,
        IRules rules)
    {

        Globals.serviceProvider = serviceProvider;

        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _settingsMonitor = settingsMonitor;
        _settingsMonitor.OnChange(UpdateKickSettings);
        _settings = _settingsMonitor.CurrentValue;

        _kick = kickPusherClient ?? throw new ArgumentNullException(nameof(kickPusherClient));

        _rules = rules;

        WireCallbacks();
    }

    private void UpdateKickSettings(KickSettings kickSettings) => _settings = kickSettings;

    public async Task Start() {
        if (await _kick.ConnectAsync(_settings.PusherKey, _settings.PusherCluster, _settings.ChatroomId)) {
            _logger.Information("[{TAG}] Connected to Pusher", "Kick");
            _kickLoggedInScope = _serviceScopeFactory.CreateScope();
        } else {
            _logger.Warning("[{TAG}] Failed to connect to Pusher", "Kick");
            _applicationLifetime.StopApplication();
        }
    }

    public async Task Stop() {
        await _kick.DisconnectAsync();
    }
}

internal static class StringUtil {
    internal static bool HasAnything(this string str) => !string.IsNullOrWhiteSpace(str);
}
