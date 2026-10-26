# SecurityCryptoLabs

Bài tập môn **An toàn và Bảo mật Thông tin**

Project gồm hai phần chính:

- Bài 1: Mã hóa đối xứng AES-256
- Bài 2: RSA-2048, SHA-256 và Chữ ký số RSA-PSS

Ngôn ngữ sử dụng: **C# / .NET 8**

---

# 1. AES-256

## 1.1. Mục tiêu

Xây dựng chương trình console thực hiện mã hóa đối xứng AES với các chức năng:

1. Sinh khóa AES-256.
2. Lưu khóa AES ra file.
3. Import khóa AES từ file.
4. Mã hóa và giải mã văn bản.
5. Mã hóa file.
6. Giải mã file.
7. Đo thời gian mã hóa và giải mã.
8. So sánh file nguồn và file sau giải mã theo từng byte.
9. Thử nghiệm với file có kích thước lớn.

---

## 1.2. Thuật toán

Project sử dụng:

- AES-256
- Chế độ CBC
- Padding PKCS7
- IV ngẫu nhiên cho mỗi lần mã hóa

AES là thuật toán mã hóa đối xứng, tức là quá trình mã hóa và giải mã sử dụng cùng một khóa bí mật.

Khóa AES-256 có kích thước:

```text
256 bit = 32 byte
```

---

## 1.3. Cấu trúc dữ liệu file mã hóa

File mã hóa được lưu theo cấu trúc:

```text
Header + IV + Ciphertext
```

IV được sinh ngẫu nhiên cho mỗi lần mã hóa.

Việc sử dụng IV giúp cùng một dữ liệu và cùng một khóa vẫn có thể tạo ra ciphertext khác nhau ở các lần mã hóa khác nhau.

---

## 1.4. Xử lý file lớn

Chương trình sử dụng `FileStream` kết hợp với `CryptoStream`.

Do đó chương trình không cần đọc toàn bộ file vào RAM trước khi mã hóa hoặc giải mã.

Điều này giúp chương trình có thể xử lý các file có kích thước lớn, ví dụ hàng trăm MB hoặc hàng GB.

---

# 2. Kết quả thử nghiệm AES

## File 100 MB

| Thuộc tính | Kết quả |
|---|---:|
| Kích thước | 100 MB |
| Thời gian mã hóa | 0.204 s |
| Tốc độ mã hóa | 490.43 MiB/s |
| Thời gian giải mã | 0.124 s |
| Tốc độ giải mã | 807.93 MiB/s |
| So sánh byte | PASS |

## File 1 GB

| Thuộc tính | Kết quả |
|---|---:|
| Kích thước | 1 GB |
| Thời gian mã hóa | 1.808 s |
| Tốc độ mã hóa | 566.26 MiB/s |
| Thời gian giải mã | 1.115 s |
| Tốc độ giải mã | 918.68 MiB/s |
| So sánh byte | PASS |

---

## 2.1. Nhận xét kết quả AES

Kết quả cho thấy AES-256 có tốc độ xử lý cao.

Với file 100 MB, tốc độ mã hóa đạt khoảng **490.43 MiB/s**, trong khi tốc độ giải mã đạt khoảng **807.93 MiB/s**.

Với file 1 GB, tốc độ mã hóa đạt khoảng **566.26 MiB/s**, còn tốc độ giải mã đạt khoảng **918.68 MiB/s**.

Thời gian xử lý tăng gần tương ứng với kích thước dữ liệu.

Sau quá trình giải mã, chương trình thực hiện so sánh file nguồn và file giải mã theo từng byte.

Kết quả:

```text
Source file == Decrypted file
```

File sau giải mã giống hoàn toàn file nguồn.

### Kết luận AES

AES-256 phù hợp để mã hóa dữ liệu có kích thước lớn vì:

- Tốc độ xử lý cao.
- Có thể mã hóa file hàng GB.
- Không cần nạp toàn bộ file vào bộ nhớ.
- Dữ liệu sau giải mã được khôi phục chính xác.

---

# 3. RSA-2048

## 3.1. Mục tiêu

Chương trình RSA thực hiện:

1. Sinh cặp khóa RSA-2048.
2. Xuất public key và private key ra file PEM.
3. Đọc và phân tích file PEM.
4. Hiển thị các tham số RSA.
5. Mã hóa dữ liệu bằng public key.
6. Giải mã dữ liệu bằng private key.
7. So sánh dữ liệu nguồn và dữ liệu sau giải mã.
8. Benchmark tốc độ RSA.
9. Thử nghiệm SHA-256.
10. Thử nghiệm chữ ký số RSA-PSS.

