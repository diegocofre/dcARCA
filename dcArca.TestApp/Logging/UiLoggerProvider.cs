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

namespace dcArca.TestApp.Logging;

/// <summary>
/// Proveedor de logging que reenvía los mensajes a un callback para mostrarlos en la UI.
/// </summary>
public sealed class UiLoggerProvider : ILoggerProvider
{
    private readonly Action<LogLevel, string, Exception?> _writeCallback;
    private readonly LogLevel _minLevel;

    /// <summary>
    /// Inicializa una nueva instancia del proveedor para UI.
    /// </summary>
    /// <param name="writeCallback">Acción que se ejecutará por cada mensaje.</param>
    /// <param name="minLevel">Nivel mínimo a reportar.</param>
    public UiLoggerProvider(Action<LogLevel, string, Exception?> writeCallback, LogLevel minLevel = LogLevel.Information)
    {
        _writeCallback = writeCallback ?? throw new ArgumentNullException(nameof(writeCallback));
        _minLevel = minLevel;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new UiLogger(categoryName, _writeCallback, _minLevel);

    /// <inheritdoc />
    public void Dispose()
    {
        // No dispose necesario
    }

    private sealed class UiLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Action<LogLevel, string, Exception?> _callback;
        private readonly LogLevel _minLevel;

        public UiLogger(string categoryName, Action<LogLevel, string, Exception?> callback, LogLevel minLevel)
        {
            _categoryName = categoryName;
            _callback = callback;
            _minLevel = minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel && logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);
            _callback(logLevel, $"[{_categoryName}] {message}", exception);
        }
    }
}
