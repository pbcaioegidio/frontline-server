#include <iostream>
#include <fstream>
#include <sstream>
#include <iomanip>
#include <string>
#include <vector>
#include <cstdint>
#include <cstring>
#include <ctime>
#include <algorithm>
#include <filesystem>
#include <zlib.h>

namespace fs = std::filesystem;

struct Config {
    std::string clientVersion = "1.0.0";
    std::string patchUrl = "http://localhost/patch/";
    std::string sourcePath;
    std::string outputPath;
    std::string jsonFile = "patchlist.json";
};

struct FileEntry {
    std::string relativePath;
    std::string zipRelative;
    uintmax_t   size = 0;
    std::string md5;
};

namespace md5_impl {
    inline uint32_t rotl(uint32_t x, int n) { return (x << n) | (x >> (32 - n)); }

    static const uint32_t K[64] = {
        0xd76aa478,0xe8c7b756,0x242070db,0xc1bdceee,0xf57c0faf,0x4787c62a,
        0xa8304613,0xfd469501,0x698098d8,0x8b44f7af,0xffff5bb1,0x895cd7be,
        0x6b901122,0xfd987193,0xa679438e,0x49b40821,0xf61e2562,0xc040b340,
        0x265e5a51,0xe9b6c7aa,0xd62f105d,0x02441453,0xd8a1e681,0xe7d3fbc8,
        0x21e1cde6,0xc33707d6,0xf4d50d87,0x455a14ed,0xa9e3e905,0xfcefa3f8,
        0x676f02d9,0x8d2a4c8a,0xfffa3942,0x8771f681,0x6d9d6122,0xfde5380c,
        0xa4beea44,0x4bdecfa9,0xf6bb4b60,0xbebfbc70,0x289b7ec6,0xeaa127fa,
        0xd4ef3085,0x04881d05,0xd9d4d039,0xe6db99e5,0x1fa27cf8,0xc4ac5665,
        0xf4292244,0x432aff97,0xab9423a7,0xfc93a039,0x655b59c3,0x8f0ccc92,
        0xffeff47d,0x85845dd1,0x6fa87e4f,0xfe2ce6e0,0xa3014314,0x4e0811a1,
        0xf7537e82,0xbd3af235,0x2ad7d2bb,0xeb86d391
    };
    static const int S[64] = {
        7,12,17,22,7,12,17,22,7,12,17,22,7,12,17,22,
        5, 9,14,20,5, 9,14,20,5, 9,14,20,5, 9,14,20,
        4,11,16,23,4,11,16,23,4,11,16,23,4,11,16,23,
        6,10,15,21,6,10,15,21,6,10,15,21,6,10,15,21
    };

    void process_block(uint32_t s[4], const uint8_t* blk) {
        uint32_t M[16];
        for (int i = 0; i < 16; ++i)
            M[i] = uint32_t(blk[i * 4]) | (uint32_t(blk[i * 4 + 1]) << 8)
            | (uint32_t(blk[i * 4 + 2]) << 16) | (uint32_t(blk[i * 4 + 3]) << 24);
        uint32_t a = s[0], b = s[1], c = s[2], d = s[3];
        for (int i = 0; i < 64; ++i) {
            uint32_t F; int g;
            if (i < 16) { F = (b & c) | (~b & d); g = i; }
            else if (i < 32) { F = (d & b) | (~d & c); g = (5 * i + 1) % 16; }
            else if (i < 48) { F = b ^ c ^ d;        g = (3 * i + 5) % 16; }
            else { F = c ^ (b | ~d);     g = (7 * i) % 16; }
            uint32_t t = d; d = c; c = b;
            b = b + rotl(a + F + K[i] + M[g], S[i]);
            a = t;
        }
        s[0] += a;s[1] += b;s[2] += c;s[3] += d;
    }

    std::string hash_file(const std::string& path) {
        std::ifstream f(path, std::ios::binary);
        if (!f) return "";
        uint32_t s[4] = { 0x67452301,0xefcdab89,0x98badcfe,0x10325476 };
        uint8_t buf[64];
        uint64_t total = 0;
        while (f) {
            f.read(reinterpret_cast<char*>(buf), 64);
            std::streamsize n = f.gcount();
            if (n <= 0) break;
            total += uint64_t(n);
            if (n == 64) { process_block(s, buf); continue; }
            uint64_t bits = total * 8;
            buf[n] = 0x80;
            if (n < 56) {
                std::fill(buf + n + 1, buf + 56, 0);
                for (int i = 0;i < 8;++i) buf[56 + i] = uint8_t(bits >> (i * 8));
                process_block(s, buf);
            }
            else {
                std::fill(buf + n + 1, buf + 64, 0);
                process_block(s, buf);
                uint8_t last[64] = {};
                for (int i = 0;i < 8;++i) last[56 + i] = uint8_t(bits >> (i * 8));
                process_block(s, last);
            }
        }
        std::ostringstream ss;
        ss << std::hex << std::setfill('0');
        for (int i = 0;i < 4;++i)
            for (int b = 0;b < 4;++b)
                ss << std::setw(2) << ((s[i] >> (b * 8)) & 0xff);
        return ss.str();
    }
}

