using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Server.Game.Rcon
{
    public abstract class RconReceive
    {
        private string[] _parts;
        private Dictionary<string, object> _jsonData;
        private int _offset = 2;
        private bool _isJsonMode = false;

        /// <summary>
        /// Initialize the packet with legacy pipe-delimited format
        /// </summary>
        /// <param name="Parts"></param>
        public void Init(string[] Parts)
        {
            _parts = Parts;
            _isJsonMode = false;
            _offset = 2; // Reset offset
        }

        /// <summary>
        /// Initialize the packet with JSON format
        /// </summary>
        /// <param name="jsonData"></param>
        public void InitJson(Dictionary<string, object> jsonData)
        {
            _jsonData = jsonData;
            _isJsonMode = true;
        }

        /// <summary>
        /// Get string value by key (JSON mode) or by sequential pop (legacy mode)
        /// </summary>
        public string PopString()
        {
            if (_isJsonMode)
            {
                throw new InvalidOperationException("Use PopString(key) method in JSON mode");
            }

            if (_parts == null || _offset >= _parts.Length)
            {
                throw new IndexOutOfRangeException("No more parts available to pop");
            }

            string Next = _parts[_offset]; // Corregido: era *parts[*offset]
            _offset++;
            return Next;
        }

        /// <summary>
        /// Get string value by key (JSON mode only)
        /// </summary>
        public string PopString(string key)
        {
            if (!_isJsonMode)
            {
                throw new InvalidOperationException("Use PopString() method in legacy mode");
            }

            if (_jsonData == null || !_jsonData.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key '{key}' not found in JSON data");
            }

            return _jsonData[key]?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Get int value by sequential pop (legacy mode)
        /// </summary>
        public int PopInt()
        {
            if (_isJsonMode)
            {
                throw new InvalidOperationException("Use PopInt(key) method in JSON mode");
            }

            if (_parts == null || _offset >= _parts.Length)
            {
                throw new IndexOutOfRangeException("No more parts available to pop");
            }

            int Next = int.Parse(_parts[_offset]);
            _offset++;
            return Next;
        }

        /// <summary>
        /// Get int value by key (JSON mode only)
        /// </summary>
        public int PopInt(string key)
        {
            if (!_isJsonMode)
            {
                throw new InvalidOperationException("Use PopInt() method in legacy mode");
            }

            if (_jsonData == null || !_jsonData.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key '{key}' not found in JSON data");
            }

            if (int.TryParse(_jsonData[key]?.ToString(), out int result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{_jsonData[key]}' to int");
        }

        /// <summary>
        /// Get uint value by sequential pop (legacy mode)
        /// </summary>
        public uint PopUint()
        {
            if (_isJsonMode)
            {
                throw new InvalidOperationException("Use PopUint(key) method in JSON mode");
            }

            if (_parts == null || _offset >= _parts.Length)
            {
                throw new IndexOutOfRangeException("No more parts available to pop");
            }

            uint Next = uint.Parse(_parts[_offset]);
            _offset++;
            return Next;
        }

        /// <summary>
        /// Get uint value by key (JSON mode only)
        /// </summary>
        public uint PopUint(string key)
        {
            if (!_isJsonMode)
            {
                throw new InvalidOperationException("Use PopUint() method in legacy mode");
            }

            if (_jsonData == null || !_jsonData.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key '{key}' not found in JSON data");
            }

            if (uint.TryParse(_jsonData[key]?.ToString(), out uint result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{_jsonData[key]}' to uint");
        }

        /// <summary>
        /// Get long value by sequential pop (legacy mode)
        /// </summary>
        public long PopLong()
        {
            if (_isJsonMode)
            {
                throw new InvalidOperationException("Use PopLong(key) method in JSON mode");
            }

            if (_parts == null || _offset >= _parts.Length)
            {
                throw new IndexOutOfRangeException("No more parts available to pop");
            }

            long Next = long.Parse(_parts[_offset]);
            _offset++;
            return Next;
        }

        /// <summary>
        /// Get long value by key (JSON mode only)
        /// </summary>
        public long PopLong(string key)
        {
            if (!_isJsonMode)
            {
                throw new InvalidOperationException("Use PopLong() method in legacy mode");
            }

            if (_jsonData == null || !_jsonData.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key '{key}' not found in JSON data");
            }

            if (long.TryParse(_jsonData[key]?.ToString(), out long result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{_jsonData[key]}' to long");
        }

        /// <summary>
        /// Check if running in JSON mode
        /// </summary>
        public bool IsJsonMode => _isJsonMode;

        /// <summary>
        /// True when the JSON payload carries this key (optional command fields)
        /// </summary>
        public bool Has(string key)
        {
            return _isJsonMode && _jsonData != null && _jsonData.ContainsKey(key);
        }

        /// <summary>
        /// Get string value by key, or a fallback when the caller omitted it
        /// </summary>
        public string PopStringOr(string key, string fallback)
        {
            return Has(key) ? PopString(key) : fallback;
        }

        /// <summary>
        /// Reset the offset for legacy mode
        /// </summary>
        public void ResetOffset()
        {
            _offset = 2;
        }

        /// <summary>
        /// Get remaining parts count (legacy mode only)
        /// </summary>
        public int RemainingParts => _isJsonMode ? 0 : (_parts?.Length - _offset) ?? 0;

        public abstract void Run();

    }
}