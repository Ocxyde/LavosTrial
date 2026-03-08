# Local LLM Server Setup Guide

**Project:** Code.Lavos  
**Version:** 1.0.0  
**License:** GPL-3.0  
**Date:** 2026-03-07

---

## Overview

This configuration enables integration with local LLM servers for AI-assisted features in Code.Lavos.

---

## Supported Providers

| Provider | Default Port | Config Key |
|----------|--------------|------------|
| **Ollama** | 11434 | `ollama` |
| **LM Studio** | 1234 | `lmstudio` |
| **KoboldCPP** | 5001 | `koboldcpp` |
| **text-generation-webui** | 5000 | `oobabooga` |

---

## Quick Start

### 1. Install Ollama (Recommended)

```bash
# Windows (PowerShell)
winget install Ollama.Ollama

# Or download from: https://ollama.com
```

### 2. Pull a Model

```bash
# Small model (fast, low VRAM)
ollama pull llama3.2:1b

# Medium model (balanced)
ollama pull llama3.2:3b

# Large model (best quality, needs 8GB+ VRAM)
ollama pull llama3.2:8b
```

### 3. Verify Server

```bash
# Check server status
curl http://localhost:11434/api/tags

# Test generation
curl http://localhost:11434/api/generate -d "{\"model\":\"llama3.2:3b\",\"prompt\":\"Hello\"}"
```

### 4. Configure Unity

Edit `Config/LLM_Server_Config.json`:

```json
{
  "llm": {
    "enabled": true,
    "provider": "ollama",
    "server": {
      "baseUrl": "http://localhost:11434"
    },
    "model": {
      "name": "llama3.2:3b"
    }
  }
}
```

---

## Configuration Reference

### Server Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseUrl` | string | `http://localhost:11434` | LLM server URL |
| `generateEndpoint` | string | `/api/generate` | Text generation endpoint |
| `chatEndpoint` | string | `/api/chat` | Chat completion endpoint |
| `timeout` | int | `120000` | Request timeout (ms) |
| `maxRetries` | int | `3` | Max retry attempts |

### Model Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `name` | string | `llama3.2:3b` | Model identifier |
| `contextWindow` | int | `4096` | Max context tokens |
| `maxTokens` | int | `2048` | Max output tokens |

### Generation Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `temperature` | float | `0.7` | Creativity (0.0-2.0) |
| `topP` | float | `0.9` | Nucleus sampling (0.0-1.0) |
| `topK` | int | `40` | Top-K sampling |
| `repeatPenalty` | float | `1.1` | Repetition penalty |
| `seed` | int | `-1` | Random seed (-1 = random) |

---

## Provider-Specific Setup

### Ollama

**Pros:** Easy setup, many models, good API  
**Cons:** Less control than KoboldCPP

```json
{
  "provider": "ollama",
  "server": {
    "baseUrl": "http://localhost:11434"
  },
  "model": {
    "name": "llama3.2:3b"
  }
}
```

**Recommended Models:**
- `llama3.2:1b` - Very fast, 2GB VRAM
- `llama3.2:3b` - Balanced, 4GB VRAM
- `llama3.2:8b` - Best quality, 8GB VRAM
- `mistral:7b` - Great all-rounder, 6GB VRAM
- `qwen2.5:7b` - Excellent for code, 6GB VRAM

---

### LM Studio

**Pros:** GUI, easy model management  
**Cons:** Slightly slower than Ollama

```json
{
  "provider": "lmstudio",
  "server": {
    "baseUrl": "http://localhost:1234"
  }
}
```

**Setup:**
1. Download from https://lmstudio.ai
2. Download a model in the app
3. Click "Start Server"
4. Set `baseUrl` to `http://localhost:1234`

---

### KoboldCPP

**Pros:** Maximum control, low VRAM usage  
**Cons:** More complex setup

```json
{
  "provider": "koboldcpp",
  "server": {
    "baseUrl": "http://localhost:5001"
  }
}
```

