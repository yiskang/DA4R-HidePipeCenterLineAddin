// (C) Copyright 2023 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software
// in object code form for any purpose and without fee is hereby
// granted, provided that the above copyright notice appears in
// all copies and that both that copyright notice and the limited
// warranty and restricted rights notice below appear in all
// supporting documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK,
// INC. DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL
// BE UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is
// subject to restrictions set forth in FAR 52.227-19 (Commercial
// Computer Software - Restricted Rights) and DFAR 252.227-7013(c)
// (1)(ii)(Rights in Technical Data and Computer Software), as
// applicable.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using DesignAutomationFramework;
using Autodesk.Revit.DB;
using Newtonsoft.Json;

namespace HidePipeCenterLineAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        private void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            LogTrace("Design Automation Ready event triggered...");

            e.Succeeded = true;
            e.Succeeded = this.DoTask(e.DesignAutomationData);
        }
        private bool DoTask(DesignAutomationData data)
        {
            if (data == null)
                return false;

            Application app = data.RevitApp;
            if (app == null)
            {
                LogTrace("Error occured");
                LogTrace("Invalid Revit App");
                return false;
            }

            string modelPath = data.FilePath;
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                LogTrace("Error occured");
                LogTrace("Invalid File Path");
                return false;
            }

            var doc = data.RevitDoc;
            if (doc == null)
            {
                LogTrace("Error occured");
                LogTrace("Invalid Revit DB Document");
                return false;
            }

            var inputParams = JsonConvert.DeserializeObject<InputParams>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "params.json")));
            if (inputParams == null)
            {
                LogTrace("Invalid Input Params or Empty JSON Inpu.t");
                return false;
            }

            try
            {
                LogTrace("Collecting views...");

                IEnumerable<ElementId> viewIds = null;

                LogTrace("- From given view Ids...");
                try
                {
                    if (inputParams.ViewIds == null || inputParams.ViewIds.Count() <= 0)
                    {
                        throw new InvalidDataException("Invalid input` viewIds` while the `viewSetName` value is not specified!");
                    }

                    var viewElemIds = new List<ElementId>();
                    foreach (var viewGuid in inputParams.ViewIds)
                    {
                        var view = doc.GetElement(viewGuid) as View;
                        if (view == null || (view.ViewTemplateId.IntegerValue != ElementId.InvalidElementId.IntegerValue))
                        {
                            LogTrace(string.Format("Warning: No view found with gieven unqique id `{0}`", viewGuid));
                            continue;
                        }

                        viewElemIds.Add(view.Id);
                    }

                    viewIds = viewElemIds;
                }
                catch (Exception ex)
                {
                    this.PrintError(ex);
                    return false;
                }

                LogTrace("Starting turning off Pipe centerline...");

                using (var transGroup = new TransactionGroup(doc, "Turn off Pipe centerline visibility"))
                {
                    transGroup.Start();

                    foreach (var viewId in viewIds)
                    {
                        var view = doc.GetElement(viewId) as View;
                        this.TurnOffPipeCenterLine(doc, view);
                    }

                    transGroup.Assimilate();
                }

                LogTrace($"... DONE ...");

                ModelPath path = ModelPathUtils.ConvertUserVisiblePathToModelPath("result.rvt");
                doc.SaveAs(path, new SaveAsOptions());

                LogTrace("Successfully save changes to the RVT file");
            }
            catch (Autodesk.Revit.Exceptions.InvalidPathArgumentException ex)
            {
                this.PrintError(ex);
                return false;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException ex)
            {
                this.PrintError(ex);
                return false;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
                this.PrintError(ex);
                return false;
            }
            catch (Exception ex)
            {
                this.PrintError(ex);
                return false;
            }
            LogTrace("Successfully extract asset locations` to `result.json`...");
            return true;
        }

        private void TurnOffPipeCenterLine(Document doc, View view)
        {
            var pipeCurvesCenterLineCate = Category.GetCategory(doc, BuiltInCategory.OST_PipeCurvesCenterLine);

            if (view.CanCategoryBeHidden(pipeCurvesCenterLineCate.Id)) //!<<< or view3d.CanCategoryBeHidden(pipeCurvesCenterLineCate.Id)
            {
                using (var trans = new Transaction(doc, $"Turn off Pipe centerline visibility for view {view.Name} ({view.UniqueId})"))
                {
                    trans.Start();
                    view.SetCategoryHidden(pipeCurvesCenterLineCate.Id, true);
                    trans.Commit();
                }
            }
            else
            {
                LogTrace($"Warning - Cannot turn off pipe center line in view {view.Name} ({view.UniqueId})");
            }
        }

        private void PrintError(Exception ex)
        {
            LogTrace("Error occured");
            LogTrace(ex.Message);

            if (ex.InnerException != null)
                LogTrace(ex.InnerException.Message);
        }

        /// <summary>
        /// This will appear on the Design Automation output
        /// </summary>
        public static void LogTrace(string format, params object[] args)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine(string.Format(format, args));
#endif
            System.Console.WriteLine(format, args);
        }
    }
}
