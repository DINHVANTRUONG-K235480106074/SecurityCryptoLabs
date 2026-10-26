# SecurityCryptoLabs – AES, RSA, SHA-256 và chữ ký số (.NET 8)

Repo phục vụ bài tập môn **An toàn và bảo mật thông tin**.

## Cấu trúc

- `AesLab/`: AES-256 CBC, sinh/import/export key, mã hóa text/file, benchmark file lớn, compare byte-by-byte.
- `RsaLab/`: RSA-2048, PEM, xem tham số khóa, OAEP-SHA256, benchmark RSA, SHA-256 avalanche effect, chữ ký RSA-PSS.

## Yêu cầu

- .NET 8 SDK
- Windows/Linux/macOS đều có thể chạy console app.

## Chạy bài 1

```bash
cd AesLab
dotnet run
```

### Thử nghiệm file hàng GB

Nên tạo file test riêng trên máy, ví dụ Windows PowerShell:

```powershell
fsutil file createnew bigfile.bin 2147483648
```

Hoặc Linux/macOS:

```bash
dd if=/dev/urandom of=bigfile.bin bs=1M count=2048
```

> Lưu ý: file tạo bằng `fsutil` có thể là dữ liệu zero/sparse tùy hệ thống; để benchmark I/O thực tế nên dùng dữ liệu không nén/sparse hoặc file thực tế lớn.

Quy trình demo:
1. Sinh AES key.
2. Lưu key.
3. Mã hóa `bigfile.bin` thành `bigfile.aes`.
4. Giải mã thành `bigfile.dec.bin`.
5. Chọn compare `bigfile.bin` và `bigfile.dec.bin`.
6. Ghi lại thời gian mã hóa, giải mã, throughput MiB/s.

### Vì sao xử lý được file hàng GB?

Chương trình dùng `FileStream + CryptoStream + CopyTo` với buffer 1 MiB, tức xử lý theo luồng (streaming), **không đọc toàn bộ file vào RAM**.

### File format AES của project

```text
4 bytes  : ASCII "AES1"
16 bytes : IV
N bytes  : AES-CBC ciphertext (PKCS#7 padding)
```

Key AES-256 được lưu Base64 riêng. IV không phải bí mật và được lưu cùng ciphertext.

## Chạy bài 2

```bash
cd RsaLab
dotnet run
```

### RSA-2048 + OAEP-SHA256 và giới hạn 190 byte

Với RSA-2048:
- modulus = 2048 bit = 256 byte
- SHA-256 output = 32 byte
- OAEP giới hạn: `k - 2*hLen - 2 = 256 - 64 - 2 = 190 byte`

Vì vậy một khóa AES-256 chỉ 32 byte hoàn toàn phù hợp để mã hóa bằng RSA. Đây chính là mô hình **hybrid encryption**: RSA bảo vệ khóa AES, còn AES mã hóa dữ liệu lớn.

### PEM

PEM là DER/ASN.1 đã Base64 hóa và bọc bởi dòng `-----BEGIN ...-----` / `-----END ...-----`.

Project xuất:
- Public: SubjectPublicKeyInfo (`PUBLIC KEY`), chứa thông tin thuật toán và RSA public key `(n, e)`.
- Private: PKCS#8 (`PRIVATE KEY`), chứa `(n, e, d, p, q, dP, dQ, qInv)`.

Menu `View PEM` import PEM rồi hiển thị các thành phần RSA dưới dạng hex.

## Benchmark RSA

Benchmark dùng cùng 32 byte dữ liệu và đo:
- mã hóa bằng **public key** `(n, e)`
- giải mã bằng **private key** `(n, d, ...)`

Thông thường public operation nhanh hơn vì `e` thường nhỏ (hay gặp 65537), còn private operation phức tạp hơn. Tốc độ cụ thể phụ thuộc CPU, hệ điều hành và implementation crypto, vì vậy phải kết luận dựa trên số liệu máy chạy thực tế.

## SHA-256 avalanche effect

Menu SHA-256 nhận 2 chuỗi chỉ khác nhau rất ít, tính 2 hash 256-bit, sau đó đếm số bit khác nhau. Kỳ vọng thực nghiệm: thay đổi rất nhỏ ở input có thể làm khoảng một nửa số bit output thay đổi.

## Chữ ký số: làm sao cùng dữ liệu + cùng private key nhưng chữ ký mỗi lần khác nhau?

Dùng **RSA-PSS** thay vì RSA PKCS#1 v1.5 deterministic signature.

RSA-PSS đưa **salt ngẫu nhiên** vào quá trình tạo signature. Vì vậy:

```text
same data + same private key + different random salt
=> different valid signatures
```

Cả hai chữ ký vẫn được verify thành công bằng cùng public key.

Project dùng:

```csharp
privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
```

## Mẫu bảng kết quả báo cáo AES

| File size | Encrypt time | Encrypt speed | Decrypt time | Decrypt speed | Byte compare |
|---:|---:|---:|---:|---:|---|
| 100 MB | ... s | ... MiB/s | ... s | ... MiB/s | Equal |
| 1 GB | ... s | ... MiB/s | ... s | ... MiB/s | Equal |
| 2 GB | ... s | ... MiB/s | ... s | ... MiB/s | Equal |

### Gợi ý kết luận AES

- AES phù hợp với dữ liệu/file lớn vì tốc độ cao và có thể xử lý streaming.
- Thời gian thường tăng gần tuyến tính theo dung lượng file, nhưng chịu ảnh hưởng của tốc độ ổ đĩa/cache/CPU.
- Nếu byte compare trả `Equal`, dữ liệu sau giải mã khôi phục đúng dữ liệu nguồn.
- Không nên suy diễn tốc độ tuyệt đối giữa các máy; ghi rõ cấu hình CPU/RAM/ổ đĩa khi báo cáo.

## Mẫu bảng kết quả báo cáo RSA

| Key size | Input | Iterations | Public encrypt avg | Private decrypt avg | Ratio |
|---:|---:|---:|---:|---:|---:|
| 2048 bit | 32 bytes | 1000 | ... us | ... us | ...x |

### Gợi ý kết luận RSA

- RSA chậm hơn AES nhiều và chỉ mã hóa được dữ liệu ngắn.
- Với OAEP-SHA256 và RSA-2048, input tối đa 190 byte.
- RSA phù hợp để mã hóa khóa đối xứng hoặc tạo/kiểm tra chữ ký, không phù hợp để mã hóa trực tiếp file hàng GB.
- Public operation thường nhanh hơn private operation; hãy dùng số liệu benchmark trên máy để minh họa.

## Gợi ý commit GitHub

```bash
git init
git add .
git commit -m "feat: AES encryption lab"
git add .
git commit -m "feat: RSA SHA256 and digital signature lab"
```

Không commit private key dùng thật lên GitHub. Private key demo có thể sinh lại khi chạy chương trình.
