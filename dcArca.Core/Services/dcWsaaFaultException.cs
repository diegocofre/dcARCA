/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

namespace dcArca.Core.Services;

/// <summary>
/// Excepción específica para Faults SOAP de WSAA.
/// Incluye código de estado HTTP, faultcode, faultstring y el XML crudo.
/// </summary>
public class dcWsaaFaultException : Exception
{
    public int HttpStatusCode { get; }
    public string? FaultCode { get; }
    public string? FaultString { get; }
    public string? FaultDetail { get; }
    public string RawResponse { get; }

    public dcWsaaFaultException(
        int httpStatusCode,
        string? faultCode,
        string? faultString,
        string? faultDetail,
        string rawResponse)
        : base($"WSAA Fault (HTTP {httpStatusCode}) - {faultCode}: {faultString}\n{faultDetail}")
    {
        HttpStatusCode = httpStatusCode;
        FaultCode = faultCode;
        FaultString = faultString;
        FaultDetail = faultDetail;
        RawResponse = rawResponse;
    }
}
