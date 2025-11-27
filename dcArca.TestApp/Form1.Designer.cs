/*
 * Copyright (c) 2025 Diego Cofré Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

namespace dcArca.TestApp;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblCuitReceptor = new System.Windows.Forms.Label();
            this.txtCuitReceptor = new System.Windows.Forms.TextBox();
            this.lblConcepto = new System.Windows.Forms.Label();
            this.cmbConcepto = new System.Windows.Forms.ComboBox();
            this.lblTipoComprobante = new System.Windows.Forms.Label();
            this.cmbTipoComprobante = new System.Windows.Forms.ComboBox();
            this.lblNumeroComprobante = new System.Windows.Forms.Label();
            this.txtNumeroComprobante = new System.Windows.Forms.TextBox();
            this.btnProximo = new System.Windows.Forms.Button();
            this.lblCbteAsociado = new System.Windows.Forms.Label();
            this.txtCbteAsociado = new System.Windows.Forms.TextBox();
            this.lblCondicionIVA = new System.Windows.Forms.Label();
            this.cmbCondicionIVA = new System.Windows.Forms.ComboBox();
            this.lblFechaServicioDesde = new System.Windows.Forms.Label();
            this.dtpFechaServicioDesde = new System.Windows.Forms.DateTimePicker();
            this.lblFechaServicioHasta = new System.Windows.Forms.Label();
            this.dtpFechaServicioHasta = new System.Windows.Forms.DateTimePicker();
            this.lblFechaVencimiento = new System.Windows.Forms.Label();
            this.dtpFechaVencimiento = new System.Windows.Forms.DateTimePicker();
            this.lblImporteNeto = new System.Windows.Forms.Label();
            this.txtImporteNeto = new System.Windows.Forms.TextBox();
            this.lblIva = new System.Windows.Forms.Label();
            this.txtIva = new System.Windows.Forms.TextBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.btnCalcularTotal = new System.Windows.Forms.Button();
            this.btnAutorizar = new System.Windows.Forms.Button();
            this.lblResultado = new System.Windows.Forms.Label();
            this.txtResultado = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 23);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(351, 25);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "dcARCA - Facturación Electrónica ARCA";
            // 
            // lblConcepto / cmbConcepto
            // 
            this.lblConcepto.AutoSize = true;
            this.lblConcepto.Location = new System.Drawing.Point(20, 65);
            this.lblConcepto.Name = "lblConcepto";
            this.lblConcepto.Size = new System.Drawing.Size(70, 17);
            this.lblConcepto.Text = "Concepto:";
            this.cmbConcepto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConcepto.Location = new System.Drawing.Point(20, 85);
            this.cmbConcepto.Name = "cmbConcepto";
            this.cmbConcepto.Size = new System.Drawing.Size(360, 25);
            this.cmbConcepto.TabIndex = 0;
            this.cmbConcepto.SelectedIndexChanged += new System.EventHandler(this.cmbConcepto_SelectedIndexChanged);
            // 
            // lblCuitReceptor / txtCuitReceptor
            // 
            this.lblCuitReceptor.AutoSize = true;
            this.lblCuitReceptor.Location = new System.Drawing.Point(420, 65);
            this.lblCuitReceptor.Name = "lblCuitReceptor";
            this.lblCuitReceptor.Size = new System.Drawing.Size(95, 17);
            this.lblCuitReceptor.Text = "CUIT Receptor:";
            this.txtCuitReceptor.Location = new System.Drawing.Point(420, 85);
            this.txtCuitReceptor.Name = "txtCuitReceptor";
            this.txtCuitReceptor.PlaceholderText = "30716832151";
            this.txtCuitReceptor.Size = new System.Drawing.Size(360, 25);
            this.txtCuitReceptor.TabIndex = 1;
            this.txtCuitReceptor.TextChanged += new System.EventHandler(this.txtCuitReceptor_TextChanged);
            // 
            // lblTipoComprobante / cmbTipoComprobante
            // 
            this.lblTipoComprobante.AutoSize = true;
            this.lblTipoComprobante.Location = new System.Drawing.Point(20, 125);
            this.lblTipoComprobante.Name = "lblTipoComprobante";
            this.lblTipoComprobante.Size = new System.Drawing.Size(118, 17);
            this.lblTipoComprobante.Text = "Tipo de comprobante:";
            this.cmbTipoComprobante.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTipoComprobante.Location = new System.Drawing.Point(20, 145);
            this.cmbTipoComprobante.Name = "cmbTipoComprobante";
            this.cmbTipoComprobante.Size = new System.Drawing.Size(360, 25);
            this.cmbTipoComprobante.TabIndex = 2;
            this.cmbTipoComprobante.SelectedIndexChanged += new System.EventHandler(this.cmbTipoComprobante_SelectedIndexChanged);
            // 
            // lblNumeroComprobante / txtNumeroComprobante / btnProximo
            // 
            this.lblNumeroComprobante.AutoSize = true;
            this.lblNumeroComprobante.Location = new System.Drawing.Point(20, 185);
            this.lblNumeroComprobante.Name = "lblNumeroComprobante";
            this.lblNumeroComprobante.Size = new System.Drawing.Size(132, 17);
            this.lblNumeroComprobante.Text = "Número de comprobante:";
            this.txtNumeroComprobante.Location = new System.Drawing.Point(20, 205);
            this.txtNumeroComprobante.Name = "txtNumeroComprobante";
            this.txtNumeroComprobante.PlaceholderText = "1234";
            this.txtNumeroComprobante.Size = new System.Drawing.Size(200, 25);
            this.txtNumeroComprobante.TabIndex = 4;
            this.btnProximo.Location = new System.Drawing.Point(230, 205);
            this.btnProximo.Name = "btnProximo";
            this.btnProximo.Size = new System.Drawing.Size(150, 25);
            this.btnProximo.TabIndex = 5;
            this.btnProximo.Text = "Próximo";
            this.btnProximo.UseVisualStyleBackColor = true;
            this.btnProximo.Click += new System.EventHandler(this.btnProximo_Click);
            // 
            // lblCbteAsociado / txtCbteAsociado (hidden initially)
            // 
            this.lblCbteAsociado.AutoSize = true;
            this.lblCbteAsociado.Location = new System.Drawing.Point(20, 235);
            this.lblCbteAsociado.Name = "lblCbteAsociado";
            this.lblCbteAsociado.Size = new System.Drawing.Size(177, 17);
            this.lblCbteAsociado.Text = "Nro comprobante asociado:";
            this.lblCbteAsociado.Visible = false;
            this.txtCbteAsociado.Location = new System.Drawing.Point(20, 255);
            this.txtCbteAsociado.Name = "txtCbteAsociado";
            this.txtCbteAsociado.PlaceholderText = "Nro factura base";
            this.txtCbteAsociado.Size = new System.Drawing.Size(200, 25);
            this.txtCbteAsociado.TabIndex = 6;
            this.txtCbteAsociado.Visible = false;
            // 
            // lblCondicionIVA / cmbCondicionIVA
            // 
            this.lblCondicionIVA.AutoSize = true;
            this.lblCondicionIVA.Location = new System.Drawing.Point(420, 125);
            this.lblCondicionIVA.Name = "lblCondicionIVA";
            this.lblCondicionIVA.Size = new System.Drawing.Size(119, 17);
            this.lblCondicionIVA.Text = "Condición IVA (RG5616):";
            this.cmbCondicionIVA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCondicionIVA.Location = new System.Drawing.Point(420, 145);
            this.cmbCondicionIVA.Name = "cmbCondicionIVA";
            this.cmbCondicionIVA.Size = new System.Drawing.Size(360, 25);
            this.cmbCondicionIVA.TabIndex = 3;
            // 
            // Servicio Fechas
            // 
            this.lblFechaServicioDesde.AutoSize = true;
            this.lblFechaServicioDesde.Enabled = false;
            this.lblFechaServicioDesde.Location = new System.Drawing.Point(420, 185);
            this.lblFechaServicioDesde.Name = "lblFechaServicioDesde";
            this.lblFechaServicioDesde.Size = new System.Drawing.Size(131, 17);
            this.lblFechaServicioDesde.Text = "Fecha Servicio Desde:";
            this.dtpFechaServicioDesde.Enabled = false;
            this.dtpFechaServicioDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaServicioDesde.Location = new System.Drawing.Point(420, 205);
            this.dtpFechaServicioDesde.Name = "dtpFechaServicioDesde";
            this.dtpFechaServicioDesde.Size = new System.Drawing.Size(360, 25);
            this.dtpFechaServicioDesde.TabIndex = 6;
            this.lblFechaServicioHasta.AutoSize = true;
            this.lblFechaServicioHasta.Enabled = false;
            this.lblFechaServicioHasta.Location = new System.Drawing.Point(420, 245);
            this.lblFechaServicioHasta.Name = "lblFechaServicioHasta";
            this.lblFechaServicioHasta.Size = new System.Drawing.Size(126, 17);
            this.lblFechaServicioHasta.Text = "Fecha Servicio Hasta:";
            this.dtpFechaServicioHasta.Enabled = false;
            this.dtpFechaServicioHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaServicioHasta.Location = new System.Drawing.Point(420, 265);
            this.dtpFechaServicioHasta.Name = "dtpFechaServicioHasta";
            this.dtpFechaServicioHasta.Size = new System.Drawing.Size(360, 25);
            this.dtpFechaServicioHasta.TabIndex = 7;
            this.lblFechaVencimiento.AutoSize = true;
            this.lblFechaVencimiento.Enabled = false;
            this.lblFechaVencimiento.Location = new System.Drawing.Point(420, 305);
            this.lblFechaVencimiento.Name = "lblFechaVencimiento";
            this.lblFechaVencimiento.Size = new System.Drawing.Size(123, 17);
            this.lblFechaVencimiento.Text = "Fecha Vencimiento:";
            this.dtpFechaVencimiento.Enabled = false;
            this.dtpFechaVencimiento.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaVencimiento.Location = new System.Drawing.Point(420, 325);
            this.dtpFechaVencimiento.Name = "dtpFechaVencimiento";
            this.dtpFechaVencimiento.Size = new System.Drawing.Size(360, 25);
            this.dtpFechaVencimiento.TabIndex = 8;
            // 
            // Importes
            // 
            this.lblImporteNeto.AutoSize = true;
            this.lblImporteNeto.Location = new System.Drawing.Point(20, 275);
            this.lblImporteNeto.Name = "lblImporteNeto";
            this.lblImporteNeto.Size = new System.Drawing.Size(90, 17);
            this.lblImporteNeto.Text = "Importe Neto:";
            this.txtImporteNeto.Location = new System.Drawing.Point(20, 295);
            this.txtImporteNeto.Name = "txtImporteNeto";
            this.txtImporteNeto.PlaceholderText = "1000.00";
            this.txtImporteNeto.Size = new System.Drawing.Size(360, 25);
            this.txtImporteNeto.TabIndex = 9;
            this.txtImporteNeto.TextChanged += new System.EventHandler(this.TxtImporteNeto_TextChanged);
            this.lblIva.AutoSize = true;
            this.lblIva.Location = new System.Drawing.Point(20, 335);
            this.lblIva.Name = "lblIva";
            this.lblIva.Size = new System.Drawing.Size(66, 17);
            this.lblIva.Text = "IVA (21%):";
            this.txtIva.Location = new System.Drawing.Point(20, 355);
            this.txtIva.Name = "txtIva";
            this.txtIva.ReadOnly = true;
            this.txtIva.Size = new System.Drawing.Size(360, 25);
            this.txtIva.TabIndex = 10;
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(20, 395);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(39, 17);
            this.lblTotal.Text = "Total:";
            this.txtTotal.Location = new System.Drawing.Point(20, 415);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.ReadOnly = true;
            this.txtTotal.Size = new System.Drawing.Size(360, 25);
            this.txtTotal.TabIndex = 11;
            this.btnCalcularTotal.Location = new System.Drawing.Point(20, 455);
            this.btnCalcularTotal.Name = "btnCalcularTotal";
            this.btnCalcularTotal.Size = new System.Drawing.Size(360, 35);
            this.btnCalcularTotal.TabIndex = 12;
            this.btnCalcularTotal.Text = "Calcular IVA y Total";
            this.btnCalcularTotal.UseVisualStyleBackColor = true;
            this.btnCalcularTotal.Click += new System.EventHandler(this.BtnCalcularTotal_Click);
            // 
            // btnAutorizar
            // 
            this.btnAutorizar.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnAutorizar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAutorizar.ForeColor = System.Drawing.Color.White;
            this.btnAutorizar.Location = new System.Drawing.Point(20, 500);
            this.btnAutorizar.Name = "btnAutorizar";
            this.btnAutorizar.Size = new System.Drawing.Size(760, 45);
            this.btnAutorizar.TabIndex = 13;
            this.btnAutorizar.Text = "Autorizar Factura (Solicitar CAE)";
            this.btnAutorizar.UseVisualStyleBackColor = false;
            this.btnAutorizar.Click += new System.EventHandler(this.BtnAutorizar_Click);
            // 
            // lblResultado / txtResultado
            // 
            this.lblResultado.AutoSize = true;
            this.lblResultado.Location = new System.Drawing.Point(20, 555);
            this.lblResultado.Name = "lblResultado";
            this.lblResultado.Size = new System.Drawing.Size(69, 17);
            this.lblResultado.Text = "Resultado:";
            this.txtResultado.Location = new System.Drawing.Point(20, 575);
            this.txtResultado.Multiline = true;
            this.txtResultado.Name = "txtResultado";
            this.txtResultado.ReadOnly = true;
            this.txtResultado.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResultado.Size = new System.Drawing.Size(760, 120);
            this.txtResultado.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 720);
            this.Controls.Add(this.txtNumeroComprobante);
            this.Controls.Add(this.btnProximo);
            this.Controls.Add(this.lblNumeroComprobante);
            this.Controls.Add(this.txtCbteAsociado);
            this.Controls.Add(this.lblCbteAsociado);
            this.Controls.Add(this.cmbTipoComprobante);
            this.Controls.Add(this.lblTipoComprobante);
            this.Controls.Add(this.txtCuitReceptor);
            this.Controls.Add(this.lblCuitReceptor);
            this.Controls.Add(this.cmbConcepto);
            this.Controls.Add(this.lblConcepto);
            this.Controls.Add(this.dtpFechaVencimiento);
            this.Controls.Add(this.lblFechaVencimiento);
            this.Controls.Add(this.dtpFechaServicioHasta);
            this.Controls.Add(this.lblFechaServicioHasta);
            this.Controls.Add(this.dtpFechaServicioDesde);
            this.Controls.Add(this.lblFechaServicioDesde);
            this.Controls.Add(this.cmbCondicionIVA);
            this.Controls.Add(this.lblCondicionIVA);
            this.Controls.Add(this.txtResultado);
            this.Controls.Add(this.lblResultado);
            this.Controls.Add(this.btnAutorizar);
            this.Controls.Add(this.btnCalcularTotal);
            this.Controls.Add(this.txtTotal);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.txtIva);
            this.Controls.Add(this.lblIva);
            this.Controls.Add(this.txtImporteNeto);
            this.Controls.Add(this.lblImporteNeto);
            this.Controls.Add(this.lblTitulo);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "dcARCA - Test Facturación";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    private System.Windows.Forms.Label lblTitulo;
    private System.Windows.Forms.Label lblCuitReceptor;
    private System.Windows.Forms.TextBox txtCuitReceptor;
    private System.Windows.Forms.Label lblTipoComprobante;
    private System.Windows.Forms.ComboBox cmbTipoComprobante;
    private System.Windows.Forms.Label lblNumeroComprobante;
    private System.Windows.Forms.TextBox txtNumeroComprobante;
    private System.Windows.Forms.Label lblCondicionIVA;
    private System.Windows.Forms.ComboBox cmbCondicionIVA;
    private System.Windows.Forms.Label lblConcepto;
    private System.Windows.Forms.ComboBox cmbConcepto;
    private System.Windows.Forms.Label lblFechaServicioDesde;
    private System.Windows.Forms.DateTimePicker dtpFechaServicioDesde;
    private System.Windows.Forms.Label lblFechaServicioHasta;
    private System.Windows.Forms.DateTimePicker dtpFechaServicioHasta;
    private System.Windows.Forms.Label lblFechaVencimiento;
    private System.Windows.Forms.DateTimePicker dtpFechaVencimiento;
    private System.Windows.Forms.Label lblImporteNeto;
    private System.Windows.Forms.TextBox txtImporteNeto;
    private System.Windows.Forms.Label lblIva;
    private System.Windows.Forms.TextBox txtIva;
    private System.Windows.Forms.Label lblTotal;
    private System.Windows.Forms.TextBox txtTotal;
    private System.Windows.Forms.Button btnAutorizar;
    private System.Windows.Forms.Label lblResultado;
    private System.Windows.Forms.TextBox txtResultado;
    private System.Windows.Forms.Button btnCalcularTotal;
    private System.Windows.Forms.Button btnProximo;
    private System.Windows.Forms.Label lblCbteAsociado;
    private System.Windows.Forms.TextBox txtCbteAsociado;
}
