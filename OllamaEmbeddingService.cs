using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OllamaEmbeddingService
{
    private readonly string _ollamaUrl;
    private readonly string _model;
    private readonly HttpClient _httpClient;

    public OllamaEmbeddingService(string ollamaUrl = "http://localhost:11434", string model = "nomic-embed-text")
    {
        _ollamaUrl = ollamaUrl.TrimEnd('/');
        _model = model;
        _httpClient = new HttpClient();
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = _model,
            input = text
        };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/embed", content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            var embeddingPropertyExists = doc.RootElement.TryGetProperty("embeddings", out var embedding);
            if (!embeddingPropertyExists)
                throw new Exception("The 'embedding' property is missing in the Ollama API response.");
            var result = new float[embedding[0].GetArrayLength()];
            int i = 0;
            foreach (var v in embedding[0].EnumerateArray())
                result[i++] = v.GetSingle();
            return result;
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors
            throw new Exception("Failed to get embedding from Ollama API.", ex);
        }
        catch (JsonException ex)
        {
            // Handle JSON parsing errors
            throw new Exception("Failed to parse embedding response from Ollama API.", ex);
        }
        catch (Exception ex)
        {
            // Handle all other errors
            throw new Exception("An unexpected error occurred while getting embedding.", ex);
        }
    }

    public static float[] NormalizeEmbedding(float[] embedding)
    {
        if (embedding == null || embedding.Length == 0)
            return embedding;

        // L2 normalization
        double sumSquares = 0;
        foreach (var v in embedding)
            sumSquares += v * v;

        var norm = Math.Sqrt(sumSquares);
        if (norm == 0)
            return embedding;

        var normalized = new float[embedding.Length];
        for (int i = 0; i < embedding.Length; i++)
            normalized[i] = (float)(embedding[i] / norm);

        return normalized;
    }
}
