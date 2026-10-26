using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace RsaLab;

public static class RsaService
{
    public static (string PublicPem, string PrivatePem) GenerateKeyPair(int bits = 2048)
    {
        using RSA rsa = RSA.Create();
        rsa.KeySize = bits;
        return (rsa.ExportSubjectPublicKeyInfoPem(), rsa.ExportPkcs8PrivateKeyPem());
    }

    public static RSA ImportPem(string path)
    {
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(path));
        return rsa;
    }

    public static string DescribePem(string path)
    {
        string pem = File.ReadAllText(path);
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        bool hasPrivate;
        RSAParameters p;
        try { p = rsa.ExportParameters(true); hasPrivate = p.D is not null; }
        catch { p = rsa.ExportParameters(false); hasPrivate = false; }

        StringBuilder sb = new();
        sb.AppendLine(hasPrivate ? "Loại: RSA PRIVATE KEY / PRIVATE KEY" : "Loại: RSA PUBLIC KEY / PUBLIC KEY");
        sb.AppendLine($"Modulus n: {ToHex(p.Modulus)}");
        sb.AppendLine($"Exponent e: {ToHex(p.Exponent)} ({BytesToUInt(p.Exponent)})");

        if (hasPrivate)
        {
            sb.AppendLine($"Private exponent d: {ToHex(p.D)}");
            sb.AppendLine($"P: {ToHex(p.P)}");
            sb.AppendLine($"Q: {ToHex(p.Q)}");
            sb.AppendLine($"DP: {ToHex(p.DP)}");
            sb.AppendLine($"DQ: {ToHex(p.DQ)}");
            sb.AppendLine($"InverseQ: {ToHex(p.InverseQ)}");
        }

        sb.AppendLine();
        sb.AppendLine("PEM = Base64 của cấu trúc DER/ASN.1, bọc giữa BEGIN/END.");
        sb.AppendLine("Public key SubjectPublicKeyInfo chứa thuật toán RSA + RSAPublicKey(n,e).");
        sb.AppendLine("Private key PKCS#8 chứa thông tin thuật toán + RSAPrivateKey(n,e,d,p,q,dP,dQ,qInv).");
        return sb.ToString();
    }

    public static byte[] Encrypt(byte[] data, RSA publicKey)
    {
        int max = MaxOaepSha256InputBytes(publicKey.KeySize);
        if (data.Length > max)
            throw new ArgumentException($"Dữ liệu quá dài. RSA-{publicKey.KeySize} + OAEP-SHA256 tối đa {max} byte.");
        return publicKey.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] Decrypt(byte[] cipher, RSA privateKey) =>
        privateKey.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);

    public static int MaxOaepSha256InputBytes(int keyBits)
    {
        int k = keyBits / 8;
        int hLen = 32; // SHA-256
        return k - 2 * hLen - 2;
    }

    public static string Sha256Base64(byte[] data) => Convert.ToBase64String(SHA256.HashData(data));
    public static string Sha256Hex(byte[] data) => Convert.ToHexString(SHA256.HashData(data));

    public static byte[] SignPss(byte[] data, RSA privateKey) =>
        privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    public static bool VerifyPss(byte[] data, byte[] signature, RSA publicKey) =>
        publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    public static (double EncryptUs, double DecryptUs) Benchmark(RSA publicKey, RSA privateKey, byte[] data, int iterations = 1000)
    {
        byte[] cipher = Encrypt(data, publicKey);

        // warm-up
        _ = Encrypt(data, publicKey);
        _ = Decrypt(cipher, privateKey);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) _ = Encrypt(data, publicKey);
        sw.Stop();
        double encUs = sw.Elapsed.TotalMilliseconds * 1000 / iterations;

        sw.Restart();
        for (int i = 0; i < iterations; i++) _ = Decrypt(cipher, privateKey);
        sw.Stop();
        double decUs = sw.Elapsed.TotalMilliseconds * 1000 / iterations;

        return (encUs, decUs);
    }

    private static string ToHex(byte[]? data) => data is null ? "(none)" : Convert.ToHexString(data);

    private static ulong BytesToUInt(byte[]? bytes)
    {
        if (bytes is null) return 0;
        ulong n = 0;
        foreach (byte b in bytes) n = (n << 8) | b;
        return n;
    }
}
