using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CEXEBuilder
{
    class Program
    {
        private const string ExpectedSignature = "CEXE";
        private const int SignatureSize = 4;
        private const int ArchiveSizeLength = 4;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: CEXEBuilder <source_directory> <output_cexe_file>");
                return;
            }

            string sourceDirectory = args[0];
            string outputCEXEFile = args[1];

            try
            {
                byte[] zipContent = CreateZipContent(sourceDirectory);
                byte[] cexeContent = CreateCEXEContent(zipContent);
                File.WriteAllBytes(outputCEXEFile, cexeContent);
                Console.WriteLine("CEXE file created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static byte[] CreateZipContent(string sourceDirectory)
        {
            Console.WriteLine("Creating ZIP...");

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
                    {
                        string relativePath = Path.GetRelativePath(sourceDirectory, filePath);
                        var fileEntry = archive.CreateEntry(relativePath);

                        using (var entryStream = fileEntry.Open())
                        using (var fileStream = File.OpenRead(filePath))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }


        private static byte[] CreateCEXEContent(byte[] zipContent)
        {
            Console.WriteLine("Creating Cosmos Executable...");

            byte[] cexeContent = new byte[SignatureSize + ArchiveSizeLength + zipContent.Length];

            Array.Copy(Encoding.ASCII.GetBytes(ExpectedSignature), cexeContent, SignatureSize);
            Array.Copy(BitConverter.GetBytes(zipContent.Length), 0, cexeContent, SignatureSize, ArchiveSizeLength);

            Array.Copy(zipContent, 0, cexeContent, SignatureSize + ArchiveSizeLength, zipContent.Length);

            return cexeContent;
        }
    }
}
