using System.Xml;
using dcArca.Core.Models;
using dcArca.Core.Services;
using Xunit;

namespace dcArca.Core.Tests;

public class dcWsfeSoapBuilderTests
{
    private static dcArcaConfig BuildConfig() => new()
    {
        Cuit = "20123456786",
        CertificatePath = "no-existe.pfx",
        WsaaUrl = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
        WsfeUrl = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx",
        PadronUrl = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA5",
        PuntoVenta = 1
    };

    [Fact]
    public void BuildSolicitarCaeRequest_EscapaCaracteresEspecialesEnComprobanteAsociado()
    {
        var builder = new dcWsfeSoapBuilder(BuildConfig());
        var factura = new dcFacturaRequest
        {
            TipoComprobante = dcTipoComprobante.NotaCreditoA,
            NumeroComprobante = 1,
            Concepto = dcConcepto.Productos,
            CuitReceptor = 20123456786,
            ImporteNeto = 100m,
            ImporteIva = 21m,
            ImporteTotal = 121m,
            FechaComprobante = "20260101",
            CbteAsociadoTipo = 1,
            CbteAsociadoPtoVta = 1,
            CbteAsociadoNro = 1,
            CbteAsociadoCuit = "20123456786 & Cía <SA>"
        };

        var xml = builder.BuildSolicitarCaeRequest("token", "sign", factura, 1, 3, 1);

        // El XML resultante debe poder parsearse sin excepción: si el '&' no se
        // escapó, XmlDocument.LoadXml revienta con XmlException acá mismo.
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        Assert.Contains("20123456786 &amp; Cía &lt;SA&gt;", xml);
    }
}
