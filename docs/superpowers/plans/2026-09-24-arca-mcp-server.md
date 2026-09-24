# Facturador ARCA detrás de MCP + OAuth — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Corregir los bugs identificados en `dcArca.Core` y empaquetar sus operaciones (WSFEv1 + padrón) detrás de un servidor MCP nuevo (`dcArca.McpServer`) protegido con OAuth (JWT Bearer), consumible por cualquier app de Cadencia sin importar su stack.

**Architecture:** `dcArca.Core` no se reescribe — se corrige in-place y gana su primera cobertura de tests (xunit). Un proyecto ASP.NET Core nuevo (`dcArca.McpServer`, `net10.0`) referencia `dcArca.Core` tal cual, registra sus clientes por DI, y expone 5 operaciones como MCP tools (`[McpServerTool]`) detrás de `Microsoft.AspNetCore.Authentication.JwtBearer` + `ModelContextProtocol.AspNetCore`'s `.AddMcp()` (resource-server pattern: el server valida tokens emitidos por el Authorization Server que ya use Cadencia, no emite tokens él mismo).

**Tech Stack:** .NET 10 SDK (ya instalado en `~/.dotnet`), `dcArca.Core` se mantiene en `net8.0`, `dcArca.McpServer` en `net10.0`, `ModelContextProtocol.AspNetCore` 2.2.0, `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12, xunit 2.9.3 para tests.

---

## Antes de empezar

Confirmar que el SDK está en el PATH de la shell que va a ejecutar los comandos de este plan:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet --version
# Esperado: 10.0.401 (o superior)
```

(`~/.bashrc` ya fue actualizado con esto para shells interactivas nuevas.)

---

## Parte A — Corregir los bugs de `dcArca.Core`

### Task 1: Unificar la validación de CUIT (dedupe) + scaffold del proyecto de tests

