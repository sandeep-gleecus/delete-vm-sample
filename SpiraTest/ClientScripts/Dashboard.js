function Dashboard_exportAsImage()
{
    var element = $('#' + g_prtManager_ajaxWebParts_clientId);

    //We need to make sure that all clip paths have their # symbols properly escaped
    element.find('svg g[clip-path]').attr('clip-path', '');

    //Now convert to Canvas
    html2canvas(element[0]).then(function (canvas) { Dashboard_exportAsImage2(canvas); });
}

function Dashboard_exportAsImage2(canvas)
{
    //Now we need to convert the canvas to an image file
    var url = g_dashboard_post_url;
    var canvasData = Canvas2Image.saveAsPNG(canvas);
    if (url && canvasData)
    {
        var ajax = new XMLHttpRequest();
        ajax.open("POST", url, true);
        ajax.setRequestHeader('Content-Type', 'canvas/upload');
        ajax.onreadystatechange = function ()
        {
            if (ajax.readyState == 4)
            {
                //Call the same URL, but this time, pass the guid for the file
                window.location.href = url + "?guid=" + ajax.responseText;
            }
        }
        ajax.send(canvasData);
    }
    else
    {
        //IE doesn't support
        alert(resx.JqPlot_ImageExportNotSupport);
    }
}