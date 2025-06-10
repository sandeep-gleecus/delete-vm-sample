//TypeScript React file that supports SourceCodeRevisionFileDetails.aspx

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var $create: any;
declare var accessReact: any;
declare var $get: any;
declare var AspNetAjax$Function: any;
declare var Mustache: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var resx: any;
declare var syntaxHighlighting: any; // the client side code that uses the SyntaxHighlighter library to create the highlighted output

//data from the page
declare var mimeType: string;
declare var filename: string;
declare var revisionName: string;
declare var previousRevisionName: string;

//url templates
declare var sourceCodeViewer_urlTemplate: string;

//global object accessible by the aspx page
var sourceCodeRevisionFileAction = {
    messageBoxId: '' as string,
    branchKey: '' as string,
    fileKey: '' as string,
    revisionKey: '' as string,
    previousRevisionKey: '' as string,
    isUnified: false as boolean,

    updatePreview: function (branchKey, fileKey, revisionKey, previousRevisionKey, isUnified): void {
        // set the values into the object so they are available to success functions
        this.branchKey = branchKey;
        this.fileKey = fileKey;
        this.revisionKey = revisionKey;
        this.previousRevisionKey = previousRevisionKey;
        this.isUnified = isUnified;

        //See if we can display a preview
        var previewAvailable: boolean = false;

        //See if we have a known code or image type
        if (mimeType == 'text/markdown')
        {
            //Load the preview
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCodeFile_OpenMarkdown(SpiraContext.ProjectId, branchKey, fileKey, revisionKey, AspNetAjax$Function.createDelegate(this, this.updatePreviewMarkdown_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), 'markdownPreview');
            $('#markdownPreview').show();
            previewAvailable = true;
        }
        else
        {
            $('#markdownPreview').hide();
        }

        if (mimeType != 'text/markdown' && mimeType.substr(0, 'text'.length) == 'text' || mimeType == 'application/x-rapise' || mimeType == 'application/json' || mimeType == 'application/xml' || mimeType == 'application/x-bat') {
            //Load the preview
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCodeFile_OpenText(SpiraContext.ProjectId, branchKey, fileKey, revisionKey, AspNetAjax$Function.createDelegate(this, this.updatePreview_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), 'codePreview');
            previewAvailable = true;
            $('#codePreview').show();
        }
        else {
            $('#codePreview').hide();
        }

        if (mimeType.substr(0, 'image'.length) == 'image') {
            let url: string = sourceCodeViewer_urlTemplate.replace('{0}', fileKey).replace('{1}', revisionKey).replace('{2}', branchKey);
            previewAvailable = true;
            $('#imagePreview').show();
            $('#imgPreviewHyperLink').attr('href', url);
            $('#imgPreview').attr('src', url);
        }
        else {
            $('#imagePreview').hide();
        }

        if (previewAvailable) {
            //Hide no preview
            $('#noPreview').hide();
        }
        else {
            //Display no preview
            $('#noPreview').show();
            $('#codePreview').hide();
            $('#imagePreview').hide();
            $('#markdownPreview').hide();
        }

        //Update the tab
        $find(tabControl_id).updateHasData('tabPreview', previewAvailable);

        //See if we have the previous revision
        if (previousRevisionKey && previousRevisionKey != '')
        {
            //See if we have a known code or image type
            if (mimeType == 'text/markdown') {
                //Load the preview
                Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCodeFile_OpenMarkdown(SpiraContext.ProjectId, branchKey, fileKey, previousRevisionKey, AspNetAjax$Function.createDelegate(this, this.updatePreviewMarkdown_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), 'markdownPreviewPrevious');
                $('#markdownPreviewPrevious').show();
                previewAvailable = true;
            }
            else {
                $('#markdownPreviewPrevious').hide();
            }

            if (mimeType != 'text/markdown' && mimeType.substr(0, 'text'.length) == 'text' || mimeType == 'application/x-rapise' || mimeType == 'application/json' || mimeType == 'application/xml' || mimeType == 'application/x-bat') {
                //Load the preview
                Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCodeFile_OpenText(SpiraContext.ProjectId, branchKey, fileKey, previousRevisionKey, AspNetAjax$Function.createDelegate(this, this.updatePreview_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), 'codePreviewPrevious');
                previewAvailable = true;
                $('#codePreviewPrevious').show();
            }
            else {
                $('#codePreviewPrevious').hide();
            }

            if (mimeType.substr(0, 'image'.length) == 'image') {
                let url: string = sourceCodeViewer_urlTemplate.replace('{0}', fileKey).replace('{1}', previousRevisionKey).replace('{2}', branchKey);
                previewAvailable = true;
                $('#imagePreviewPrevious').show();
                $('#imgPreviewPreviousHyperLink').attr('href', url);
                $('#imgPreviewPrevious').attr('src', url);
            }
            else {
                $('#imagePreviewPrevious').hide();
            }

            if (previewAvailable) {
                //Hide no preview
                $('#noPreviewPrevious').hide();
            }
            else {
                //Display no preview
                $('#noPreviewPrevious').show();
                $('#codePreviewPrevious').hide();
                $('#imagePreviewPrevious').hide();
                $('#markdownPreviewPrevious').hide();
            }

            //Update the tab
            $find(tabControl_id).updateHasData('tabPreviewPrevious', previewAvailable);

            //Update the diff view
            this.updateChangesView(branchKey, fileKey, revisionKey, previousRevisionKey, isUnified);
        }
    },

    updatePreview_success: function (preview: string, elementId: string) {

        //run syntax highlighting
        let codePreview = $get(elementId);
        let extension: string = filename.split('.').pop();

        //Need to add a slight delay because we have two previews to load
        if (elementId == 'codePreviewPrevious') {
            setTimeout(function () { syntaxHighlighting.highlightElement(preview, extension, codePreview, true) }, 1000);
        }
        else
        {
            syntaxHighlighting.highlightElement(preview, extension, codePreview, true)
        }
    },
    updatePreview_failure: function (ex: any) {
        //Display no preview
        $('#noPreview').show();
        $('#codePreview').hide();
        $('#imagePreview').hide();
        $('#markdownPreview').hide();
    },

    updatePreviewMarkdown_success: function (preview: string, elementId: string) {
        let markdownPreview = $get(elementId);
        globalFunctions.clearContent(markdownPreview);
        markdownPreview.innerHTML = preview;
        globalFunctions.cleanHtml(markdownPreview);
    },


    updateChangesView: function (branchKey, fileKey, revisionKey, previousRevisionKey, isUnified) {
        //update the isUnified boolean as this can change separately from the other params
        this.isUnified = isUnified;

        //Now we need to get the changes as well if this is text
        if (mimeType.substr(0, 'text'.length) == 'text' || mimeType == 'application/x-rapise' || mimeType == 'application/json' || mimeType == 'application/xml' || mimeType == 'application/x-bat') {
            //Load the preview
            if (isUnified) {
                Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCodeFile_OpenUnifiedTextDiff(
                    SpiraContext.ProjectId,
                    branchKey,
                    fileKey,
                    revisionKey,
                    previousRevisionKey,
                    AspNetAjax$Function.createDelegate(this, this.updateUnifiedChanges_success),
                    AspNetAjax$Function.createDelegate(this, this.updateChanges_failure)
                );
            } else {
                Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCodeFile_OpenSideBySideTextDiff(
                    SpiraContext.ProjectId,
                    branchKey,
                    fileKey,
                    revisionKey,
                    previousRevisionKey,
                    AspNetAjax$Function.createDelegate(this, this.updateSideBySideChanges_success),
                    AspNetAjax$Function.createDelegate(this, this.updateChanges_failure)
                );
            }
        }
        //Or make clear that there are no differences to show
        else {
            $('#no-difference').show();
            $('#target-changes').hide();
            $find(tabControl_id).updateHasData('tabChanges', false);
        }
    },

    updateUnifiedChanges_success: function (diffModel: any) {
        var template = document.getElementById('template-unified-changes').innerHTML;
        var totalInserts = 0,
            totalDeletes = 0,
            totalChanges = 0,
            totalLineEdits = 0,
            percentInserts = 0,
            percentDeletes = 0,
            percentChanges = 0,
            lines = diffModel.lines,
            COLLAPSE_BUFFER = 3,
            TYPE_UNCHANGED = "Unchanged",
            isCollapseLines = diffModel.hasDifferences > 0 && diffModel.lines.length > 32; // 32 = an aribtrary line length above which we should collapse

        if (diffModel && diffModel.lines.length) {
            totalInserts += diffModel.lines.filter(function (line) { return line.type == "Inserted"; }).length;
            totalDeletes += diffModel.lines.filter(function (line) { return line.type == "Deleted"; }).length;
            totalChanges += diffModel.lines.filter(function (line) { return line.type == "Modified"; }).length;
            totalLineEdits = totalInserts + totalDeletes + totalChanges;
            percentInserts = Math.round((totalInserts / totalLineEdits) * 100);
            percentDeletes = Math.round((totalDeletes / totalLineEdits) * 100);
            percentChanges = Math.round((totalChanges / totalLineEdits) * 100);

            lines = this.setCollapsedLines(isCollapseLines, lines, TYPE_UNCHANGED, COLLAPSE_BUFFER);
        }

        if (totalLineEdits > 0) {
            var view = {
                hasDifferences: diffModel.hasDifferences,
                isCollapseLines: isCollapseLines,
                lines: lines,
                changesTooltip: (totalInserts + " " + resx.SourceCode_Additions + ", " + totalDeletes + " " + resx.SourceCode_Deletions + ", " + totalChanges + " " + resx.SourceCode_Changes).toLowerCase(),
                totalLineEdits: totalLineEdits,
                percentInserts: percentInserts,
                percentDeletes: percentDeletes,
                percentChanges: percentChanges,
                oldRevisionName: resx.Pagination_Previous + " (" + previousRevisionName + ")",
                newRevisionName: resx.Global_Current + " (" + revisionName + ")",
                unifiedTitle: resx.SourceCode_Diff_UnifiedView,
                splitTitle: resx.SourceCode_Diff_SplitView,
                expandTitle: resx.SourceCode_Diff_ExpandTooltip,
                collapseTitle: resx.SourceCode_Diff_CollapseTooltip
            };
            var rendered = Mustache.render(template, view);
            document.getElementById('target-changes').innerHTML = rendered;
            $('#target-changes').show();
            $find(tabControl_id).updateHasData('tabChanges', true);
        }
        else
        {
            //Display no changes
            $('#no-difference').show();
            $('#target-changes').hide();
            $find(tabControl_id).updateHasData('tabChanges', false);
        }
        // event handler for the change diff view button
        var btnDiffBoxToUnified = document.getElementById("btn-diffBox-to-split");
        if (btnDiffBoxToUnified) {
            btnDiffBoxToUnified.addEventListener("click", () => this.updateChangesView(this.branchKey, this.fileKey, this.revisionKey, this.previousRevisionKey, false));
        }
    },

    updateSideBySideChanges_success: function (diffModel: any) {
        var template = document.getElementById('template-sideBySide-changes').innerHTML;
        var combinedLines = null,
            totalInserts = 0,
            totalDeletes = 0,
            totalChanges = 0,
            totalLineEdits = 0,
            percentInserts = 0,
            percentDeletes = 0,
            percentChanges = 0,
            lines = diffModel.oldText.lines,
            COLLAPSE_BUFFER = 3,
            TYPE_UNCHANGED = "Unchanged",
            isCollapseLines = diffModel.oldText.hasDifferences > 0 && diffModel.oldText.lines.length > 32; // 32 = an aribtrary line length above which we should collapse

        if (diffModel.oldText.lines.length) {
            combinedLines = diffModel.oldText.lines.map(function (line, index) {
                var newLine = {
                    type: line.type,
                    oldPosition: line.position,
                    oldType: line.type,
                    oldText: line.text,
                    oldSubPieces: line.subPieces,
                    newPosition: diffModel.newText.lines[index].position,
                    newType: diffModel.newText.lines[index].type,
                    newText: !line.isSummary ? diffModel.newText.lines[index].text : line.text,
                    newSubPieces: diffModel.newText.lines[index].subPieces
                }
                return newLine;
            });
            var lines = this.setCollapsedLines(isCollapseLines, combinedLines, TYPE_UNCHANGED, COLLAPSE_BUFFER, true);

            totalInserts = diffModel.oldText.lines.filter(function (line) { return line.type == "Inserted"; }).length + diffModel.newText.lines.filter(function (line) { return line.type == "Inserted"; }).length;
            totalDeletes = diffModel.oldText.lines.filter(function (line) { return line.type == "Deleted"; }).length + diffModel.newText.lines.filter(function (line) { return line.type == "Deleted"; }).length;
            totalChanges = diffModel.oldText.lines.filter(function (line) { return line.type == "Modified"; }).length + diffModel.newText.lines.filter(function (line) { return line.type == "Modified"; }).length;
            totalLineEdits = totalInserts + totalDeletes + totalChanges;
            percentInserts = Math.round((totalInserts / totalLineEdits) * 100);
            percentDeletes = Math.round((totalDeletes / totalLineEdits) * 100);
            percentChanges = Math.round((totalChanges / totalLineEdits) * 100);
        }

        if (totalLineEdits > 0) {
            var view = {
                hasDifferences: diffModel.oldText.hasDifferences,
                isCollapseLines: isCollapseLines,
                lines: lines,
                changesTooltip: (totalInserts + " " + resx.SourceCode_Additions + ", " + totalDeletes + " " + resx.SourceCode_Deletions + ", " + totalChanges + " " + resx.SourceCode_Changes).toLowerCase(),
                totalLineEdits: totalLineEdits,
                percentInserts: percentInserts,
                percentDeletes: percentDeletes,
                percentChanges: percentChanges,
                oldRevisionName: resx.Pagination_Previous + " (" + previousRevisionName + ")",
                newRevisionName: resx.Global_Current + " (" + revisionName + ")",
                unifiedTitle: resx.SourceCode_Diff_UnifiedView,
                splitTitle: resx.SourceCode_Diff_SplitView,
                expandTitle: resx.SourceCode_Diff_ExpandTooltip,
                collapseTitle: resx.SourceCode_Diff_CollapseTooltip
            };
            var rendered = Mustache.render(template, view);
            document.getElementById('target-changes').innerHTML = rendered;
            $('#target-changes').show();
            $find(tabControl_id).updateHasData('tabChanges', true);
        }
        else {
            //Display no changes
            $('#no-difference').show();
            $('#target-changes').hide();
            $find(tabControl_id).updateHasData('tabChanges', false);
        }

        // event handler for the change diff view button
        var btnDiffBoxToUnified = document.getElementById("btn-diffBox-to-unified");
        if (btnDiffBoxToUnified) {
            btnDiffBoxToUnified.addEventListener("click", () => this.updateChangesView(this.branchKey, this.fileKey, this.revisionKey, this.previousRevisionKey, true));
        }
    },


    setCollapsedLines: function (isCollapseLines: boolean, lines: any, TYPE_UNCHANGED: string, COLLAPSE_BUFFER: number, isSideBySide: boolean) {
        // for files that are more than a screen long and have changes to them, iterate over lines array to allow UI to only show lines that have changed (with a buffer either side)
        if (!isCollapseLines) {
            return lines;
        } else {
            var linesCollapsed = [],
                summaryLineIndex = 0,
                showLineCount = 0;
            for (var i = 0; i < lines.length; i++) {
                const line = lines[i];

                //always show lines that are changed
                if (line.type != TYPE_UNCHANGED) {
                    line.show = true;
                }

                //otherwise assess lines backward to see if line should be in the buffer zone
                if (!line.show) {
                    for (var backward = 1; backward <= COLLAPSE_BUFFER && i - backward >= 0; backward++) {
                        if (lines[i - backward].type != TYPE_UNCHANGED) {
                            line.show = true;
                            if (backward === COLLAPSE_BUFFER) {
                                line.bufferEnd = true;
                            }
                            break;
                        }
                    }
                }

                //otherwise assess lines forward to see if line should be in the buffer zone
                if (!line.show) {
                    for (var forward = 1; forward <= COLLAPSE_BUFFER && i + forward < lines.length; forward++) {
                        if (lines[i + forward].type != TYPE_UNCHANGED) {
                            line.show = true;
                            if (forward === COLLAPSE_BUFFER) {
                                line.bufferStart = true;
                            }
                            break;
                        }
                    }
                }

                //finally, add summary line above the visible section that summarizes lines being shown
                if (line.show) {
                    showLineCount++;

                    if (line.bufferStart && i > 0 && !lines[i - 1].bufferEnd) {
                        var summaryLine = {
                            isSummary: true,
                            show: true,
                            type: "collapse-summary",
                            oldType: isSideBySide ? "collapse-summary" : null,
                            newType: isSideBySide ? "collapse-summary" : null,
                            text: "@@ +" + (isSideBySide ? line.oldPosition : line.position),
                            oldText: null,
                            newText: null
                        };
                        linesCollapsed.push(summaryLine);
                        summaryLineIndex = linesCollapsed.length - 1;
                        linesCollapsed.push(line);
                    } else {
                        linesCollapsed.push(line);
                    }
                } else {
                    //We are out of any lines to show so go back and edit the summary line.
                    if (summaryLineIndex) {
                        var finalText = linesCollapsed[summaryLineIndex].text += "," + showLineCount + " @@"
                        linesCollapsed[summaryLineIndex].text = finalText;
                        linesCollapsed[summaryLineIndex].oldText = finalText;
                        linesCollapsed[summaryLineIndex].newText = finalText;
                        summaryLineIndex = 0;
                    }
                    showLineCount = 0;
                    linesCollapsed.push(line);
                }

            }
            return linesCollapsed;
        }
    },

    updateChanges_failure: function (ex: any) {
        //Display no changes
        $('#noChanges').show();
        $('#target-changes').hide();
        $find(tabControl_id).updateHasData('tabChanges', false);
    }
};
