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
/// Representa una posible condición frente al IVA devuelta por AFIP
/// </summary>
public class dcCondicionIvaOption
{
    public string Id { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ClasesComprobante { get; set; } = string.Empty;

    public string DisplayText => string.IsNullOrWhiteSpace(ClasesComprobante)
        ? $"{Id} - {Descripcion}"
        : $"{Id} - {Descripcion} ({ClasesComprobante})";

    public override string ToString() => DisplayText;

    public bool AplicaAClase(string clase)
    {
        if (string.IsNullOrWhiteSpace(clase))
            return true;

        if (string.IsNullOrWhiteSpace(ClasesComprobante))
            return true;

        var clases = ClasesComprobante
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.ToUpperInvariant());

        return clases.Contains(clase.ToUpperInvariant());
    }
}