namespace zip_writer {

    void write16(std::ostream& o, uint16_t v) {
        uint8_t b[2] = { uint8_t(v),uint8_t(v >> 8) };
        o.write(reinterpret_cast<char*>(b), 2);
    }
    void write32(std::ostream& o, uint32_t v) {
        uint8_t b[4] = { uint8_t(v),uint8_t(v >> 8),uint8_t(v >> 16),uint8_t(v >> 24) };
        o.write(reinterpret_cast<char*>(b), 4);
    }
    void dos_datetime(uint16_t& date, uint16_t& time_val) {
        std::time_t t = std::time(nullptr);
        std::tm tm{};
        localtime_s(&tm, &t);
        date = uint16_t(((tm.tm_year - 80) << 9) | ((tm.tm_mon + 1) << 5) | tm.tm_mday);
        time_val = uint16_t((tm.tm_hour << 11) | (tm.tm_min << 5) | (tm.tm_sec / 2));
    }
    std::vector<uint8_t> deflate_data(const std::vector<uint8_t>& src) {
        z_stream zs{};
        deflateInit2(&zs, Z_BEST_COMPRESSION, Z_DEFLATED, -15, 8, Z_DEFAULT_STRATEGY);
        std::vector<uint8_t> out(src.size() + 64);
        zs.next_in = const_cast<Bytef*>(src.data());
        zs.avail_in = static_cast<uInt>(src.size());
        zs.next_out = out.data();
        zs.avail_out = static_cast<uInt>(out.size());
        deflate(&zs, Z_FINISH);
        out.resize(zs.total_out);
        deflateEnd(&zs);
        return out;
    }
    bool create(const std::string& zip_path,
        const std::string& src_file,
        const std::string& filename_inside) {

        std::ifstream in(src_file, std::ios::binary);
        if (!in) return false;
        std::vector<uint8_t> raw(
            (std::istreambuf_iterator<char>(in)),
            std::istreambuf_iterator<char>()
        );

        uint32_t crc = crc32(0, raw.data(), static_cast<uInt>(raw.size()));
        uint32_t uncomp_sz = static_cast<uint32_t>(raw.size());

        std::vector<uint8_t> comp = deflate_data(raw);
        uint32_t comp_sz = static_cast<uint32_t>(comp.size());

        uint16_t dos_date, dos_time;
        dos_datetime(dos_date, dos_time);

        uint16_t fname_len = static_cast<uint16_t>(filename_inside.size());

        std::ofstream out(zip_path, std::ios::binary);
        if (!out) return false;

        uint32_t local_offset = 0;
        write32(out, 0x04034b50);
        write16(out, 20);
        write16(out, 0);
        write16(out, 8);
        write16(out, dos_time);
        write16(out, dos_date);
        write32(out, crc);
        write32(out, comp_sz);
        write32(out, uncomp_sz);
        write16(out, fname_len);
        write16(out, 0);
        out.write(filename_inside.c_str(), fname_len);

        out.write(reinterpret_cast<char*>(comp.data()), comp_sz);

        uint32_t cd_offset = static_cast<uint32_t>(out.tellp());
        write32(out, 0x02014b50);
        write16(out, 20);
        write16(out, 20);
        write16(out, 0);
        write16(out, 8);
        write16(out, dos_time);
        write16(out, dos_date);
        write32(out, crc);
        write32(out, comp_sz);
        write32(out, uncomp_sz);
        write16(out, fname_len);
        write16(out, 0);            // extra len
        write16(out, 0);            // comment len
        write16(out, 0);            // disk start
        write16(out, 0);            // internal attr
        write32(out, 0);            // external attr
        write32(out, local_offset); // offset of local header
        out.write(filename_inside.c_str(), fname_len);

        uint32_t cd_size = static_cast<uint32_t>(out.tellp()) - cd_offset;

        // ---- End of Central Directory ----
        write32(out, 0x06054b50);
        write16(out, 0);            // disk number
        write16(out, 0);            // disk with CD
        write16(out, 1);            // entries on disk
        write16(out, 1);            // total entries
        write32(out, cd_size);
        write32(out, cd_offset);
        write16(out, 0);            // comment len

        return true;
    }
}

