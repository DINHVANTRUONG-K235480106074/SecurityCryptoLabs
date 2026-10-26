using System.Security.Cryptography;
using System.Text;
using RsaLab;

static string Ask(string message)
{
    Console.Write(message);
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

while (true)
{
    Console.WriteLine("\n=== BÀI 2 - RSA / SHA-256 / CHỮ KÝ SỐ ===");
    Console.WriteLine("1. Sinh cặp khóa RSA-2048 và xuất PEM");
    Console.WriteLine("2. View / phân tích PEM và tham số n,e,d,...");
    Console.WriteLine("3. Mã hóa dữ liệu <=190 byte -> Base64 file");
    Console.WriteLine("4. Giải mã Base64 file và so sánh nguồn");
    Console.WriteLine("5. Benchmark public encrypt vs private decrypt");
    Console.WriteLine("6. SHA-256 avalanche test");
    Console.WriteLine("7. RSA-PSS: ký 2 lần cùng dữ liệu và so sánh chữ ký");
    Console.WriteLine("0. Thoát");
    Console.Write("Chọn: ");

    switch (Console.ReadLine())
    {
        case "1":
        {
            var keys = RsaService.GenerateKeyPair();
            string pub = Ask("File public PEM (vd public.pem): ");
            string pri = Ask("File private PEM (vd private.pem): ");
            File.WriteAllText(pub, keys.PublicPem);
            File.WriteAllText(pri, keys.PrivatePem);
            Console.WriteLine("Đã sinh và lưu cặp khóa RSA-2048.");
            break;
        }
        case "2":
        {
            string pem = Ask("File PEM: ");
            Console.WriteLine(RsaService.DescribePem(pem));
            break;
        }
        case "3":
        {
            string pub = Ask("Public PEM: ");
            string src = Ask("File dữ liệu nguồn (<=190 byte): ");
            string dst = Ask("File Base64 mã hóa: ");
            byte[] data = File.ReadAllBytes(src);
            using RSA rsa = RsaService.ImportPem(pub);
            byte[] cipher = RsaService.Encrypt(data, rsa);
            File.WriteAllText(dst, Convert.ToBase64String(cipher));
            Console.WriteLine($"Nguồn: {data.Length} byte; Cipher: {cipher.Length} byte; đã lưu Base64.");
            break;
        }
        case "4":
        {
            string pri = Ask("Private PEM: ");
            string srcOriginal = Ask("File nguồn ban đầu để compare: ");
            string enc = Ask("File Base64 mã hóa: ");
            using RSA rsa = RsaService.ImportPem(pri);
            byte[] plain = RsaService.Decrypt(Convert.FromBase64String(File.ReadAllText(enc)), rsa);
            byte[] original = File.ReadAllBytes(srcOriginal);
            Console.WriteLine($"Giải mã: {plain.Length} byte");
            Console.WriteLine(plain.SequenceEqual(original) ? "KẾT QUẢ: dữ liệu giải mã GIỐNG nguồn." : "KẾT QUẢ: dữ liệu giải mã KHÁC nguồn.");
            break;
        }
        case "5":
        {
            string pub = Ask("Public PEM: ");
            string pri = Ask("Private PEM: ");
            int iterations = int.TryParse(Ask("Số vòng benchmark (vd 1000): "), out int n) ? n : 1000;
            using RSA publicRsa = RsaService.ImportPem(pub);
            using RSA privateRsa = RsaService.ImportPem(pri);
            byte[] data = RandomNumberGenerator.GetBytes(32); // mô phỏng khóa AES-256
            var r = RsaService.Benchmark(publicRsa, privateRsa, data, iterations);
            Console.WriteLine($"Public-key encrypt trung bình: {r.EncryptUs:F2} us/lần");
            Console.WriteLine($"Private-key decrypt trung bình: {r.DecryptUs:F2} us/lần");
            Console.WriteLine($"Tỷ lệ decrypt/encrypt: {r.DecryptUs / r.EncryptUs:F2}x");
            break;
        }
        case "6":
        {
            string a = Ask("Chuỗi A: ");
            string b = Ask("Chuỗi B (chỉ đổi rất ít): ");
            byte[] ha = SHA256.HashData(Encoding.UTF8.GetBytes(a));
            byte[] hb = SHA256.HashData(Encoding.UTF8.GetBytes(b));
            int diffBits = 0;
            for (int i = 0; i < ha.Length; i++) diffBits += System.Numerics.BitOperations.PopCount((uint)(ha[i] ^ hb[i]));
            Console.WriteLine($"SHA256(A): {Convert.ToHexString(ha)}");
            Console.WriteLine($"SHA256(B): {Convert.ToHexString(hb)}");
            Console.WriteLine($"Số bit hash khác nhau: {diffBits}/256 = {diffBits / 256.0:P2}");
            break;
        }
        case "7":
        {
            string pri = Ask("Private PEM: ");
            string pub = Ask("Public PEM: ");
            string text = Ask("Dữ liệu cần ký: ");
            byte[] data = Encoding.UTF8.GetBytes(text);
            using RSA privateRsa = RsaService.ImportPem(pri);
            using RSA publicRsa = RsaService.ImportPem(pub);
            byte[] s1 = RsaService.SignPss(data, privateRsa);
            byte[] s2 = RsaService.SignPss(data, privateRsa);
            Console.WriteLine($"Signature #1: {Convert.ToBase64String(s1)}");
            Console.WriteLine($"Signature #2: {Convert.ToBase64String(s2)}");
            Console.WriteLine($"Hai chữ ký giống nhau? {s1.SequenceEqual(s2)}");
            Console.WriteLine($"Verify #1: {RsaService.VerifyPss(data, s1, publicRsa)}");
            Console.WriteLine($"Verify #2: {RsaService.VerifyPss(data, s2, publicRsa)}");
            Console.WriteLine("RSA-PSS có salt ngẫu nhiên nên cùng dữ liệu + cùng private key vẫn tạo được chữ ký khác nhau.");
            break;
        }
        case "0": return;
        default: Console.WriteLine("Lựa chọn không hợp lệ."); break;
    }
}
