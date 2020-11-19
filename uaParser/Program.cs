using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace uaParser
{
    
    public class Counter
    {
        public Counter(string name, int counts)
        {
            Name = name;
            Counts = counts;
        }
        public string Name { get; set; }
        public int Counts { get; set; }
    }

    static class Program
    {
        public static List<string> FilesNames;
        static void Main(string[] args)
        {
            FilesNames = new List<string>();

            var counter = 0;
            var fileName = string.Format("ualist{0:yyyyMMddHHmm}.txt", DateTime.Now);
            var lines = new List<string>();
            var tmpList = new List<ClientInfo>();
            var uaParser = Parser.GetDefault();
            
            //Console.WriteLine("Czyszczę plik z zbednych danych");
            //var readLines = File.ReadLines("u_ex160630.log");
            //var readLineCount = readLines.Count();
            //foreach (var rl in readLines)
            //{
            //    var array = rl.Split((string[]) null, StringSplitOptions.RemoveEmptyEntries);
            //    if(array.Length > 8 && array.Length <= 17) lines.Add(array[9]);
            //    ShowPercentProgress("Przetworzono ", counter++, readLineCount);
            //}
            //var fileContent = File.ReadAllLines("u_ex160630.log");
            //var array = fileContent.Select(l => l.Split((string[])null, StringSplitOptions.RemoveEmptyEntries));
            //File.WriteAllLines(fileName, lines, Encoding.UTF8);

            lines.Clear();
            counter = 0;


            //tne plik na czesci

            //string baseName = "ualist201607061312";


            //var lineUaCut = File.ReadLines(baseName+".txt").Count()/6;
            //var newfiles = new int[] { lineUaCut, lineUaCut, lineUaCut, lineUaCut, lineUaCut, lineUaCut };
            //SplitFiles(newfiles, baseName + ".txt");

            FilesNames.Add("ualist201607061312_4.txt");
            FilesNames.Add("ualist201607061312_5.txt");
            try
            {
                foreach (var file1 in FilesNames)
                {
                    counter = 0;
                    var lineUa = File.ReadLines(file1);
                    var lineUaCount = lineUa.Count();

                    Console.WriteLine("Przetwarzanie UserAgentów " + file1);
                    foreach (var line in lineUa)
                    {
                        try
                        {
                            var c = uaParser.Parse(line);
                            tmpList.Add(c);
                            ShowPercentProgress("Przetworzono ", counter++, lineUaCount);
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                    Console.WriteLine("Zapis danych");
                    var filetoSave = new StreamWriter(string.Format("dump{0:yyyyMMddHHmm}.txt", DateTime.Now));

                    var deviceBrandCount = tmpList.GroupBy(g => g.Device.Brand)
                        .Select(s => new Counter((string.IsNullOrWhiteSpace(s.Key) ? "Other" : s.Key), s.Count()))
                        .ToList();

                    filetoSave.WriteLine("Device:");
                    deviceBrandCount.ForEach(f =>
                    {
                        filetoSave.WriteLine("{0} {1}", f.Name, f.Counts);
                        var deviceFamilyCount =
                            tmpList.Where(w => w.Device.Brand == f.Name)
                                .GroupBy(g => g.Device.Family)
                                .Select(s => string.Format("{0} {1}", s.Key.ToString(), s.Count().ToString()))
                                .ToList();
                        deviceFamilyCount.ForEach(ff =>
                        {
                            filetoSave.WriteLine("\t" + ff);
                        });
                    });

                    filetoSave.WriteLine();
                    filetoSave.WriteLine("Os:");
                    var oSFamilyCount = tmpList.GroupBy(g => g.OS.Family)
                        .Select(s => string.Format("{0} {1}", s.Key.ToString(), s.Count().ToString())).ToList();
                    oSFamilyCount.ForEach(f =>
                    {
                        filetoSave.WriteLine(f);

                    });

                    filetoSave.WriteLine();
                    filetoSave.WriteLine("UserAgent:");
                    var userAgentFamilyCount =
                        tmpList.GroupBy(g => g.UserAgent.Family)
                            .Select(s => string.Format("{0} {1}", s.Key.ToString(), s.Count().ToString()))
                            .ToList();
                    userAgentFamilyCount.ForEach(f =>
                    {
                        filetoSave.WriteLine(f);
                    });

                    filetoSave.Close();
                }



            }
            catch (Exception ex)
            {
                
            }

            Console.ReadLine();

        }

        public static void SplitFiles(int[] newFiles, string inputFile)
        {
            string baseName = Path.GetFileNameWithoutExtension(inputFile);
            string extension = Path.GetExtension(inputFile);
            using (TextReader reader = File.OpenText(inputFile))
            {
                for (int i = 0; i < newFiles.Length; i++)
                {
                    string outputFile = baseName + "_"+ i + extension;
                    FilesNames.Add(outputFile);
                    // Could put this into the CopyLines method if you wanted
                    if (File.Exists(outputFile))
                    {
                        // Better than silently returning, I'd suggest...
                        throw new IOException("File already exists: " + outputFile);
                    }

                    CopyLines(reader, outputFile, newFiles[i]);
                }
            }
        }

        private static void CopyLines(TextReader reader, string outputFile, int count)
        {
            using (TextWriter writer = File.CreateText(outputFile))
            {
                for (int i = 0; i < count; i++)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        return; // Premature end of input
                    }
                    writer.WriteLine(line);
                }
            }
        }

        static void ShowPercentProgress(string message, int currElementIndex, int totalElementCount)
        {
            if (currElementIndex < 0 || currElementIndex >= totalElementCount)
            {
                return; //throw new InvalidOperationException("currElement out of range");
            }
            var percent = (100 * (currElementIndex + 1)) / totalElementCount;
            Console.Write("\r{0}{1}% ", message, percent);
            switch (currElementIndex % 4)
            {
                case 0: Console.Write("/"); break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
            }
        }
    }
}