// ============================================================
//  JSON builder
// ============================================================
std::string escape_json(const std::string& s) {
    std::ostringstream o;
    for (char c : s) {
        if (c == '"')  o << "\\\"";
        else if (c == '\\') o << "\\\\";
        else if (c == '\n') o << "\\n";
        else if (c == '\r') o << "\\r";
        else              o << c;
    }
    return o.str();
}

void generate_json(const Config& cfg, const std::vector<FileEntry>& files) {
    std::ostringstream j;
    j << "{\n";
    j << "  \"clientVersion\": \"" << escape_json(cfg.clientVersion) << "\",\n";
    j << "  \"patchUrl\": \"" << escape_json(cfg.patchUrl) << "\",\n";
    j << "  \"files\": [\n";
    for (size_t i = 0; i < files.size(); ++i) {
        const auto& f = files[i];
        j << "    {\n";
        j << "      \"path\": \"" << escape_json(f.relativePath) << "\",\n";
        j << "      \"zip\":  \"" << escape_json(f.zipRelative) << "\",\n";
        j << "      \"size\": " << f.size << ",\n";
        j << "      \"md5\":  \"" << f.md5 << "\"\n";
        j << "    }";
        if (i + 1 < files.size()) j << ",";
        j << "\n";
    }
    j << "  ]\n}\n";

    std::ofstream out(cfg.jsonFile);
    if (!out) { std::cout << "\n  [ERRO] Nao foi possivel criar: " << cfg.jsonFile << "\n"; return; }
    out << j.str();

    // preview
    std::cout << "\n  [OK] " << cfg.jsonFile << " gerado com " << files.size() << " arquivo(s).\n\n";
    std::istringstream prev(j.str());
    std::string line; int n = 0;
    while (std::getline(prev, line) && n++ < 14)
        std::cout << "  " << line << "\n";
    if (n == 14) std::cout << "  ...\n";
}

std::vector<FileEntry> process_directory(const fs::path& srcDir,
    const fs::path& outDir,
    const fs::path& baseDir) {
    std::vector<FileEntry> entries;

    std::error_code ec;
    for (const auto& entry : fs::recursive_directory_iterator(srcDir, ec)) {
        if (!entry.is_regular_file()) continue;

        fs::path abs = entry.path();
        std::string rel = fs::relative(abs, baseDir).generic_string();

        if (!rel.empty() && rel[0] != '/') rel = "/" + rel;

        fs::path relDir = fs::relative(abs.parent_path(), baseDir);
        fs::path zipDir = outDir / relDir;
        fs::create_directories(zipDir, ec);

        std::string zipName = abs.filename().string() + ".zip";
        fs::path    zipFull = zipDir / zipName;
        std::string zipRelStr = (relDir / zipName).generic_string();
        if (!zipRelStr.empty() && zipRelStr[0] != '/') zipRelStr = "/" + zipRelStr;

        FileEntry fe;
        fe.relativePath = rel;
        fe.zipRelative = zipRelStr;
        fe.size = entry.file_size();
        fe.md5 = md5_impl::hash_file(abs.string());

        bool ok = zip_writer::create(zipFull.string(), abs.string(), abs.filename().string());
        std::cout << (ok ? "  File: " : "  [ERRO] ") << abs.string() << "\n";

        entries.push_back(std::move(fe));
    }
    return entries;
}

void clear_screen() {
#ifdef _WIN32
    system("cls");
#else
    system("clear");
#endif
}

void print_header() {
    std::cout
        << " LAUNCHER UPDATER GENERATOR\n"
        << " \n\n";
}

void print_menu(const Config& c) {
    std::cout << " Current configuration\n\n";
    std::cout << " Client Version: " << c.clientVersion << "\n";
    std::cout << " Patch URL: " << c.patchUrl << "\n";
    std::cout << " Source folder: " << (c.sourcePath.empty() ? "(undefined)" : c.sourcePath) << "\n";
    std::cout << " Output folder: " << (c.outputPath.empty() ? "(undefined)" : c.outputPath) << "\n";
    std::cout << " JSON name: " << c.jsonFile << "\n";
    std::cout << "\n";
    std::cout << "";
    std::cout << " [1] Compress files\n";
    std::cout << " [2] Generate JSON\n";
    std::cout << "";
    std::cout << " [C] Change settings\n";
    std::cout << " [0] Exit\n\n";
    std::cout << " Options: ";
}

void print_config_menu(const Config& c) {
    std::cout << "\n  Settings:\n\n";
    std::cout << " [1] Client Version: " << c.clientVersion << "\n";
    std::cout << " [2] Patch URL: " << c.patchUrl << "\n";
    std::cout << " [3] Source folder: " << (c.sourcePath.empty() ? "(empty)" : c.sourcePath) << "\n";
    std::cout << " [4] Output folder: " << (c.outputPath.empty() ? "(empty)" : c.outputPath) << "\n";
    std::cout << " [5] JSON name: " << c.jsonFile << "\n";
    std::cout << " [0] Return\n\n";
    std::cout << " Options: ";
}

