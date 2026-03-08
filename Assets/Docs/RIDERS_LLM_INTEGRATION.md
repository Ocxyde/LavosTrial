# JetBrains Rider LLM Integration Guide

**Project:** Code.Lavos  
**Version:** 1.0.0  
**Date:** 2026-03-07  
**License:** GPL-3.0

---

## Overview

Connect your local LLM server to JetBrains Rider for AI-assisted coding directly in the IDE.

---

## Method 1: JetBrains AI Assistant (Official)

### Setup JetBrains AI

1. **Open Rider Settings**
   - `File` → `Settings` (Ctrl+Alt+S)
   - Go to `Tools` → `JetBrains AI Assistant`

2. **Enable AI Assistant**
   - Check `Enable AI Assistant`
   - Sign in with JetBrains account (required)

3. **Configure Local Provider** (if supported)
   - Currently, JetBrains AI uses cloud services
   - For local LLM, use Method 2 or 3 below

---

## Method 2: External Tool Integration (Recommended)

### Step 1: Create PowerShell Script

Create file: `Tools/llm-chat.ps1`

```powershell
# llm-chat.ps1 - Interactive LLM chat for Rider
param(
    [string]$Prompt = ""
)

$CONFIG_PATH = "D:\travaux_Unity\CodeDotLavos\Config\LLM_Server_Config.json"
$config = Get-Content $CONFIG_PATH | ConvertFrom-Json

$baseUrl = $config.llm.server.baseUrl
$model = $config.llm.model.name
$temperature = $config.llm.generation.temperature

if ([string]::IsNullOrEmpty($Prompt)) {
    $Prompt = Read-Host "Enter your question"
}

$body = @{
    model = $model
    prompt = $Prompt
    stream = $false
    options = @{
        temperature = $temperature
        top_p = $config.llm.generation.topP
        num_predict = $config.llm.model.maxTokens
    }
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/generate" -Method Post -Body $body -ContentType "application/json"
    Write-Host "`n $($response.response)" -ForegroundColor Green
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Step 2: Add as External Tool in Rider

1. **Open External Tools Settings**
   - `File` → `Settings` → `Tools` → `External Tools`

2. **Add New Tool**
   - Click `+` (Add)
   - Fill in:
     ```
     Name: LLM Chat
     Program: powershell.exe
     Arguments: -NoExit -ExecutionPolicy Bypass -File "D:\travaux_Unity\CodeDotLavos\Tools\llm-chat.ps1"
     Working Directory: D:\travaux_Unity\CodeDotLavos
     ```

3. **Add Keyboard Shortcut**
   - `File` → `Settings` → `Keymap`
   - Search for `External Tools - LLM Chat`
   - Assign shortcut (e.g., `Ctrl+Alt+L`)

---

## Method 3: Rider Plugin - CodeGPT (Best Integration)

### Install CodeGPT Plugin

1. **Open Plugin Manager**
   - `File` → `Settings` → `Plugins`
   - Click `Marketplace`

2. **Search and Install**
   - Search: `CodeGPT`
   - Click `Install`
   - Restart Rider

3. **Configure CodeGPT**
   - `File` → `Settings` → `Tools` → `CodeGPT`
   - Select Provider: `Ollama`
   - Base URL: `http://localhost:11434`
   - Model: `llama3.2:3b`

### CodeGPT Features

- ✅ Chat sidebar (Alt+Cmd+G)
- ✅ Code explanation
- ✅ Code generation
- ✅ Refactoring suggestions
- ✅ Bug fixes
- ✅ Unit test generation

---

## Method 4: Continue.dev Plugin (Alternative)

### Install Continue.dev

1. **Open Plugin Manager**
   - `File` → `Settings` → `Plugins` → `Marketplace`

2. **Search and Install**
   - Search: `Continue`
   - Click `Install`
   - Restart Rider

3. **Configure Continue**
   - Creates `.continue/config.json` in project root
   - Edit config:

```json
{
  "models": [
    {
      "title": "Ollama",
      "provider": "ollama",
      "model": "llama3.2:3b",
      "apiBase": "http://localhost:11434"
    }
  ],
  "tabAutocompleteModel": {
    "title": "Ollama",
    "provider": "ollama",
    "model": "llama3.2:1b",
    "apiBase": "http://localhost:11434"
  }
}
```

### Continue.dev Features

- ✅ Tab autocomplete
- ✅ Chat sidebar
- ✅ Highlight code + ask questions
- ✅ Edit code with AI (Ctrl+I)
- ✅ Multiple model support

---

## Method 5: Terminal Integration (Simple)

### Create Batch File

Create: `Tools/llm.cmd`

```batch
@echo off
set /p PROMPT="Enter question: "
curl -X POST http://localhost:11434/api/generate -H "Content-Type: application/json" -d "{\"model\":\"llama3.2:3b\",\"prompt\":\"%PROMPT%\",\"stream\":false}"
pause
```

### Use in Rider Terminal

1. Open Rider terminal (Alt+F12)
2. Run: `Tools\llm.cmd`
3. Type your question

---

## Quick Reference: Keyboard Shortcuts

| Action | Shortcut | Method |
|--------|----------|--------|
| LLM Chat | `Ctrl+Alt+L` | External Tool |
| CodeGPT Chat | `Alt+Cmd+G` | CodeGPT Plugin |
| Continue Chat | `Ctrl+Shift+L` | Continue Plugin |
| Edit with AI | `Ctrl+I` | Continue Plugin |
| Tab Autocomplete | `Tab` | Continue Plugin |
| Rider Terminal | `Alt+F12` | Built-in |

---

## Configuration Files

### For CodeGPT

**Location:** Rider Settings → Tools → CodeGPT

