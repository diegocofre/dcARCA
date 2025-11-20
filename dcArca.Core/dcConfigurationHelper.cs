/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using Microsoft.Extensions.Configuration;
using dcArca.Core.Models;

namespace dcArca.Core;

/// <summary>
/// Helper para cargar la configuración desde appsettings.json
/// </summary>
public static class dcConfigurationHelper
{
    /// <summary>
    /// Carga la configuración de ARCA desde el archivo appsettings.json
    /// </summary>
    public static dcArcaConfig LoadFromJson(string configPath = "appsettings.json")
    {
        var resolvedPath = ResolveConfigPath(configPath);
        var basePath = Path.GetDirectoryName(resolvedPath)!;
        var fileName = Path.GetFileName(resolvedPath);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(fileName, optional: false, reloadOnChange: true)
            .Build();

        var config = new dcArcaConfig();
        configuration.GetSection("dcArcaConfig").Bind(config);

        ValidateConfig(config);
        return config;
    }

    private static string ResolveConfigPath(string configPath)
    {
        if (Path.IsPathRooted(configPath) && File.Exists(configPath))
        {
            return configPath;
        }

        var candidateBases = new[]
        {
            Directory.GetCurrentDirectory(),
            AppDomain.CurrentDomain.BaseDirectory,
            AppContext.BaseDirectory
        };

        foreach (var baseDir in candidateBases.Where(dir => !string.IsNullOrWhiteSpace(dir)))
        {
            var candidate = Path.Combine(baseDir!, configPath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"No se encontró el archivo de configuración: {configPath}");
    }

    private static void ValidateConfig(dcArcaConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Cuit))
            throw new InvalidOperationException("El CUIT del emisor no está configurado");

        if (!EsCuitValido(config.Cuit))
            throw new InvalidOperationException("El CUIT del emisor debe tener 11 dígitos numéricos y un dígito verificador válido.");

        if (string.IsNullOrWhiteSpace(config.CertificatePath))
            throw new InvalidOperationException("La ruta del certificado no está configurada");

        if (!File.Exists(config.CertificatePath))
            throw new FileNotFoundException($"No se encontró el certificado: {config.CertificatePath}");

        if (string.IsNullOrWhiteSpace(config.WsfeUrl))
            throw new InvalidOperationException("La URL del servicio WSFEv1 no está configurada");

        if (string.IsNullOrWhiteSpace(config.PadronUrl))
            throw new InvalidOperationException("La URL del servicio de padrón (ws_sr_padron_a5) no está configurada");

        if (config.PuntoVenta <= 0)
            throw new InvalidOperationException("PuntoVenta debe ser un entero mayor que 0 y estar dado de alta en AFIP.");
    }

    private static bool EsCuitValido(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
        {
            return false;
        }

        var sanitized = new string(cuit.Where(char.IsDigit).ToArray());
        if (sanitized.Length != 11)
        {
            return false;
        }

        if (!long.TryParse(sanitized, out _))
        {
            return false;
        }

        var multiplicadores = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        var suma = 0;

        for (var i = 0; i < 10; i++)
        {
            suma += (sanitized[i] - '0') * multiplicadores[i];
        }

        var verificador = 11 - (suma % 11);
        if (verificador == 11) verificador = 0;
        if (verificador == 10) verificador = 9;

        return verificador == (sanitized[10] - '0');
    }
}
