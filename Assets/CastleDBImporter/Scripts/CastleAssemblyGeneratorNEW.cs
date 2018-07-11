using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;


namespace CastleDBImporter
{
    public class AssembyBuilderExample
    {
        [MenuItem("CastleDB Importer/Rebuild Assembly")]
        public static void BuildCastleDBAssembly()
        {
            //maybe do something here to load the TextAsset
            // BuildAssembly();
        }

        [MenuItem("CastleDB Importer/Rebuild SimpleJSON DLL")]
        public static void BuildSimpleJSON()
        {
            BuildSimpleJSONDLL();
        }

        [MenuItem("CastleDB Importer/Rebuild CastleDB DLL")]
        public static void BuildCastleDB()
        {
            BuildCastleDBDLL();
        }

        public static void BuildCastleDBDLL()
        {
            var scripts = new[] { "Assets/CastleDBImporter/Scripts/CastleDB.cs"};
            var outputAssembly = "Temp/MyAssembly/CastleDB.dll";
            var assemblyProjectPath = "Assets/CastleDBImporter/CompiledTypes/CastleDB.dll";

            Directory.CreateDirectory("Temp/MyAssembly");


            var assemblyBuilder = new AssemblyBuilder(outputAssembly, scripts);

            // Exclude a reference to the copy of the assembly in the Assets folder, if any.
            assemblyBuilder.excludeReferences = new string[] { assemblyProjectPath };

            // Called on main thread
            assemblyBuilder.buildStarted += delegate(string assemblyPath)
            {
                Debug.LogFormat("Assembly build started for {0}", assemblyPath);
            };

            // Called on main thread
            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Assembly build finished for {0}", assemblyPath);
                Debug.LogFormat("Warnings: {0} - Errors: {0}", errorCount, warningCount);

                foreach (CompilerMessage message in compilerMessages)
                {
                    if(message.type == CompilerMessageType.Error)
                    {
                        Debug.Log("ERROR: " + message.message);
                    }
                }

                if(errorCount == 0)
                {
                    File.Copy(outputAssembly, assemblyProjectPath, true);
                    AssetDatabase.ImportAsset(assemblyProjectPath);
                }
            };

            // Start build of assembly
            if(!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
                return;
            }

                while(assemblyBuilder.status != AssemblyBuilderStatus.Finished)
                    System.Threading.Thread.Sleep(10);
        }
        public static void BuildSimpleJSONDLL()
        {
            var scripts = new[] { "Assets/CastleDBImporter/Scripts/SimpleJSON.cs"};
            var outputAssembly = "Temp/MyAssembly/SimpleJSON.dll";
            var assemblyProjectPath = "Assets/CastleDBImporter/CompiledTypes/SimpleJSON.dll";

            Directory.CreateDirectory("Temp/MyAssembly");


            var assemblyBuilder = new AssemblyBuilder(outputAssembly, scripts);

            // Exclude a reference to the copy of the assembly in the Assets folder, if any.
            assemblyBuilder.excludeReferences = new string[] { assemblyProjectPath };

            // Called on main thread
            assemblyBuilder.buildStarted += delegate(string assemblyPath)
            {
                Debug.LogFormat("Assembly build started for {0}", assemblyPath);
            };

            // Called on main thread
            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Assembly build finished for {0}", assemblyPath);
                Debug.LogFormat("Warnings: {0} - Errors: {0}", errorCount, warningCount);

                if(errorCount == 0)
                {
                    File.Copy(outputAssembly, assemblyProjectPath, true);
                    AssetDatabase.ImportAsset(assemblyProjectPath);
                }
            };

            // Start build of assembly
            if(!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
                return;
            }

