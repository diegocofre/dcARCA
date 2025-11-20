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
        this.Size = new System.Drawing.Size(400, 300);

        // Crear controles
        var lblTitle = new Label
        {
            Text = "dcARCA - Test de Servicios",
            Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(360, 40)
        };

        var lblSubtitle = new Label
        {
            Text = "Seleccione el servicio a probar:",
            Font = new System.Drawing.Font("Arial", 10),
            Location = new System.Drawing.Point(20, 70),
            Size = new System.Drawing.Size(360, 20)
        };

        var btnFacturacion = new Button
        {
            Text = "📄 Facturación Electrónica (WSFE)",
            Font = new System.Drawing.Font("Arial", 11),
            Location = new System.Drawing.Point(50, 110),
            Size = new System.Drawing.Size(300, 50),
            FlatStyle = FlatStyle.Flat
        };
        btnFacturacion.Click += BtnFacturacion_Click;

        var btnConsultaCuit = new Button
        {
            Text = "🔍 Consulta de CUIT (Padron)",
            Font = new System.Drawing.Font("Arial", 11),
            Location = new System.Drawing.Point(50, 170),
            Size = new System.Drawing.Size(300, 50),
            FlatStyle = FlatStyle.Flat
        };
        btnConsultaCuit.Click += BtnConsultaCuit_Click;

        var lblVersion = new Label
        {
            Text = "Versión 1.0 - Homologación",
            Font = new System.Drawing.Font("Arial", 8),
            ForeColor = System.Drawing.Color.Gray,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Location = new System.Drawing.Point(20, 240),
            Size = new System.Drawing.Size(360, 20)
        };

        // Agregar controles al formulario
        this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnFacturacion, btnConsultaCuit, lblVersion });
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
}