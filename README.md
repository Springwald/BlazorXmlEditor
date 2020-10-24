# Blazor xml editor

A tag-view-style xml editor for dotnet blazor webassembly

> This is a port of the xml-editor for dotnet WinForms, which was created 2006 for the Windows version of the [GaitoBotEditor](https://www.gaitobot.de).
> The project is currently under revision to remove old issues from 2006. Therefore it still contains German source code and outdated coding paradigms. 

Project is developed and maintained by [Daniel Springwald](https://blog.springwald.de)

## Demo

[live demo](https://www.springwald.de/demos/BlazorXmlEditor/)

## Requirements

The following libraries are required:

- Blazor Webassembly
- Bootstrap 4
- Font Awesome by Dave Gandy - http://fontawesome.io
- CurrieTechnologies.Razor.Clipboard - https://github.com/Basaingeal/Razor.Clipboard
- Blazor Extensions Canvas - https://github.com/BlazorExtensions/Canvas

## Documentation

### Installation

In `Program.cs` add [CurrieTechnologies.Razor.Clipboard service](https://github.com/Basaingeal/Razor.Clipboard):

```csharp
builder.Services.AddClipboard();
```

In `index.html` head add

```html
<link href="_content/de.springwald.xml.blazor/springwaldXmlEditBlazor.css" rel="stylesheet" />
```

In `index.html` body add [Blazor Extensions Canvas](https://github.com/BlazorExtensions/Canvas), [CurrieTechnologies.Razor.Clipboard service](https://github.com/Basaingeal/Razor.Clipboard) and the blazor xml editor JS:

```html
<script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
<script src="_content/CurrieTechnologies.Razor.Clipboard/clipboard.min.js"></script>
<script src="_content/de.springwald.xml.blazor/springwaldXmlEditBlazor.js"></script>

```

### Use the standard layout

### Create an own layout

```html
<div class="row">
    <div class="col-8">
        <ActionsToolbar EditorContext="this.editorContext"/>
        <XmlEditor EditorContext="this.editorContext" OnReady="this.EditorIsReady" />
    </div>
    <div class="col-4">
        <h5>insert element</h5>
        <AddElement EditorContext="this.editorContext" />
        <hr />
        <h5>edit attributes</h5>
        <EditAttributes EditorContext="this.editorContext"/>
    </div>
</div>
```

## Third party material

This toolbox also contains modules from other authors - in unchanged or revised form.

For details see [the license info](LICENSE.md).
