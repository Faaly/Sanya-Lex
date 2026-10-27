#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "[INFO] Building Sanya-Lex (Release, Self-Contained)..."
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true

PUBLISH_DIR="bin/Release/net10.0/linux-x64/publish"

echo "[INFO] Copying ONNX Runtime library..."
if [ -f "/usr/lib/libonnxruntime.so" ]; then
    cp /usr/lib/libonnxruntime.so "$PUBLISH_DIR/onnxruntime.dll"
    cp /usr/lib/libonnxruntime.so "$PUBLISH_DIR/libonnxruntime.so"
elif [ -f "/usr/local/lib/libonnxruntime.so" ]; then
    cp /usr/local/lib/libonnxruntime.so "$PUBLISH_DIR/onnxruntime.dll"
    cp /usr/local/lib/libonnxruntime.so "$PUBLISH_DIR/libonnxruntime.so"
else
    echo "[WARNING] libonnxruntime.so not found in /usr/lib/ or /usr/local/lib!"
fi

echo "[INFO] Installing to /opt/sanya-lex..."
sudo mkdir -p /opt/sanya-lex
sudo rm -rf /opt/sanya-lex/*
sudo cp -r "$PUBLISH_DIR"/* /opt/sanya-lex/

BINARY_NAME="sanya-lex" 

sudo ln -sf "/opt/sanya-lex/$BINARY_NAME" /usr/local/bin/sanya-lex
sudo chmod +x /usr/local/bin/sanya-lex

echo "[SUCCESS] Sanya-Lex successfully published and linked to /usr/local/bin/sanya-lex!"