# Especificaciones técnicas de Servicios Web – WS_SR_constancia_inscripcion

## Historial de modificaciones

| Ver | Fecha | Edición | Descripción |
|---|---|---|---|
| 1.0 | 13/01/17 | DINTR | Versión inicial del documento |
| 2.0 | 27/11/17 | DINTR | Cambio de nombre del WS de ws_sr_padron_a5 a ws_sr_constancia_inscripcion |
| 3.0 | 27/11/18 | DINTR | Nuevo método getPersonaList |
| 3.1 | 08/02/19 | DINTR | Se documenta, por ejemplo, error en getPersonaList cuando no existe el CUIT |
| 3.2 | 08/03/19 | DINTR | Se agrega la descripción del campo impuesto en datosMonotributo |
| 3.3 | 17/10/19 | DINTR | Se agregan los metodos getPersona_v2 y getPersonaList_v2 |
| 3.4 | 02/05/23 | DI SIRE | Agregado de anexo con listado de errores por validaciones y se incorpora el dato de persona fallecida en Datos Generales |
| 3.5 | 13/12/2023 | DI SIRE | Se elimina la referencia a "TIPO COMPONENTE - Componentes de la persona jurídica monotributista por no estar vigente. |
| 3.6 | 31/01/2025 | DI SIRE | Se agrega al impuesto: la descripción del motivo (`<motivo>`) y el id del estado (`<estadoImpuesto>`) |
| 3.7 | 04/07/2025 | DI SIRE | Se incorpora mensaje de error al punto 5.3 |

---

## Contenido (Indice)

