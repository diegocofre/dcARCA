/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dcArca.Core;
using dcArca.Core.Models;
using dcArca.Core.Services;

namespace dcArca.TestApp;

public partial class ConsultaCuitForm : Form
{
    private dcArcaConfig? _config;
    private dcPadronClient? _padronClient;
    private TextBox? _txtCuit;
    private TextBox? _txtResultado;
    private Button? _btnConsultar;

    public ConsultaCuitForm()
    {
        this.Text = "dcARCA - Consulta de CUIT";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(700, 600);
        this.MinimumSize = new System.Drawing.Size(550, 400);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);

        CargarConfiguracion();
        InicializarControles();
    }

    private void InicializarControles()
    {
        var mainLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 3,
            Dock = DockStyle.Fill,
            Padding = new Padding(16)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Input
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Result
        this.Controls.Add(mainLayout);

        // --- Title ---
        var lblTitle = new Label
        {
            Text = "Consulta de Información de CUIT",
            Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        mainLayout.Controls.Add(lblTitle, 0, 0);

        // --- Input Panel ---
        var inputPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 0)
        };

        var lblCuit = new Label
        {
            Text = "CUIT:",
            AutoSize = true,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 8, 4, 0)
        };

        _txtCuit = new TextBox
        {
            Width = 140,
            MaxLength = 13,
            Margin = new Padding(0, 5, 8, 0)
        };
        _txtCuit.TextChanged += TxtCuit_TextChanged;

        _btnConsultar = new Button
        {
            Text = "🔍 Consultar",
            AutoSize = true,
            Enabled = false,
            Margin = new Padding(0, 3, 0, 0)
        };
        _btnConsultar.Click += BtnConsultar_Click;

        inputPanel.Controls.Add(lblCuit);
        inputPanel.Controls.Add(_txtCuit);
        inputPanel.Controls.Add(_btnConsultar);
        mainLayout.Controls.Add(inputPanel, 0, 1);

        // --- Result TextBox ---
        _txtResultado = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new System.Drawing.Font("Consolas", 9.5F),
            Margin = new Padding(0, 10, 0, 0)
        };
        mainLayout.Controls.Add(_txtResultado, 0, 2);
    }

    private void CargarConfiguracion()
    {
        try
        {
            _config = dcConfigurationHelper.LoadFromJson("appsettings.json");
            InicializarClientes();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar configuración:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InicializarClientes()
    {
        if (_config != null)
        {
            _padronClient = new dcPadronClient(_config);
        }
    }

    private void TxtCuit_TextChanged(object? sender, EventArgs e)
    {
        var habilitar = _txtCuit != null
            && long.TryParse(_txtCuit.Text, out _)
            && _txtCuit.Text.Length == 11;

        if (_btnConsultar != null)
        {
            _btnConsultar.Enabled = habilitar;
        }
    }

    private async void BtnConsultar_Click(object? sender, EventArgs e)
    {
        if (_config == null || _padronClient == null || _txtCuit == null || _txtResultado == null || _btnConsultar == null)
        {
            MessageBox.Show("Configuración no cargada correctamente.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!long.TryParse(_txtCuit.Text, out long cuit) || _txtCuit.Text.Length != 11)
        {
            MessageBox.Show("Por favor, ingrese un CUIT válido (11 dígitos).",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnConsultar.Enabled = false;
        _btnConsultar.Text = "Consultando...";
        _txtResultado.Text = "Consultando información del CUIT en AFIP...\r\nPor favor, espere...";
        Application.DoEvents();

        try
        {
            var resultado = new StringBuilder();
            resultado.AppendLine($"=== CONSULTA DE CUIT: {_txtCuit?.Text ?? "N/D"} ===");
            resultado.AppendLine($"Fecha de consulta: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            resultado.AppendLine();

            // Consultar padrón (única fuente de verdad para la consulta de CUIT)
            resultado.AppendLine("🔍 Validando CUIT en padrón ARCA (ws_sr_padron_a5)...");
            var padronResult = await _padronClient.GetPersonaAsync(cuit);

            if (padronResult.Success && padronResult.Existe)
            {
                resultado.AppendLine("✓ El CUIT figura registrado en el padrón oficial.");

                if (!string.IsNullOrWhiteSpace(padronResult.EstadoClave))
                {
                    resultado.AppendLine($"  • Estado de la clave fiscal: {padronResult.EstadoClave}");
                    resultado.AppendLine(padronResult.EstaActivo
                        ? "    ↳ El estado AC indica CUIT ACTIVO."
                        : "    ↳ Estado distinto de AC, revisar situación antes de facturar.");
                }

                if (!string.IsNullOrWhiteSpace(padronResult.RazonSocial))
                {
                    resultado.AppendLine($"  • Razón social: {padronResult.RazonSocial}");
                }
                else if (!string.IsNullOrWhiteSpace(padronResult.Nombre))
                {
                    resultado.AppendLine($"  • Nombre y apellido: {padronResult.Nombre} {padronResult.Apellido}".Trim());
                }

                if (!string.IsNullOrWhiteSpace(padronResult.TipoPersona))
                    resultado.AppendLine($"  • Tipo de persona: {padronResult.TipoPersona}");

                if (!string.IsNullOrWhiteSpace(padronResult.TipoClave))
                    resultado.AppendLine($"  • Tipo de clave: {padronResult.TipoClave}");

                if (!string.IsNullOrWhiteSpace(padronResult.NumeroDocumento))
                    resultado.AppendLine($"  • Nº documento: {padronResult.NumeroDocumento}");

                if (padronResult.Caracterizaciones.Count > 0)
                {
                    resultado.AppendLine("  • Caracterizaciones registradas:");
                    foreach (var car in padronResult.Caracterizaciones)
                        resultado.AppendLine($"     - {car}");
                }

                if (padronResult.Actividades.Count > 0)
                {
                    resultado.AppendLine("  • Actividades declaradas:");
                    foreach (var act in padronResult.Actividades)
                        resultado.AppendLine($"     - {act}");
                }

                if (padronResult.Regimenes.Count > 0)
                {
                    resultado.AppendLine("  • Regímenes registrados:");
                    foreach (var reg in padronResult.Regimenes)
                        resultado.AppendLine($"     - {reg}");
                }
            }
            else
            {
                resultado.AppendLine("✗ No se pudo validar el CUIT contra el padrón.");
                if (!string.IsNullOrWhiteSpace(padronResult.ErrorCodigo))
                {
                    resultado.AppendLine($"  Código: {padronResult.ErrorCodigo} - {padronResult.ErrorDescripcion ?? padronResult.Mensaje}");
                }
                else if (!string.IsNullOrWhiteSpace(padronResult.Mensaje))
                {
                    resultado.AppendLine($"  Detalle: {padronResult.Mensaje}");
                }
            }

            resultado.AppendLine();
            resultado.AppendLine("ℹ️ Fuente: Servicio ws_sr_padron_a5 (Homologación)");

            _txtResultado!.Text = resultado.ToString();
        }
        catch (Exception ex)
        {
            if (_txtResultado != null)
                _txtResultado.Text = $"✗ ERROR AL CONSULTAR CUIT\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}";
            MessageBox.Show($"Error al consultar CUIT:\n\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (_btnConsultar != null)
            {
                _btnConsultar.Enabled = true;
                _btnConsultar.Text = "🔍 Consultar";
            }
        }
    }
}
