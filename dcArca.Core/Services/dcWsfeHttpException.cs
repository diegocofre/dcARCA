/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using System;
using System.Net;

namespace dcArca.Core.Services;

/// <summary>
/// Excepción que encapsula errores HTTP obtenidos al invocar el WSFE.
/// </summary>
public sealed class dcWsfeHttpException : Exception
{
    /// <summary>
    /// Código HTTP devuelto por el servicio.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Contenido textual de la respuesta devuelta por el servicio.
    /// </summary>
    public string ResponseBody { get; }

    /// <summary>
    /// Crea una nueva instancia de la excepción.
    /// </summary>
    /// <param name="statusCode">Código HTTP devuelto.</param>
    /// <param name="responseBody">Cuerpo devuelto por el servicio.</param>
    /// <param name="message">Mensaje descriptivo opcional.</param>
    /// <param name="innerException">Excepción interna opcional.</param>
    public dcWsfeHttpException(HttpStatusCode statusCode, string responseBody, string? message = null, Exception? innerException = null)
        : base(message ?? $"Error HTTP {(int)statusCode} ({statusCode}).", innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