                while(assemblyBuilder.status != AssemblyBuilderStatus.Finished)
                    System.Threading.Thread.Sleep(10);
        }

        public static void BuildAssembly(CastleDB.RootNode root)
        {
            //TODO: need to delete previous dll, then build this one
            //TODO: need to do path management and create a path if it doesnt exsit and allow for a custom path
            var outputAssembly = "Temp/CastleDBAssembly/CastleDBAssembly.dll";
            var assemblyProjectPath = "Assets/CastleDBImporter/CompiledTypes/CastleDBAssembly.dll";

            Directory.CreateDirectory("Temp/CastleDBAssembly");

            // Create scripts
            List<string> scripts = new List<string>();

            foreach (CastleDB.SheetNode sheet in root.Sheets)
            {
                string scriptPath = "Temp/CastleDBAssembly/"+sheet.Name+".cs";
                scripts.Add(scriptPath);
                string scriptName = Path.GetFileNameWithoutExtension(scriptPath);
                //geneate the fields with type
                string fieldText = "";
                int typeCount = sheet.Columns.Count;
                int numRows = sheet.Rows.Count;
                for (int i = 0; i < typeCount; i++)
                {
                    CastleDB.ColumnNode column = sheet.Columns[i];
                    Type fieldType = CastleDBUtils.GetTypeFromCastleDBTypeStr(column.TypeStr);
                    if(fieldType != typeof(Enum)) //non-enum, normal field
                    {
                        fieldText += ("public " + fieldType.ToString() + " " + column.Name + ";\n");
                    }
                    else //enum type
                    {
                        string[] enumValueNames = CastleDBUtils.GetEnumValuesFromTypeString(column.TypeStr);
                        string enumEntries = "";
                        string attribute = "";
                        string newEnum = "";
                        if(CastleDBUtils.GetTypeNumFromCastleDBTypeString(column.TypeStr) == "10") //flag
                        {
                            attribute = "[FlagsAttribute]";
                            fieldText += ("public " + column.Name + "Flag" + " " + column.Name + ";\n");
                            for (int val = 0; val < enumValueNames.Length; val++)
                            {
                                enumEntries += (enumValueNames[val] + " = " + (int)Math.Pow(2, val));
                                if(val + 1 < enumValueNames.Length) {enumEntries += ",";};
                            }
                            newEnum = attribute + " public enum " + column.Name + "Flag" + " { " + enumEntries + " }";
                        }
                        else
                        {
                            fieldText += ("public " + column.Name + "Enum" + " " + column.Name + ";\n");
                            for (int val = 0; val < enumValueNames.Length; val++)
                            {
                                enumEntries += (enumValueNames[val] + " = " + val);
                                if(val + 1 < enumValueNames.Length) {enumEntries += ",";};
                            }
                            newEnum = attribute + " public enum " + column.Name + "Enum" + " { " + enumEntries + " }";
                        }
                        // string newEnum = string.Format(
                        // @"{0} public enum {1} 
                        // {
                        //     {2}
                        // }"
                        // ,attribute,column.Name,enumEntries);
                        fieldText += newEnum;
                    }
                }

                //generate the constructor that sets the fields based on the passed in value
                string constructorText = "";
                constructorText += $"SimpleJSON.JSONNode node = root.GetSheetWithName(\"{sheet.Name}\").Rows[(int)line];\n";
                for (int i = 0; i < typeCount; i++)
                {
                    CastleDB.ColumnNode column = sheet.Columns[i];
                    string castText = CastleDBUtils.GetCastStringFromCastleDBTypeStr(column.TypeStr);
                    string enumCast = "";
                    if(CastleDBUtils.GetTypeNumFromCastleDBTypeString(column.TypeStr) == "10")
                    {
                        enumCast = string.Format("({0}Flag)",column.Name);
                    }
                    else if(CastleDBUtils.GetTypeNumFromCastleDBTypeString(column.TypeStr) == "5")
                    {
                        enumCast = string.Format("({0}Enum)",column.Name);
                    }
                    constructorText += $"{column.Name} = {enumCast}node[\"{column.Name}\"]{castText};\n";
                    // constructorText += (string.Format("{0} = {2}lineNode[\"{0}\"]{1};\n",column.Name,castText,enumCast));
                }

                //need to construct an enum of possible types
                //also need to have some utility functions like Type.CreateAllObjects returns list of all objects
                
                string possibleValuesText = $"public enum {sheet.Name}values {{ \n";
                Debug.Log($"{sheet.Name} numrows: {numRows}");
                for (int i = 0; i < numRows; i++)
                {
                     possibleValuesText += sheet.Rows[i]["id"]; //TODO: need to have an identifying global name
                     if(i+1 < numRows){ possibleValuesText += ", \n";}
                }
                possibleValuesText += "\n }";

                string fullClassText = $"using UnityEngine;\n using System;\n using SimpleJSON;\n using CastleDBImporter;\n public class {sheet.Name} \n {{ \n {fieldText} \n {possibleValuesText} public {sheet.Name} (CastleDB.RootNode root, {sheet.Name}values line) {{ \n {constructorText} \n }}  }}";
                Debug.Log("Generating CDB Class: " + sheet.Name);
                File.WriteAllText(scriptPath, fullClassText);
            }

            var assemblyBuilder = new AssemblyBuilder(outputAssembly, scripts.ToArray());

            // Exclude a reference to the copy of the assembly in the Assets folder, if any.
            assemblyBuilder.excludeReferences = new string[] { assemblyProjectPath };
            assemblyBuilder.additionalReferences = new string [] {"Assets/CastleDBImporter/CompiledTypes/SimpleJSON.dll","Assets/CastleDBImporter/CompiledTypes/CastleDB.dll"}; //TODO: need to have this path set dynamically

            AssetDatabase.DeleteAsset(assemblyProjectPath); //TODO: not working?

            // Called on main thread
            assemblyBuilder.buildStarted += delegate(string assemblyPath)
            {
                Debug.LogFormat("Assembly build started for {0}", assemblyPath);
            };

            // Called on main thread
            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Assembly build finished for {0}", assemblyPath);
                Debug.LogFormat("Warnings: {0} - Errors: {0}", errorCount, warningCount);

                foreach (CompilerMessage message in compilerMessages)
                {
                    if(message.type == CompilerMessageType.Error)
                    {
                        Debug.Log("ERROR: " + message.message);
                    }
                }

                if(errorCount == 0)
                {
                    File.Copy(outputAssembly, assemblyProjectPath, true);
                    AssetDatabase.ImportAsset(assemblyProjectPath);
                }
            };

            // Start build of assembly
            if(!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
                return;
            }

            while(assemblyBuilder.status != AssemblyBuilderStatus.Finished)
            {
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}