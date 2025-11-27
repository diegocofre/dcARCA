/*
 * Copyright (c) 2025 Diego Cofré Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using System.Globalization;
using System.Linq;
using dcArca.Core;
using dcArca.Core.Models;
using dcArca.Core.Services;

namespace dcArca.TestApp;

public partial class ConsultaComprobanteForm : Form
{
    private ComboBox cboTipoComprobante = null!;
    private TextBox txtNumeroComprobante = null!;
    private Button btnConsultar = null!;
    private TextBox txtResultado = null!;
    private Label lblEstado = null!;

    public ConsultaComprobanteForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Consultar Comprobante";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Size = new System.Drawing.Size(700, 600);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        // Label título
        var lblTitle = new Label
        {
            Text = "Consulta de Comprobante (FECompConsultar)",
            Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(650, 30)
        };

        // Tipo de Comprobante
        var lblTipo = new Label
        {
            Text = "Tipo de Comprobante:",
            Location = new System.Drawing.Point(20, 70),
            Size = new System.Drawing.Size(150, 20)
        };

        cboTipoComprobante = new ComboBox
        {
            Location = new System.Drawing.Point(180, 68),
            Size = new System.Drawing.Size(480, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        CargarTiposComprobante();

        // Número de Comprobante
        var lblNumero = new Label
        {
            Text = "Número de Comprobante:",
            Location = new System.Drawing.Point(20, 110),
            Size = new System.Drawing.Size(150, 20)
        };

        txtNumeroComprobante = new TextBox
        {
            Location = new System.Drawing.Point(180, 108),
            Size = new System.Drawing.Size(200, 25)
        };

        // Botón Consultar
        btnConsultar = new Button
        {
            Text = "🔍 Consultar",
            Location = new System.Drawing.Point(180, 150),
            Size = new System.Drawing.Size(150, 35),
            Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
        };
        btnConsultar.Click += BtnConsultar_Click;

        // Label Estado
        lblEstado = new Label
        {
            Text = "Listo para consultar",
            Location = new System.Drawing.Point(20, 200),
            Size = new System.Drawing.Size(650, 20),
            ForeColor = System.Drawing.Color.Blue
        };

        // TextBox Resultado
        var lblResultado = new Label
        {
            Text = "Resultado:",
            Location = new System.Drawing.Point(20, 230),
            Size = new System.Drawing.Size(100, 20)
        };

        txtResultado = new TextBox
        {
            Location = new System.Drawing.Point(20, 255),
            Size = new System.Drawing.Size(640, 280),
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            Font = new System.Drawing.Font("Courier New", 9)
        };

        // Agregar controles
        this.Controls.AddRange(new Control[]
        {
            lblTitle, lblTipo, cboTipoComprobante,
            lblNumero, txtNumeroComprobante,
            btnConsultar, lblEstado, lblResultado, txtResultado
        });
    }

    private void CargarTiposComprobante()
    {
        var tipos = Enum.GetValues<dcTipoComprobante>()
            .Select(tipo => new { Texto = tipo.ToDisplayString(), Valor = tipo })
            .OrderBy(item => (int)item.Valor)
            .ToList();

        cboTipoComprobante.DisplayMember = "Texto";
        cboTipoComprobante.ValueMember = "Valor";
        cboTipoComprobante.DataSource = tipos;
    }

    private async void BtnConsultar_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtNumeroComprobante.Text))
            {
                MessageBox.Show("Debe ingresar el número de comprobante.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!long.TryParse(txtNumeroComprobante.Text, out long numeroComprobante) || numeroComprobante <= 0)
            {
                MessageBox.Show("El número de comprobante debe ser un valor numérico positivo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tipoComprobante = (dcTipoComprobante)cboTipoComprobante.SelectedValue!;

            btnConsultar.Enabled = false;
            lblEstado.Text = "Consultando comprobante...";
            lblEstado.ForeColor = System.Drawing.Color.Blue;
            txtResultado.Text = "";
            Application.DoEvents();

            var config = dcConfigurationHelper.LoadFromJson("appsettings.json");
            using var client = new dcWsfeClient(config);

            var resultado = await client.FECompConsultarAsync(numeroComprobante, tipoComprobante);

            if (resultado.Success)
            {
                lblEstado.Text = "✓ Comprobante encontrado";
                lblEstado.ForeColor = System.Drawing.Color.Green;
                
                txtResultado.Text = FormatearResultado(resultado);
            }
            else
            {
                lblEstado.Text = "✗ Error en la consulta";
                lblEstado.ForeColor = System.Drawing.Color.Red;
                
                txtResultado.Text = $"ERROR: {resultado.Mensaje}\r\n\r\n";
                txtResultado.Text += $"Código: {resultado.Codigo}\r\n\r\n";
                
                if (resultado.Errores.Count > 0)
                {
                    txtResultado.Text += "Errores:\r\n";
                    foreach (var error in resultado.Errores)
                    {
                        txtResultado.Text += $"  - {error}\r\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lblEstado.Text = "✗ Error inesperado";
            lblEstado.ForeColor = System.Drawing.Color.Red;
            txtResultado.Text = $"EXCEPCIÓN: {ex.Message}\r\n\r\n{ex.StackTrace}";
            MessageBox.Show($"Error al consultar comprobante:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnConsultar.Enabled = true;
        }
    }

    private string FormatearResultado(dcFacturaResponse resultado)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine("              DATOS DEL COMPROBANTE");
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine();
        
        sb.AppendLine($"✓ Estado: {(resultado.Success ? "AUTORIZADO" : "RECHAZADO")}");
        sb.AppendLine($"Número de Comprobante: {resultado.NumeroComprobante}");
        sb.AppendLine($"Tipo: {Describe(resultado.TipoComprobante)} | Punto de Venta: {resultado.PuntoVenta?.ToString() ?? "-"}");
        sb.AppendLine($"Desde: {resultado.CbteDesde?.ToString() ?? "-"} | Hasta: {resultado.CbteHasta?.ToString() ?? "-"}");
        sb.AppendLine($"Concepto: {Describe(resultado.Concepto)}");
        sb.AppendLine($"Fecha Comprobante: {FormatearFechaAfip(resultado.FechaComprobante)}");
        if (!string.IsNullOrWhiteSpace(resultado.FechaServicioDesde) || !string.IsNullOrWhiteSpace(resultado.FechaServicioHasta))
        {
            sb.AppendLine($"Periodo Servicio: {FormatearFechaAfip(resultado.FechaServicioDesde)} al {FormatearFechaAfip(resultado.FechaServicioHasta)}");
        }
        if (!string.IsNullOrWhiteSpace(resultado.FechaVencimientoPago))
        {
            sb.AppendLine($"Vencimiento Pago: {FormatearFechaAfip(resultado.FechaVencimientoPago)}");
        }
        sb.AppendLine($"Doc Tipo: {Describe(resultado.DocTipo)} | Doc Nro: {resultado.DocNro?.ToString() ?? "-"}");
        sb.AppendLine($"Condición IVA Receptor: {Describe(resultado.CondicionIvaReceptor)}");
        
        if (!string.IsNullOrEmpty(resultado.Cae))
        {
            sb.AppendLine($"CAE: {resultado.Cae}");
        }
        
        if (!string.IsNullOrEmpty(resultado.FechaVencimientoCae))
        {
            sb.AppendLine($"Fecha Vencimiento CAE: {resultado.FechaVencimientoCae}");
        }
        
        if (!string.IsNullOrEmpty(resultado.FechaProceso))
        {
            sb.AppendLine($"Fecha Proceso: {resultado.FechaProceso}");
        }
        
        sb.AppendLine();
        sb.AppendLine("───────────────────────────────────────────────────────");
        sb.AppendLine("IMPORTES");
        sb.AppendLine("───────────────────────────────────────────────────────");
        sb.AppendLine($"Importe Total: ${resultado.ImporteTotal:N2}");
        sb.AppendLine($"Importe Neto: ${resultado.ImporteNeto:N2}");
        sb.AppendLine($"Importe No Gravado: ${resultado.ImporteNoGravado:N2}");
        sb.AppendLine($"Importe Exento: ${resultado.ImporteExento:N2}");
        sb.AppendLine($"Importe Tributos: ${resultado.ImporteTributos:N2}");
        sb.AppendLine($"Importe IVA: ${resultado.ImporteIva:N2}");

        if (!string.IsNullOrWhiteSpace(resultado.MonedaId))
        {
            sb.AppendLine($"Moneda: {resultado.MonedaId} | Cotización: {resultado.MonedaCotizacion:N4}");
        }

        if (resultado.Iva.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────────────────");
            sb.AppendLine("IVA DETALLADO");
            sb.AppendLine("───────────────────────────────────────────────────────");
                foreach (var iva in resultado.Iva)
                {
                    sb.AppendLine($"  {Describe(iva.Alicuota)}: Base ${iva.BaseImponible:N2} → IVA ${iva.Importe:N2}");
                }
        }
        
        if (resultado.Observaciones.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────────────────");
            sb.AppendLine("OBSERVACIONES");
            sb.AppendLine("───────────────────────────────────────────────────────");
            foreach (var obs in resultado.Observaciones)
            {
                sb.AppendLine($"  ⚠ {obs}");
            }
        }
        
        if (resultado.Errores.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────────────────");
            sb.AppendLine("ERRORES");
            sb.AppendLine("───────────────────────────────────────────────────────");
            foreach (var error in resultado.Errores)
            {
                sb.AppendLine($"  ✗ {error}");
            }
        }
        
        if (!string.IsNullOrEmpty(resultado.Mensaje))
        {
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────────────────");
            sb.AppendLine("MENSAJE");
            sb.AppendLine("───────────────────────────────────────────────────────");
            sb.AppendLine(resultado.Mensaje);
        }
        
        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════");
        
        return sb.ToString();
    }

    private static string Describe(dcTipoComprobante? value) => value?.ToDisplayString() ?? "-";

    private static string Describe(dcConcepto? value) => value?.ToDisplayString() ?? "-";

    private static string Describe(dcTipoDocumento? value) => value?.ToDisplayString() ?? "-";

    private static string Describe(dcCondicionIvaReceptor? value) => value?.ToDisplayString() ?? "-";

    private static string Describe(dcAlicuotaIva? value) => value?.ToDisplayString() ?? "Sin detalle";

    private static string FormatearFechaAfip(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length != 8)
        {
            return string.IsNullOrWhiteSpace(valor) ? "-" : valor;
        }

        return DateTime.TryParseExact(valor, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
            ? fecha.ToString("dd/MM/yyyy")
            : valor;
    }
}