**Setup:**
1. Download from GitHub (lostruud/koboldcpp)
2. Run: `koboldcpp.exe model.gguf --port 5001`
3. Set `baseUrl` to `http://localhost:5001`

---

### text-generation-webui (Oobabooga)

**Pros:** Most features, extensions  
**Cons:** Heavy, complex

```json
{
  "provider": "text-generation-webui",
  "server": {
    "baseUrl": "http://localhost:5000"
  }
}
```

**Setup:**
1. Install from GitHub (oobabooga/text-generation-webui)
2. Run: `python server.py --api`
3. Set `baseUrl` to `http://localhost:5000`

---

## C# Usage Example

```csharp
using UnityEngine;
using System.Collections;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public class LLMClient : MonoBehaviour
{
    private HttpClient _httpClient;
    private LLMConfig _config;

    private void Start()
    {
        _httpClient = new HttpClient();
        LoadConfig();
    }

    private void LoadConfig()
    {
        TextAsset json = Resources.Load<TextAsset>("LLM_Server_Config");
        _config = JsonConvert.DeserializeObject<LLMConfig>(json.text);
    }

    public IEnumerator GenerateResponse(string prompt, System.Action<string> callback)
    {
        string url = _config.llm.server.baseUrl + _config.llm.server.generateEndpoint;

        var request = new
        {
            model = _config.llm.model.name,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = _config.llm.generation.temperature,
                top_p = _config.llm.generation.topP,
                num_predict = _config.llm.model.maxTokens
            }
        };

        string json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = yield return _httpClient.PostAsync(url, content);
        string result = yield return response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<GenerateResponse>(result);
        callback?.Invoke(data.response);
    }
}

[System.Serializable]
public class LLMConfig
{
    public LLMSettings llm;
}

[System.Serializable]
public class LLMSettings
{
    public bool enabled;
    public string provider;
    public ServerConfig server;
    public ModelConfig model;
    public GenerationConfig generation;
}

[System.Serializable]
public class ServerConfig
{
    public string baseUrl;
    public string generateEndpoint;
    public string chatEndpoint;
    public int timeout;
    public int maxRetries;
}

[System.Serializable]
public class ModelConfig
{
    public string name;
    public int contextWindow;
    public int maxTokens;
}

[System.Serializable]
public class GenerationConfig
{
    public float temperature;
    public float topP;
    public int topK;
    public float repeatPenalty;
    public int seed;
}

public class GenerateResponse
{
    public string model;
    public string response;
    public bool done;
}
```

---

## Troubleshooting

### Connection Refused

**Problem:** Cannot connect to server  
**Solution:**
1. Verify server is running
2. Check `baseUrl` is correct
3. Check firewall settings
4. Try `curl http://localhost:11434/api/tags`

### Model Not Found

**Problem:** Model not downloaded  
**Solution:**
```bash
ollama pull llama3.2:3b
```

### Timeout Errors

**Problem:** Request takes too long  
**Solution:**
1. Increase `timeout` in config
2. Use smaller model
3. Reduce `maxTokens`
4. Reduce `contextWindow`

### Out of Memory

**Problem:** GPU VRAM exhausted  
**Solution:**
1. Use smaller model (1b instead of 8b)
2. Reduce `contextWindow`
3. Close other GPU applications
4. Use CPU mode (slower but works)

---

## Performance Tips

| Model | VRAM | Context | Speed | Quality |
|-------|------|---------|-------|---------|
| `llama3.2:1b` | 2GB | 4K | Fast | Good |
| `llama3.2:3b` | 4GB | 4K | Medium | Better |
| `llama3.2:8b` | 8GB | 4K | Slow | Best |
| `mistral:7b` | 6GB | 8K | Medium | Excellent |
| `qwen2.5:7b` | 6GB | 32K | Medium | Best for Code |

---

## Security Notes

- Local servers are **offline by default** (no data sent externally)
- API key field is for future cloud integration (currently unused)
- All processing happens on your machine
- No telemetry or data collection

---

## License

This configuration is part of Code.Lavos, licensed under GPL-3.0.

---

**Setup complete! Happy coding with AI assistance!**
