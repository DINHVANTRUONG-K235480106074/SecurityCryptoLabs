using AesLab;

static string Ask(string message)
{
    Console.Write(message);
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

byte[]? currentKey = null;

while (true)
{
    Console.WriteLine("\n=== BÀI 1 - AES-256 CBC ===");
    Console.WriteLine("1. Sinh khóa AES-256");
    Console.WriteLine("2. Lưu khóa ra file");
    Console.WriteLine("3. Import khóa từ file");
    Console.WriteLine("4. Mã hóa / giải mã văn bản");
    Console.WriteLine("5. Mã hóa file + đo thời gian");
    Console.WriteLine("6. Giải mã file + đo thời gian");
    Console.WriteLine("7. So sánh byte-by-byte 2 file");
    Console.WriteLine("0. Thoát");
    Console.Write("Chọn: ");

    switch (Console.ReadLine())
    {
        case "1":
            currentKey = AesService.GenerateKey();
            Console.WriteLine($"Key Base64: {Convert.ToBase64String(currentKey)}");
            break;

        case "2":
            if (currentKey is null) { Console.WriteLine("Chưa có khóa."); break; }
            string saveKey = Ask("Đường dẫn lưu key (vd aes-key.txt): ");
            AesService.SaveKey(saveKey, currentKey);
            Console.WriteLine("Đã lưu khóa.");
            break;

        case "3":
            currentKey = AesService.LoadKey(Ask("File key: "));
            Console.WriteLine($"Đã import AES-{currentKey.Length * 8}.");
            break;

        case "4":
            if (currentKey is null) { Console.WriteLine("Chưa có khóa."); break; }
            string text = Ask("Nhập văn bản: ");
            string cipher = AesService.EncryptText(text, currentKey);
            Console.WriteLine($"Cipher Base64:\n{cipher}");
            Console.WriteLine($"Giải mã lại:\n{AesService.DecryptText(cipher, currentKey)}");
            break;

        case "5":
            if (currentKey is null) { Console.WriteLine("Chưa có khóa."); break; }
            string inFile = Ask("File nguồn: ");
            string encFile = Ask("File mã hóa đầu ra: ");
            var et = AesService.EncryptFile(inFile, encFile, currentKey);
            double encMB = new FileInfo(inFile).Length / 1024d / 1024d;
            Console.WriteLine($"Mã hóa: {et.TotalSeconds:F3} s; tốc độ: {encMB / et.TotalSeconds:F2} MiB/s");
            break;

        case "6":
            if (currentKey is null) { Console.WriteLine("Chưa có khóa."); break; }
            string encIn = Ask("File đã mã hóa: ");
            string decOut = Ask("File giải mã đầu ra: ");
            var dt = AesService.DecryptFile(encIn, decOut, currentKey);
            double decMB = new FileInfo(decOut).Length / 1024d / 1024d;
            Console.WriteLine($"Giải mã: {dt.TotalSeconds:F3} s; tốc độ: {decMB / dt.TotalSeconds:F2} MiB/s");
            break;

        case "7":
            string a = Ask("File A: ");
            string b = Ask("File B: ");
            var cmp = AesService.CompareFilesByBytes(a, b);
            Console.WriteLine(cmp.Equal ? "KẾT QUẢ: GIỐNG NHAU 100% theo byte." : $"KẾT QUẢ: KHÁC NHAU tại offset {cmp.FirstDifferentOffset}.");
            break;

        case "0": return;
        default: Console.WriteLine("Lựa chọn không hợp lệ."); break;
    }
}
