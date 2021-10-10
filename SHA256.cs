using System;
using System.Linq;
using System.Text;

namespace HashAlgorithm
{
    static class SHA256
    {
        //Constants
        private static uint h0;
        private static uint h1;
        private static uint h2;
        private static uint h3;
        private static uint h4;
        private static uint h5;
        private static uint h6;
        private static uint h7;
        //
        private static void InitConstants()
        {
            h0 = 0x6a09e667;
            h1 = 0xbb67ae85;
            h2 = 0x3c6ef372;
            h3 = 0xa54ff53a;
            h4 = 0x510e527f;
            h5 = 0x9b05688c;
            h6 = 0x1f83d9ab;
            h7 = 0x5be0cd19;
        }
        //Array of constants
        private static uint[] _ArrayOfConstants = new uint[64]
        {
                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
                0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
                0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
                0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
                0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
                0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
                0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
                0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };
        //
        private static string GetStringInUnicodeFromCodesInUTF8(string str)
        {
            byte[] arrayForCodes = Encoding.UTF8.GetBytes(str);
            string res = "";
            foreach (var code in arrayForCodes)
            {
                res += (char)code;
            }
            return res;
        }
        private static void AppendSingleBitAndPaddingBytes(ref string str)
        {
            str += (char)0x80;
            while (str.Length % 64 != 56)
                str += (char)0x00;
        }
        private static void AppendSourceLengthInBits(ref string str, int stringInUtf8_length)
        {
            uint lengthInBits = (uint)(stringInUtf8_length * 8);
            string extraEightBytes = "";
            for (int i = 0; i < 8; i++)
            {
                extraEightBytes = (char)(lengthInBits % 256) + extraEightBytes;
                lengthInBits /= 256;
            }
            str += extraEightBytes;
        }
        public static string GenerateHash(string sourceString)
        {
            InitConstants();
            string copyOfSourceString = GetStringInUnicodeFromCodesInUTF8(sourceString);
            int copyOfSourceString_Length = copyOfSourceString.Length; //length of string in UTF8
            AppendSingleBitAndPaddingBytes(ref copyOfSourceString);
            AppendSourceLengthInBits(ref copyOfSourceString, copyOfSourceString_Length);
            //variables for compression cycle
            uint a, b, c, d, e, f, g, h;
            uint number1 = 0, number2 = 0;
            uint s0, s1, ch, temp1, temp2, maj;
            //
            //variable for main cycle
            uint[] groupOfBytes = { };
            //
            while (copyOfSourceString.Length != 0)
            {
                groupOfBytes = GetSixtyFourWordsFromString(copyOfSourceString);
                //changing zero bytes
                for (int i = 16; i < 64; i++)
                {
                    number1 = (RightRotate(groupOfBytes[i - 15], 7)) ^ (RightRotate(groupOfBytes[i - 15], 18)) ^ (groupOfBytes[i - 15] >> 3);
                    number2 = (RightRotate(groupOfBytes[i - 2], 17)) ^ (RightRotate(groupOfBytes[i - 2], 19)) ^ (groupOfBytes[i - 2] >> 10);
                    groupOfBytes[i] = groupOfBytes[i - 16] + number1 + groupOfBytes[i - 7] + number2;
                }
                //
                //Compression cycle
                a = h0;
                b = h1;
                c = h2;
                d = h3;
                e = h4;
                f = h5;
                g = h6;
                h = h7;
                for (int i = 0; i < 64; i++)
                {
                    s1 = RightRotate(e, 6) ^ RightRotate(e, 11) ^ RightRotate(e, 25);
                    ch = (e & f) ^ ((~e) & g);
                    temp1 = h + s1 + ch + _ArrayOfConstants[i] + groupOfBytes[i];
                    s0 = RightRotate(a, 2) ^ RightRotate(a, 13) ^ RightRotate(a, 22);
                    maj = (a & b) ^ (a & c) ^ (b & c);
                    temp2 = s0 + maj;
                    h = g;
                    g = f;
                    f = e;
                    e = d + temp1;
                    d = c;
                    c = b;
                    b = a;
                    a = temp1 + temp2;
                }
                h0 += a;
                h1 += b;
                h2 += c;
                h3 += d;
                h4 += e;
                h5 += f;
                h6 += g;
                h7 += h;
                //
                RemoveFirst64Bytes(ref copyOfSourceString);
            }
            return $"{GetNumberInHex(h0)}{GetNumberInHex(h1)}{GetNumberInHex(h2)}{GetNumberInHex(h3)}" +
                $"{GetNumberInHex(h4)}{GetNumberInHex(h5)}{GetNumberInHex(h6)}{GetNumberInHex(h7)}";
        }
        private static string GetNumberInHex(uint number)
        {
            string convertedNumber = "";
            for (int i = 0; i < 8; i++)
            {
                convertedNumber = Convert.ToString(number % 16, toBase: 16) + convertedNumber;
                number /= 16;
            }
            return convertedNumber;
        }
        private static uint[] GetSixtyFourWordsFromString(string str)
        {
            uint[] arrayOfWords = new uint[64];
            ushort[] tempArrayForJoining = new ushort[4];
            string tempStringForBinNumber = "";

            string fourthByte = ""; //from left
            string thirdByte = "";
            string secondByte = "";
            string firstByte = ""; //to right


            for (int i = 0; i < 64; i += 4)
            {
                tempArrayForJoining[0] = (ushort)str[i];
                tempArrayForJoining[1] = (ushort)str[i + 1];
                tempArrayForJoining[2] = (ushort)str[i + 2];
                tempArrayForJoining[3] = (ushort)str[i + 3];

                firstByte = Convert.ToString(tempArrayForJoining[3], toBase: 2);
                secondByte = Convert.ToString(tempArrayForJoining[2], toBase: 2);
                thirdByte = Convert.ToString(tempArrayForJoining[1], toBase: 2);
                fourthByte = Convert.ToString(tempArrayForJoining[0], toBase: 2);

                tempStringForBinNumber = $"{fourthByte}" +
                                         $"{AddZeroesToLeft(thirdByte)}{thirdByte}" +
                                         $"{AddZeroesToLeft(secondByte)}{secondByte}" +
                                         $"{AddZeroesToLeft(firstByte)}{firstByte}";
                arrayOfWords[i / 4] = Convert.ToUInt32(tempStringForBinNumber, fromBase: 2); //4 bytes in one  32-bit word
            }
            for (int i = 16; i < 64; i++)
            {
                arrayOfWords[i] = 0;
            }
            return arrayOfWords;
        }
        private static string AddZeroesToLeft(string str)
        {
            return str.Length == 8 ? String.Empty : String.Join("", Enumerable.Range(1, 8 - str.Length).Select(elem => "0"));
        }
        private static uint RightRotate(uint number, int amountOfShifts) =>
            number = (number << Math.Abs((32 - amountOfShifts))) | (number >> amountOfShifts);
        private static void RemoveFirst64Bytes(ref string str)
        {
            str = str.Remove(0, 64);
        }
    }
}
