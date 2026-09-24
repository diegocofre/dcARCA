/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

namespace dcArca.Core.Models;

/// <summary>
/// Validación del dígito verificador de CUIT, compartida por la configuración
/// y por los modelos de factura (antes duplicada en ambos lugares).
/// </summary>
public static class dcCuitValidator
{
    private static readonly int[] Multiplicadores = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

    /// <summary>
    /// Valida que el CUIT tenga 11 dígitos y un dígito verificador correcto.
    /// Ignora caracteres no numéricos (guiones, espacios) antes de validar.
    /// </summary>
    public static bool EsValido(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            return false;

        var sanitized = new string(cuit.Where(char.IsDigit).ToArray());
        if (sanitized.Length != 11 || !long.TryParse(sanitized, out _))
            return false;

        var suma = 0;
        for (var i = 0; i < 10; i++)
            suma += (sanitized[i] - '0') * Multiplicadores[i];

        var verificador = 11 - (suma % 11);
        if (verificador == 11) verificador = 0;
        if (verificador == 10) verificador = 9;

        return verificador == (sanitized[10] - '0');
    }
}
