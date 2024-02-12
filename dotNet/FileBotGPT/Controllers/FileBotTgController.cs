
using FileBotGPT.InterfacesServices;
using FileBotGPT.Model;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace FileBotGPT.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FilebotTgController : ControllerBase
    {
        // Declare a private field for GPT request service
        private readonly IGptRequestService _fileBotGptService;

        // Declare a private field for file processing service
        private readonly IProcessingFileService _processingFileService;
        public FilebotTgController(IGptRequestService fileBotGptService, IProcessingFileService processingFileService)
        {
            //Inject GPT request service, file processing service through constructor
            _fileBotGptService = fileBotGptService;
            _processingFileService = processingFileService;
        }

        // Endpoint for handling Python bot requests
        [HttpPost("PythonBotRequest")]
        public async Task<IActionResult> PythonPostBotAsync([FromBody] PostBotRequest data)
        {
            Debug.WriteLine($"Received data from client: {data}");
            try
            {
                // Call GPT request service to process data and generate content
                string content = await _fileBotGptService.PostGptRequestAsync(data);

                //string content = "\"# Contents\r\n1.\tIntroduction\r\n2.\tCommon Types of Cats\r\n3.\tRare Types of Cats\r\n4.\tDomestic Cats\r\n<a name=\"introduction\"></a>Введение\r\nКошки - это многообразные домашние питомцы с множеством различных пород, каждая из которых обладает своими уникальными чертами и характеристиками. Большая часть котов может быть классифицирована как домашние, однако существует множество редких и экзотических пород.\r\n<a name=\"common_cat\"></a>Распространенные виды кошек\r\nСамыми распространенными породами кошек являются Персидская, Бенгальская, Сиамская и Мейн-Кун. Например, Персидская кошка известна своим длинным пушистым мехом и смиренным характером. Между тем, Мейн-Куны являются одной из крупнейших пород кошек и обладают доброго и игривого нрава.\r\n<a name=\"rare_cat\"></a>Редкие виды кошек\r\nНекоторые редкие породы включают в себя Саванну, Сфинкс, и Кошку-рыболова. Саванна - это кошка с большим размером тела, похожая на диких львов по окраске. Кошки-рыболовы, как можно догадаться по названию, любят воду и умеют плавать.\r\n<a name=\"domestic_cat\"></a>Домашние кошки\r\nДомашния кошкa, также известные как \"дворовые\" кошки, не принадлежат к определенной породе. Они могут варьироваться по окрасу, размеру и характеру, но обычно они дружелюбны и хорошо адаптированы к жизни с людьми.\r\nДля кого-то можно выбрать определённую породу кошки, основываясь на их образе жизни, потребностях и предпочтениях в характере кошки\"\r\n";


                // Process bot response data
                MemoryStream dataStream = await _processingFileService.PostBotResponse(data, content);

                // Prepare HTTP response with file attachment
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(dataStream);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    // Set the file name for attachment
                    FileName = data.file_name + ".docx"
                };

                // Return the file stream as response
                return Ok(await response.Content.ReadAsStreamAsync());
            }
            catch
            {
                // Return a bad request response if an exception occurs
                return BadRequest("Bad");
            }
        }
    }


}