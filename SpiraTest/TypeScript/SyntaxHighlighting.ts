//TypeScript React file that supports syntax highlighting

//external dependendencies (js libraries)
declare var SyntaxHighlighter: any;
declare var AspNetAjax$Function: any;
declare var Prism: any;


//global object accessible by the aspx page
var syntaxHighlighting = {} as any;
syntaxHighlighting = {
    highlightElement: function (code: string, extension: string, element, showLineNumbers: boolean) {
        let languageName: string = extension;
        // update the language name for certain extensions
        switch (extension.toLowerCase()) {
            case "bash":
            case "sh":
            case "cfm":
            case "cf":
                languageName = "bash";
                break;
            case "bat":
                languageName = "batch";
                break;
            case "cp":
            case "m":
            case "mm":
                languageName = "cpp";
                break;
            case "pas":
                languageName = "delphi";
                break;
            case "erl":
            case "hrl":
                languageName = "erlang";
                break;
            case "gitattributes":
                languageName = "gitignore";
                break;
            case "feature":
                languageName = "gherkin";
                break;
            case "map":
                languageName = "json";
                break;
            case "pl":
                languageName = "perl";
                break;
            case "ps1":
            case "psm1":
            case "psd1":
            case "pssc":
            case "psrc":
                languageName = "powershell";
                break;
            case "tsx":
                languageName = "ts";
                break;
            case "vbnet":
            case "vbs":
                languageName = "vb";
                break;
            case "sstest":
            case "xhtml":
            case "htm":
            case "xslt":
            case "xsd":
            case "xsl":
            case "asax":
            case "ascx":
            case "ashx":
            case "asp":
            case "aspx":
            case "build":
            case "config":
            case "csproj":
            case "resx":
            case "ps1xml":
            case "cdxml":
                languageName = "xml";
                break;
        }

        // make sure the element is cleared first
        globalFunctions.clearContent(element);

        // create the inner elements for pre and code
        let codeElement = document.createElement('code');
        if (code)
        {
            //The text will have duplicate newlines
            codeElement.textContent = code;
        }
        else
        {
            codeElement.innerText = '';
        }
        globalFunctions.cleanHtml(codeElement);

        let preElement = document.createElement('pre');
        preElement.className = `language-${languageName} ${showLineNumbers ? "line-numbers" : ""}`;
        preElement.setAttribute("style", "white-space: pre-wrap;");
        preElement.appendChild(codeElement);

        // add the elements to the DOM
        element.appendChild(preElement);

        // run syntax highlighter
        Prism.highlightElement(codeElement);
    }
}
