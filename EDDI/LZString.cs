using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Eddi
{
    public class LZString
    {

        private class Context_Compress
        {
            public Dictionary<string, int> dictionary { get; set; }
            public Dictionary<string, bool> dictionaryToCreate { get; set; }
            public string c { get; set; }
            public string wc { get; set; }
            public string w { get; set; }
            public int enlargeIn { get; set; }
            public int dictSize { get; set; }
            public int numBits { get; set; }
            public Context_Compress_Data data { get; set; }
        }

        private class Context_Compress_Data
        {
            public string str { get; set; }
            public int val { get; set; }
            public int position { get; set; }
        }

        private class Decompress_Data
        {
            public string str { get; set; }
            public int val { get; set; }
            public int position { get; set; }
            public int index { get; set; }
        }

        private static Context_Compress_Data writeBit(int value, Context_Compress_Data data)
        {
            data.val = (data.val << 1) | value;

            if (data.position == 15)
            {
                data.position = 0;
                data.str += (char)data.val;
                data.val = 0;
            }
            else
                data.position++;

            return data;
        }

        private static Context_Compress_Data writeBits(int numbits, int value, Context_Compress_Data data)
        {

            for (var i = 0; i < numbits; i++)
            {
                data = writeBit(value & 1, data);
                value = value >> 1;
            }

            return data;
        }

        private static Context_Compress produceW(Context_Compress context)
        {

            if (context.dictionaryToCreate.ContainsKey(context.w))
            {
                if (context.w[0] < 256)
                {
                    context.data = writeBits(context.numBits, 0, context.data);
                    context.data = writeBits(8, context.w[0], context.data);
                }
                else
                {
                    context.data = writeBits(context.numBits, 1, context.data);
                    context.data = writeBits(16, context.w[0], context.data);
                }

                context = decrementEnlargeIn(context);
                context.dictionaryToCreate.Remove(context.w);
            }
            else
            {
                context.data = writeBits(context.numBits, context.dictionary[context.w], context.data);
            }

            return context;
        }

        private static Context_Compress decrementEnlargeIn(Context_Compress context)
        {

            context.enlargeIn--;
            if (context.enlargeIn == 0)
            {
                context.enlargeIn = (int)Math.Pow(2, context.numBits);
                context.numBits++;
            }
            return context;
        }

        public static string compress(string uncompressed)
        {

            Context_Compress context = new Context_Compress();
            Context_Compress_Data data = new Context_Compress_Data();

            context.dictionary = new Dictionary<string, int>();
            context.dictionaryToCreate = new Dictionary<string, bool>();
            context.c = "";
            context.wc = "";
            context.w = "";
            context.enlargeIn = 2;
            context.dictSize = 3;
            context.numBits = 2;

            data.str = "";
            data.val = 0;
            data.position = 0;

            context.data = data;

            try
            {
                for (int i = 0; i < uncompressed.Length; i++)
                {
                    context.c = uncompressed[i].ToString();

                    if (!context.dictionary.ContainsKey(context.c))
                    {
                        context.dictionary[context.c] = context.dictSize++;
                        context.dictionaryToCreate[context.c] = true;
                    };

                    context.wc = context.w + context.c;

                    if (context.dictionary.ContainsKey(context.wc))
                    {
                        context.w = context.wc;
                    }
                    else
                    {
                        context = produceW(context);
                        context = decrementEnlargeIn(context);
                        context.dictionary[context.wc] = context.dictSize++;
                        context.w = context.c;
                    }
                }

                if (context.w != "")
                {
                    context = produceW(context);
                }

                // Mark the end of the stream
                context.data = writeBits(context.numBits, 2, context.data);

                // Flush the last char
                while (true)
                {
                    context.data.val = (context.data.val << 1);
                    if (context.data.position == 15)
                    {
                        context.data.str += (char)context.data.val;
                        break;
                    }
                    else
                        context.data.position++;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return context.data.str;
        }

        private static int readBit(Decompress_Data data)
        {

            var res = data.val & data.position;

            data.position >>= 1;

            if (data.position == 0)
            {
                data.position = 32768;

                // This 'if' check doesn't appear in the orginal lz-string javascript code.
                // Added as a check to make sure we don't exceed the length of data.str
                // The javascript charCodeAt will return a NaN if it exceeds the index but will not error out
                if (data.index < data.str.Length)
                {
                    data.val = data.str[data.index++]; // data.val = data.string.charCodeAt(data.index++); <---javascript equivilant
                }
            }

            return res > 0 ? 1 : 0;
        }

        private static int readBits(int numBits, Decompress_Data data)
        {

            int res = 0;
            int maxpower = (int)Math.Pow(2, numBits);
            int power = 1;

            while (power != maxpower)
            {
                res |= readBit(data) * power;
                power <<= 1;
            }

            return res;
        }

        public static string decompress(string compressed)
        {

            Decompress_Data data = new Decompress_Data();

            List<string> dictionary = new List<string>();
            int next = 0;
            int enlargeIn = 4;
            int numBits = 3;
            string entry = "";
            StringBuilder result = new StringBuilder();
            int i = 0;
            string w = "";
            string sc = "";
            int c = 0;
            int errorCount = 0;

            data.str = compressed;
            data.val = (int)compressed[0];
            data.position = 32768;
            data.index = 1;

            try
            {
                for (i = 0; i < 3; i++)
                {
                    dictionary.Add(i.ToString());
                }

                next = readBits(2, data);

                switch (next)
                {
                    case 0:
                        sc = Convert.ToChar(readBits(8, data)).ToString();
                        break;
                    case 1:
                        sc = Convert.ToChar(readBits(16, data)).ToString();
                        break;
                    case 2:
                        return "";
                }

                dictionary.Add(sc);

                result.Append(sc);
                w = result.ToString();

                while (true)
                {
                    c = readBits(numBits, data);
                    int cc = c;

                    switch (cc)
                    {
                        case 0:
                            if (errorCount++ > 10000)
                                throw new Exception("To many errors");

                            sc = Convert.ToChar(readBits(8, data)).ToString();
                            dictionary.Add(sc);
                            c = dictionary.Count - 1;
                            enlargeIn--;

                            break;
                        case 1:
                            sc = Convert.ToChar(readBits(16, data)).ToString();
                            dictionary.Add(sc);
                            c = dictionary.Count - 1;
                            enlargeIn--;

                            break;
                        case 2:
                            return result.ToString();
                    }

                    if (enlargeIn == 0)
                    {
                        enlargeIn = (int)Math.Pow(2, numBits);
                        numBits++;
                    }

                    if (dictionary.Count - 1 >= c) // if (dictionary[c] ) <------- original Javascript Equivalant
                    {
                        entry = dictionary[c];
                    }
                    else
                    {
                        if (c == dictionary.Count)
                        {
                            entry = w + w[0];
                        }
                        else
                        {
                            return null;
                        }
                    }

                    result.Append(entry);
                    dictionary.Add(w + entry[0]);
                    enlargeIn--;
                    w = entry;

                    if (enlargeIn == 0)
                    {
                        enlargeIn = (int)Math.Pow(2, numBits);
                        numBits++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string compressToUTF16(string input)
        {

            string output = "";
            int status = 0;
            int current = 0;

            try
            {
                if (input == null)
                    throw new Exception("Input is Null");

                input = compress(input);
                if (input.Length == 0)
                    return input;

                for (int i = 0; i < input.Length; i++)
                {
                    int c = (int)input[i];
                    switch (status++)
                    {
                        case 0:
                            output += (char)((c >> 1) + 32);
                            current = (c & 1) << 14;
                            break;
                        case 1:
                            output += (char)((current + (c >> 2)) + 32);
                            current = (c & 3) << 13;
                            break;
                        case 2:
                            output += (char)((current + (c >> 3)) + 32);
                            current = (c & 7) << 12;
                            break;
                        case 3:
                            output += (char)((current + (c >> 4)) + 32);
                            current = (c & 15) << 11;
                            break;
                        case 4:
                            output += (char)((current + (c >> 5)) + 32);
                            current = (c & 31) << 10;
                            break;
                        case 5:
                            output += (char)((current + (c >> 6)) + 32);
                            current = (c & 63) << 9;
                            break;
                        case 6:
                            output += (char)((current + (c >> 7)) + 32);
                            current = (c & 127) << 8;
                            break;
                        case 7:
                            output += (char)((current + (c >> 8)) + 32);
                            current = (c & 255) << 7;
                            break;
                        case 8:
                            output += (char)((current + (c >> 9)) + 32);
                            current = (c & 511) << 6;
                            break;
                        case 9:
                            output += (char)((current + (c >> 10)) + 32);
                            current = (c & 1023) << 5;
                            break;
                        case 10:
                            output += (char)((current + (c >> 11)) + 32);
                            current = (c & 2047) << 4;
                            break;
                        case 11:
                            output += (char)((current + (c >> 12)) + 32);
                            current = (c & 4095) << 3;
                            break;
                        case 12:
                            output += (char)((current + (c >> 13)) + 32);
                            current = (c & 8191) << 2;
                            break;
                        case 13:
                            output += (char)((current + (c >> 14)) + 32);
                            current = (c & 16383) << 1;
                            break;
                        case 14:
                            output += (char)((current + (c >> 15)) + 32);
                            output += (char)((c & 32767) + 32);
                            status = 0;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return output + (char)(current + 32);
        }

        public static string decompressFromUTF16(string input)
        {

            string output = "";
            int status = 0;
            int current = 0;
            int i = 0;

            try
            {
                if (input == null)
                    throw new Exception("input is Null");

                while (i < input.Length)
                {
                    int c = ((int)input[i]) - 32;

                    switch (status++)
                    {
                        case 0:
                            current = c << 1;
                            break;
                        case 1:
                            output += (char)(current | (c >> 14));
                            current = (c & 16383) << 2;
                            break;
                        case 2:
                            output += (char)(current | (c >> 13));
                            current = (c & 8191) << 3;
                            break;
                        case 3:
                            output += (char)(current | (c >> 12));
                            current = (c & 4095) << 4;
                            break;
                        case 4:
                            output += (char)(current | (c >> 11));
                            current = (c & 2047) << 5;
                            break;
                        case 5:
                            output += (char)(current | (c >> 10));
                            current = (c & 1023) << 6;
                            break;
                        case 6:
                            output += (char)(current | (c >> 9));
                            current = (c & 511) << 7;
                            break;
                        case 7:
                            output += (char)(current | (c >> 8));
                            current = (c & 255) << 8;
                            break;
                        case 8:
                            output += (char)(current | (c >> 7));
                            current = (c & 127) << 9;
                            break;
                        case 9:
                            output += (char)(current | (c >> 6));
                            current = (c & 63) << 10;
                            break;
                        case 10:
                            output += (char)(current | (c >> 5));
                            current = (c & 31) << 11;
                            break;
                        case 11:
                            output += (char)(current | (c >> 4));
                            current = (c & 15) << 12;
                            break;
                        case 12:
                            output += (char)(current | (c >> 3));
                            current = (c & 7) << 13;
                            break;
                        case 13:
                            output += (char)(current | (c >> 2));
                            current = (c & 3) << 14;
                            break;
                        case 14:
                            output += (char)(current | (c >> 1));
                            current = (c & 1) << 15;
                            break;
                        case 15:
                            output += (char)(current | c);
                            status = 0;
                            break;
                    }

                    i++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return decompress(output);
        }

        public static string compressToBase64(string input)
        {

            string _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            string output = "";

            // Using the data type 'double' for these so that the .Net double.NaN & double.IsNaN functions can be used
            // later in the function. .Net doesn't have a similar function for regular integers.
            double chr1, chr2, chr3 = 0.0;

            int enc1 = 0;
            int enc2 = 0;
            int enc3 = 0;
            int enc4 = 0;
            int i = 0;

            try
            {
                if (input == null)
                    throw new Exception("input is Null");

                input = compress(input);

                while (i < input.Length * 2)
                {
                    if (i % 2 == 0)
                    {
                        chr1 = (int)input[i / 2] >> 8;
                        chr2 = (int)input[i / 2] & 255;
                        if (i / 2 + 1 < input.Length)
                            chr3 = (int)input[i / 2 + 1] >> 8;
                        else
                            chr3 = double.NaN;//chr3 = NaN; <------ original Javascript Equivalent
                    }
                    else
                    {
                        chr1 = (int)input[(i - 1) / 2] & 255;
                        if ((i + 1) / 2 < input.Length)
                        {
                            chr2 = (int)input[(i + 1) / 2] >> 8;
                            chr3 = (int)input[(i + 1) / 2] & 255;
                        }
                        else
                        {
                            chr2 = chr3 = double.NaN; // chr2 = chr3 = NaN; <------ original Javascript Equivalent
                        }
                    }
                    i += 3;


                    enc1 = (int)(Math.Round(chr1)) >> 2;

                    // The next three 'if' statements are there to make sure we are not trying to calculate a value that has been 
                    // assigned to 'double.NaN' above. The orginal Javascript functions didn't need these checks due to how
                    // Javascript functions.
                    // Also, due to the fact that some of the variables are of the data type 'double', we have to do some type
                    // conversion to get the 'enc' variables to be the correct value.
                    if (!double.IsNaN(chr2))
                    {
                        enc2 = (((int)(Math.Round(chr1)) & 3) << 4) | ((int)(Math.Round(chr2)) >> 4);
                    }

                    if (!double.IsNaN(chr2) && !double.IsNaN(chr3))
                    {
                        enc3 = (((int)(Math.Round(chr2)) & 15) << 2) | ((int)(Math.Round(chr3)) >> 6);
                    }
                    // added per issue #3 logged by ReuvenT
                    else
                    {
                        enc3 = 0;
                    }

                    if (!double.IsNaN(chr3))
                    {

                        enc4 = (int)(Math.Round(chr3)) & 63;
                    }

                    if (double.IsNaN(chr2)) //if (isNaN(chr2)) <------ original Javascript Equivalent
                    {
                        enc3 = enc4 = 64;
                    }
                    else if (double.IsNaN(chr3)) //else if (isNaN(chr3)) <------ original Javascript Equivalent
                    {
                        enc4 = 64;
                    }

                    output = output + _keyStr[enc1] + _keyStr[enc2] + _keyStr[enc3] + _keyStr[enc4];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return output;
        }

        public static string decompressFromBase64(string input)
        {
            string _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

            string output = "";
            int output_ = 0;
            int ol = 0;
            int chr1, chr2, chr3 = 0;
            int enc1, enc2, enc3, enc4 = 0;
            int i = 0;

            try
            {
                if (input == null)
                    throw new Exception("input is Null");

                var regex = new Regex(@"[^A-Za-z0-9-\+\/\=]");
                input = regex.Replace(input, "");

                while (i < input.Length)
                {
                    enc1 = _keyStr.IndexOf(input[i++]);
                    enc2 = _keyStr.IndexOf(input[i++]);
                    enc3 = _keyStr.IndexOf(input[i++]);
                    enc4 = _keyStr.IndexOf(input[i++]);

                    chr1 = (enc1 << 2) | (enc2 >> 4);
                    chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                    chr3 = ((enc3 & 3) << 6) | enc4;

                    if (ol % 2 == 0)
                    {
                        output_ = chr1 << 8;

                        if (enc3 != 64)
                        {
                            output += (char)(output_ | chr2);
                        }

                        if (enc4 != 64)
                        {
                            output_ = chr3 << 8;
                        }
                    }
                    else
                    {
                        output = output + (char)(output_ | chr1);

                        if (enc3 != 64)
                        {
                            output_ = chr2 << 8;
                        }
                        if (enc4 != 64)
                        {
                            output += (char)(output_ | chr3);
                        }
                    }
                    ol += 3;
                }

                // Send the output out to the main decompress function
                output = decompress(output);
            }
            catch (Exception)
            {
                throw;
            }

            return output;
        }

    }

}