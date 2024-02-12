using FileBotGPT.InterfacesServices;
using FileBotGPT.Model;

using Aspose.Words;

using Aspose.Words.Loading;
using System.Text.RegularExpressions;
using NPOI.XWPF.UserModel;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace FileBotGPT.Services
{
    public class ProcessingFileService : IProcessingFileService
    {
        // Method to generate a Stream from a string
        static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();  // Create a new MemoryStream
            var writer = new StreamWriter(stream); // Create a new StreamWriter for the MemoryStream
            writer.Write(s); // Write the string to the StreamWriter
            writer.Flush(); // Flush the StreamWriter to ensure all buffered data is written to the underlying stream
            stream.Position = 0; // Reset the position of the MemoryStream to the beginning
            return stream; // Return the MemoryStream as a Stream
        }

        // Method to create a Word document from GPT text
        public MemoryStream MakeFile(PostBotRequest botData, string gptText)
        {
            const string folderDir = "C:\\Users\\79832\\Documents\\ ";
            string pattern = @"<a.*?</a>";
            gptText = Regex.Replace(gptText, pattern, "");
            gptText = gptText.Replace("\\n", "\n");
            gptText = gptText.Trim('"');


            // Create a Stream from the GPT text
            using (var stream = GenerateStreamFromString(gptText))
            {
                // Configure HTML load options for Aspose.Words
                HtmlLoadOptions loadOptions = new HtmlLoadOptions(LoadFormat.Markdown, "", folderDir);
                var doc = new Aspose.Words.Document(stream, loadOptions); // Load the Stream into an Aspose.Words Document
                DocumentBuilder builder = new DocumentBuilder(doc); // Create a DocumentBuilder for the document

                // Get the font from the DocumentBuilder
                Font font = builder.Font;
                font.Name = botData.font;
                font.Size = 12;

                // Save the document to a MemoryStream
                var dataStream = new MemoryStream();
                doc.Save(dataStream, SaveFormat.Docx);

                dataStream.Seek(0, SeekOrigin.Begin); // Reset the position of the MemoryStream to the beginning

                //// Save stream to file
                //var fileStream = File.Create(Path.Combine(folderDir + "Document2.docx"));
                //dataStream.Seek(0, SeekOrigin.Begin);
                //dataStream.CopyTo(fileStream);
                //fileStream.Close();

                // Save to file
                //doc.Save(Path.Combine(folderDir + "Document2.docx"));

                // Return the modified document as a MemoryStream
                return EditDocxDoc(dataStream);
            }
        }

        // Method to edit a DOCX document by removing the first paragraph and clearing headers and footers
        private MemoryStream EditDocxDoc(MemoryStream dataStream)
        {

            // Create a new XWPFDocument from the provided MemoryStream
            XWPFDocument doc = new XWPFDocument(dataStream);

            // Remove the first paragraph if it exists
            if (doc.Paragraphs.Count > 0)
            {
                doc.RemoveBodyElement(0);
            }

            // Create empty header and footer objects to clear existing headers and footers
            CT_Ftr afooter = new CT_Ftr();
            CT_Hdr aheader = new CT_Hdr();

            // Iterate through each header and footer in the document and clear it
            foreach (XWPFHeader header in doc.HeaderList)
            {
                header.SetHeaderFooter(aheader);
            }
            foreach (XWPFFooter footer in doc.FooterList)
            {
                footer.SetHeaderFooter(afooter);
            }

            // Create a new MemoryStream to store the modified document
            var modifiedStream = new MemoryStream();

            // Write the modified document to the MemoryStream
            doc.Write(modifiedStream);

            // Close the document to release resources
            doc.Close();

            // Reset the position of the MemoryStream to the beginning
            modifiedStream.Seek(0, SeekOrigin.Begin);

            // Return the modified MemoryStream
            return modifiedStream;

        }

        // Method to post bot response by processing bot data and GPT text
        public async Task<MemoryStream> PostBotResponse(PostBotRequest botData, string gptText)
        {
            // Call MakeFile method to generate a file based on bot data and GPT text
            MemoryStream dataStream = MakeFile(botData, gptText);

            // Return the MemoryStream containing the generated file
            return dataStream;
        }
    }
}
