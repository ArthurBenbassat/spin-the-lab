using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Fermyon.Spin.Sdk;
using Home.BL;
using Home.Model;
using Home.shared;
using Microsoft.Extensions.Logging;
using HttpMethod = Fermyon.Spin.Sdk.HttpMethod;

namespace Home;

public static class Handler
{
    private delegate HttpResponse RequestHandlerDelegate(HttpRequest request);

    public static readonly ILogger Logger;
    private static IManager _manager;

    private static Dictionary<string, RequestHandlerDelegate> _routes = new Dictionary<string, RequestHandlerDelegate>()
    {
        {"/products", GetProducts},
        {"/products/{id}", GetProduct},
        {"/categories", GetCategories},
    };

    private static HttpResponse GetCategories(HttpRequest request)
    {
        var categories = _manager.GetProducts();
        if (!categories.Any())
            return NotFound();
        return OkObject(JsonSerializer.Serialize(categories, DefaultOptions));
    }

    private static HttpResponse GetProduct(HttpRequest request)
    {
        var pathElements = request.Headers["spin-path-info"].Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathElements.Length != 2)
            return BadRequestString("Invalid path");
        var id = pathElements[1];
        if (!int.TryParse(id, out var productId))
            return BadRequestString("Invalid product id");
        var product = _manager.GetProduct(productId);
        if (product == null)
            return BadRequestString("Product not found");
        return OkObject(JsonSerializer.Serialize(product, DefaultOptions));
    }

    private static HttpResponse GetProducts(HttpRequest request)
    {
        // request.ParsedParameters().Get("categoryId");
        IList<Product> products;
        if (request.ParsedParameters().Get("categoryId") != null)
        {
            products = _manager.GetProductsByCategory(int.Parse(request.ParsedParameters().Get("categoryId") ?? "0"));
        }
        else
        {
            products = _manager.GetProducts();
        }
        if (!products.Any())
            return NotFound();
        return OkObject(JsonSerializer.Serialize(products, DefaultOptions));
    }

    public static JsonSerializerOptions DefaultOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    static Handler()
    {
        Logger = new SpinLogger();
        _manager = new Manager(new Repository.Repository());
    }

    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        Logger.LogInformation($"Got request: {JsonSerializer.Serialize(request.Url)}");

        var requestPath = request.Headers["spin-path-info"];
        var routeFound = _routes.TryGetValue(requestPath, out var handler);

        try
        {
            if (routeFound && null != handler) return handler(request);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error during request handling: {ex.Message}");
            Logger.LogError(ex.StackTrace);
            return BadRequestException(ex);
        }

        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "text/plain"},
            },
            BodyAsString = "Requested route not found",
        };


        if (request.Method == HttpMethod.Post)
        {
            return SetJsonFile(request.Body);
        }

        //return GetJsonFile();
    }

    private static HttpResponse SetJsonFile(Optional<Buffer> requestBody)
    {
        // error assets zitten geprecompileerd in de dll en kunnen niet worden overschreven
        if (requestBody.HasContent())
        {
            var textJson = requestBody.AsString();
            Console.WriteLine(textJson);
            File.WriteAllBytes("/assets/home.json", Encoding.UTF8.GetBytes(textJson));
            return OkObject("OK");
        }

        return BadRequestString("No body");
    }
    
    
    private static HttpResponse BadRequestString(string s)
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
            },
            BodyAsString = s,
        };
    }

    private static HttpResponse BadRequestException(Exception ex)
    {
        var statusCode = HttpStatusCode.BadRequest;
        if (ex is HttpRequestException)
        {
            var hre = ex as HttpRequestException;
            if (hre != null && hre.StatusCode != null)
                statusCode = (HttpStatusCode)hre.StatusCode;
        }
        return new HttpResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
            },
            BodyAsString = ex.ToString(),
        };
    }
    
    private static HttpResponse NotFound()
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound
        };
    }

    private static HttpResponse OkObject(string s)
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/json" },
            },
            BodyAsString = s,
        };
    }
}
static class QueryStringParser
{
    public static NameValueCollection ParsedParameters(this HttpRequest httpRequest)
    {
        var indexOfQuestionMark = httpRequest.Url.IndexOf("?");
        var url = httpRequest.Url;
        if (indexOfQuestionMark > 0)
        {
            url = url.Substring(indexOfQuestionMark + 1);
            return HttpUtility.ParseQueryString(url);
        }
        else
            return new NameValueCollection();
    }
}