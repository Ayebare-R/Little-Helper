using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;



namespace LittleHelper

{

    public partial class ChatWindow : Window

    {
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Autodesk.Revit.DB.Document _doc;
        private List<ChatMessage> _conversationHistory = new List<ChatMessage>();
        private string _latestJsonCommand = null;

        public ChatWindow(UIApplication uiapp)

        {

            InitializeComponent();

            _uiapp = uiapp;

            _uidoc = uiapp.ActiveUIDocument;

            _doc = _uidoc.Document;

        }



        private async void SendButton_Click(object sender, RoutedEventArgs e)

        {

            string userInput = UserInput.Text;

            // Clear the input box
            UserInput.Text = string.Empty;


            if (!string.IsNullOrEmpty(userInput))

            {

                // Display user's message
                ChatHistory.AppendText("User: " + userInput + "\n");
                _conversationHistory.Add(new ChatMessage { role = "user", content = userInput });

                try
                {

                    // Call ChatGPT API
                    (string userMessage, string jsonCommand) = await ChatGPTService.GetResponse(_conversationHistory);

                    // Add assistant's response to the conversation 
                    // TODO: Add the jsonCommand the the conversation history as well because I think it's relvant for context
                    _conversationHistory.Add(new ChatMessage { role = "assistant", content = userMessage });

                    // Display ChatGPT's response
                    ChatHistory.AppendText("Assistant: " + userMessage + "\n");

                    if (!string.IsNullOrEmpty(jsonCommand))
                    {
                        _latestJsonCommand = jsonCommand;
                    }

                }
                catch (Exception ex)
                {
                    // Display error message
                    ChatHistory.AppendText("Assistant: Sorry, an error occurred: " + ex.Message + "\n");
                    Console.WriteLine("Exception: " + ex.ToString());
                }
            }


        }


        private void ViewRevitCodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_latestJsonCommand))
            {
                RevitCodeWindow revitCodeWindow = new RevitCodeWindow(_latestJsonCommand);
                revitCodeWindow.Show();
            }
            else
            {
                MessageBox.Show("No JSON commands available. Interact with the assistant to generate one.", 
                                "Revit Code Viewer", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
            }
        }

        private void ExecuteRevitCodeButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(_latestJsonCommand)) {
                MessageBox.Show(
                    "No JSON command available.",
                    "Execute Revit Code",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );


            }

            if (RevitCommand.TryExecuteJsonCommand(_uiapp, _latestJsonCommand, out string errorMessage)) {
                MessageBox.Show(
                    "Succesfully executed the JSON command!",
                    "Execute Revit Code",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            else {
                MessageBox.Show($"Failed to execute JSON command: \n\n{errorMessage}",
                "Execute Revit Code",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            }
        }
    }   

}