```
Provider: Ollama
Base URL: http://localhost:11434
Model: llama3.2:3b
Temperature: 0.7
Max Tokens: 2048
```

### For Continue.dev

**Location:** `.continue/config.json` (project root)

```json
{
  "models": [
    {
      "title": "Ollama Llama3.2",
      "provider": "ollama",
      "model": "llama3.2:3b",
      "apiBase": "http://localhost:11434"
    }
  ],
  "tabAutocompleteModel": {
    "title": "Ollama Fast",
    "provider": "ollama",
    "model": "llama3.2:1b",
    "apiBase": "http://localhost:11434"
  },
  "embeddingsProvider": {
    "provider": "ollama",
    "model": "nomic-embed-text",
    "apiBase": "http://localhost:11434"
  }
}
```

---

## Recommended Setup

### For Best Experience:

1. **Install Continue.dev Plugin** (free, open-source)
2. **Use two models:**
   - `llama3.2:1b` for tab autocomplete (fast)
   - `llama3.2:3b` for chat and explanations (balanced)
3. **Configure shortcuts:**
   - `Ctrl+Shift+L` for chat
   - `Ctrl+I` for inline edits
   - `Tab` for autocomplete

### Alternative (No Plugin):

1. **Use External Tool method**
2. **Create PowerShell script**
3. **Assign keyboard shortcut**
4. **Use Rider terminal for quick queries**

---

## Testing Your Setup

### Test 1: Connection

```bash
# In Rider terminal
curl http://localhost:11434/api/tags
```

Expected: List of installed models

### Test 2: Generation

```bash
curl http://localhost:11434/api/generate -d "{\"model\":\"llama3.2:3b\",\"prompt\":\"Hello\"}"
```

Expected: Response text

### Test 3: Plugin

1. Open CodeGPT/Continue sidebar
2. Type: "Explain this code"
3. Select some C# code
4. Send message

Expected: AI explanation

---

## Troubleshooting

### Plugin Not Showing

**Problem:** Plugin installed but not visible  
**Solution:**
- Restart Rider completely
- Check `File` → `Settings` → `Plugins` → `Installed`
- Verify plugin is enabled

### Connection Refused

**Problem:** Cannot connect to Ollama  
**Solution:**
```bash
# Check if Ollama is running
ollama list

# Start Ollama server
ollama serve
```

### Slow Responses

**Problem:** AI takes too long  
**Solution:**
1. Use smaller model: `llama3.2:1b`
2. Reduce `maxTokens` in config
3. Reduce `contextWindow`
4. Enable GPU acceleration

### Autocomplete Not Working

**Problem:** Tab autocomplete disabled  
**Solution:**
- `File` → `Settings` → `Editor` → `General` → `Code Completion`
- Enable `Show suggestions as you type`
- Check Continue.dev settings for autocomplete model

---

## Advanced: Custom Integration

### Create C# Helper Class

Add to your project: `Assets/Scripts/Editor/LLMHelper.cs`

```csharp
using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Threading.Tasks;

public class LLMHelper : EditorWindow
{
    private string _prompt = "";
    private string _response = "";
    private HttpClient _client = new HttpClient();

    [MenuItem("Tools/LLM Chat")]
    public static void ShowWindow()
    {
        GetWindow<LLMHelper>("LLM Chat");
    }

    private void OnGUI()
    {
        GUILayout.Label("LLM Chat - Code.Lavos", EditorStyles.boldLabel);

        _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.Height(100));

        if (GUILayout.Button("Send to LLM"))
        {
            SendPrompt();
        }

        if (!string.IsNullOrEmpty(_response))
        {
            GUILayout.Label("Response:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_response, GUILayout.Height(200));
        }
    }

    private async void SendPrompt()
    {
        var config = LoadConfig();
        var request = new
        {
            model = config.model.name,
            prompt = _prompt,
            stream = false
        };

        var content = new StringContent(
            JsonUtility.ToJson(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync(
            config.server.baseUrl + "/api/generate",
            content
        );

        var result = await response.Content.ReadAsStringAsync();
        _response = result;
    }

    private LLMConfig LoadConfig()
    {
        // Load from Config/LLM_Server_Config.json
        // Implement JSON parsing
    }
}
```

---

## Performance Tips

| Model | Use Case | Speed | Quality |
|-------|----------|-------|---------|
| `llama3.2:1b` | Tab autocomplete | ⚡⚡⚡ | ⭐⭐ |
| `llama3.2:3b` | Chat, explanations | ⚡⚡ | ⭐⭐⭐ |
| `llama3.2:8b` | Complex refactoring | ⚡ | ⭐⭐⭐⭐ |
| `qwen2.5:7b` | Code generation | ⚡⚡ | ⭐⭐⭐⭐ |

---

## Security Notes

- ✅ All processing is **local** (no data sent to cloud)
- ✅ No API keys required for local servers
- ✅ Your code never leaves your machine
- ✅ Firewall blocks external access by default

---

## Recommended Models for Coding

| Model | Size | VRAM | Code Quality | Speed |
|-------|------|------|--------------|-------|
| `llama3.2:1b` | 1B | 2GB | ⭐⭐ | ⚡⚡⚡ |
| `llama3.2:3b` | 3B | 4GB | ⭐⭐⭐ | ⚡⚡ |
| `qwen2.5:7b` | 7B | 6GB | ⭐⭐⭐⭐ | ⚡⚡ |
| `deepseek-coder:6.7b` | 7B | 6GB | ⭐⭐⭐⭐⭐ | ⚡ |
| `codellama:7b` | 7B | 6GB | ⭐⭐⭐⭐ | ⚡⚡ |

---

**Setup complete! Happy coding with AI assistance in Rider!**

---

*Guide generated - 2026-03-07 - Unity 6 (6000.3.7f1) compatible*
