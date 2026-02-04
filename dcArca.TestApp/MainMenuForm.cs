/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */


namespace dcArca.TestApp;

public partial class MainMenuForm : Form
{
    public MainMenuForm()
    {
        this.Text = "dcARCA - Menú Principal";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(420, 400);

        var tableLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        this.Controls.Add(tableLayout);

        // RowStyles
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Title
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Subtitle
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F)); // Button 1
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F)); // Button 2
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F)); // Button 3
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Version

        // Controls
        var lblTitle = new Label
        {
            Text = "Test de Servicios",
            Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        var lblSubtitle = new Label
        {
            Text = "Seleccione el servicio a probar:",
            Font = new System.Drawing.Font("Segoe UI", 10F),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        };

        var btnFacturacion = new Button
        {
            Text = "📄 Facturación Electrónica (WSFE)",
            Font = new System.Drawing.Font("Segoe UI", 11F),
            Dock = DockStyle.Fill,
            Margin = new Padding(20, 8, 20, 8)
        };
        btnFacturacion.Click += BtnFacturacion_Click;

        var btnConsultaCuit = new Button
        {
            Text = "🔍 Consulta de CUIT (Padron)",
            Font = new System.Drawing.Font("Segoe UI", 11F),
            Dock = DockStyle.Fill,
            Margin = new Padding(20, 8, 20, 8)
        };
        btnConsultaCuit.Click += BtnConsultaCuit_Click;

        var btnConsultaComprobante = new Button
        {
            Text = "📋 Consultar Comprobante",
            Font = new System.Drawing.Font("Segoe UI", 11F),
            Dock = DockStyle.Fill,
            Margin = new Padding(20, 8, 20, 8)
        };
        btnConsultaComprobante.Click += BtnConsultaComprobante_Click;

        var lblVersion = new Label
        {
            Text = "Versión 1.0",
            Font = new System.Drawing.Font("Segoe UI", 8F),
            ForeColor = System.Drawing.Color.Gray,
            TextAlign = System.Drawing.ContentAlignment.BottomCenter,
            Dock = DockStyle.Fill
        };

        tableLayout.Controls.Add(lblTitle, 0, 0);
        tableLayout.Controls.Add(lblSubtitle, 0, 1);
        tableLayout.Controls.Add(btnFacturacion, 0, 2);
        tableLayout.Controls.Add(btnConsultaCuit, 0, 3);
        tableLayout.Controls.Add(btnConsultaComprobante, 0, 4);
        tableLayout.Controls.Add(lblVersion, 0, 5);
    }

    private void BtnFacturacion_Click(object? sender, EventArgs e)
    {
        var formFacturacion = new Form1();
        formFacturacion.Show();
    }

    private void BtnConsultaCuit_Click(object? sender, EventArgs e)
    {
        var formConsultaCuit = new ConsultaCuitForm();
        formConsultaCuit.Show();
    }

    private void InitializeComponent()
    {

    }

    private void BtnConsultaComprobante_Click(object? sender, EventArgs e)
    {
        var formConsultaComprobante = new ConsultaComprobanteForm();
        formConsultaComprobante.Show();
    }
}
