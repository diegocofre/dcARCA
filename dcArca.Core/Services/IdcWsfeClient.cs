/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using dcArca.Core.Models;

namespace dcArca.Core.Services;

public interface IdcWsfeClient
{
    Task<dcFacturaResponse> FECompUltimoAutorizadoAsync(dcTipoComprobante tipoComprobante, CancellationToken cancellationToken = default);

    Task<dcFacturaResponse> FECompConsultarAsync(long numeroComprobante, dcTipoComprobante tipoComprobante, CancellationToken cancellationToken = default);

    Task<dcFacturaResponse> FECAESolicitarAsync(dcFacturaRequest factura, CancellationToken cancellationToken = default);

    Task<dcFacturaResponse> SolicitarCaeAsync(dcFacturaRequest factura, CancellationToken cancellationToken = default);

    Task<List<dcCondicionIvaOption>> GetCondicionesIVAReceptorAsync(int docTipo, long docNro, dcTipoComprobante tipoComprobante, CancellationToken cancellationToken = default);
}