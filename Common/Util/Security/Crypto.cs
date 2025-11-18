using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NahidaImpact.Data.Models.Sdk;

namespace NahidaImpact.Util.Security;

public class Crypto
{
    private static readonly Random SecureRandom = new();
    private static readonly Logger _logger = new("Crypto");
    public static byte[] DISPATCH_KEY { get; private set; } = Array.Empty<byte>();
    public static byte[] DISPATCH_SEED { get; private set; } = Array.Empty<byte>();
    public static byte[] ENCRYPT_KEY { get; private set; } = Array.Empty<byte>();
    public static byte[] ENCRYPT_SEED_BUFFER { get; private set; } = Array.Empty<byte>();
    
    public static ulong ENCRYPT_SEED = ulong.Parse("11468049314633205968");
    
    public static RSA? SigningKey { get; private set; }
    public static Dictionary<int, RSA> EncryptionKeys { get; } = new();
    
    public static void LoadKeys()
    {
        try
        {
            // Load scheduling key
            DISPATCH_KEY = File.ReadAllBytes("Config/security/dispatchKey.bin");
            DISPATCH_SEED = File.ReadAllBytes("Config/security/dispatchSeed.bin");
            
            // Load encryption key
            ENCRYPT_KEY = File.ReadAllBytes("Config/security/secretKey.bin");
            ENCRYPT_SEED_BUFFER = File.ReadAllBytes("Config/security/secretKeyBuffer.bin");
            
            // Load signature private key
            var signingKeyBytes = File.ReadAllBytes("Config/security/SigningKey.der");
            SigningKey = RSA.Create();
            SigningKey.ImportPkcs8PrivateKey(signingKeyBytes, out _);
            
            // Load the game public key
            var gameKeysDir = "Config/security/game_keys";
            if (Directory.Exists(gameKeysDir))
            {
                var pattern = new Regex(@"([0-9]*)_Pub\.der");
                
                foreach (var file in Directory.GetFiles(gameKeysDir, "*_Pub.der"))
                {
                    var fileName = Path.GetFileName(file);
                    var match = pattern.Match(fileName);
                    
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int keyId))
                    {
                        var keyBytes = File.ReadAllBytes(file);
                        var rsa = RSA.Create();
                        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                        EncryptionKeys[keyId] = rsa;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while loading keys: {ex.Message}");
        }
    }
    
    public static byte[] Xor(string data, byte[] key)
    {
        byte[] result = Encoding.UTF8.GetBytes(data);
        Xor(result, key);

        return result;
    }

    public static void Xor(byte[] packet, byte[] key)
    {
        try {
            for (int i = 0; i < packet.Length; i++) {
                packet[i] ^= key[i % key.Length];
            }
        } catch (Exception e) {
            _logger.Error("Crypto error.", e);
        }
    }
    
    // Simple way to create a unique session key
    public static string CreateSessionKey(string accountUid)
    {
        var random = new byte[64];
        SecureRandom.NextBytes(random);

        var temp = accountUid + "." + DateTime.Now.Ticks + "." + SecureRandom;

        try
        {
            var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(temp));
            return Convert.ToBase64String(bytes);
        }
        catch
        {
            var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(temp));
            return Convert.ToBase64String(bytes);
        }
    }
    
    public static QueryCurRegionRspJson EncryptAndSignRegionData(byte[] regionInfo, string keyId)
    {
        if (string.IsNullOrEmpty(keyId))
            throw new ArgumentException("Key ID was not set", nameof(keyId));
        if (!int.TryParse(keyId, out int id))
            throw new ArgumentException("Invalid Key ID format", nameof(keyId));
        if (!EncryptionKeys.TryGetValue(id, out var publicKey))
            throw new KeyNotFoundException($"No encryption key found for ID: {keyId}");
        if (SigningKey == null)
            throw new InvalidOperationException("Signing key has not been initialized");
        
        // 分块加密
        const int chunkSize = 245; // 256 - 11
        int dataLength = regionInfo.Length;
        int numChunks = (int)Math.Ceiling(dataLength / (double)chunkSize);
        
        using var encryptedStream = new MemoryStream();
        for (int i = 0; i < numChunks; i++)
        {
            int offset = i * chunkSize;
            int length = Math.Min(chunkSize, dataLength - offset);
            var chunk = regionInfo.AsSpan(offset, length);
            
            byte[] encryptedChunk = publicKey.Encrypt(
                chunk.ToArray(), RSAEncryptionPadding.Pkcs1);
            
            encryptedStream.Write(encryptedChunk);
        }
        
        // 创建签名
        byte[] signature = SigningKey.SignData(
            regionInfo, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        
        return new QueryCurRegionRspJson
        {
            Content = Convert.ToBase64String(encryptedStream.ToArray()),
            Sign = Convert.ToBase64String(signature)
        };
    }
    
    public static RSA GetDispatchEncryptionKey(int key)
    {
        return EncryptionKeys[key];
    }

    public static ulong GenerateEncryptKeyAndSeed(byte[] encryptKey)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] seedBytes = new byte[8];
        rng.GetBytes(seedBytes);
        var encryptSeed = BitConverter.ToUInt64(seedBytes, 0);
        var mt = new MT19937(encryptSeed);
        var newSeed = mt.Int63();
        mt = new MT19937(newSeed);
        mt.Int63(); 
        for (int i = 0; i < 4096 >> 3; i++)
        {
            var rand = mt.Int63();
            encryptKey[i << 3] = (byte)(rand >> 56);
            encryptKey[(i << 3) + 1] = (byte)(rand >> 48);
            encryptKey[(i << 3) + 2] = (byte)(rand >> 40);
            encryptKey[(i << 3) + 3] = (byte)(rand >> 32);
            encryptKey[(i << 3) + 4] = (byte)(rand >> 24);
            encryptKey[(i << 3) + 5] = (byte)(rand >> 16);
            encryptKey[(i << 3) + 6] = (byte)(rand >> 8);
            encryptKey[(i << 3) + 7] = (byte)rand;
        }
    
        return encryptSeed;
    }
    
    public static byte[] GenerateSecretKey(ulong seed)
    {
        byte[] key = GC.AllocateUninitializedArray<byte>(0x1000);
        Span<byte> keySpan = key.AsSpan();

        MT19937 mt = new(seed);
        mt.Int63();

        for (int i = 0; i < 0x1000; i += 8)
        {
            BinaryPrimitives.WriteUInt64BigEndian(keySpan[i..], mt.Int63());
        }

        return key;
    }
}