- [INTRODUCCIÓN](#1-introducción)
- [ALCANCE](#11-alcance)
- [DEFINICIONES](#12-definiciones-siglas-y-abreviaturas)
- [WEB SERVICES](#2-web-services)
    - [SITIO DE CONSULTA Y CANAL DE ATENCIÓN](#21-sitio-de-consulta-y-canal-de-atención)
    - [AUTENTICACIÓN](#22-autenticación)
    - [URLS](#23-urls)
    - [ID DEL SERVICIO](#24-id-del-servicio)
- [MÉTODOS](#3-métodos)
    - [DUMMY (VERIFICACIÓN DEL SERVICIO)](#31-dummy-verificación-del-servicio)
    - [MÉTODO GETPERSONA_V2](#32-método-getpersona_v2)
    - [MÉTODO GETPERSONALIST_V2](#método-getpersonalist_v2)
- [DEFINICIONES DE TIPOS DE DATOS](#definiciones-de-tipos-de-datos)
    - [TIPOS DE DATOS SIMPLES](#tipos-de-datos-simples)
    - [TIPOS DE DATOS COMPLEJOS](#tipos-de-datos-complejos)
- [ANEXOS](#anexos)
    - [VALORES TIPOREGIMEN](#valores-tiporegimen)
    - [VALORES TIPODATOADICIONAL](#valores-tipodatoadicional)
    - [VALIDACIONES Y MENSAJES DE ERROR](#validaciones-y-mensajes-de-error)

---

## 1 Introducción

El servicio de Consulta de la Constancia de Inscripción de Padrón, antes llamado de Alcance 5 (ws_sr_padron_a5), permite que un organismo externo acceda a los datos de la constancia de un contribuyente registrado en el Padrón de ARCA.

La consulta se realiza mediante un webService SOAP que básicamente recibe como parámetro una CUIT y responde, con los datos que constituyen la constancia de inscripción, del contribuyente identificado con la misma.

Este documento está dirigido a quienes tengan la misión de probar y utilizar este webService.

Para tener acceso a este webService el organismo usuario debe obtener un ticket de acceso.

El proceso de obtención del ticket de acceso esta fuera del alcance de este documento.

### 1.1 Alcance

Este WS se puede utilizar para acceder a datos de un contribuyente relacionados con su constancia de inscripción.

### 1.2 Definiciones, Siglas y Abreviaturas

- **SOAP**: Simple Object Access Protocol
- **WSDL**: Web Services Definition Language
- **WSAA**: Web Service de Autenticación y Autorización de ARCA
- **WSPCI**: Web Service de Padrón Constancia de Inscripción
- **CE**: Cliente externo usuario de los webServices de ARCA
- **CUIT**: Clave Unica de Identificación Tributaria. Campo numérico de 11 dígitos que identificada unívocamente a un contribuyente.
- **SSO**: Ticket para poder acceder a los webServices de ARCA. Son generados por WSAA.
- **SUPA**: Sistema único de parámetros

---

## 2. Web Services

### 2.1. Sitio de consulta y canal de atención

Para consultas acerca de la arquitectura de Web Services, autenticación y autorización dirigirse a `http://www.arca.gob.ar/ws/`.

Las consultas sobre aspectos técnicos del WS deberán ser remitidas a la cuenta `sri@arca.gob.ar`. Para su mejor tratamiento, se solicita detallar en el asunto la denominación del WS y ambiente de que se trate (Producción y Homologación), como así también adjuntar request y response.

Para consultar propias del negocio o normativas, contactarse mediante el sitio `www.arca.gob.ar/consultas`.

### 2.2. Autenticación

Para la utilización de los métodos el webService, a excepción del dummy, se debe enviar en cada solicitud, el token y el sign, información que es obtenida del WSAA (Web Service de Autenticación y Autorización), en respuesta a una solicitud de ticket de acceso.

### 2.3. URLs:

| Descripción | URL |
|---|---|
| Conexión al servicio en ambiente de Testing | `https://awshomo.arca.gov.ar/sr-padron/webservices/personaServiceA5` |
| WSDLdel servicio en ambiente de Testing | `https://awshomo.arca.gov.ar/sr-padron/webservices/personaServiceA5?WSDL` |
| Conexión al servicio en ambiente de Producción| `https://aws.arca.gov.ar/sr-padron/webservices/personaServiceA5` |
| WSDL del servicio en ambiente de Producción| `https://aws.arca.gov.ar/sr-padron/webservices/personaServiceA5?WSDL` |

### 2.4. ID del Servicio

El id del servicio es `ws_sr_constancia_inscripcion`. El mismo es el nombre de servicio que se deberá usar al solicitar a WSAA el Ticket de Acceso.

---

## 3. Métodos

### 3.1. dummy (Verificación del servicio)

- **Nombre método**: dummy
- **Descripción**: El método dummy verifica el estado y la disponibilidad de los elementos principales del servicio (aplicación, autenticación y base de datos).

#### 3.1.1. Solicitud:

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
xmlns:a5="http://a5.soap.ws.server.puc.sr/">
   <soapenv:Header/>
   <soapenv:Body>
      <a5:dummy/>
   </soapenv:Body>
</soapenv:Envelope>
```

#### 3.1.2. Respuesta:

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <ns2:dummyResponse xmlns:ns2="http://a5.soap.ws.server.puc.sr/">
         <return>
            <appserver>?</appserver>
            <authserver>?</authserver>
            <dbserver>?</dbserver>
         </return>
      </ns2:dummyResponse>
   </soap:Body>
</soap:Envelope>
```
Donde `dummyResponse` es del tipo `dummyResponse` definido en el WSDL y contiene la etiqueta return del tipo `dummyReturn`.
Los valores de los atributos `appserver`, `authserver` y `dbserver` pueden ser OK o, en caso de falla, ERROR.

#### 3.1.3. Ejemplo:

**Invocación del método:**

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
xmlns:a5="http://a5.soap.ws.server.puc.sr/">
   <soapenv:Header/>
   <soapenv:Body>
      <a5:dummy/>
   </soapenv:Body>
</soapenv:Envelope>
```

**Respuesta del método:**

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <ns2:dummyResponse xmlns:ns2="http://a5.soap.ws.server.puc.sr/">
         <return>
            <appserver>OK</appserver>
            <authserver>OK</authserver>
            <dbserver>OK</dbserver>
         </return>
      </ns2:dummyResponse>
   </soap:Body>
</soap:Envelope>
```

### 3.2. Método getPersona_v2

- **Nombre método**: getPersona_v2
- **Descripción**: Devuelve el detalle de todos los datos, correspondientes a la constancia de inscripción, del contribuyente solicitado.

#### 3.2.1. Solicitud

**Esquema:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
xmlns:a5="http://a5.soap.ws.server.puc.sr/">
   <soapenv:Header/>
   <soapenv:Body>
      <a5:getPersona_v2>
         <token>?</token>
         <sign>?</sign>
         <cuitRepresentada>?</cuitRepresentada>
         <idPersona>?</idPersona>
      </a5:getPersona_v2>
   </soapenv:Body>
</soapenv:Envelope>
```
Donde `a5:getPersona_v2` es del tipo `getPersona_v2` y engloba los parámetros de entrada:
- **token y sign**: Los mismos son devueltos por el web service de autenticación WSAA.
- **cuitRepresentada**: Debe coincidir con alguna de las CUITS listadas en la sección relations del token enviado. Debe ser en representación de que organismo se solicita la operación.
- **idPersona**: Es la clave de la que se solicitan los datos.

#### 3.2.2. Respuesta

**Esquema:**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <ns2:getPersona_v2Response xmlns:ns2="http://a5.soap.ws.server.puc.sr/">
         <personaReturn>
            <metadata>
               <fechaHora>?</fechaHora>
               <servidor>?</servidor>
            </metadata>
            <datosGenerales>
              <apellido>...</apellido>
              <domicilioFiscal> ... </domicilioFiscal>
              <esSucesion>- -SI - -</esSucesion>
              <estadoClave>...</estadoClave>
              <idPersona>...</idPersona>
              <mesCierre>..</mesCierre>
              <nombre>...</nombre>
              <tipoClave>...</tipoClave>
              <tipoPersona>...</tipoPersona>
            </datosGenerales>
            <datosRegimenGeneral>
              <actividad>... </actividad>
              <impuesto>... </impuesto>
           </datosRegimenGeneral>
             <datosMonotributo>...</datosMonotributo>
            <errorConstancia>....</errorConstancia>
            <errorRegimenGeneral>....</errorRegimenGeneral>
            <errorMonotributo>....</errorMonotributo>
        </personaReturn>
      </ns2:getPersona_v2Response>
   </soap:Body>
</soap:Envelope>
```
Donde `getPersona_v2Response`, `personaReturn`, `metadata`, `datosGenerales`, `datosRegimenGeneral`, `datosMonotributo`, `errorConstancia`, `errorRegimenGeneral` y `errorMonotributo` son del tipo del mismo nombre, definidos en el WSDL del servicio.

#### 3.2.3. Ejemplo

**Invocación del método:**
```xml
<?xml version="1.0"?>
<soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
<soap-env:Body>
<ns0:getPersona xmlns:ns0="http://a5.soap.ws.server.puc.sr/">
<token>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU
9InllcyI/
Pgo8c3NvIHZlcnNpb249IjIuMCI+CiAgICA8aWQgc3JjPSJDTj13c2FhaG9tbywgTz1BRklQL
CBDPUFSLCBTRVJJQUxOVU1CRVI9Q1VJVCAzMzY5MzQ1MDIzOSIgdW5pcXVlX2lkP
SIzNjU3MDc2ODA3IiBnZW5fdGltZT0iMTY4MzA1NDYyMiIgZXhwX3RpbWU9IjE2ODMwO
Tc4ODIiLz4KICAgIDxvcGVyYXRpb24gdHlwZT0ibG9naW4iIHZhbHVlPSJncmFudGVkIj4KI
CAgICAgICA8bG9naW4gZW50aXR5PSIzMzY5MzQ1MDIzOSIgc2VydmljZT0id3Nfc3JfcG
Fkcm9uX2E1IiB1aWQ9IlNFUklBTE5VTUJFUj1DVUlUIDI3Mjk4NjcyNDc4LCBDTj1waXBlc
yIgYXV0aG1ldGhvZD0iY21zIiByZWdtZXRob2Q9IjIyIj4KICAgICAgICAgICAgPHJlbGF0aW
9ucz4KICAgICAgICAgICAgICAgIDxyZWxhdGlvbiBrZXk9IjI3Mjk4NjcyNDc4IiByZWx0eXBl
PSI0Ii8+CiAgICAgICAgICAgIDwvcmVsYXRpb25zPgogICAgICAgIDwvbG9naW4+CiAgICA
8L29wZXJhdGlvbj4KPC9zc28+Cg==</token>
<sign>gl51bULQ9MnY29OoCi3GlCs4uBIlr5V7tdcEhQD0Jnwe0i6rfYdoqb4Xhx4SxHs+GN
Ctv+xUCASkkzYtx5puMY9ict9zpsYRMGQb93VwnyQXumn1ExPAAqd4YiVCXJhFVvREco
8IbtYrpDgPKiH0UiZjNj7fWpNTnqojy+kS8Eg=</sign>
<cuitRepresentada>27298672478</cuitRepresentada>
<idPersona>20201731594</idPersona>
</ns0:getPersona>
</soap-env:Body>
</soap-env:Envelope>
```

**Respuesta del ejemplo:**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
 <soap:Body>
 <ns2:getPersonaResponse xmlns:ns2="http://a5.soap.ws.server.puc.sr/">
 <personaReturn>
 <datosGenerales>
 <apellido>MICHELLE ELIZABETH</apellido>
 <domicilioFiscal>
 <codPostal>5000</codPostal>
 <descripcionProvincia>CORDOBA</descripcionProvincia>
 <direccion>SANTA FE 7516</direccion>
 <idProvincia>3</idProvincia>
 <localidad>BARRIO YAPEYU *</localidad>
 <tipoDomicilio>FISCAL</tipoDomicilio>
 </domicilioFiscal>
 <esSucesion>SI</esSucesion>
 <estadoClave>ACTIVO</estadoClave>
 <idPersona>20201731594</idPersona>
 <mesCierre>12</mesCierre>
 <nombre>FELIX</nombre>
 <tipoClave>CUIT</tipoClave>
 <tipoPersona>FISICA</tipoPersona>
 </datosGenerales>
 <datosRegimenGeneral>
 <actividad>
 <idActividad>11121</idActividad>
 <nomenclador>883</nomenclador>
 <orden>1</orden>
 <periodo>201409</periodo>
 </actividad>
 <impuesto>
 <descripcionImpuesto>DERECHO ESPECIFICO</descripcionImpuesto>
 <estadoImpuesto>AC</estadoImpuesto>
 <idImpuesto>2015</idImpuesto>
 <motivo>INSCRIPCIÓN TRAMITADA EN AGENCIA</motivo>
 <periodo>201801</periodo>
 </impuesto>
 </datosRegimenGeneral>
 <metadata>
 <fechaHora>2023-05-02T16:40:52.693-03:00</fechaHora>
 <servidor>setiwsh2</servidor>
 </metadata>
 </personaReturn>
 </ns2:getPersonaResponse>
 </soap:Body>
</soap:Envelope>
```
*¿Y el método getPersona?* Este sigue estando para conservar la compatibilidad con soluciones ya desarrolladas pero alentamos a la adopción de este nuevo método que incluye todas las actividades del monotributista y las caracterizaciones vigentes.
