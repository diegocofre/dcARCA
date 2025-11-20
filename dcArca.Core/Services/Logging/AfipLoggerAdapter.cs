/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using System;
using Microsoft.Extensions.Logging;

namespace dcArca.Core.Services.Logging;

/// <summary>
/// Adaptador que conecta un <see cref="ILogger"/> con la interfaz liviana <see cref="IAfipLogger"/>.
/// </summary>
public sealed class AfipLoggerAdapter : IAfipLogger
{
    private readonly ILogger _logger;

    /// <summary>
    /// Crea una nueva instancia del adaptador.
    /// </summary>
    /// <param name="logger">Instancia de <see cref="ILogger"/> a la que se delegará el registro.</param>
    public AfipLoggerAdapter(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void LogTrace(string message) => _logger.LogTrace(message);

    /// <inheritdoc />
    public void LogDebug(string message) => _logger.LogDebug(message);

    /// <inheritdoc />
    public void LogInformation(string message) => _logger.LogInformation(message);

    /// <inheritdoc />
    public void LogWarning(string message) => _logger.LogWarning(message);

    /// <inheritdoc />
    public void LogError(string message, Exception? exception = null) => _logger.LogError(exception, message);

    /// <inheritdoc />
    public void LogCritical(string message, Exception? exception = null) => _logger.LogCritical(exception, message);
}
