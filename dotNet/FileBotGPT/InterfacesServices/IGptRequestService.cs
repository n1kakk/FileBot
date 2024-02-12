using FileBotGPT.Model;


namespace FileBotGPT.InterfacesServices
{
    public interface IGptRequestService
    {
        public Task<string> PostGptRequestAsync(PostBotRequest data);
    }
}
