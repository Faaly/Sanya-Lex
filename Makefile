# Variables
PROJECT_NAME=Sanya-Lex
PUBLISH_DIR=bin/Release/net10.0/linux-x64/publish
INSTALL_DIR ?= /opt/sanya-lex
BINARY_PATH=$(INSTALL_DIR)/$(PROJECT_NAME)
LINK_PATH ?= /usr/local/bin/sanya-lex
ONNX_SRC ?= /usr/lib/libonnxruntime.so

all: build

build:
	@echo "--- Start Dotnet Publish ---"
	dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
	@echo "--- Copy ONNX Runtime ---"
	cp $(ONNX_SRC) $(PUBLISH_DIR)/onnxruntime.dll

install:
	@echo "--- Clean sanya-lex in opt and usr ---"
	sudo rm -r $(INSTALL_DIR)
	sudo rm  $(LINK_PATH)
	@echo "--- Copy $(INSTALL_DIR) ---"
	sudo cp -r $(PUBLISH_DIR) $(INSTALL_DIR)
	@echo "--- Create Symlink ---"
	sudo ln -sf $(BINARY_PATH) $(LINK_PATH)
	@echo "--- Done! Sanya-Lex is global ---"

deploy: build install