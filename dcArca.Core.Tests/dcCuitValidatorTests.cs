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
