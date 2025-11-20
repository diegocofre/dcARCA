/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using dcArca.Core;
using dcArca.Core.Models;
using dcArca.Core.Services;
using dcArca.Core.Services.Logging;
using dcArca.TestApp.Logging;
using Microsoft.Extensions.Logging;

namespace dcArca.TestApp;

public partial class Form1 : Form
{
    private readonly LogViewerForm _logViewer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IAfipLogger _afipLogger;
    private dcArcaConfig? _config;
    private dcWsfeClient? _wsfeClient;
    private bool _cargandoCondiciones;
    private long? _ultimoCuitConsultado;
    private bool _cargandoTipoComprobante;
    private readonly List<TipoComprobanteOption> _tiposComprobante = new()
    {
        new TipoComprobanteOption(1, "1 - Factura A"),
        new TipoComprobanteOption(2, "2 - Nota de Débito A"),
        new TipoComprobanteOption(3, "3 - Nota de Crédito A"),
        new TipoComprobanteOption(6, "6 - Factura B"),
        new TipoComprobanteOption(7, "7 - Nota de Débito B"),
        new TipoComprobanteOption(8, "8 - Nota de Crédito B"),
        new TipoComprobanteOption(11, "11 - Factura C"),
        new TipoComprobanteOption(12, "12 - Nota de Débito C"),
        new TipoComprobanteOption(13, "13 - Nota de Crédito C"),
        new TipoComprobanteOption(51, "51 - Factura M"),
        new TipoComprobanteOption(52, "52 - Nota de Débito M"),
        new TipoComprobanteOption(53, "53 - Nota de Crédito M")
    };

    private readonly List<ConceptoOption> _conceptos = new()
    {
        new ConceptoOption(dcConcepto.Productos, "1 - Productos"),
        new ConceptoOption(dcConcepto.Servicios, "2 - Servicios"),
        new ConceptoOption(dcConcepto.ProductosYServicios, "3 - Productos y Servicios")
    };

