using System.Collections.Generic;
using System.IO;

namespace MyExtensions
{
    class EncipheredStreamReader : StreamReader
    {
        private readonly string _pathToEncipheredFile;
        private readonly string _pathToLabelFile;
        public EncipheredStreamReader(string pathToEncipheredFile, string pathToLabelFile) : base(pathToEncipheredFile)
        {
            _pathToEncipheredFile = pathToEncipheredFile;
            _pathToLabelFile = pathToLabelFile;
        }
        private int _indexOfLines = 0;
        private int _amountOfLines = 0;
        private List<string> _listOfLabelStrings = new List<string>();
        private string[] _tempArrayOfStrings = { };
        public void ReadLabelsFile()
        {
            string line = "";
            StreamReader inputFile = new StreamReader(_pathToLabelFile);
            while ((line = inputFile.ReadLine()) != null)
            {
                _listOfLabelStrings.Add(line);
            }
            _amountOfLines = _listOfLabelStrings.Count;
            inputFile.Close();
        }
        public string ReadEncipheredLine()
        {
            string rebuiltEncipheredLine = ReadLine();
            if (rebuiltEncipheredLine == null)
                return null;
            _tempArrayOfStrings = new string[] { };
            _tempArrayOfStrings = _listOfLabelStrings[_indexOfLines].Trim(' ').Split(' ');
            var dictionaryOfLabels = new Dictionary<int, char>();
            if (_tempArrayOfStrings[0] != "")
            {
                foreach (var elem in _tempArrayOfStrings)
                {
                    if (elem[0] == 'r')
                        dictionaryOfLabels.Add(int.Parse(elem.Substring(2, elem.Length - 2)), '\r');
                    else
                        dictionaryOfLabels.Add(int.Parse(elem.Substring(2, elem.Length - 2)), '\n');
                }
                foreach (var elem in dictionaryOfLabels)
                {
                    rebuiltEncipheredLine = rebuiltEncipheredLine.Insert(elem.Key, elem.Value.ToString());
                }
            }
            _indexOfLines++;
            return rebuiltEncipheredLine;
        }
    }
    class EncipheredStreamWriter : StreamWriter
    {
        private readonly string _pathToLabelFile;
        private readonly bool _isAppendDataInFiles;
        private bool _isAppendDataInLabelFile;
        public EncipheredStreamWriter(string pathToEncipheredFile, string pathToLabelFile, bool isAppend = false)
            : base(pathToEncipheredFile, append: isAppend)
        {
            _pathToLabelFile = pathToLabelFile;
            _isAppendDataInFiles = isAppend;
            _isAppendDataInLabelFile = isAppend;
        }
        public void WriteEncipheredLine(string str)
        {
            string processedString = "", stringWithLabels = "";
            int index = 0;
            foreach (char symbol in str)
            {
                if (symbol == '\r')
                {
                    stringWithLabels += $"r:{index} ";
                }
                else if (symbol == '\n')
                {
                    stringWithLabels += $"n:{index} ";
                }
                else
                {
                    processedString += symbol;
                }
                index++;
            }
            WriteLine(processedString);
            StreamWriter labelFile = new StreamWriter(_pathToLabelFile, append: _isAppendDataInLabelFile);
            labelFile.WriteLine(stringWithLabels);
            labelFile.Close();
            if (!_isAppendDataInLabelFile)
            {
                _isAppendDataInLabelFile = true; //I need to append data in a label file after first writing.
            }
        }
    }
}
