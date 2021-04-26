XmlEditorGetBoundingClientRect = (element, param) => {
    if (element) {
        return element.getBoundingClientRect();
    } else {
        return null;
    }
};


window.browserResize = {
    registerResizeCallback: function () {
        window.addEventListener("resize", browserResize.resized);
    },
    resized: function () {
        DotNet.invokeMethodAsync("de.springwald.xml.blazor", 'OnBrowserResize').then(data => data);
    }
}

function XmlEditorFocusElement(element) {
    if (element instanceof HTMLElement) {
        element.focus();
    }
}
