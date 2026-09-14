using System.Security.Cryptography;
using System.Text;
using Icap.Domain.Services;
using Microsoft.Extensions.Options;

namespace Icap.Infrastructure.Security;

/// <summary>
/// Genera el QRHash firmando (HMAC-SHA256) el folio + el id del recibo con
/// la misma clave de JWT, y lo codifica en Base64Url. Así el frontend puede
/// pintar el QR con angularx-qrcode, y en un futuro escáner de validación
/// se puede re-firmar y comparar para detectar recibos falsificados/alterados.
/// </summary>
public sealed class QrHashGenerator : IQrHashGenerator
{
    private readonly JwtSettings _jwtSettings;

    public QrHashGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string Generate(string folioNumber, string receiptId)
    {
        var payload = $"{folioNumber}:{receiptId}";
        var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        var signature = Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        return $"{folioNumber}.{signature}";
    }
}
