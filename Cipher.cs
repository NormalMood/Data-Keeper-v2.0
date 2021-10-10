using System.Text;
using System.Collections.Generic;
namespace EncipherData
{
    sealed class Cipher
    {
        private byte[] _UserKey;
        private bool _IsEncipheringModeAvailable = true;
        public Cipher(string key)
        {
            _UserKey = Encoding.UTF8.GetBytes(key);
            InitKBlock();
        }
        private const int _amountOfBytes = 256;
        private byte[] _S = new byte[_amountOfBytes];
        private byte[] _K = new byte[_amountOfBytes];
        private string _SourceText;
        private byte[] _SourceTextInBytes;
        public string SourceText
        {
            get
            {
                return _SourceText;
            }
            set
            {
                _SourceText = "";
                _ConvertedText = "";
                _SourceText = value;
            }
        }
        private string _ConvertedText;
        private List<byte> _ListConvertedTextInBytes;
        public string ConvertedText
        {
            get { return _ConvertedText; }
        }
        private void InitSBlock()
        {
            for (int i = 0; i < _amountOfBytes; i++)
            {
                _S[i] = (byte)i;
            }
        }
        private void InitKBlock()
        {

            int keyLength = _UserKey.Length;
            for (int i = 0; i < _amountOfBytes; i++)
            {
                _K[i] = _UserKey[i % keyLength];
            }
        }
        private void MixSBlock()
        {
            int j = 0;
            for (int i = 0; i < _amountOfBytes; i++)
            {
                j = (j + _S[i] + _K[i]) % _amountOfBytes;
                Swap(ref _S[i], ref _S[j]);
            }
        }
        private void Swap(ref byte number1, ref byte number2)
        {
            byte temp = number2;
            number2 = number1;
            number1 = temp;
        }
        public void Encipher()
        {
            InitSBlock();
            MixSBlock();

            if (_IsEncipheringModeAvailable)
            {
                _SourceTextInBytes = Encoding.UTF8.GetBytes(_SourceText);
            }
            int sourceTextInBytesLength = _SourceTextInBytes.Length;
            _ListConvertedTextInBytes = new List<byte>();
            int i = 0, j = 0;
            byte temp = 0;
            byte key = 0;
            for (int index = 0; index < sourceTextInBytesLength; index++)
            {
                i = (i + 1) % _amountOfBytes;
                j = (j + _S[i]) % _amountOfBytes;
                Swap(ref _S[i], ref _S[j]);
                temp = (byte)((_S[i] + _S[j]) % _amountOfBytes);
                key = _S[temp];
                _ListConvertedTextInBytes.Add((byte)(_SourceTextInBytes[index] ^ key));
            }

            if (_IsEncipheringModeAvailable)
            {
                foreach (var elem in _ListConvertedTextInBytes)
                {
                    _ConvertedText += (char)elem;
                }
            }
        }
        public void Decipher()
        {
            _IsEncipheringModeAvailable = false;
            _SourceTextInBytes = new byte[_SourceText.Length];
            int _SourceTextInBytesLength = _SourceTextInBytes.Length;
            for (int i = 0; i < _SourceTextInBytesLength; i++)
            {
                _SourceTextInBytes[i] = (byte)_SourceText[i];
            }
            Encipher();
            _ConvertedText = Encoding.UTF8.GetString(_ListConvertedTextInBytes.ToArray());
            _IsEncipheringModeAvailable = true;
        }
    }
}
