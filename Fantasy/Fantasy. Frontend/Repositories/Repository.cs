using System.Text;
using System.Text.Json;

namespace Fantasy._Frontend.Repositories;

public class Repository : IRepository
{
    private readonly HttpClient _httpClient;

    //Javascript usa propiedades en minúscula y C# usa propiedades en Mayúscula
    //Esta es una propiedad de lectura y ayuda para que sea indiferente si las prop vienen en minúscula,
    //se mapean y se hacen compactibles con las propiedades en may de C#
    private JsonSerializerOptions _jsonDefaultOptions => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    public Repository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseWrapper<T>> GetAsync<T>(string url)
    {
        var responseHttp = await _httpClient.GetAsync(url);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswer<T>(responseHttp);
            return new HttpResponseWrapper<T>(response, false, responseHttp);
        }
        return new HttpResponseWrapper<T>(default, true, responseHttp);
    }

    //Este método maneja los Post que no esperan respuesta ose que interactúa con los métodos del Backend que devuelven (return NoContent();)
    public async Task<HttpResponseWrapper<object>> PostAsync<T>(string url, T model)
    {
        var messageJSON = JsonSerializer.Serialize(model);//Serializa (convierte la clase o model a un Stream o Json)
        //Lo codifico(le digo en que formato voy a manejar los datos, en este caso Encoding.UTF8 por que estoy usando español donde se usan tildes, ñ),
        //Y le digo que los datos los voy a maneja como Json("application/json")
        var messageContet = new StringContent(messageJSON, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PostAsync(url, messageContet);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<TActionResponse>> PostAsync<T, TActionResponse>(string url, T model)
    {
        var messageJSON = JsonSerializer.Serialize(model);//Serializa
        var messageContet = new StringContent(messageJSON, Encoding.UTF8, "application/json");//Codifica
        var responseHttp = await _httpClient.PostAsync(url, messageContet);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswer<TActionResponse>(responseHttp);
            return new HttpResponseWrapper<TActionResponse>(response, false, responseHttp);
        }
        return new HttpResponseWrapper<TActionResponse>(default, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    private async Task<T> UnserializeAnswer<T>(HttpResponseMessage responseHttp)
    {
        var response = await responseHttp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(response, _jsonDefaultOptions)!;
    }
}