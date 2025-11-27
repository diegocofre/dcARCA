/*
 * Copyright (c) 2025 Diego Cofré Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using System;
using System.Collections.Generic;

namespace dcArca.Core.Models;

/// <summary>
/// Helpers para convertir los códigos AFIP en textos legibles para UI o logs.
/// </summary>
public static class dcAfipEnumExtensions
{
    private static readonly IReadOnlyDictionary<dcTipoComprobante, string> TipoComprobanteText = new Dictionary<dcTipoComprobante, string>
    {
        { dcTipoComprobante.FacturaA, "Factura A" },
        { dcTipoComprobante.NotaDebitoA, "Nota de Débito A" },
        { dcTipoComprobante.NotaCreditoA, "Nota de Crédito A" },
        { dcTipoComprobante.FacturaB, "Factura B" },
        { dcTipoComprobante.NotaDebitoB, "Nota de Débito B" },
        { dcTipoComprobante.NotaCreditoB, "Nota de Crédito B" },
        { dcTipoComprobante.FacturaC, "Factura C" },
        { dcTipoComprobante.NotaDebitoC, "Nota de Débito C" },
        { dcTipoComprobante.NotaCreditoC, "Nota de Crédito C" },
        { dcTipoComprobante.FacturaM, "Factura M" },
        { dcTipoComprobante.NotaDebitoM, "Nota de Débito M" },
        { dcTipoComprobante.NotaCreditoM, "Nota de Crédito M" }
    };

    private static readonly IReadOnlyDictionary<dcTipoDocumento, string> TipoDocumentoText = new Dictionary<dcTipoDocumento, string>
    {
        { dcTipoDocumento.CUIT, "CUIT" },
        { dcTipoDocumento.CUIL, "CUIL" },
        { dcTipoDocumento.CDI, "CDI" },
        { dcTipoDocumento.LE, "Libreta de Enrolamiento" },
        { dcTipoDocumento.LC, "Libreta Cívica" },
        { dcTipoDocumento.Pasaporte, "Pasaporte" },
        { dcTipoDocumento.DNI, "DNI" },
        { dcTipoDocumento.ConsumidorFinal, "Consumidor Final" },
        { dcTipoDocumento.CUITExtranjero, "CUIT del exterior" }
    };

    private static readonly IReadOnlyDictionary<dcConcepto, string> ConceptoText = new Dictionary<dcConcepto, string>
    {
        { dcConcepto.Productos, "Productos" },
        { dcConcepto.Servicios, "Servicios" },
        { dcConcepto.ProductosYServicios, "Productos y Servicios" }
    };

    private static readonly IReadOnlyDictionary<dcCondicionIvaReceptor, string> CondicionIvaText = new Dictionary<dcCondicionIvaReceptor, string>
    {
        { dcCondicionIvaReceptor.ResponsableInscripto, "Responsable Inscripto" },
        { dcCondicionIvaReceptor.ResponsableNoInscripto, "Responsable no Inscripto" },
        { dcCondicionIvaReceptor.NoResponsable, "No Responsable" },
        { dcCondicionIvaReceptor.SujetoExento, "Sujeto Exento" },
        { dcCondicionIvaReceptor.ConsumidorFinal, "Consumidor Final" },
        { dcCondicionIvaReceptor.ResponsableMonotributo, "Responsable Monotributo" },
        { dcCondicionIvaReceptor.SujetoNoCategorizado, "Sujeto no Categorizado" },
        { dcCondicionIvaReceptor.ProveedorDelExterior, "Proveedor del Exterior" },
        { dcCondicionIvaReceptor.ClienteDelExterior, "Cliente del Exterior" },
        { dcCondicionIvaReceptor.IVALiberado19640, "IVA liberado Ley 19.640" },
        { dcCondicionIvaReceptor.ResponsableInscriptoAgentePercepcion, "Resp. Inscripto agente de percepción" },
        { dcCondicionIvaReceptor.PequenoContribuyenteEventual, "Pequeño contribuyente eventual" },
        { dcCondicionIvaReceptor.MonotributistaSocial, "Monotributista social" },
        { dcCondicionIvaReceptor.PequenoContribuyenteEventualSocial, "Pequeño contribuyente eventual social" }
    };

    private static readonly IReadOnlyDictionary<dcAlicuotaIva, string> AlicuotaText = new Dictionary<dcAlicuotaIva, string>
    {
        { dcAlicuotaIva.NoGravado, "No gravado (0%)" },
        { dcAlicuotaIva.Diez_Cinco, "IVA 10.5%" },
        { dcAlicuotaIva.Veintiuno, "IVA 21%" },
        { dcAlicuotaIva.Veintisiete, "IVA 27%" },
        { dcAlicuotaIva.Cinco, "IVA 5%" },
        { dcAlicuotaIva.Dos_Cinco, "IVA 2.5%" }
    };

    public static string ToDisplayString(this dcTipoComprobante value) => Format(value, TipoComprobanteText);

    public static string ToDisplayString(this dcTipoDocumento value) => Format(value, TipoDocumentoText);

    public static string ToDisplayString(this dcConcepto value) => Format(value, ConceptoText);

    public static string ToDisplayString(this dcCondicionIvaReceptor value) => Format(value, CondicionIvaText);

    public static string ToDisplayString(this dcAlicuotaIva value) => Format(value, AlicuotaText);

    private static string Format<TEnum>(TEnum value, IReadOnlyDictionary<TEnum, string> descriptions)
        where TEnum : struct, Enum
    {
        var code = Convert.ToInt32(value);
        return descriptions.TryGetValue(value, out var descripcion)
            ? $"{code} - {descripcion}"
            : $"{code} - {value}";
    }
}
