/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using System;

namespace dcArca.Core.Models;

/// <summary>
/// Configuración para conectar con los servicios de ARCA
/// </summary>
public class dcArcaConfig
{
    /// <summary>
    /// CUIT del emisor (empresa que factura)
    /// </summary>
    public string Cuit { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo de certificado .pfx
    /// </summary>
    public string CertificatePath { get; set; } = string.Empty;

    /// <summary>
    /// Password del certificado .pfx
    /// </summary>
    public string CertificatePassword { get; set; } = string.Empty;

    /// <summary>
    /// URL del servicio WSAA para autenticación
    /// </summary>
    public string WsaaUrl { get; set; } = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";

    /// <summary>
    /// URL del servicio WSFEv1 para facturación electrónica
    /// </summary>
    public string WsfeUrl { get; set; } = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";

    /// <summary>
    /// URL del servicio de padrón (ws_sr_padron_a5) para validar CUITs
    /// </summary>
    public string PadronUrl { get; set; } = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA5";

    /// <summary>
    /// Punto de venta
    /// </summary>
    public int PuntoVenta { get; set; } = 1;


}
