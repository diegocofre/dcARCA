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
/// Logger liviano utilizado por los servicios de AFIP para desacoplar la salida de logs.
/// </summary>
public interface IAfipLogger
{
    /// <summary>
    /// Registra un mensaje con nivel <see cref="LogLevel.Trace"/>.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogTrace(string message);

    /// <summary>
    /// Registra un mensaje con nivel <see cref="LogLevel.Debug"/>.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogDebug(string message);

    /// <summary>
    /// Registra un mensaje con nivel <see cref="LogLevel.Information"/>.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogInformation(string message);

    /// <summary>
    /// Registra un mensaje con nivel <see cref="LogLevel.Warning"/>.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogWarning(string message);

    /// <summary>
    /// Registra un mensaje con nivel <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    /// <param name="exception">Excepción asociada para incluir detalles adicionales.</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Registra un mensaje con nivel <see cref="LogLevel.Critical"/>.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    /// <param name="exception">Excepción asociada para incluir detalles adicionales.</param>
    void LogCritical(string message, Exception? exception = null);
}
