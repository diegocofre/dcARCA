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
/// Resultado simplificado de la consulta al padrón (ws_sr_padron_a5)
/// </summary>
public class dcPadronPersonaResult
{
    public long CuitConsultado { get; set; }
    public bool Success { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    public string? EstadoClave { get; set; }
    public string? TipoPersona { get; set; }
    public string? TipoClave { get; set; }
    public string? RazonSocial { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? ErrorCodigo { get; set; }
    public string? ErrorDescripcion { get; set; }

    public List<string> Caracterizaciones { get; } = new();
    public List<string> Actividades { get; } = new();
    public List<string> Regimenes { get; } = new();

    public bool Existe => Success && string.IsNullOrWhiteSpace(ErrorCodigo);

    public bool EstaActivo =>
        string.Equals(EstadoClave, "ACTIVO", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(EstadoClave, "AC", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(EstadoClave, "A", StringComparison.OrdinalIgnoreCase);
}
