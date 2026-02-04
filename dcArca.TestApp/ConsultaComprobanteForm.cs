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
        this.SuspendLayout();

        this.Text = "Consultar Comprobante";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(700, 600);
        this.MinimumSize = new System.Drawing.Size(500, 450);
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;

        // Layout principal
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(8)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // title
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // tipo
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // numero + btn
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // estado
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // resultado label
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // resultado textbox

        // Label título (spans both cols)
        var lblTitle = new Label
        {
            Text = "Consulta de Comprobante (FECompConsultar)",
            Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        mainLayout.Controls.Add(lblTitle, 0, 0);
        mainLayout.SetColumnSpan(lblTitle, 2);

        // Tipo de Comprobante
        var lblTipo = new Label { Text = "Tipo:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, AutoSize = true };
        cboTipoComprobante = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        CargarTiposComprobante();
        mainLayout.Controls.Add(lblTipo, 0, 1);
        mainLayout.Controls.Add(cboTipoComprobante, 1, 1);

        // Número de Comprobante + botón en un panel flexible
        var lblNumero = new Label { Text = "Número:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, AutoSize = true };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
        txtNumeroComprobante = new TextBox { Width = 200, Margin = new Padding(0, 3, 6, 3) };
        btnConsultar = new Button { Text = "🔍 Consultar", AutoSize = true, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(0, 0, 0, 0) };
        btnConsultar.Click += BtnConsultar_Click;
        flow.Controls.Add(txtNumeroComprobante);
        flow.Controls.Add(btnConsultar);
        mainLayout.Controls.Add(lblNumero, 0, 2);
        mainLayout.Controls.Add(flow, 1, 2);

        // Label Estado (spans both cols)
        lblEstado = new Label { Text = "Listo para consultar", ForeColor = System.Drawing.Color.Blue, Dock = DockStyle.Fill, AutoSize = true };
        mainLayout.Controls.Add(lblEstado, 0, 3);
        mainLayout.SetColumnSpan(lblEstado, 2);

        // Resultado label
        var lblResultado = new Label { Text = "Resultado:", Dock = DockStyle.Fill, AutoSize = true };
        mainLayout.Controls.Add(lblResultado, 0, 4);
        mainLayout.SetColumnSpan(lblResultado, 2);

        // Resultado textbox que ocupa el resto y se adapta
        txtResultado = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            Font = new System.Drawing.Font("Courier New", 9),
            Dock = DockStyle.Fill
        };
        mainLayout.Controls.Add(txtResultado, 0, 5);
        mainLayout.SetColumnSpan(txtResultado, 2);

        this.Controls.Add(mainLayout);

        this.ResumeLayout(false);
        this.PerformLayout();
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