---

# 4. RSA Public Key và Private Key

## Public Key

Public key bao gồm hai tham số chính:

```text
(n, e)
```

Trong đó:

- `n`: modulus.
- `e`: public exponent.

Trong thử nghiệm:

```text
e = 65537
```

Đây là giá trị public exponent rất phổ biến trong RSA.

---

## Private Key

Private key chứa nhiều tham số hơn:

```text
n
e
d
p
q
DP
DQ
InverseQ
```

Trong đó:

- `d`: private exponent.
- `p`, `q`: hai số nguyên tố được sử dụng để tạo modulus `n`.
- `DP`, `DQ`, `InverseQ`: các tham số phục vụ tối ưu phép tính RSA bằng Chinese Remainder Theorem.

---

# 5. Định dạng PEM

PEM là dạng biểu diễn Base64 của dữ liệu nhị phân DER/ASN.1.

Ví dụ public key:

```text
-----BEGIN PUBLIC KEY-----
...
-----END PUBLIC KEY-----
```

Private key:

```text
-----BEGIN PRIVATE KEY-----
...
-----END PRIVATE KEY-----
```

Public key chứa thông tin thuật toán RSA và các tham số công khai `(n, e)`.

Private key chứa toàn bộ thông tin bí mật của RSA.

---

# 6. Mã hóa RSA

Project sử dụng:

```text
RSA-2048
OAEP-SHA256
```

Với RSA-2048:

```text
2048 bit = 256 byte
```

Kích thước dữ liệu tối đa khi dùng OAEP với SHA-256 được tính:

```text
mLen <= k - 2*hLen - 2
```

Trong đó:

```text
k = 256 byte
hLen = 32 byte
```

Suy ra:

```text
256 - 2*32 - 2 = 190 byte
```

Do đó dữ liệu đầu vào phải có kích thước:

```text
<= 190 byte
```

Khóa AES-256 chỉ có:

```text
32 byte
```

nên RSA rất phù hợp để mã hóa khóa AES.

Đây cũng là cơ sở của mô hình mã hóa lai:

```text
RSA → mã hóa khóa AES

AES → mã hóa dữ liệu lớn
```

---

# 7. Kiểm tra mã hóa và giải mã RSA

Dữ liệu thử nghiệm được mã hóa bằng:

```text
Public Key
```

Sau đó giải mã bằng:

```text
Private Key
```

Chương trình so sánh dữ liệu sau giải mã với dữ liệu nguồn.

Kết quả:

```text
PASS
```

Dữ liệu sau giải mã giống hoàn toàn dữ liệu ban đầu.

---

# 8. Benchmark RSA

Benchmark được thực hiện với:

```text
RSA-2048
1000 vòng lặp
```

Kết quả:

| Thao tác | Thời gian trung bình |
|---|---:|
| Public-key Encrypt | 15.07 µs/lần |
| Private-key Decrypt | 432.36 µs/lần |

Tỷ lệ:

```text
Private Decrypt / Public Encrypt = 28.70x
```

---

## 8.1. Nhận xét benchmark RSA

Phép toán sử dụng public key nhanh hơn đáng kể so với private key.

Public key sử dụng exponent:

```text
e = 65537
```

Đây là một số tương đối nhỏ, giúp phép lũy thừa modular được thực hiện nhanh.

Trong khi đó, private exponent `d` có kích thước lớn hơn nhiều.

Vì vậy phép toán sử dụng private key yêu cầu nhiều phép tính hơn.

Trong thử nghiệm thực tế:

```text
Private-key decrypt chậm hơn khoảng 28.70 lần.
```

---

# 9. SHA-256

SHA-256 là hàm băm mật mã tạo ra kết quả có kích thước:

```text
256 bit
```

Một đặc tính quan trọng của hàm băm mật mã là **Avalanche Effect**.

Điều này có nghĩa là một thay đổi rất nhỏ ở dữ liệu đầu vào sẽ làm kết quả hash thay đổi rất lớn.

---

## 9.1. Thử nghiệm Avalanche Effect

Hai chuỗi được sử dụng:

```text
hello123
hello124
```

Chúng chỉ khác nhau một ký tự.

Kết quả chương trình tính được:

```text
Số bit hash khác nhau = 127 / 256
```

Tỷ lệ:

```text
49.61%
```

---

## 9.2. Nhận xét

Mặc dù đầu vào chỉ thay đổi một ký tự nhưng gần một nửa số bit trong kết quả SHA-256 thay đổi.

Kết quả thực nghiệm:

