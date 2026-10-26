using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AesLab;

public static class AesService
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("AES1");

    public static byte[] GenerateKey()
    {
        byte[] key = new byte[32]; // AES-256
        RandomNumberGenerator.Fill(key);
        return key;
    }

    public static void SaveKey(string path, byte[] key)
    {
        File.WriteAllText(path, Convert.ToBase64String(key));
    }

    public static byte[] LoadKey(string path)
    {
        byte[] key = Convert.FromBase64String(File.ReadAllText(path).Trim());
        if (key.Length is not (16 or 24 or 32))
            throw new CryptographicException("AES key phải dài 16, 24 hoặc 32 byte.");
        return key;
    }

    public static string EncryptText(string plainText, byte[] key)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using MemoryStream output = new();
        output.Write(aes.IV);
        using (CryptoStream crypto = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        using (StreamWriter writer = new(crypto, Encoding.UTF8))
        {
            writer.Write(plainText);
        }
        return Convert.ToBase64String(output.ToArray());
    }

    public static string DecryptText(string base64Cipher, byte[] key)
    {
        byte[] all = Convert.FromBase64String(base64Cipher);
        if (all.Length < 16) throw new CryptographicException("Ciphertext không hợp lệ.");

        byte[] iv = all[..16];
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using MemoryStream input = new(all, 16, all.Length - 16);
        using CryptoStream crypto = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader reader = new(crypto, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static TimeSpan EncryptFile(string inputPath, string outputPath, byte[] key)
    {
        const int bufferSize = 1024 * 1024; // 1 MiB buffer; không nạp cả file vào RAM

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        Stopwatch sw = Stopwatch.StartNew();
        using FileStream input = new(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
        using FileStream output = new(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan);

        output.Write(Magic);
        output.Write(aes.IV); // Header = 4-byte magic + 16-byte IV

        using (CryptoStream crypto = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            input.CopyTo(crypto, bufferSize);
            crypto.FlushFinalBlock();
        }

        output.Flush(flushToDisk: true);
        sw.Stop();
        return sw.Elapsed;
    }

    public static TimeSpan DecryptFile(string inputPath, string outputPath, byte[] key)
    {
        const int bufferSize = 1024 * 1024;
        Stopwatch sw = Stopwatch.StartNew();

        using FileStream input = new(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
        Span<byte> magic = stackalloc byte[4];
        if (input.Read(magic) != 4 || !magic.SequenceEqual(Magic))
            throw new CryptographicException("File không đúng định dạng AES1.");

        byte[] iv = new byte[16];
        if (input.Read(iv, 0, iv.Length) != iv.Length)
            throw new CryptographicException("File thiếu IV.");

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using FileStream output = new(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan);
        using CryptoStream crypto = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        crypto.CopyTo(output, bufferSize);
        output.Flush(flushToDisk: true);

        sw.Stop();
        return sw.Elapsed;
    }

    public static (bool Equal, long FirstDifferentOffset) CompareFilesByBytes(string fileA, string fileB)
    {
        FileInfo a = new(fileA);
        FileInfo b = new(fileB);
        if (a.Length != b.Length) return (false, Math.Min(a.Length, b.Length));

        const int bufferSize = 1024 * 1024;
        byte[] ba = new byte[bufferSize];
        byte[] bb = new byte[bufferSize];
        long offset = 0;

        using FileStream fa = File.OpenRead(fileA);
        using FileStream fb = File.OpenRead(fileB);
        while (true)
        {
            int ra = fa.Read(ba, 0, ba.Length);
            int rb = fb.Read(bb, 0, bb.Length);
            if (ra != rb) return (false, offset);
            if (ra == 0) return (true, -1);

            for (int i = 0; i < ra; i++)
            {
                if (ba[i] != bb[i]) return (false, offset + i);
            }
            offset += ra;
        }
    }
}