    public Form1()
    {
        _logViewer = new LogViewerForm();
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(new UiLoggerProvider(_logViewer.AppendLog, LogLevel.Information));
        });
        _logger = _loggerFactory.CreateLogger<Form1>();
        _afipLogger = new AfipLoggerAdapter(_loggerFactory.CreateLogger("dcWsfe"));

        InitializeComponent();

        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "arca.ico");
            if (File.Exists(iconPath))
            {
                using var fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read);
                this.Icon = new Icon(fs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al cargar el icono ARCA: {ex.Message}");
        }

        Load += (_, _) => PositionLogWindow();
        LocationChanged += (_, _) => PositionLogWindow();
        FormClosing += Form1_FormClosing;
        _logViewer.Show(this);

        InicializarComboTipoComprobante();
        InicializarComboCondicion();
        InicializarComboConcepto();
        ActualizarCamposServicio(forzarValores: true);
        CargarConfiguracion();
        _logger.LogInformation("Formulario de facturación listo para operar.");
    }

    private void InicializarComboTipoComprobante()
    {
        _cargandoTipoComprobante = true;
        cmbTipoComprobante.DisplayMember = nameof(TipoComprobanteOption.Descripcion);
        cmbTipoComprobante.ValueMember = nameof(TipoComprobanteOption.Codigo);
        cmbTipoComprobante.DataSource = _tiposComprobante.ToList();
        cmbTipoComprobante.SelectedIndex = -1;
        _cargandoTipoComprobante = false;
    }

    private void InicializarComboCondicion()
    {
        cmbCondicionIVA.DataSource = null;
        cmbCondicionIVA.Items.Clear();
        cmbCondicionIVA.Items.Add("Ingrese un CUIT válido para consultar la condición IVA");
        cmbCondicionIVA.SelectedIndex = 0;
        cmbCondicionIVA.Enabled = false;
    }

    private void InicializarComboConcepto()
    {
        cmbConcepto.DataSource = null;
        cmbConcepto.Items.Clear();
        cmbConcepto.DisplayMember = nameof(ConceptoOption.DisplayText);
        cmbConcepto.ValueMember = nameof(ConceptoOption.Value);
        cmbConcepto.DataSource = _conceptos.ToList();
        cmbConcepto.SelectedIndex = 0; // default Productos
    }

    private async void cmbConcepto_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbConcepto.SelectedItem is not ConceptoOption opcion)
            return;

        ActualizarCamposServicio();

        // Si cambia el concepto, reinicializamos opciones de condición y la consulta
        InicializarComboCondicion();
        _ultimoCuitConsultado = null;

        if (long.TryParse(txtCuitReceptor.Text, out long cuit) && txtCuitReceptor.Text.Length == 11)
        {
            await CargarCondicionesIvaAsync(cuit);
        }
    }

    private void CargarConfiguracion()
    {
        try
        {
            _config = dcConfigurationHelper.LoadFromJson("appsettings.json");
            RefrescarClienteWsfe();

            txtResultado.Text = $"✓ Configuración cargada correctamente.\r\n" +
                                $"CUIT Emisor: {_config?.Cuit}\r\n" +
                                $"Punto de Venta: {_config?.PuntoVenta}\r\n" +
                                $"Entorno: Homologación";
        }
        catch (Exception ex)
        {
            txtResultado.Text = $"✗ Error al cargar configuración:\r\n{ex.Message}\r\n\r\n" +
                                $"Por favor, configure appsettings.json correctamente.";
            MessageBox.Show($"Error al cargar configuración:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnProximo_Click(object sender, EventArgs e)
    {
        if (_config == null)
        {
            MessageBox.Show("La configuración no está disponible. Verifique appsettings.json.",
                "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cmbTipoComprobante.SelectedItem is not TipoComprobanteOption tipoSeleccionado)
        {
            MessageBox.Show("Seleccione el tipo de comprobante antes de consultar el próximo número.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbTipoComprobante.Focus();
            return;
        }

        try
        {
            btnProximo.Enabled = false;
            btnProximo.Text = "Consultando...";
            txtResultado.Text = "Consultando último comprobante autorizado...";

            if (_wsfeClient == null)
            {
                RefrescarClienteWsfe();
            }

            var wsfeClient = _wsfeClient ?? throw new InvalidOperationException("No se pudo inicializar el cliente WSFE.");

            var response = await wsfeClient.FECompUltimoAutorizadoAsync((dcTipoComprobante)tipoSeleccionado.Codigo);

            if (response.Success)
            {
                var siguiente = response.NumeroComprobante + 1;
                txtNumeroComprobante.Text = siguiente.ToString();
                txtResultado.Text =
                    $"Último comprobante autorizado para tipo {tipoSeleccionado.Codigo}: {response.NumeroComprobante}.\r\n";
            }
            else
            {
                txtResultado.Text =
                    $"✗ No se pudo consultar el último comprobante:\r\n{response.Mensaje}";
                MessageBox.Show($"No se pudo obtener el próximo comprobante.\n\n{response.Mensaje}",
                    "WSFE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            txtResultado.Text = $"✗ Error al consultar próximo comprobante:\r\n{ex.Message}";
            MessageBox.Show($"Ocurrió un error al consultar el próximo comprobante.\n\n{ex.Message}",
                "WSFE", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnProximo.Text = "Próximo";
            btnProximo.Enabled = true;
        }
    }

    private void TxtImporteNeto_TextChanged(object? sender, EventArgs e)
    {
        CalcularTotales();
    }

    private void BtnCalcularTotal_Click(object? sender, EventArgs e)
    {
        CalcularTotales();
    }

    private void CalcularTotales()
    {
        bool esClaseC = cmbTipoComprobante.SelectedItem is TipoComprobanteOption tipo && EsComprobanteClaseC(tipo.Codigo);

        if (decimal.TryParse(txtImporteNeto.Text, out decimal importeNeto))
        {
            decimal iva = esClaseC ? 0m : Math.Round(importeNeto * 0.21m, 2);
            decimal total = esClaseC ? importeNeto : importeNeto + iva;

            txtIva.Text = iva.ToString("F2");
            txtTotal.Text = total.ToString("F2");
        }
        else
        {
            txtIva.Text = "";
            txtTotal.Text = "";
        }
    }

    private async void BtnAutorizar_Click(object? sender, EventArgs e)
    {
        if (_config == null)
        {
            MessageBox.Show("La configuración no está cargada correctamente.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Validar campos
        if (!long.TryParse(txtCuitReceptor.Text, out long cuitReceptor) || cuitReceptor <= 0)
        {
            MessageBox.Show("Por favor, ingrese un CUIT receptor válido.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCuitReceptor.Focus();
            return;
        }

        if (!decimal.TryParse(txtImporteNeto.Text, out decimal importeNeto) || importeNeto <= 0)
        {
            MessageBox.Show("Por favor, ingrese un importe neto válido.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtImporteNeto.Focus();
            return;
        }

        if (cmbTipoComprobante.SelectedItem is not TipoComprobanteOption tipoSeleccionado)
        {
            MessageBox.Show("Por favor, seleccione el tipo de comprobante.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbTipoComprobante.Focus();
            return;
        }

        bool esClaseC = EsComprobanteClaseC(tipoSeleccionado.Codigo);

        if (!decimal.TryParse(txtIva.Text, out decimal iva) || (!esClaseC && iva <= 0) || (esClaseC && iva != 0))
        {
            var mensajeIva = esClaseC
                ? "Para comprobantes C, el IVA debe ser 0."
                : "Por favor, calcule el IVA antes de autorizar.";

            MessageBox.Show(mensajeIva,
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            btnCalcularTotal.Focus();
            return;
        }

        if (!decimal.TryParse(txtTotal.Text, out decimal total) || total <= 0)
        {
            MessageBox.Show("Por favor, calcule el total antes de autorizar.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            btnCalcularTotal.Focus();
            return;
        }

        if (esClaseC && Math.Abs(total - importeNeto) > 0.01m)
        {
            MessageBox.Show("En comprobantes C, el total debe coincidir con el importe neto.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtImporteNeto.Focus();
            return;
        }

        if (!long.TryParse(txtNumeroComprobante.Text, out long numeroComprobante) || numeroComprobante <= 0)
        {
            MessageBox.Show("Por favor, ingrese el número de comprobante a autorizar.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNumeroComprobante.Focus();
            return;
        }

        if (cmbConcepto.SelectedItem is not ConceptoOption conceptoSeleccionado)
        {
            MessageBox.Show("Por favor, seleccione el concepto de la factura (Productos / Servicios).",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbConcepto.Focus();
            return;
        }

        // Deshabilitar botón y mostrar progreso
        btnAutorizar.Enabled = false;
        btnAutorizar.Text = "Procesando...";
        txtResultado.Text = "Solicitando autorización a AFIP...\r\nPor favor, espere...";
        Application.DoEvents();

        try
        {
            // Crear cliente WSFEv1
            if (_wsfeClient == null)
            {
                RefrescarClienteWsfe();
            }

            var wsfeClient = _wsfeClient;
            if (wsfeClient == null)
                throw new InvalidOperationException("No se pudo inicializar el cliente WSFE.");

            if (cmbCondicionIVA.SelectedItem is not dcCondicionIvaOption condicionSeleccionada)
            {
                MessageBox.Show("Por favor, consulte y seleccione la condición IVA del receptor (RG 5616).",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Crear request de factura
            decimal totalParaEnviar = esClaseC ? importeNeto : total;
            decimal ivaParaEnviar = esClaseC ? 0m : iva;

            var facturaRequest = new dcFacturaRequest
            {
                CuitReceptor = cuitReceptor,
                TipoDocReceptor = 80, // CUIT
                CondicionIvaReceptor = condicionSeleccionada.Id,
                ImporteNeto = esClaseC ? totalParaEnviar : importeNeto,
                ImporteIva = ivaParaEnviar,
                ImporteTotal = totalParaEnviar,
                FechaComprobante = DateTime.Now.ToString("yyyyMMdd"),
                TipoComprobante = (dcTipoComprobante)tipoSeleccionado.Codigo,
                Concepto = conceptoSeleccionado.Value,
                NumeroComprobante = numeroComprobante
            };

            if (conceptoSeleccionado.Value != dcConcepto.Productos)
            {
                facturaRequest.FechaServicioDesde = dtpFechaServicioDesde.Value.ToString("yyyyMMdd");
                facturaRequest.FechaServicioHasta = dtpFechaServicioHasta.Value.ToString("yyyyMMdd");
                facturaRequest.FechaVencimiento = dtpFechaVencimiento.Value.ToString("yyyyMMdd");
            }

            // Si es Nota de Crédito / Débito, requerir comprobante asociado
            if (EsNota(tipoSeleccionado.Codigo))
            {
                if (!long.TryParse(txtCbteAsociado.Text, out long nroAsociado) || nroAsociado <= 0)
                {
                    MessageBox.Show("Para notas de crédito / débito debe indicar el número de la factura asociada.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCbteAsociado.Focus();
                    return;
                }

                facturaRequest.CbteAsociadoNro = nroAsociado;
                facturaRequest.CbteAsociadoPtoVta = _config.PuntoVenta;
                facturaRequest.CbteAsociadoTipo = ObtenerTipoFacturaBaseParaNota(tipoSeleccionado.Codigo);
                // Se podría consultar la fecha del comprobante base con FECompConsultarAsync para enviarla, opcional.
                // var consultaBase = await wsfeClient.FECompConsultarAsync(nroAsociado, facturaRequest.CbteAsociadoTipo.Value);
                // if (consultaBase.Success) { facturaRequest.CbteAsociadoFecha = DateTime.Now.ToString("yyyyMMdd"); }
            }

            // Solicitar CAE
            var response = await wsfeClient.FECAESolicitarAsync(facturaRequest);

            // Mostrar resultado
            if (response.Success)
            {
                // Consulta cruzada al WSFE para validar que los datos coinciden
                var consulta = await wsfeClient.FECompConsultarAsync(response.NumeroComprobante, facturaRequest.TipoComprobante!.Value);
                bool coincidenciaCae = consulta.Success && consulta.Cae == response.Cae;
                bool coincidenciaVto = consulta.Success && consulta.CaeVencimiento == response.CaeVencimiento;

                txtResultado.Text = $"✓ FACTURA AUTORIZADA EXITOSAMENTE\r\n\r\n" +
                                    $"Número de Comprobante: {response.NumeroComprobante}\r\n" +
                                    $"Tipo de Comprobante: {facturaRequest.TipoComprobante} ({tipoSeleccionado.Codigo})\r\n" +
                                    $"CAE: {response.Cae}\r\n" +
                                    $"Vencimiento CAE: {response.CaeVencimiento}\r\n" +
                                    $"Condición IVA receptor: {facturaRequest.CondicionIvaReceptor ?? "N/D"}\r\n" +
                                    $"Resultado: {response.Resultado}\r\n" +
                                    (consulta.Success
                                        ? $"\r\nValidación WSFE: OK (CAE {(coincidenciaCae ? "coincide" : "NO coincide")}, Vto {(coincidenciaVto ? "coincide" : "NO coincide")})\r\n"
                                        : $"\r\nValidación WSFE: NO DISPONIBLE ({consulta.Mensaje})\r\n") +
                                    "\r\n" +
                                    $"Importe Neto: $ {importeNeto:F2}\r\n" +
                                    $"IVA 21%: $ {iva:F2}\r\n" +
                                    $"Total: $ {total:F2}";

                if (response.Observaciones.Count > 0)
                {
                    txtResultado.Text += $"\r\n\r\nObservaciones:\r\n" +
                                        string.Join("\r\n", response.Observaciones);
                }

                MessageBox.Show($"Factura autorizada exitosamente.\n\nCAE: {response.Cae}\nVencimiento: {response.CaeVencimiento}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                txtResultado.Text = $"✗ ERROR AL AUTORIZAR FACTURA\r\n\r\n" +
                                    $"Resultado: {(!string.IsNullOrEmpty(response.Resultado) ? response.Resultado : "N/D")}\r\n" +
                                    $"Mensaje: {response.Mensaje}\r\n";

                if (response.Errores.Count > 0)
                {
                    txtResultado.Text += $"\r\nErrores:\r\n" +
                                        string.Join("\r\n", response.Errores);
                }

                if (response.Observaciones.Count > 0)
                {
                    txtResultado.Text += $"\r\n\r\nObservaciones:\r\n" +
                                        string.Join("\r\n", response.Observaciones);
                }

                MessageBox.Show($"No se pudo autorizar la factura.\n\n{response.Mensaje}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            txtResultado.Text = $"✗ EXCEPCIÓN AL PROCESAR:\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}";
            MessageBox.Show($"Ocurrió un error al procesar la solicitud:\n\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnAutorizar.Enabled = true;
            btnAutorizar.Text = "Autorizar Factura (Solicitar CAE)";
        }
    }

    private async void txtCuitReceptor_TextChanged(object sender, EventArgs e)
    {
        if (_cargandoCondiciones)
            return;

        if (!long.TryParse(txtCuitReceptor.Text, out long cuit) || txtCuitReceptor.Text.Length != 11)
        {
            _ultimoCuitConsultado = null;
            InicializarComboCondicion();
            return;
        }

        if (_ultimoCuitConsultado == cuit)
            return;

        await CargarCondicionesIvaAsync(cuit);
    }

    private async Task CargarCondicionesIvaAsync(long cuitReceptor)
    {
        if (_config == null)
            return;

        try
        {
            _cargandoCondiciones = true;
            cmbCondicionIVA.Enabled = false;
            cmbCondicionIVA.DataSource = null;
            cmbCondicionIVA.Items.Clear();
            cmbCondicionIVA.Items.Add("Consultando condición IVA en AFIP...");
            cmbCondicionIVA.SelectedIndex = 0;

            if (_wsfeClient == null)
            {
                RefrescarClienteWsfe();
            }

            if (_wsfeClient == null)
                throw new InvalidOperationException("No se pudo inicializar el cliente WSFE.");

            if (cmbTipoComprobante.SelectedItem is not TipoComprobanteOption tipoSelConsulta)
            {
                cmbCondicionIVA.DataSource = null;
                cmbCondicionIVA.Items.Clear();
                cmbCondicionIVA.Items.Add("Seleccione un tipo de comprobante para consultar");
                cmbCondicionIVA.SelectedIndex = 0;
                return;
            }

            var tipoComprobanteParaConsulta = (dcTipoComprobante)tipoSelConsulta.Codigo;

            var opciones = await _wsfeClient.GetCondicionesIVAReceptorAsync(80, cuitReceptor, tipoComprobanteParaConsulta);

            if (opciones.Count == 0)
            {
                cmbCondicionIVA.DataSource = null;
                cmbCondicionIVA.Items.Clear();
                cmbCondicionIVA.Items.Add("AFIP no devolvió opciones para este CUIT");
                cmbCondicionIVA.SelectedIndex = 0;
                return;
            }

            cmbCondicionIVA.DataSource = null;
            cmbCondicionIVA.DisplayMember = nameof(dcCondicionIvaOption.DisplayText);
            cmbCondicionIVA.ValueMember = nameof(dcCondicionIvaOption.Id);
            cmbCondicionIVA.DataSource = opciones;
            cmbCondicionIVA.Enabled = true;
            _ultimoCuitConsultado = cuitReceptor;
        }
        catch (Exception ex)
        {
            cmbCondicionIVA.DataSource = null;
            cmbCondicionIVA.Items.Clear();
            cmbCondicionIVA.Items.Add("Error al cargar condición IVA");
            cmbCondicionIVA.SelectedIndex = 0;
            txtResultado.Text = $"✗ Error al consultar Condición IVA:\r\n{ex.Message}";
        }
        finally
        {
            _cargandoCondiciones = false;
        }
    }

    private void RefrescarClienteWsfe()
    {
        if (_config != null)
        {
            _wsfeClient = new dcWsfeClient(_config, logger: _afipLogger);
        }
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_logViewer.IsDisposed)
        {
            _logViewer.Close();
        }

        _loggerFactory.Dispose();
    }

    private void PositionLogWindow()
    {
        if (_logViewer.IsDisposed)
        {
            return;
        }

        var workingArea = Screen.FromControl(this).WorkingArea;
        var desiredX = Location.X + Width + 10;
        var desiredY = Location.Y;

        if (desiredX + _logViewer.Width > workingArea.Right)
        {
            desiredX = workingArea.Right - _logViewer.Width;
        }
        if (desiredX < workingArea.Left)
        {
            desiredX = workingArea.Left;
        }

        if (desiredY + _logViewer.Height > workingArea.Bottom)
        {
            desiredY = workingArea.Bottom - _logViewer.Height;
        }
        if (desiredY < workingArea.Top)
        {
            desiredY = workingArea.Top;
        }

        _logViewer.Location = new Point(desiredX, desiredY);
    }

    private void ActualizarCamposServicio(bool forzarValores = false)
    {
        if (dtpFechaServicioDesde == null || dtpFechaServicioHasta == null || dtpFechaVencimiento == null ||
            lblFechaServicioDesde == null || lblFechaServicioHasta == null || lblFechaVencimiento == null)
        {
            return;
        }

        bool requiereFechas = cmbConcepto.SelectedItem is ConceptoOption opcion && opcion.Value != dcConcepto.Productos;
        bool estabanHabilitados = dtpFechaServicioDesde.Enabled;

        dtpFechaServicioDesde.Enabled = requiereFechas;
        dtpFechaServicioHasta.Enabled = requiereFechas;
        dtpFechaVencimiento.Enabled = requiereFechas;

        lblFechaServicioDesde.Enabled = requiereFechas;
        lblFechaServicioHasta.Enabled = requiereFechas;
        lblFechaVencimiento.Enabled = requiereFechas;

        if (requiereFechas && (!estabanHabilitados || forzarValores))
        {
            EstablecerFechasServicioPorDefecto();
        }
    }

    private void EstablecerFechasServicioPorDefecto()
    {
        if (dtpFechaServicioDesde == null || dtpFechaServicioHasta == null || dtpFechaVencimiento == null)
            return;

        var hoy = DateTime.Today;
        dtpFechaServicioDesde.Value = hoy;
        dtpFechaServicioHasta.Value = hoy;
        dtpFechaVencimiento.Value = hoy.AddDays(30);
    }

    private async void cmbTipoComprobante_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargandoTipoComprobante)
            return;

        if (cmbTipoComprobante.SelectedItem is not TipoComprobanteOption opcion)
            return;

        InicializarComboCondicion();
        _ultimoCuitConsultado = null;

        CalcularTotales();

        // Mostrar / ocultar controles de comprobante asociado para notas
        bool esNota = EsNota(opcion.Codigo);
        if (lblCbteAsociado != null && txtCbteAsociado != null)
        {
            lblCbteAsociado.Visible = esNota;
            txtCbteAsociado.Visible = esNota;
            if (!esNota)
            {
                txtCbteAsociado.Text = string.Empty;
            }
        }

        if (long.TryParse(txtCuitReceptor.Text, out long cuit) && txtCuitReceptor.Text.Length == 11)
        {
            await CargarCondicionesIvaAsync(cuit);
        }
    }

    private static bool EsComprobanteClaseC(int codigo) => codigo is 11 or 12 or 13;
    private static bool EsNota(int codigo) => codigo is 2 or 3 or 7 or 8 or 12 or 13 or 52 or 53;
    private static int ObtenerTipoFacturaBaseParaNota(int codigoNota) => codigoNota switch
    {
        2 or 3 => 1,      // Nota A -> Factura A
        7 or 8 => 6,      // Nota B -> Factura B
        12 or 13 => 11,   // Nota C -> Factura C
        52 or 53 => 51,   // Nota M -> Factura M
        _ => 0
    };

    private sealed record TipoComprobanteOption(int Codigo, string Descripcion);
    private sealed record ConceptoOption(dcConcepto Value, string DisplayText);
}