```text
127 / 256 = 49.61%
```

Điều này thể hiện rõ hiệu ứng avalanche của SHA-256.

---

# 10. Chữ ký số RSA-PSS

Chữ ký số RSA được tạo bằng private key và được kiểm tra bằng public key.

Mô hình:

```text
Data
  ↓
SHA-256
  ↓
Hash
  ↓
RSA Private Key
  ↓
Digital Signature
```

Trong project sử dụng:

```text
RSA-PSS + SHA-256
```

---

# 11. Yêu cầu đặc biệt

Yêu cầu:

```text
Cùng dữ liệu
+
Cùng người ký
+
Cùng private key
=
Chữ ký khác nhau ở các lần ký
```

Nếu sử dụng RSA PKCS#1 v1.5, chữ ký thường có tính xác định.

Do đó cùng dữ liệu và cùng private key sẽ tạo ra cùng chữ ký.

Để giải quyết yêu cầu này, project sử dụng:

```text
RSA-PSS
```

---

# 12. Vì sao RSA-PSS tạo chữ ký khác nhau?

RSA-PSS sử dụng một giá trị:

```text
Random Salt
```

trong quá trình tạo chữ ký.

Do salt được tạo ngẫu nhiên ở mỗi lần ký nên:

```text
Data giống nhau
+
Private Key giống nhau
+
Salt khác nhau
=
Signature khác nhau
```

Tuy nhiên các chữ ký vẫn có thể được xác minh bằng cùng public key.

---

# 13. Kết quả thử nghiệm RSA-PSS

Dữ liệu thử nghiệm:

```text
hello123
```

Cùng dữ liệu được ký hai lần bằng cùng `private.pem`.

Kết quả:

```text
Signature #1 != Signature #2
```

Tuy nhiên:

```text
Verify #1: True
Verify #2: True
```

Điều này chứng minh RSA-PSS đáp ứng yêu cầu:

> Cùng dữ liệu và cùng người ký nhưng chữ ký số ở các lần ký khác nhau.

---

# 14. Cấu trúc Project

```text
SecurityCryptoLabs
│
├── AesLab
│   ├── AesLab.csproj
│   ├── Program.cs
│   └── AesService.cs
│
├── RsaLab
│   ├── RsaLab.csproj
│   ├── Program.cs
│   └── RsaService.cs
│
├── README.md
└── .gitignore
```

---

# 15. Yêu cầu môi trường

Project sử dụng:

```text
.NET 8
```

Kiểm tra phiên bản:

```bash
dotnet --version
```

---

# 16. Chạy bài AES

```bash
cd AesLab
dotnet run
```

Menu:

```text
1. Sinh khóa AES-256
2. Lưu khóa ra file
3. Import khóa từ file
4. Mã hóa / giải mã văn bản
5. Mã hóa file + đo thời gian
6. Giải mã file + đo thời gian
7. So sánh byte-by-byte 2 file
0. Thoát
```

---

# 17. Chạy bài RSA

```bash
cd RsaLab
dotnet run
```

Menu:

```text
1. Sinh cặp khóa RSA-2048 và xuất PEM
2. View / phân tích PEM và tham số n,e,d,...
3. Mã hóa dữ liệu <=190 byte
4. Giải mã và so sánh nguồn
5. Benchmark public encrypt vs private decrypt
6. SHA-256 avalanche test
7. RSA-PSS
0. Thoát
```

---

# 18. Kết luận chung

Qua bài thực hành có thể thấy AES và RSA có các đặc điểm rất khác nhau.

AES là mã hóa đối xứng, có tốc độ cao và phù hợp với việc mã hóa dữ liệu lớn.

RSA là mã hóa bất đối xứng, tốc độ thấp hơn AES nhưng giải quyết được bài toán phân phối khóa và chữ ký số.

Trong hệ thống thực tế, AES và RSA thường được sử dụng kết hợp:

```text
RSA → bảo vệ khóa AES

AES → mã hóa dữ liệu lớn

SHA-256 → kiểm tra tính toàn vẹn

RSA-PSS → chữ ký số
```

Kết quả thực nghiệm cho thấy:

- AES xử lý file 1 GB với tốc độ hàng trăm MiB/s.
- File AES sau giải mã giống hoàn toàn file nguồn.
- RSA public operation nhanh hơn private operation đáng kể.
- SHA-256 thể hiện rõ avalanche effect với 49.61% số bit thay đổi.
- RSA-PSS cho phép cùng dữ liệu và cùng private key tạo ra chữ ký khác nhau ở mỗi lần ký nhưng vẫn xác minh thành công.