using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;

namespace LittleHelper
{
    /// <summary>
    /// A utility class that can parse JSON commands (created by ChatGPT)
    /// and execute them in Revit—e.g., create a wall.
    /// 
    /// This no longer implements IExternalCommand directly (though it could),
    /// and it does not call ChatGPT. Instead, it relies on ChatWindow or
    /// another UI to supply the JSON.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class RevitCommand
    {
        /// <summary>
        /// Attempts to parse and execute the provided JSON command.
        /// Returns true if successful, or false with an errorMessage otherwise.
        /// 
        /// Usage Example (from ChatWindow or elsewhere):
        ///     if (RevitCommand.TryExecuteJsonCommand(uiapp, json, out string err))
        ///     {
        ///         // Succeeded
        ///     }
        ///     else
        ///     {
        ///         // Failed - check err
        ///     }
        /// </summary>
        public static bool TryExecuteJsonCommand(UIApplication uiapp, string jsonCommand, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrEmpty(jsonCommand))
            {
                errorMessage = "No JSON command to process.";
                return false;
            }

            // 1) Validate and parse the JSON
            if (!JsonValidator.TryValidateJson(jsonCommand, out JObject parsedJson, out string validationError))
            {
                errorMessage = $"JSON Validation Failed: {validationError}";
                return false;
            }

            // 2) Check the method
            string method = parsedJson["method"]?.ToString();
            if (string.IsNullOrEmpty(method))
            {
                errorMessage = "JSON missing 'method' field.";
                return false;
            }

            if (method.Equals("CreateWall", StringComparison.OrdinalIgnoreCase))
            {
                // 3) Extract 'parameters'
                JObject parameters = parsedJson["parameters"] as JObject;
                if (parameters == null)
                {
                    errorMessage = "JSON missing 'parameters' object.";
                    return false;
                }

                // 4) Create the wall
                return CreateWall(uiapp, parameters, out errorMessage);
            }
            else
            {
                // If it's some other command, you could handle it here.
                // For now, just report that it's not recognized.
                errorMessage = $"Unsupported method '{method}'.";
                return false;
            }
        }

        /// <summary>
        /// Actually create the wall from the parameters in the JSON.
        /// Returns true if successful, otherwise false with an error message.
        /// </summary>
        private static bool CreateWall(UIApplication uiapp, JObject parameters, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                // 1) Grab current Document
                Document doc = uiapp.ActiveUIDocument.Document;

                // 2) Parse basic parameters
                double length   = parameters["length"]?.ToObject<double>() ?? 0;
                double height   = parameters["height"]?.ToObject<double>() ?? 0;
                double thickness= parameters["thickness"]?.ToObject<double>() ?? 0;

                // "type": the name of the WallType
                string wallTypeName = parameters["type"]?.ToString() ?? "Generic - 200mm";

                // "startPoint": where the wall begins
                JObject startPoint = parameters["startPoint"] as JObject;
                if (startPoint == null)
                {
                    errorMessage = "JSON missing 'startPoint' object.";
                    return false;
                }
                double sx = startPoint["x"]?.ToObject<double>() ?? 0;
                double sy = startPoint["y"]?.ToObject<double>() ?? 0;
                double sz = startPoint["z"]?.ToObject<double>() ?? 0;

                // For demonstration, assume these values are in meters
                // Convert from meters to Revit internal units (feet)
                double lengthFeet    = UnitUtils.ConvertToInternalUnits(length,   SpecTypeId.Length);
                double heightFeet    = UnitUtils.ConvertToInternalUnits(height,   SpecTypeId.Length);
                double thicknessFeet = UnitUtils.ConvertToInternalUnits(thickness,SpecTypeId.Length);

                // Convert the startPoint
                XYZ startXYZ = new XYZ(
                    UnitUtils.ConvertToInternalUnits(sx, SpecTypeId.Length),
                    UnitUtils.ConvertToInternalUnits(sy, SpecTypeId.Length),
                    UnitUtils.ConvertToInternalUnits(sz, SpecTypeId.Length));

                // Let's place the wall along the X-axis from the start point
                XYZ endXYZ = startXYZ + (XYZ.BasisX * lengthFeet);

                // 3) Find or create a suitable WallType
                WallType wallType = new FilteredElementCollector(doc)
                    .OfClass(typeof(WallType))
                    .Cast<WallType>()
                    .FirstOrDefault(wt => wt.Name.Equals(wallTypeName, StringComparison.OrdinalIgnoreCase));

                if (wallType == null)
                {
                    errorMessage = $"Could not find wall type '{wallTypeName}' in project.";
                    return false;
                }

                // 4) Find or assume "Level 1"
                Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault(l => l.Name.Equals("Level 1", StringComparison.OrdinalIgnoreCase));

                if (level == null)
                {
                    errorMessage = "Could not find Level 1. Please specify a valid level or rename your Revit levels.";
                    return false;
                }

                // 5) Create the wall in a transaction
                using (Transaction tx = new Transaction(doc, "Create Wall from ChatGPT"))
                {
                    tx.Start();

                    Line wallLine = Line.CreateBound(startXYZ, endXYZ);
                    Wall newWall = Wall.Create(doc, wallLine, wallType.Id, level.Id, heightFeet, 0, false, false);

                    // You could do more advanced logic here, e.g., 
                    // handling the thickness by picking a different WallType or adjusting its structure.
                    
                    tx.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error creating wall: {ex.Message}";
                return false;
            }
        }
    }
}
