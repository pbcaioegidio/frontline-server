#pragma once

#include <cstdint>
#include <filesystem>
#include <string>
#include <vector>

namespace ConfigFileEncrypter
{
    inline constexpr uint8_t InitialFeedback = 0xA7;
    inline constexpr uint8_t RotateBits = 3;

    inline constexpr const char* CryptoKey = "PointBlank.Config.Security";
    inline constexpr const char* ConfigXmlFileName = "config.xml";
    inline constexpr const char* ConfigEncryptedFileName = "config.zpt";
    inline constexpr const char* LauncherXmlFileName = "Launcher.xml";
    inline constexpr const char* LauncherEncryptedFileName = "launcher.svl";

    uint8_t RotateLeft(uint8_t value, int bits);
    std::vector<uint8_t> ReadAllBytes(const std::filesystem::path& path);
    void WriteAllBytes(const std::filesystem::path& path, const std::vector<uint8_t>& bytes);
    std::vector<uint8_t> Encrypt(const std::vector<uint8_t>& plainBytes);
    void EncryptFile(const std::filesystem::path& inputPath, const std::filesystem::path& outputPath);
    void EncryptLauncherFiles(const std::filesystem::path& baseFolder);
}
