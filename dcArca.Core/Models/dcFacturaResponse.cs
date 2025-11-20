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
/// Respuesta de AFIP al solicitar autorización de una factura
/// </summary>
public class dcFacturaResponse
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Código de Autorización Electrónico
    /// </summary>
    public string Cae { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vencimiento del CAE (formato YYYYMMDD)
    /// </summary>
    public string CaeVencimiento { get; set; } = string.Empty;

    /// <summary>
    /// Número de comprobante asignado
    /// </summary>
    public long NumeroComprobante { get; set; }

    /// <summary>
    /// Resultado del procesamiento (A = Aprobado, R = Rechazado, etc.)
    /// </summary>
    public string Resultado { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje de error o información adicional
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Código identificador del resultado o error
    /// </summary>
    public string? Codigo { get; set; }

    /// <summary>
    /// Observaciones devueltas por AFIP
    /// </summary>
    public List<string> Observaciones { get; set; } = new();

    /// <summary>
    /// Errores devueltos por AFIP
    /// </summary>
    public List<string> Errores { get; set; } = new();
}
