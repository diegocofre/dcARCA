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
/// Tipos de documento según especificación AFIP
/// </summary>
public enum dcTipoDocumento
{
    /// <summary>
    /// CUIT - Clave Única de Identificación Tributaria
    /// </summary>
    CUIT = 80,

    /// <summary>
    /// CUIL - Código Único de Identificación Laboral
    /// </summary>
    CUIL = 86,

    /// <summary>
    /// CDI - Cédula de Identidad
    /// </summary>
    CDI = 87,

    /// <summary>
    /// LE - Libreta de Enrolamiento
    /// </summary>
    LE = 89,

    /// <summary>
    /// LC - Libreta Cívica
    /// </summary>
    LC = 90,

    /// <summary>
    /// Pasaporte
    /// </summary>
    Pasaporte = 94,

    /// <summary>
    /// DNI - Documento Nacional de Identidad
    /// </summary>
    DNI = 96,

    /// <summary>
    /// Consumidor Final (sin documento)
    /// </summary>
    ConsumidorFinal = 99,

    /// <summary>
    /// CUIT del país de residencia
    /// </summary>
    CUITExtranjero = 30
}

/// <summary>
/// Códigos de alícuota de IVA según AFIP
/// </summary>
public enum dcAlicuotaIva
{
    /// <summary>
    /// No gravado (0%)
    /// </summary>
    NoGravado = 3,

    /// <summary>
    /// IVA 10.5%
    /// </summary>
    Diez_Cinco = 4,

    /// <summary>
    /// IVA 21% (General)
    /// </summary>
    Veintiuno = 5,

    /// <summary>
    /// IVA 27%
    /// </summary>
    Veintisiete = 6,

    /// <summary>
    /// IVA 5%
    /// </summary>
    Cinco = 8,

    /// <summary>
    /// IVA 2.5%
    /// </summary>
    Dos_Cinco = 9
}

/// <summary>
/// Tipos de concepto de factura
/// </summary>
public enum dcConcepto
{
    /// <summary>
    /// Productos
    /// </summary>
    Productos = 1,

    /// <summary>
    /// Servicios
    /// </summary>
    Servicios = 2,

    /// <summary>
    /// Productos y Servicios
    /// </summary>
    ProductosYServicios = 3
}

/// <summary>
/// Tipos de comprobante según AFIP
/// </summary>
public enum dcTipoComprobante
{
    /// <summary>
    /// Factura A
    /// </summary>
    FacturaA = 1,

    /// <summary>
    /// Nota de Débito A
    /// </summary>
    NotaDebitoA = 2,

    /// <summary>
    /// Nota de Crédito A
    /// </summary>
    NotaCreditoA = 3,

    /// <summary>
    /// Factura B
    /// </summary>
    FacturaB = 6,

    /// <summary>
    /// Nota de Débito B
    /// </summary>
    NotaDebitoB = 7,

    /// <summary>
    /// Nota de Crédito B
    /// </summary>
    NotaCreditoB = 8,

    /// <summary>
    /// Factura C
    /// </summary>
    FacturaC = 11,

    /// <summary>
    /// Nota de Débito C
    /// </summary>
    NotaDebitoC = 12,

    /// <summary>
    /// Nota de Crédito C
    /// </summary>
    NotaCreditoC = 13,

    /// <summary>
    /// Factura M
    /// </summary>
    FacturaM = 51,

    /// <summary>
    /// Nota de Débito M
    /// </summary>
    NotaDebitoM = 52,

    /// <summary>
    /// Nota de Crédito M
    /// </summary>
    NotaCreditoM = 53
}
