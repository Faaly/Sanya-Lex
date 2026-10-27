# Sanya-Lex

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/your/repo/license)
[![C#](https://img.shields.io/badge/language-C%23-orange.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ONNX Runtime](https://img.shields.io/badge/ONNX_Runtime-Embedded-blue.svg)](https://onnxruntime.ai/)

---

## Overview
**Sanya-Lex** is a lightweight, high-performance sub-system for **SANYA** (*Shell-integrated Artificial Neural Yielding Assistant. A shell-native, local ReAct agent and MCP client with a JIT-RAG tool architecture.*)
Sanya-Lex handles **local RAG (Retrieval-Augmented Generation)**, **semantic facts**, and **dynamic tool injection** using **C# Native AOT Binary**, **ONNX Runtime for embeddings**, and **SQLite Vector Database**.

---

## Features
- **Local RAG** – Retrieve and generate context from local data.
- **Semantic Facts** – Store and query structured knowledge.
- **Dynamic Tool Injection** – Register and invoke tools on-the-fly.
- **Lightweight & Fast** – Optimized for Zsh integration.

---

## Prerequisites
- **.NET 10 SDK** (for Native AOT compilation)
- **ONNX Runtime** (embedded for embeddings)
- **SQLite** (for vector storage)

---

## Installation

### 1. Clone the Repository
```sh
git clone https://github.com/Faaly/Sanya-Lex.git
cd sanya-lex
```

### 2. Build the Project
```sh
dotnet publish -c Release -r linux-x64 --self-contained true
```

### 3. Install Dependencies

#### Model Requirements
Sanya-Lex requires an ONNX-exported Sentence-Transformer model and its matching tokenizer placed in `~/.local/share/sanya/models/`:
- **Default Model:** `all-MiniLM-L6-v2` (384-dimensional embeddings)
- **Files needed:**
  - `model.onnx`
  - `tokenizer.json`

> **Note:** Ensure the ONNX model outputs 384-dimensional vectors to match the database schema.

### Installation via Script
For a quick system-wide installation, use the provided script:
```sh
sh ./install.sh
```
This will compile the project and symlink the binary to:
```sh
/usr/local/bin/sanya-lex
```

---

## Configuration
### Auto-Generated Settings
On first launch, Sanya-Lex generates:
```json
~/.config/sanya/settings.json
```

### Environment Variables (Optional)
Override default paths via environment variables:
```sh
export SANYA_LEX_DB_PATH="/custom/path/to/db"
export SANYA_LEX_MODEL_PATH="/custom/path/to/model.onnx"
export SANYA_LEX_TOKENIZER_PATH="/custom/path/to/tokenizer.json"
```

---

## Usage
### CLI Commands
| Command | Description |
|---------|-------------|
| `sanya-lex --ltmAdd --fact "User lives in Germany"` | Add a semantic fact |
| `sanya-lex --ltmAdd --tool "git_status description..."` | Register a tool |
| `sanya-lex --fact "Where does the user live?"` | Query facts |
| `sanya-lex --tool "check git status"` | Invoke a tool |

### Example Workflow
```sh
# Add a fact
sanya-lex --ltmAdd --fact "Project uses Rust"

# Query facts
sanya-lex --fact "What language is used in the project?"
```

---

## License
MIT © 2026 Faaly