std::string prompt(const std::string& msg) {
    std::cout << "\n  " << msg;
    std::string s;
    std::getline(std::cin >> std::ws, s);
    if (s.size() >= 2 && s.front() == '"' && s.back() == '"')
        s = s.substr(1, s.size() - 2);
    return s;
}

void wait_enter() {
    std::cout << "\n  Press ENTER to continue...";
    std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
}

bool validate(const Config& cfg, bool need_output = false) {
    if (cfg.sourcePath.empty()) {
        std::cout << "\n  [ERROR] Define the source folder in [C] > [3].\n";
        return false;
    }
    if (!fs::exists(cfg.sourcePath)) {
        std::cout << "\n  [ERROR] Source folder not found: " << cfg.sourcePath << "\n";
        return false;
    }
    if (need_output && cfg.outputPath.empty()) {
        std::cout << "\n  [ERROR] Define the output folder in[C] > [4].\n";
        return false;
    }
    return true;
}

int main() {
    Config cfg;
    std::vector<FileEntry> cached_entries;

    while (true) {
        clear_screen();
        print_header();
        print_menu(cfg);

        std::string opt; std::getline(std::cin, opt);
        if (opt.empty()) continue;
        char ch = (char)std::toupper((unsigned char)opt[0]);

        // ---- Sair ----
        if (ch == '0') {
            std::cout << "\n  Ate mais!\n\n";
            break;

            // ---- Opcao 1: Compactar ----
        }
        else if (ch == '1') {
            if (!validate(cfg, true)) { wait_enter(); continue; }

            fs::path outDir = fs::path(cfg.outputPath);
            std::error_code ec;
            fs::create_directories(outDir, ec);

            std::cout << "\n  Compressing files...\n\n";
            cached_entries = process_directory(
                fs::path(cfg.sourcePath),
                outDir,
                fs::path(cfg.sourcePath)
            );
            std::cout << "\n  [OK] " << cached_entries.size() << "compressed file(s) in: " << cfg.outputPath << "\n";
            wait_enter();

            // ---- Opcao 2: Gerar JSON ----
        }
        else if (ch == '2') {
            if (!validate(cfg)) { wait_enter(); continue; }

            // Se nao compactou antes, escaneia agora (sem criar ZIPs)
            if (cached_entries.empty()) {
                std::cout << "\n  No previous scans. Scanning folder to assemble JSON...\n\n";
                std::error_code ec;
                for (const auto& entry : fs::recursive_directory_iterator(cfg.sourcePath, ec)) {
                    if (!entry.is_regular_file()) continue;
                    fs::path abs = entry.path();
                    std::string rel = fs::relative(abs, cfg.sourcePath).generic_string();
                    if (!rel.empty() && rel[0] != '/') rel = "/" + rel;

                    fs::path relDir = fs::relative(abs.parent_path(), cfg.sourcePath);
                    std::string zipN = abs.filename().string() + ".zip";
                    std::string zipR = (relDir / zipN).generic_string();
                    if (!zipR.empty() && zipR[0] != '/') zipR = "/" + zipR;

                    FileEntry fe;
                    fe.relativePath = rel;
                    fe.zipRelative = zipR;
                    fe.size = entry.file_size();
                    fe.md5 = md5_impl::hash_file(abs.string());
                    std::cout << "  + " << rel << "\n";
                    cached_entries.push_back(std::move(fe));
                }
            }

            if (cached_entries.empty()) {
                std::cout << "\n[WARNING] No files found.\n";
                wait_enter(); continue;
            }

            generate_json(cfg, cached_entries);
            wait_enter();

            // ---- Configuracoes ----
        }
        else if (ch == 'C') {
            while (true) {
                clear_screen();
                print_header();
                print_config_menu(cfg);
                std::string c2; std::getline(std::cin, c2);
                if (c2.empty()) continue;
                char cc = (char)std::toupper((unsigned char)c2[0]);
                if (cc == '0') break;
                else if (cc == '1') cfg.clientVersion = prompt("New Version: ");
                else if (cc == '2') cfg.patchUrl = prompt("New URL Patch: ");
                else if (cc == '3') { cfg.sourcePath = prompt("Source folder: "); cached_entries.clear(); }
                else if (cc == '4') cfg.outputPath = prompt("Output folder: ");
                else if (cc == '5') { cfg.jsonFile = prompt("JSON name: "); if (cfg.jsonFile.empty()) cfg.jsonFile = "patchlist.json"; }
            }
        }
    }
    return 0;
}