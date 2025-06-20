# LittleHelper - Revit AI Assistant

**Transform natural language into Revit actions**  
An Autodesk Revit plugin integrating OpenAI to convert BIM requests into executable API commands through conversational AI.

## 🚀 Key Features
- Understands basic user requests like _ "Create a 10m concrete wall at origin"
- Creates a valid JSON commad for Revit API integration
- If there are missing paramerters, it requests for them through conversation
- Passes the instruction to the Revit API for execution

## ⚙️ Technical Highlights
- Maintains conversation history for coherent interactions

## 🛠️ Installation
1. Add OpenAI API key to `ChatGPTService.cs`
2. Build with .NET 4.8 & Revit 2024+
3. Deploy DLL to `%APPDATA%\Autodesk\Revit\Addins\2024`
