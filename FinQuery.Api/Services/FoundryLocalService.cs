using Microsoft.AI.Foundry.Local;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace FinQuery.Api.Services;

public class FoundryLocalService
{
    private readonly ILogger<FoundryLocalService> _logger;
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    // Cache the loaded models so we don't re-load them on every request
    private IModel? _embeddingModel;
    private IModel? _chatModel;
    private OpenAIClient? _openAiClient;

    public FoundryLocalService(ILogger<FoundryLocalService> logger)
    {
        _logger = logger;
    }

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            _logger.LogInformation("Initializing Microsoft AI Foundry Local SDK...");

            // FoundryLocalManager is a singleton — CreateAsync can only be called ONCE.
            // If it was already created (e.g. from a previous hot-reload), just use Instance.
            try
            {
                var config = new Configuration 
                { 
                    AppName = "finquery",
                    Web = new Configuration.WebService { Urls = "http://127.0.0.1:5272" }
                };
                await FoundryLocalManager.CreateAsync(config, _logger);
            }
            catch (FoundryLocalException ex) when (ex.Message.Contains("already been created"))
            {
                _logger.LogInformation("FoundryLocalManager already exists, reusing Instance.");
            }

            var result = FoundryLocalManager.Instance.ToString() ?? "OK";
            _logger.LogInformation("Foundry.Local.Core initialized successfully: {Result}", result);

            // Start the web service (OpenAI-compatible REST endpoint on port 5272)
            try
            {
                await FoundryLocalManager.Instance.StartWebServiceAsync();
                _logger.LogInformation("Foundry Local web service started on port 5272.");
            }
            catch (FoundryLocalException ex) when (ex.Message.Contains("already"))
            {
                _logger.LogInformation("Foundry Local web service already running on port 5272.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not start web service: {Message}", ex.Message);
            }

            _isInitialized = true;
            _logger.LogInformation("Microsoft AI Foundry Local SDK initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Microsoft AI Foundry Local SDK.");
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<OpenAIClient> GetOrCreateOpenAIClientAsync()
    {
        if (_openAiClient != null) return _openAiClient;

        _openAiClient = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential("unused"),
            new OpenAIClientOptions { Endpoint = new Uri("http://localhost:5272/v1") }
        );
        return _openAiClient;
    }

    /// <summary>
    /// Gets an EmbeddingClient using the stable OpenAI-compatible HTTP endpoint on port 5272.
    /// The web service is started separately by FoundryLocalManager.StartWebServiceAsync().
    /// See: https://learn.microsoft.com/en-us/azure/foundry-local/how-to/how-to-generate-embeddings
    /// </summary>
    public async Task<EmbeddingClient?> GetEmbeddingClientAsync(string modelAlias = "qwen3-embedding-0.6b")
    {
        await EnsureInitializedAsync();

        try
        {
            if (_embeddingModel == null)
            {
                var mgr = FoundryLocalManager.Instance;
                var catalog = await mgr.GetCatalogAsync();
                _embeddingModel = await catalog.GetModelAsync(modelAlias);

                if (_embeddingModel == null)
                {
                    _logger.LogWarning("Model '{ModelAlias}' not found in catalog.", modelAlias);
                    return null;
                }

                _logger.LogInformation("Loading embedding model '{ModelAlias}'...", modelAlias);
                await _embeddingModel.DownloadAsync(p => _logger.LogInformation("Downloading {ModelAlias}: {Progress:F1}%", modelAlias, p));
                await _embeddingModel.LoadAsync();
                _logger.LogInformation("Embedding model '{ModelId}' loaded successfully.", _embeddingModel.Id);
            }

            // Stable: use the OpenAI-compatible HTTP endpoint via the web service on port 5272
            var httpClient = await GetOrCreateOpenAIClientAsync();
            return httpClient.GetEmbeddingClient(_embeddingModel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring EmbeddingClient for '{ModelAlias}'.", modelAlias);
            return null;
        }
    }

    public async Task<ChatClient?> GetChatClientAsync(string modelAlias = "qwen2.5-0.5b")
    {
        await EnsureInitializedAsync();

        try
        {
            if (_chatModel == null)
            {
                var mgr = FoundryLocalManager.Instance;
                var catalog = await mgr.GetCatalogAsync();
                _chatModel = await catalog.GetModelAsync(modelAlias);

                if (_chatModel == null)
                {
                    _logger.LogWarning("Model '{ModelAlias}' not found in catalog.", modelAlias);
                    return null;
                }

                _logger.LogInformation("Loading chat model '{ModelAlias}'...", modelAlias);
                await _chatModel.DownloadAsync(p => _logger.LogInformation("Downloading {ModelAlias}: {Progress:F1}%", modelAlias, p));
                await _chatModel.LoadAsync();
                _logger.LogInformation("Model {ModelId} loaded successfully: {Result}", _chatModel.Id, _chatModel.ToString());
            }

            // Stable: use the OpenAI-compatible HTTP endpoint via the web service on port 5272
            var httpClient = await GetOrCreateOpenAIClientAsync();
            return httpClient.GetChatClient(_chatModel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring ChatClient for '{ModelAlias}'.", modelAlias);
            return null;
        }
    }
}
