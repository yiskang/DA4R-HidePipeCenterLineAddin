# Hide Pipe Centerline from the views by change visibility settings with Design Automation

[![Design-Automation](https://img.shields.io/badge/Design%20Automation-v3-green.svg)](http://developer.autodesk.com/)

![Windows](https://img.shields.io/badge/Plugins-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)
[![Revit-2021](https://img.shields.io/badge/Revit-2021-lightgrey.svg)](http://autodesk.com/revit)

## APS DA Setup

### Activity via [POST activities](https://aps.autodesk.com/en/docs/design-automation/v3/reference/http/activities-POST/)

```json
{
    "commandLine": [
        "$(engine.path)\\\\revitcoreconsole.exe /i \"$(args[inputFile].path)\" /al \"$(appbundles[HidePipeCenterLine].path)\""
    ],
    "parameters": {
        "inputFile": {
            "verb": "get",
            "description": " Input Revit File",
            "required": true,
            "localName": " $(inputFile)"
        },
        "inputJson": {
            "verb": "get",
            "description": " input Json parameters",
            "localName": "params.json"
        },
        "outputRVT": {
            "verb": "put",
            "description": "Output Revit File",
            "localName": "result.rvt"
        }
    },
    "id": "yoursalias.HidePipeCenterLineActivity+dev",
    "engine": "Autodesk.Revit+2021",
    "appbundles": [
        "yoursalias.HidePipeCenterLine+dev"
    ],
    "settings": {},
    "description": "Activity for hiding pipe center lines from views",
    "version": 1
}
```

### Workitem via [POST workitems](https://aps.autodesk.com/en/docs/design-automation/v3/reference/http/workitems-POST/)

```json
{
    "activityId": "yoursalias.HidePipeCenterLineActivity+dev",
    "arguments": {
   "inputFile": {
     "verb": "get",
     "url": "https://developer.api.autodesk.com/oss/v2/signedresources/...?region=US"
   },
   "inputJson": {
     "verb": "get",
     "url": "data:application/json,{ 'viewIds': [ '429ba882-f0a0-40fa-96e6-c6e02d9fc601-00011abc' ] }"
   },
   "outputDwg": {
     "verb": "put",
     "url": "https://developer.api.autodesk.com/oss/v2/signedresources/...?region=US"
   }
 }
}
```

### Tips & Tricks

- **viewIds**: It's a set of Revit view's [UniqueId](https://www.revitapidocs.com/2023/f9a9cb77-6913-6d41-ecf5-4398a24e8ff8.htm) retrieved by Revit API, or the `viewableID` that can be found in the response of APS Model Derivative [GET :urn/manifest](https://aps.autodesk.com/en/docs/model-derivative/v2/reference/http/manifest/urn-manifest-GET/).

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

Eason Kang [@yiskang](https://twitter.com/yiskang), [Developer Advocacy and Support](http://aps.autodesk.com)