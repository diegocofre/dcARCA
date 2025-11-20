# dcARCA - Facturación Electrónica Argentina

**dcARCA** es un componente .NET 8 para implementar facturación electrónica Argentina utilizando el web service **WSFEv1 de AFIP (ARCA)** y consultar el padrón oficial **ws_sr_padron_a5** para validar CUIT. 
Esta librería NO ES un producto oficial de ARCA ni del Gobierno Argentino,sino una implementación independiente desarrollada por **DC Sistemas**. [www.diegocofre.com.ar](http://www.diegocofre.com.ar)

## Licencia
Este proyecto está licenciado bajo **Apache License 2.0**.  
Copyright (c) 2025 Diego Cofré, DC Sistemas www.diegocofre.com.ar

## � Quick Start (5 minutos)

### 1. Requisitos previos
- .NET 8 SDK ([Descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- Certificado digital ARCA (.pfx)

### 2. Clonar y compilar
```powershell
cd c:\ruta\a\tu\proyecto
dotnet build
```

### 3. Configurar
Copia `appsettings.example.json` a `appsettings.json` y edita:
```json
{
  "dcArcaConfig": {
    "Cuit": "TU_CUIT",
    "CertificatePath": "C:\\Ruta\\Certificado.pfx",
    "CertificatePassword": "TU_PASSWORD",
    "WsaaUrl": "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
    "WsfeUrl": "https://wswhomo.afip.gov.ar/wsfev1/service.asmx",
    "PadronUrl": "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA5",
    "PuntoVenta": 1
  }
}
```

### 4. Ejecutar
```powershell
dotnet run --project dcArca.TestApp\dcArca.TestApp.csproj
```

### 5. Probar
- Selecciona "Facturación Electrónica" en el menú
- Ingresa CUIT receptor: `20123456789`
- Importe neto: `1000.00`
- Clic "Calcular IVA y Total" → "Autorizar Factura"

## 📦 Instalación

### Requisitos del Sistema
- **.NET 8 SDK** o superior
- **Windows 10/11** (para WinForms)
- **Certificado digital ARCA** válido

### Compilación
```powershell
# Clonar repositorio
git clone https://github.com/tu-repo/dcArca.git
cd dcArca

# Compilar
dotnet build

# Ejecutar aplicación de prueba
dotnet run --project dcArca.TestApp
```

## 🔐 Configuración del Certificado ARCA

### Paso 1: Generar CSR
```powershell
# Instalar OpenSSL
choco install openssl

# Generar clave privada
openssl genrsa -out certificado.key 2048

# Crear CSR
openssl req -new -key certificado.key -out certificado.csr -subj "/C=AR/O=TuEmpresa/CN=WSFE/serialNumber=CUIT TU_CUIT"
```

### Paso 2: Subir a ARCA
1. Accede a https://auth.afip.gov.ar/
2. Ingresa con CUIT y Clave Fiscal
3. Ve a "WSASS - Certificados para Testing"
4. Sube el `.csr` para servicio `wsfe`
5. Descarga el `.crt` firmado

### Paso 3: Generar .pfx
```powershell
# Combinar clave privada y certificado
openssl pkcs12 -export -out certificado.pfx -inkey certificado.key -in certificado.crt -password pass:TU_PASSWORD
```

### Paso 4: Autorizar Servicios
En ARCA, autoriza el certificado para:
- `wsfe` (facturación)
- `ws_sr_padron_a5` (padrón, opcional)

**Nota**: Para homologación, registra un "Computador Fiscal" si es requerido.

## ⚙️ Configuración

Edita `appsettings.json`:

```json
{
  "dcArcaConfig": {
    "Cuit": "20123456789",
    "CertificatePath": "C:\\Certificados\\certificado.pfx",
    "CertificatePassword": "password123",
    "WsaaUrl": "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
    "WsfeUrl": "https://wswhomo.afip.gov.ar/wsfev1/service.asmx",
    "PadronUrl": "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA5",
    "PuntoVenta": 1
  }
}
```

**URLs de Producción**:
- WSAA: `https://wsaa.afip.gov.ar/ws/services/LoginCms`
- WSFEv1: `https://servicios1.afip.gov.ar/wsfev1/service.asmx`
- Padrón: `https://aws.afip.gov.ar/sr-padron/webservices/personaServiceA5`

## 🧪 Ejemplos de Uso

### Factura Básica
```csharp
using dcArca.Core.Models;
using dcArca.Core.Services;

var config = dcConfigurationHelper.LoadFromJson("appsettings.json");
var client = new dcWsfeClient(config);

var factura = new dcFacturaRequest
{
    CuitReceptor = 20123456789,
    TipoDocReceptor = 80,
    Concepto = dcConcepto.Productos,
    TipoComprobante = dcTipoComprobante.FacturaB,
    NumeroComprobante = 1,
    ImporteNeto = 1000.00m,
    ImporteIva = 210.00m,
    ImporteTotal = 1210.00m,
    FechaComprobante = "20251119"
};

var resultado = await client.FECAESolicitarAsync(factura);
if (resultado.Success)
{
    Console.WriteLine($"CAE: {resultado.Cae}");
}
```

### Nota de Crédito/Débito
Para notas de crédito o débito, debes especificar el comprobante asociado (CbteAsociado*) para cumplir con la regla 10197 de ARCA:
```csharp
var nota = new dcFacturaRequest
{
    // ... campos básicos ...
    TipoComprobante = dcTipoComprobante.NotaCreditoB, // 8
    // Comprobante asociado (factura original)
    CbteAsociadoTipo = 6, // Tipo de la factura base (FacturaB)
    CbteAsociadoPtoVta = 1, // Punto de venta de la factura base
    CbteAsociadoNro = 123, // Número de la factura base
    // Opcional: CbteAsociadoFecha = "20251119"
};
```

### Factura de Servicios
```csharp
var factura = new dcFacturaRequest
{
    // ... campos básicos ...
    Concepto = dcConcepto.Servicios,
    FechaServicioDesde = "20251101",
    FechaServicioHasta = "20251130",
    FechaVencimiento = "20251215"
};
```

### Validar CUIT
```csharp
var padron = new dcPadronClient(config);
var persona = await padron.GetPersonaAsync(20123456789);
if (persona.Success && persona.Existe)
{
    Console.WriteLine($"Estado: {persona.EstadoClave}");
}
```

### Manejo de Errores
```csharp
var resultado = await client.FECAESolicitarAsync(factura);
if (!resultado.Success)
{
    Console.WriteLine($"Error: {resultado.Mensaje}");
    foreach (var err in resultado.Errores) Console.WriteLine($"- {err}");
}
```

## 📚 API Reference

### dcWsfeClient
- `FECAESolicitarAsync(dcFacturaRequest)`: Solicita CAE
- `FECompUltimoAutorizadoAsync(dcTipoComprobante)`: Último número

### dcPadronClient
- `GetPersonaAsync(long cuit)`: Consulta padrón A5

### dcFacturaRequest
- Propiedades: CuitReceptor, ImporteNeto, ImporteIva, etc.
- Para notas: CbteAsociadoTipo, CbteAsociadoPtoVta, CbteAsociadoNro (obligatorios)
- `ValidarCuit()`: Valida formato CUIT
- `EsNota()`: Indica si es nota de crédito/débito
- `CumpleReglaNotas10197()`: Valida que notas tengan comprobante asociado

## 🐛 Troubleshooting

### Certificado no encontrado
- Verifica ruta en `appsettings.json` (usa `\\`)
- Asegura que el archivo .pfx exista

### Token inválido
- Autoriza el certificado para `wsfe` en ARCA
- Para padrón: autoriza para `ws_sr_padron_a5`

### CUIT no autorizado
- Verifica que tu CUIT tenga permisos en ARCA

### Punto de venta no habilitado
- Configura el punto de venta en ARCA

### Errores comunes ARCA
- 10016: CUIT inválido
- 10048: Comprobante duplicado
- 10197: Notas de crédito/débito requieren comprobante asociado (CbteAsoc)
- 1217: Certificado no autorizado

## 🔒 Seguridad

- Nunca subas .pfx al repositorio
- Agrega `appsettings.json` a `.gitignore`
- Usa variables de entorno o Key Vault en producción
- Rota certificados antes del vencimiento

## 📖 Referencias

- [ARCA WSFEv1](https://www.afip.gob.ar/ws/WSFE/documentacion.asp)
- [Padrón A5](https://www.afip.gob.ar/ws/documentacion/ws-padron-a5.asp)
- [WSAA](https://www.afip.gob.ar/ws/WSAA/wsaa.html)

**dc sistemas** http://www.diegocofre.com.ar - Soluciones en .NET
