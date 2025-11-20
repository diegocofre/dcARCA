/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */


namespace dcArca.Core.Services.Logging;

/// <summary>
/// Implementación que descarta los mensajes. Se usa cuando no se provee un logger explícito.
/// </summary>
public sealed class NoOpAfipLogger : IAfipLogger
{
    /// <inheritdoc />
    public void LogTrace(string message)
    {
    }

    /// <inheritdoc />
    public void LogDebug(string message)
    {
    }

    /// <inheritdoc />
    public void LogInformation(string message)
    {
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
    }

    /// <inheritdoc />
    public void LogError(string message, Exception? exception = null)
    {
    }

    /// <inheritdoc />
    public void LogCritical(string message, Exception? exception = null)
    {
    }

    /// <summary>
    /// Instancia singleton para evitar múltiples asignaciones.
    /// </summary>
    public static NoOpAfipLogger Instance { get; } = new();
}
