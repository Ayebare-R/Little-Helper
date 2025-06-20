# LittleHelper - Revit AI Assistant

**Transform natural language into Revit actions**  
An Autodesk Revit plugin integrating OpenAI to convert BIM requests into executable API commands through conversational AI.

## 🚀 Key Features
- **AI-Powered BIM Assistant**: Understands requests like _"Create a 10m concrete wall at origin with 3 windows"_
- **Smart Command Generation**: Creates validated JSON commands for Revit API operations
- **Interactive Dialogue**: Requests missing parameters through conversation
- **One-Click Execution**: Instantly creates Revit elements (walls currently supported)
- **Code Inspection**: View generated API commands before execution
- **Unit Conversion**: Automatic metric (meters) to Revit internal units

## ⚙️ Technical Highlights
- **Context-Aware**: Maintains conversation history for coherent interactions
- **Validation Pipeline**:  
  ✓ JSON schema verification  
  ✓ Mandatory field checks  
  ✓ Parameter integrity validation
- **Token Optimization**: Auto-truncates long conversations (3500 token limit)

## 🛠️ Installation
1. Add OpenAI API key to `ChatGPTService.cs`
2. Build with .NET 4.8 & Revit 2024+
3. Deploy DLL to `%APPDATA%\Autodesk\Revit\Addins\2024`
