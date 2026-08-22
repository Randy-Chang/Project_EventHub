using QRCoder;

namespace EventHub.Presentation;

public sealed class QrCodePngGenerator
{
    public byte[] Generate(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("QR Code payload 不可為空白。", nameof(payload));
        }

        return PngByteQRCodeHelper.GetQRCode(
            payload,
            QRCodeGenerator.ECCLevel.Q,
            8,
            true);
    }
}
