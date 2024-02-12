using FileBotGPT.Model;
using Microsoft.AspNetCore.Mvc;
using Aspose.Words;

namespace FileBotGPT.InterfacesServices
{
    public interface IProcessingFileService
    {
        public Task<MemoryStream> PostBotResponse(PostBotRequest botData, string gptText);
    }
}
