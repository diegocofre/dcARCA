/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using Microsoft.Extensions.Logging;

namespace dcArca.TestApp.Logging;

/// <summary>
/// Proveedor de logging que escribe todas las entradas en un archivo de texto plano.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _syncRoot = new();

    /// <summary>
    /// Crea una nueva instancia del proveedor.
    /// </summary>
    /// <param name="filePath">Ruta completa del archivo donde se registrarán los logs.</param>
    public FileLoggerProvider(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("El path del archivo de log es obligatorio.", nameof(filePath));
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _filePath = filePath;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _filePath, _syncRoot);

    /// <inheritdoc />
    public void Dispose()
    {
        // No hay recursos no administrados.
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _filePath;
        private readonly object _syncRoot;

        public FileLogger(string categoryName, string filePath, object syncRoot)
        {
            _categoryName = categoryName;
            _filePath = filePath;
            _syncRoot = syncRoot;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

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
            if (string.IsNullOrWhiteSpace(message) && exception == null)
            {
                return;
            }

            var entry = $"{DateTimeOffset.Now:O} [{logLevel}] {_categoryName}: {message}";
            if (exception != null)
            {
                entry += Environment.NewLine + exception;
            }

            lock (_syncRoot)
            {
                File.AppendAllText(_filePath, entry + Environment.NewLine);
            }
        }
    }
}
