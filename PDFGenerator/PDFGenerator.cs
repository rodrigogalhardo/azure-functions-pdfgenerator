using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace PDFGenerator
{
    public static class PDFGenerator
    {
        [FunctionName("PDFGenerator")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info("C# HTTP trigger function processed a request.");
            log.Info($"Request data: {req.Content}");

            // faz o parse query parameter
            string link = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "link", true) == 0)
                .Value;

            if (link == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "informe o link por query string para conversão em PDF");

            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            WebKitConverterSettings settings = new WebKitConverterSettings();

            var path = System.IO.Path.GetFullPath(context.FunctionAppDirectory+"\\QtBinaries");

            log.Info($"Folder Path: {path}");

            settings.WebKitPath = path;
            //Seta o WebKit path
            //settings.WebKitPath = @"D:\projects\algorithmic\Algorithmic.PDFGenerator\PDFGenerator\QtBinaries\";
            //Assina WebKit settings para HTML converter 
            htmlConverter.ConverterSettings = settings;
            //Converte a URL para PDF 
            PdfDocument document = htmlConverter.Convert(link);
            MemoryStream ms = new MemoryStream();

            ms.Position = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(ms.ToArray());
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"AvraPDF-{new DateTime().ToShortTimeString().ToString()}.pdf"
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }
    }
}
