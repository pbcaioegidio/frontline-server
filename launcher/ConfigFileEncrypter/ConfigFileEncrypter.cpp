#include "ConfigFileEncrypter.h"
#include <fstream>
#include <iostream>
#include <stdexcept>

namespace fs = std::filesystem;

namespace ConfigFileEncrypter
{
    uint8_t RotateLeft(uint8_t value, int bits)
    {
        return static_cast<uint8_t>((value << bits) | (value >> (8 - bits)));
    }
    std::vector<uint8_t> ReadAllBytes(const fs::path& path)
    {
        std::ifstream input(path, std::ios::binary);
        if (!input)
            throw std::runtime_error("nao foi possivel abrir " + path.string());

        input.seekg(0, std::ios::end);
        const std::streamsize size = input.tellg();
        input.seekg(0, std::ios::beg);

        std::vector<uint8_t> buffer(static_cast<size_t>(size));
        if (size > 0 && !input.read(reinterpret_cast<char*>(buffer.data()), size))
            throw std::runtime_error("nao foi possivel ler " + path.string());

        return buffer;
    }
    void WriteAllBytes(const fs::path& path, const std::vector<uint8_t>& bytes)
    {
        std::ofstream output(path, std::ios::binary | std::ios::trunc);
        if (!output)
            throw std::runtime_error("nao foi possivel criar " + path.string());

        if (!bytes.empty())
            output.write(reinterpret_cast<const char*>(bytes.data()), static_cast<std::streamsize>(bytes.size()));
    }
    std::vector<uint8_t> Encrypt(const std::vector<uint8_t>& plainBytes)
    {
        std::vector<uint8_t> encrypted;
        encrypted.reserve(plainBytes.size());

        uint8_t feedback = InitialFeedback;

        for (size_t i = 0; i < plainBytes.size(); i++)
        {
            const uint8_t key = static_cast<uint8_t>(CryptoKey[i % std::char_traits<char>::length(CryptoKey)]);
            const uint8_t positionMask = static_cast<uint8_t>((i * 37) + 0x5A);
            const uint8_t mask = static_cast<uint8_t>(key + positionMask + feedback);
            const uint8_t value = RotateLeft(static_cast<uint8_t>(plainBytes[i] ^ mask), RotateBits);

            encrypted.push_back(value);
            feedback = value;
        }

        return encrypted;
    }
    void EncryptFile(const fs::path& inputPath, const fs::path& outputPath)
    {
        if (!fs::exists(inputPath))
            throw std::runtime_error("arquivo nao encontrado: " + inputPath.string());

        const std::vector<uint8_t> plainBytes = ReadAllBytes(inputPath);
        const std::vector<uint8_t> encrypted = Encrypt(plainBytes);

        WriteAllBytes(outputPath, encrypted);

        std::cout << "OK: " << inputPath.filename().string()
                  << " -> " << outputPath.filename().string()
                  << " (" << encrypted.size() << " bytes)\n";
    }
    void EncryptLauncherFiles(const fs::path& baseFolder)
    {
        EncryptFile(baseFolder / ConfigXmlFileName, baseFolder / ConfigEncryptedFileName);
        EncryptFile(baseFolder / LauncherXmlFileName, baseFolder / LauncherEncryptedFileName);
    }
}
int main(int argc, char* argv[])
{
    try
    {
        const fs::path baseFolder = argc > 1
            ? fs::path(argv[1])
            : fs::current_path();

        ConfigFileEncrypter::EncryptLauncherFiles(baseFolder);

        return 0;
    }
    catch (const std::exception& ex)
    {
        std::cerr << "Erro: " << ex.what() << '\n';
        return 1;
    }
}