Hoy el dígito verificador de CUIT está implementado dos veces con comportamiento distinto:
- `dcConfigurationHelper.EsCuitValido` ([dcArca.Core/dcConfigurationHelper.cs:100](../../../dcArca.Core/dcConfigurationHelper.cs#L100)) — sanitiza caracteres no numéricos antes de validar.
- `dcFacturaRequest.ValidarCuit` ([dcArca.Core/Models/dcFacturaRequest.cs:40](../../../dcArca.Core/Models/dcFacturaRequest.cs#L40)) — no sanitiza, usa `int.Parse` carácter por carácter.

Se extrae a un único helper `dcCuitValidator.EsValido(string?)` (superset: sanitiza + valida). Esta es la primera lógica no trivial del repo con test, así que esta tarea también scaffoldea el proyecto de tests.

**Files:**
- Create: `dcArca.Core.Tests/dcArca.Core.Tests.csproj`
- Create: `dcArca.Core.Tests/dcCuitValidatorTests.cs`
- Create: `dcArca.Core/Models/dcCuitValidator.cs`
- Modify: `dcArca.Core/dcConfigurationHelper.cs`
- Modify: `dcArca.Core/Models/dcFacturaRequest.cs`

- [ ] **Step 1: Crear el proyecto de tests**

```bash
cd /home/fabi/code/dcARCA
dotnet new xunit -n dcArca.Core.Tests -f net10.0
```

- [ ] **Step 2: Reemplazar el csproj generado por uno con versiones fijas**

`dcArca.Core.Tests/dcArca.Core.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.10.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.0" />
    <PackageReference Include="coverlet.collector" Version="10.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\dcArca.Core\dcArca.Core.csproj" />
  </ItemGroup>

</Project>
```

Borrar el `UsingsTests.cs` / `Class1.cs` de ejemplo que haya generado la plantilla:

```bash
rm -f dcArca.Core.Tests/UnitTest1.cs
```

- [ ] **Step 3: Agregar el proyecto a la solución**

```bash
dotnet sln dcArca.sln add dcArca.Core.Tests/dcArca.Core.Tests.csproj
```

- [ ] **Step 4: Escribir el test que falla**

`dcArca.Core.Tests/dcCuitValidatorTests.cs`:

```csharp
using dcArca.Core.Models;
using Xunit;

namespace dcArca.Core.Tests;

public class dcCuitValidatorTests
{
    [Theory]
    [InlineData("20123456786", true)]   // dígito verificador correcto
    [InlineData("20-12345678-6", true)] // con guiones, debe sanitizar
    [InlineData("20123456789", false)]  // dígito verificador incorrecto
    [InlineData("2012345678", false)]   // 10 dígitos, longitud inválida
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EsValido_ValidaDigitoVerificador(string? cuit, bool esperado)
    {
        Assert.Equal(esperado, dcCuitValidator.EsValido(cuit));
    }
}
```

- [ ] **Step 5: Correr el test y verificar que falla (no compila, `dcCuitValidator` no existe)**

```bash
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj 2>&1 | tail -15
```

Expected: `error CS0246: The type or namespace name 'dcCuitValidator' could not be found`

- [ ] **Step 6: Crear el helper compartido**

`dcArca.Core/Models/dcCuitValidator.cs`:

```csharp
/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

namespace dcArca.Core.Models;

/// <summary>
/// Validación del dígito verificador de CUIT, compartida por la configuración
/// y por los modelos de factura (antes duplicada en ambos lugares).
/// </summary>
public static class dcCuitValidator
{
    private static readonly int[] Multiplicadores = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

    /// <summary>
    /// Valida que el CUIT tenga 11 dígitos y un dígito verificador correcto.
    /// Ignora caracteres no numéricos (guiones, espacios) antes de validar.
    /// </summary>
    public static bool EsValido(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            return false;

        var sanitized = new string(cuit.Where(char.IsDigit).ToArray());
        if (sanitized.Length != 11 || !long.TryParse(sanitized, out _))
            return false;

        var suma = 0;
        for (var i = 0; i < 10; i++)
            suma += (sanitized[i] - '0') * Multiplicadores[i];

        var verificador = 11 - (suma % 11);
        if (verificador == 11) verificador = 0;
        if (verificador == 10) verificador = 9;

        return verificador == (sanitized[10] - '0');
    }
}
```

- [ ] **Step 7: Correr el test y verificar que pasa**

```bash
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj 2>&1 | tail -15
```

Expected: `Passed!  - Failed: 0, Passed: 6, Skipped: 0`

- [ ] **Step 8: Reemplazar los dos duplicados por el helper**

En `dcArca.Core/dcConfigurationHelper.cs`, reemplazar el método privado `EsCuitValido` completo (líneas ~93-118) y su único call site:

```csharp
// antes: if (!EsCuitValido(config.Cuit))
if (!dcCuitValidator.EsValido(config.Cuit))
    throw new InvalidOperationException("El CUIT del emisor debe tener 11 dígitos numéricos y un dígito verificador válido.");
```

Borrar por completo el método privado `EsCuitValido` (ya no se usa).

En `dcArca.Core/Models/dcFacturaRequest.cs`, reemplazar el cuerpo de `ValidarCuit`:

```csharp
/// <summary>
/// Valida que el CUIT tenga 11 dígitos y un dígito verificador correcto
/// </summary>
public bool ValidarCuit() => dcCuitValidator.EsValido(CuitReceptor.ToString());
```

(Esto además corrige un bug menor: la versión vieja no sanitizaba, así que un CUIT con separadores fallaba antes de llegar siquiera a comparar el dígito verificador — con `CuitReceptor` siendo `long` no puede pasar en la práctica, pero ahora ambos call sites comparten exactamente la misma regla.)

- [ ] **Step 9: Correr todos los tests y el build completo**

```bash
dotnet build dcArca.sln 2>&1 | tail -15
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj 2>&1 | tail -15
```

Expected: build succeeded, 6/6 tests passed.

- [ ] **Step 10: Commit**

```bash
git add dcArca.sln dcArca.Core.Tests dcArca.Core/Models/dcCuitValidator.cs \
        dcArca.Core/dcConfigurationHelper.cs dcArca.Core/Models/dcFacturaRequest.cs
git commit -m "fix: unify duplicated CUIT checksum validation, add test project"
```

---

### Task 2: Fusionar las respuestas de error duplicadas en `dcWsfeClient`

`CrearRespuestaValidacion` y `CrearRespuestaError` en [dcArca.Core/Services/dcWsfeClient.cs:360-378](../../../dcArca.Core/Services/dcWsfeClient.cs#L360) son idénticas salvo el nombre. Es un rename mecánico, no requiere test nuevo (no hay lógica, solo construcción de objeto).

**Files:**
- Modify: `dcArca.Core/Services/dcWsfeClient.cs`

- [ ] **Step 1: Eliminar `CrearRespuestaError` y usar `CrearRespuestaValidacion` en su lugar**

Borrar el método `CrearRespuestaError` completo:

```csharp
private static dcFacturaResponse CrearRespuestaError(string codigo, string mensaje)
{
    var response = new dcFacturaResponse
    {
        Success = false,
        Mensaje = mensaje,
        Codigo = codigo
    };
    response.Errores.Add(mensaje);
    return response;
}
```

Reemplazar sus 3 call sites (`FECompUltimoAutorizadoAsync`, `FECompConsultarAsync`, `FECAESolicitarAsync`) para que llamen a `CrearRespuestaValidacion` en su lugar — misma firma `(string codigo, string mensaje)`, mismo comportamiento:

```csharp
// antes: return CrearRespuestaError("FEULTIMO_ERROR", $"Error al consultar último comprobante: {ex.Message}");
return CrearRespuestaValidacion("FEULTIMO_ERROR", $"Error al consultar último comprobante: {ex.Message}");
```

(repetir el mismo cambio de nombre de método en los otros dos catch blocks que usan `CrearRespuestaError`).

- [ ] **Step 2: Build**

```bash
dotnet build dcArca.sln 2>&1 | tail -10
```

Expected: `Build succeeded`, sin referencias colgantes a `CrearRespuestaError`.

- [ ] **Step 3: Commit**

```bash
git add dcArca.Core/Services/dcWsfeClient.cs
git commit -m "refactor: merge duplicate error-response builders in dcWsfeClient"
```

---

### Task 3: Corregir el bug de `DateTime.Now` vs `DateTime.UtcNow` en el fallback de token

En [dcArca.Core/Services/dcArcaAuthService.cs:135](../../../dcArca.Core/Services/dcArcaAuthService.cs#L135), dentro del catch de `coe.alreadyAuthenticated`:

```csharp
if (!string.IsNullOrEmpty(_token) && DateTime.Now < _tokenExpiration)
```

`_tokenExpiration` siempre se setea en UTC (`expiration.UtcDateTime` en `ParseWsaaResponse`, o `DateTime.UtcNow.AddHours(12)`). Todo el resto del archivo compara contra `DateTime.UtcNow`. En Argentina (UTC-3), un token vencido hace menos de 3 horas pasa esta comparación como si fuera válido, y se lo devuelve igual — WSFE lo va a rechazar.

Es un cambio de un carácter dentro de un método privado acoplado a HTTP real (`RequestNewTokenAsync`); no hay forma de testearlo unitariamente sin inyectar un seam de tiempo/HTTP que nadie pidió — se corrige directo, sin test dedicado (regla YAGNI de un-liners triviales).

**Files:**
- Modify: `dcArca.Core/Services/dcArcaAuthService.cs:135`

- [ ] **Step 1: Aplicar el fix**

```csharp
// antes: if (!string.IsNullOrEmpty(_token) && DateTime.Now < _tokenExpiration)
if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiration)
```

- [ ] **Step 2: Build**

```bash
dotnet build dcArca.Core/dcArca.Core.csproj 2>&1 | tail -10
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add dcArca.Core/Services/dcArcaAuthService.cs
git commit -m "fix: compare cached token expiration against UtcNow, not local time"
```

---

### Task 4: Escapar los campos interpolados en los envelopes SOAP

`dcWsfeSoapBuilder` interpola strings directo dentro de XML armado a mano (ver [dcArca.Core/Services/dcWsfeSoapBuilder.cs](../../../dcArca.Core/Services/dcWsfeSoapBuilder.cs)). Un valor con `&`, `<` o `>` (por ejemplo en `FechaComprobante` si llegara mal formado, o en los campos de comprobante asociado) rompe el XML enviado a AFIP. Es lógica real (recorre múltiples campos, condicional por comprobante), así que lleva test.

**Files:**
- Modify: `dcArca.Core/Services/dcWsfeSoapBuilder.cs`
- Test: `dcArca.Core.Tests/dcWsfeSoapBuilderTests.cs`

- [ ] **Step 1: Escribir el test que falla**

`dcArca.Core.Tests/dcWsfeSoapBuilderTests.cs`:

```csharp
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

        Assert.Contains("20123456786 &amp; C\u00eda &lt;SA&gt;", xml);
    }
}
```

- [ ] **Step 2: Correr el test y verificar que falla**

```bash
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj --filter dcWsfeSoapBuilderTests 2>&1 | tail -20
```

Expected: `XmlException` al hacer `doc.LoadXml(xml)` — el `&` sin escapar rompe el parseo.

- [ ] **Step 3: Escapar los campos de string interpolados**

En `dcArca.Core/Services/dcWsfeSoapBuilder.cs`, agregar `using System.Security;` al inicio del archivo, y envolver con `SecurityElement.Escape(...)` todos los campos de tipo `string` que se interpolan directo en XML. Concretamente, en `AppendCondicionIva`:

```csharp
private static void AppendCondicionIva(StringBuilder sb, dcFacturaRequest factura)
{
    if (factura.CondicionIvaReceptor != null)
    {
        int id = (int)factura.CondicionIvaReceptor;
        sb.AppendLine($"                        <ar:CondicionIVAReceptorId>{id}</ar:CondicionIVAReceptorId>");
    }

    // Comprobantes asociados (Notas de crédito/débito) - Regla 10197
    if (factura.EsNota() && factura.CbteAsociadoTipo.HasValue && factura.CbteAsociadoPtoVta.HasValue && factura.CbteAsociadoNro.HasValue)
    {
        sb.AppendLine("                        <ar:CbtesAsoc>");
        sb.AppendLine("                            <ar:CbteAsoc>");
        sb.AppendLine($"                                <ar:Tipo>{factura.CbteAsociadoTipo.Value}</ar:Tipo>");
        sb.AppendLine($"                                <ar:PtoVta>{factura.CbteAsociadoPtoVta.Value}</ar:PtoVta>");
        sb.AppendLine($"                                <ar:Nro>{factura.CbteAsociadoNro.Value}</ar:Nro>");
        if (!string.IsNullOrWhiteSpace(factura.CbteAsociadoCuit))
        {
            sb.AppendLine($"                                <ar:Cuit>{SecurityElement.Escape(factura.CbteAsociadoCuit)}</ar:Cuit>");
        }
        if (!string.IsNullOrWhiteSpace(factura.CbteAsociadoFecha))
        {
            sb.AppendLine($"                                <ar:CbteFch>{SecurityElement.Escape(factura.CbteAsociadoFecha)}</ar:CbteFch>");
        }
        sb.AppendLine("                            </ar:CbteAsoc>");
        sb.AppendLine("                        </ar:CbtesAsoc>");
    }
    else if (factura.EsNota() && !string.IsNullOrWhiteSpace(factura.PeriodoAsocDesde) && !string.IsNullOrWhiteSpace(factura.PeriodoAsocHasta))
    {
        // Alternativa: periodo asociado
        sb.AppendLine("                        <ar:PeriodoAsoc>");
        sb.AppendLine($"                            <ar:FchDesde>{SecurityElement.Escape(factura.PeriodoAsocDesde)}</ar:FchDesde>");
        sb.AppendLine($"                            <ar:FchHasta>{SecurityElement.Escape(factura.PeriodoAsocHasta)}</ar:FchHasta>");
        sb.AppendLine("                        </ar:PeriodoAsoc>");
    }
}
```

Y en `AppendServiceDates`:

```csharp
private static void AppendServiceDates(StringBuilder sb, int concepto, dcFacturaRequest factura)
{
    if (concepto == 1)
    {
        return;
    }

    if (!string.IsNullOrWhiteSpace(factura.FechaServicioDesde))
    {
        sb.AppendLine($"                        <ar:FchServDesde>{SecurityElement.Escape(factura.FechaServicioDesde)}</ar:FchServDesde>");
    }

    if (!string.IsNullOrWhiteSpace(factura.FechaServicioHasta))
    {
        sb.AppendLine($"                        <ar:FchServHasta>{SecurityElement.Escape(factura.FechaServicioHasta)}</ar:FchServHasta>");
    }

    if (!string.IsNullOrWhiteSpace(factura.FechaVencimiento))
    {
        sb.AppendLine($"                        <ar:FchVtoPago>{SecurityElement.Escape(factura.FechaVencimiento)}</ar:FchVtoPago>");
    }
}
```

Y en `BuildSolicitarCaeRequest`, la línea de `CbteFch` (fecha del comprobante principal):

```csharp
// antes: sb.AppendLine($"                        <ar:CbteFch>{factura.FechaComprobante}</ar:CbteFch>");
sb.AppendLine($"                        <ar:CbteFch>{SecurityElement.Escape(factura.FechaComprobante)}</ar:CbteFch>");
```

(No hace falta escapar `_config.Cuit`, tokens/sign de WSAA, ni los campos numéricos — esos no vienen de input externo variable o ya están garantizados numéricos por el tipo.)

- [ ] **Step 4: Correr el test y verificar que pasa**

```bash
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj --filter dcWsfeSoapBuilderTests 2>&1 | tail -15
```

Expected: `Passed!  - Failed: 0, Passed: 1, Skipped: 0`

- [ ] **Step 5: Correr toda la suite de tests**

```bash
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj 2>&1 | tail -15
```

Expected: 7/7 tests passed.

- [ ] **Step 6: Commit**

```bash
git add dcArca.Core/Services/dcWsfeSoapBuilder.cs dcArca.Core.Tests/dcWsfeSoapBuilderTests.cs
git commit -m "fix: escape interpolated string fields in SOAP envelopes"
```

---

### Task 5: Eliminar paquetes NuGet muertos y alinear versiones

`dcArca.Core.csproj` referencia `System.Security.Cryptography.Xml`, `System.ServiceModel.Http` y `System.ServiceModel.Primitives` — ya se confirmó con `grep -rn` que ninguno se usa en el código (ver análisis previo). El primero además dispara 16 warnings `NU1903` de severidad alta en cada build. `Microsoft.Extensions.Logging.Abstractions` está en `8.0.1` mientras el resto de los paquetes de `Microsoft.Extensions.*` va en `10.0.0`.

**Files:**
- Modify: `dcArca.Core/dcArca.Core.csproj`

- [ ] **Step 1: Confirmar de nuevo que no hay usos (red de seguridad antes de borrar)**

```bash
grep -rn "ServiceModel\|Cryptography.Xml\|SignedXml" --include=*.cs dcArca.Core dcArca.Core.Tests
```

Expected: sin resultados (ya verificado antes de escribir este plan).

- [ ] **Step 2: Editar el csproj**

`dcArca.Core/dcArca.Core.csproj`, reemplazar el `<ItemGroup>` de `PackageReference`:

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
    <!-- Usada por dcArcaAuthService para firmar el TRA (SignedCms/CmsSigner) al armar el CMS/PKCS7 del login WSAA.
         Antes llegaba transitivamente vía System.Security.Cryptography.Xml (paquete con vulnerabilidades NU1903, removido). -->
    <PackageReference Include="System.Security.Cryptography.Pkcs" Version="10.0.0" />
  </ItemGroup>
```

(se borran las 3 referencias a `System.Security.Cryptography.Xml`, `System.ServiceModel.Http` y `System.ServiceModel.Primitives`; se sube `Microsoft.Extensions.Logging.Abstractions` de `8.0.1` a `10.0.0`).

> **Corrección post-ejecución:** el `grep` del Step 1 solo buscaba `ServiceModel|Cryptography.Xml|SignedXml` y no detectó que `dcArcaAuthService.cs` usa `System.Security.Cryptography.Pkcs.SignedCms/CmsSigner/ContentInfo` (namespace **Pkcs**, no **Xml**) para firmar el TRA. Ese tipo llegaba transitivamente a través del paquete `System.Security.Cryptography.Xml` que se está borrando acá. Al quitarlo, el build rompe con `CS1069` sobre esos tres tipos. El fix es agregar `System.Security.Cryptography.Pkcs` explícito (sin advisories, a diferencia de `.Xml`) — ya incluido en el bloque de arriba.

- [ ] **Step 3: Restore + build limpio**

```bash
rm -rf dcArca.Core/obj dcArca.Core/bin
dotnet build dcArca.Core/dcArca.Core.csproj 2>&1 | tail -15
```

Expected: `Build succeeded`, **0 warnings NU1903** (antes había 16). (Se compila solo `dcArca.Core`, no todo `dcArca.sln`: `dcArca.TestApp` es WinForms `net8.0-windows` y no compila en Linux — falla preexistente, no relacionada con este plan.)

- [ ] **Step 4: Correr toda la suite de tests para confirmar que nada se rompió**

```bash
dotnet test dcArca.Core.Tests/dcArca.Core.Tests.csproj 2>&1 | tail -15
```

Expected: 7/7 tests passed.

- [ ] **Step 5: Commit**

```bash
git add dcArca.Core/dcArca.Core.csproj
git commit -m "chore: drop unused ServiceModel/Cryptography.Xml packages, align Logging.Abstractions to 10.0.0"
```

---

## Parte B — `dcArca.McpServer`: el facturador detrás de MCP + OAuth

Patrón: **resource server**. `dcArca.McpServer` no emite tokens ni implementa un Authorization Server propio — valida bearer tokens JWT emitidos por el Authorization Server que ya use Cadencia (Entra ID, Auth0, Keycloak, lo que sea — cualquiera que hable OIDC/JWT). Esto sigue el patrón oficial del SDK de MCP para C# (sample `ProtectedMcpServer` del repo `modelcontextprotocol/csharp-sdk`).

### Task 6: Scaffold del proyecto `dcArca.McpServer`

**Files:**
- Create: `dcArca.McpServer/dcArca.McpServer.csproj`
- Create: `dcArca.McpServer/Program.cs` (placeholder mínimo, se completa en Task 8)
- Modify: `dcArca.sln`
- Modify: `.gitignore`

- [ ] **Step 1: Crear el proyecto**

```bash
cd /home/fabi/code/dcARCA
dotnet new web -n dcArca.McpServer -f net10.0
```

- [ ] **Step 2: Reemplazar el csproj generado**

`dcArca.McpServer/dcArca.McpServer.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="2.2.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.12" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\dcArca.Core\dcArca.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Agregar a la solución**

```bash
dotnet sln dcArca.sln add dcArca.McpServer/dcArca.McpServer.csproj
```

- [ ] **Step 4: Proteger el `appsettings.json` real igual que en `TestApp`**

`.gitignore` ya excluye `appsettings.*.json` globalmente (con excepción de `appsettings.example.json`) — confirmar que sigue cubriendo esta carpeta nueva:

```bash
grep -n "appsettings" .gitignore
```

Expected: la regla `appsettings.*.json` (con `!appsettings.example.json`) ya está y aplica a cualquier subcarpeta nueva, no hace falta tocar `.gitignore`.

- [ ] **Step 5: Build**

```bash
dotnet build dcArca.sln 2>&1 | tail -15
```

Expected: `Build succeeded` (el `Program.cs` que generó `dotnet new web` todavía compila solo, se reemplaza en Task 8).

- [ ] **Step 6: Commit**

```bash
git add dcArca.sln dcArca.McpServer
git commit -m "chore: scaffold dcArca.McpServer project"
```

---

### Task 7: Configuración — `appsettings.example.json` + registrar los clientes de Core por DI

**Files:**
- Create: `dcArca.McpServer/appsettings.example.json`
- Create: `dcArca.McpServer/appsettings.json` (no se commitea — está en `.gitignore`, se crea local para poder correr el server)

- [ ] **Step 1: Crear el archivo de ejemplo (éste sí se commitea)**

`dcArca.McpServer/appsettings.example.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "dcArcaConfig": {
    "Cuit": "20123456789",
    "CertificatePath": "C:\\Ruta\\A\\SuCertificado.pfx",
    "CertificatePassword": "CAMBIAR_ME",
    "WsaaUrl": "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
    "WsfeUrl": "https://wswhomo.afip.gov.ar/wsfev1/service.asmx",
    "PadronUrl": "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA5",
    "PuntoVenta": 1
  },
  "Jwt": {
    "Authority": "https://CAMBIAR-por-tu-authorization-server.example.com",
    "Audience": "https://localhost:7071/"
  }
}
```

`Jwt:Authority` y `Jwt:Audience` apuntan al Authorization Server real de Cadencia (el que emite los tokens que van a usar las apps clientes) — este plan no lo implementa, se asume que ya existe.

- [ ] **Step 2: Copiar a `appsettings.json` local para poder correr/testear**

```bash
cp dcArca.McpServer/appsettings.example.json dcArca.McpServer/appsettings.json
```

(este archivo queda sin trackear por git; cada quien lo completa con su propio `CertificatePath`/`Authority` real).

- [ ] **Step 3: Commit (solo el example)**

```bash
git add dcArca.McpServer/appsettings.example.json
git commit -m "chore: add appsettings.example.json for dcArca.McpServer"
```

---

### Task 8: `Program.cs` — JWT Bearer + host MCP + DI de los clientes de Core

Sigue el patrón oficial del sample `ProtectedMcpServer` del SDK C# de MCP (`.AddMcp()` para exponer resource metadata OAuth + `MapMcp().RequireAuthorization()`).

**Files:**
- Modify: `dcArca.McpServer/Program.cs`

- [ ] **Step 1: Reemplazar `Program.cs` completo**

`dcArca.McpServer/Program.cs`:

```csharp
using dcArca.Core;
using dcArca.Core.Models;
using dcArca.Core.Services;
using dcArca.Core.Services.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var jwtAuthority = builder.Configuration["Jwt:Authority"]
    ?? throw new InvalidOperationException("Falta configurar Jwt:Authority en appsettings.json");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Falta configurar Jwt:Audience en appsettings.json");

// dcArcaConfig se carga con el mismo helper que usa dcArca.TestApp, valida CUIT/certificado al arrancar
var arcaConfig = dcConfigurationHelper.LoadFromJson(
    Path.Combine(builder.Environment.ContentRootPath, "appsettings.json"));

builder.Services.AddSingleton(arcaConfig);
builder.Services.AddSingleton<IAfipLogger>(sp =>
    new AfipLoggerAdapter(sp.GetRequiredService<ILoggerFactory>().CreateLogger("dcArca")));
builder.Services.AddSingleton<dcArcaAuthService>(sp => new dcArcaAuthService(
    arcaConfig.WsaaUrl, arcaConfig.CertificatePath, arcaConfig.CertificatePassword, arcaConfig.Cuit,
    logger: sp.GetRequiredService<IAfipLogger>()));
builder.Services.AddSingleton<IdcWsfeClient, dcWsfeClient>();
builder.Services.AddSingleton<IdcPadronClient, dcPadronClient>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = jwtAuthority;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = jwtAudience,
        ValidIssuer = jwtAuthority,
    };
})
.AddMcp(options =>
{
    options.ResourceMetadata = new()
    {
        AuthorizationServers = { jwtAuthority },
        ScopesSupported = ["arca:facturar", "arca:consultar"],
    };
});

builder.Services.AddAuthorization();

builder.Services.AddMcpServer()
    .WithTools<ArcaTools>()
    .WithHttpTransport(options =>
    {
        options.SessionMode = HttpServerSessionMode.Stateless;
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp().RequireAuthorization();

app.Run();

public partial class Program { }
```

`public partial class Program { }` al final es necesario para que `WebApplicationFactory<Program>` (Task 10) pueda referenciar el entry point desde el proyecto de tests — es el patrón estándar de ASP.NET Core con top-level statements.

`IdcWsfeClient`/`dcWsfeClient` y `dcPadronClient` ya resuelven `dcArcaAuthService` opcionalmente por constructor (`authService = null` los hace crear uno propio) — acá se lo pasamos explícito para que las tres piezas (`dcWsfeClient`, `dcPadronClient`, y quien más lo necesite) reusen el mismo `dcArcaAuthService` y su cache de token, en vez de cada uno abrir su propio login contra WSAA. Nota: `dcWsfeClient` y `dcPadronClient` toman `dcArcaAuthService` concreto, no una interfaz — como se registra `AddSingleton<dcArcaAuthService>` explícito arriba, el contenedor lo resuelve igual al construir `dcWsfeClient`/`dcPadronClient` vía DI (ambos constructores tienen `authService` como segundo parámetro opcional, .NET DI lo completa por tipo).

- [ ] **Step 2: Build**

```bash
dotnet build dcArca.McpServer/dcArca.McpServer.csproj 2>&1 | tail -20
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add dcArca.McpServer/Program.cs
git commit -m "feat: wire JWT bearer auth + MCP host + Core DI in dcArca.McpServer"
```

---

### Task 9: `ArcaTools` — las 5 operaciones expuestas como MCP tools

**Files:**
- Create: `dcArca.McpServer/ArcaTools.cs`

- [ ] **Step 1: Crear la clase de tools**

`dcArca.McpServer/ArcaTools.cs`:

```csharp
using System.ComponentModel;
using dcArca.Core.Models;
using dcArca.Core.Services;
using ModelContextProtocol.Server;

namespace dcArca.McpServer;

/// <summary>
/// Operaciones de facturación electrónica ARCA (WSFEv1 + padrón) expuestas como MCP tools.
/// Delegan directamente en dcArca.Core; esta clase no tiene lógica de negocio propia.
/// </summary>
[McpServerToolType]
public sealed class ArcaTools
{
    private readonly IdcWsfeClient _wsfe;
    private readonly IdcPadronClient _padron;

    public ArcaTools(IdcWsfeClient wsfe, IdcPadronClient padron)
    {
        _wsfe = wsfe;
        _padron = padron;
    }

    [McpServerTool, Description("Consulta el último número de comprobante autorizado por AFIP para un tipo de comprobante dado, en el punto de venta configurado.")]
    public Task<dcFacturaResponse> ConsultarUltimoComprobante(
        [Description("Tipo de comprobante AFIP (ej: 1=Factura A, 6=Factura B, 11=Factura C).")] dcTipoComprobante tipoComprobante,
        CancellationToken cancellationToken)
        => _wsfe.FECompUltimoAutorizadoAsync(tipoComprobante, cancellationToken);

    [McpServerTool, Description("Consulta los datos de un comprobante ya emitido/autorizado por AFIP.")]
    public Task<dcFacturaResponse> ConsultarComprobante(
        [Description("Número del comprobante a consultar.")] long numeroComprobante,
        [Description("Tipo de comprobante AFIP (ej: 1=Factura A, 6=Factura B, 11=Factura C).")] dcTipoComprobante tipoComprobante,
        CancellationToken cancellationToken)
        => _wsfe.FECompConsultarAsync(numeroComprobante, tipoComprobante, cancellationToken);

    [McpServerTool, Description("Solicita a AFIP la autorización (CAE) de una factura. Los importes deben cumplir ImporteTotal = ImporteNeto + ImporteIva.")]
    public Task<dcFacturaResponse> SolicitarCae(
        [Description("Tipo de comprobante AFIP a autorizar.")] dcTipoComprobante tipoComprobante,
        [Description("Número de comprobante a autorizar (CbteDesde/CbteHasta).")] long numeroComprobante,
        [Description("Concepto: 1=Productos, 2=Servicios, 3=Productos y Servicios.")] dcConcepto concepto,
        [Description("CUIT del receptor (sin guiones).")] long cuitReceptor,
        [Description("Tipo de documento del receptor (80=CUIT, 96=DNI, 99=Consumidor Final).")] int tipoDocReceptor,
        [Description("Condición frente al IVA del receptor (obligatoria por RG 5616).")] dcCondicionIvaReceptor condicionIvaReceptor,
        [Description("Importe neto gravado (sin IVA).")] decimal importeNeto,
        [Description("Importe de IVA.")] decimal importeIva,
        [Description("Importe total (debe ser ImporteNeto + ImporteIva).")] decimal importeTotal,
        [Description("Fecha del comprobante en formato YYYYMMDD.")] string fechaComprobante,
        CancellationToken cancellationToken)
    {
        var factura = new dcFacturaRequest
        {
            TipoComprobante = tipoComprobante,
            NumeroComprobante = numeroComprobante,
            Concepto = concepto,
            CuitReceptor = cuitReceptor,
            TipoDocReceptor = tipoDocReceptor,
            CondicionIvaReceptor = condicionIvaReceptor,
            ImporteNeto = importeNeto,
            ImporteIva = importeIva,
            ImporteTotal = importeTotal,
            FechaComprobante = fechaComprobante,
        };

        return _wsfe.FECAESolicitarAsync(factura, cancellationToken);
    }

    [McpServerTool, Description("Consulta las condiciones de IVA válidas para un receptor dado, según el tipo de comprobante a emitir.")]
    public Task<List<dcCondicionIvaOption>> ConsultarCondicionesIva(
        [Description("Tipo de documento del receptor (80=CUIT, 96=DNI).")] int docTipo,
        [Description("Número de documento del receptor.")] long docNro,
        [Description("Tipo de comprobante AFIP a emitir.")] dcTipoComprobante tipoComprobante,
        CancellationToken cancellationToken)
        => _wsfe.GetCondicionesIVAReceptorAsync(docTipo, docNro, tipoComprobante, cancellationToken);

    [McpServerTool, Description("Consulta los datos registrales de un CUIT en el padrón de AFIP (razón social, estado, actividades).")]
    public Task<dcPadronPersonaResult> ConsultarPadron(
        [Description("CUIT a consultar (sin guiones).")] long cuit,
        CancellationToken cancellationToken)
        => _padron.GetPersonaAsync(cuit, cancellationToken);
}
```

Nota sobre `SolicitarCae`: no repite las validaciones de negocio (importes, fechas, regla 10197) — esas ya viven en `dcWsfeClient.FECAESolicitarAsync` (ver [dcArca.Core/Services/dcWsfeClient.cs:154-260](../../../dcArca.Core/Services/dcWsfeClient.cs#L154)) y devuelven un `dcFacturaResponse` con `Success=false` + `Errores` cuando algo no cumple. El tool es un passthrough deliberado — no hay que duplicar esa lógica acá.

- [ ] **Step 2: Build**

```bash
dotnet build dcArca.McpServer/dcArca.McpServer.csproj 2>&1 | tail -20
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add dcArca.McpServer/ArcaTools.cs
git commit -m "feat: expose WSFEv1 + padron operations as MCP tools"
```

---

### Task 10: Test de integración — el server rechaza requests sin token

Único chequeo automatizado que le corresponde a este host: probar que la parte que le agregamos (auth) efectivamente bloquea. No re-probamos lo que ya prueba `dcArca.Core.Tests` (parsing, validaciones) ni pegamos contra AFIP real.

**Files:**
- Create: `dcArca.McpServer.Tests/dcArca.McpServer.Tests.csproj`
- Create: `dcArca.McpServer.Tests/McpAuthTests.cs`
- Create: `dcArca.McpServer/appsettings.Testing.json`
- Modify: `dcArca.sln`

- [ ] **Step 1: Crear el proyecto de test de integración**

```bash
cd /home/fabi/code/dcARCA
dotnet new xunit -n dcArca.McpServer.Tests -f net10.0
rm -f dcArca.McpServer.Tests/UnitTest1.cs
dotnet sln dcArca.sln add dcArca.McpServer.Tests/dcArca.McpServer.Tests.csproj
```

- [ ] **Step 2: csproj con referencia al server + paquete de testing web**

`dcArca.McpServer.Tests/dcArca.McpServer.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.10.1" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\dcArca.McpServer\dcArca.McpServer.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: `appsettings.Testing.json` — config falsa para que el host arranque sin secretos reales**

El `Program.cs` (Task 8) llama a `dcConfigurationHelper.LoadFromJson` que valida que el certificado exista en disco (`File.Exists(config.CertificatePath)`, ver [dcArca.Core/dcConfigurationHelper.cs:82](../../../dcArca.Core/dcConfigurationHelper.cs#L82)). Para que `WebApplicationFactory` pueda levantar el host en el test sin un `.pfx` real, se genera un archivo vacío que solo necesita *existir* — el test nunca ejercita el flujo que lo abre, porque corta antes en el 401 de auth.

`dcArca.McpServer/appsettings.Testing.json`:

```json
{
  "Logging": {
    "LogLevel": { "Default": "Warning" }
  },
  "AllowedHosts": "*",
  "dcArcaConfig": {
    "Cuit": "20123456786",
    "CertificatePath": "test-cert-placeholder.pfx",
    "CertificatePassword": "",
    "WsaaUrl": "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
    "WsfeUrl": "https://wswhomo.afip.gov.ar/wsfev1/service.asmx",
    "PadronUrl": "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA5",
    "PuntoVenta": 1
  },
  "Jwt": {
    "Authority": "https://test-authority.invalid",
    "Audience": "https://test-audience.invalid/"
  }
}
```

- [ ] **Step 4: Escribir el test que falla**

`dcArca.McpServer.Tests/McpAuthTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace dcArca.McpServer.Tests;

public class McpAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public McpAuthTests(WebApplicationFactory<Program> factory)
    {
        var contentRoot = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "dcArca.McpServer");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(contentRoot);
            builder.UseEnvironment("Testing");
        });
    }

    [Fact]
    public async Task McpEndpoint_SinToken_Devuelve401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/", new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/list"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

Antes de correr el test, crear el placeholder de certificado que referencia `appsettings.Testing.json`:

```bash
touch dcArca.McpServer/test-cert-placeholder.pfx
```

Y confirmar que `Program.cs` (Task 8) lee `appsettings.{Environment}.json` — ASP.NET Core lo hace automático vía `CreateBuilder`, pero como el server carga la config de ARCA manualmente con `dcConfigurationHelper.LoadFromJson(Path.Combine(..., "appsettings.json"))` con nombre fijo, hay que apuntarlo al archivo correcto por entorno. Ajustar esa línea en `dcArca.McpServer/Program.cs`:

```csharp
// antes:
// var arcaConfig = dcConfigurationHelper.LoadFromJson(
//     Path.Combine(builder.Environment.ContentRootPath, "appsettings.json"));

var arcaSettingsFile = builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing"
    ? $"appsettings.{builder.Environment.EnvironmentName}.json"
    : "appsettings.json";
var arcaConfig = dcConfigurationHelper.LoadFromJson(
    Path.Combine(builder.Environment.ContentRootPath, arcaSettingsFile));
```

Esto también cubre el caso normal: en `Development`/`Production` sigue leyendo `appsettings.json` (no hay `appsettings.Development.json` con sección `dcArcaConfig` propia en este plan), y en `Testing` lee el archivo con el certificado placeholder.

Marcar `appsettings.Testing.json` para copiarse al output, en `dcArca.McpServer/dcArca.McpServer.csproj`:

```xml
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Testing.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="test-cert-placeholder.pfx">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
```

- [ ] **Step 5: Correr el test y verificar que falla o no compila**

```bash
dotnet test dcArca.McpServer.Tests/dcArca.McpServer.Tests.csproj 2>&1 | tail -25
```

Expected en esta primera corrida: si `Program.cs` todavía no tenía la resolución por entorno del Step 4, el test falla al arrancar el host (`FileNotFoundException` buscando el `.pfx` real). Confirmar ese fallo antes de aplicar el ajuste de `Program.cs` de arriba.

- [ ] **Step 6: Aplicar el ajuste de `Program.cs` + csproj mostrados en el Step 4 y volver a correr**

```bash
dotnet test dcArca.McpServer.Tests/dcArca.McpServer.Tests.csproj 2>&1 | tail -25
```

Expected: `Passed!  - Failed: 0, Passed: 1, Skipped: 0`

- [ ] **Step 7: Commit**

```bash
git add dcArca.sln dcArca.McpServer.Tests dcArca.McpServer/appsettings.Testing.json \
        dcArca.McpServer/dcArca.McpServer.csproj dcArca.McpServer/Program.cs
git commit -m "test: verify MCP endpoint rejects unauthenticated requests"
```

- [ ] **Step 8: Confirmar que el placeholder de certificado no se commitea por error**

```bash
git status --short dcArca.McpServer/test-cert-placeholder.pfx
```

Expected: sin salida (el `.gitignore` ya excluye `*.pfx` globalmente) — si aparece como `A` (staged), sacarlo con `git restore --staged dcArca.McpServer/test-cert-placeholder.pfx` antes de commitear.

---

## Fuera de alcance (deliberado)

- **No se implementa el Authorization Server.** `Jwt:Authority`/`Jwt:Audience` asumen que Cadencia ya tiene (o va a elegir) un IdP externo que hable OIDC. Agregar uno acá sería construir infraestructura que no se pidió.
- **No se migra `dcArca.Core` a `net10.0`.** Compila limpio en `net8.0` con el SDK 10 instalado y un proyecto `net10.0` puede referenciarlo sin problema. Migrarlo es un cambio aparte, de cero riesgo pero también cero beneficio inmediato para este objetivo.
- **No se resuelven las alícuotas de IVA hardcodeadas ni la falta de file-lock en el cache de token entre procesos** (bugs #2 y #4 del análisis original) — son cambios de comportamiento más grandes, no bugs de una línea; si querés, van en un plan aparte.

---

## Self-Review

**Cobertura del pedido del usuario:**
- ✅ Corregir bug `DateTime.Now`/`UtcNow` → Task 3.
- ✅ Corregir paquetes NuGet muertos/vulnerables → Task 5.
- ✅ Corregir duplicación de validación de CUIT → Task 1.
- ✅ Corregir duplicación `CrearRespuestaValidacion`/`CrearRespuestaError` → Task 2.
- ✅ Corregir XML sin escapar → Task 4.
- ✅ Empaquetar el facturador detrás de un MCP → Tasks 6, 8, 9.
- ✅ Con OAuth → Task 8 (JWT Bearer + `.AddMcp()` resource metadata).
- ✅ Consumible por las distintas apps de Cadencia → MCP sobre HTTP, agnóstico de stack del cliente (confirmado con el usuario).

**No incluidos del análisis original, marcados explícitamente arriba:** alícuotas de IVA hardcodeadas, file-lock del cache de token, token/sign guardados en claro en disco. Se dejan fuera de este plan a propósito — no se pidieron y son cambios de comportamiento, no bugs puntuales.
