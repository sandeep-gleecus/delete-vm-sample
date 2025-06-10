# Instructions
To store static data for installation into Database:

- Create a CSV file in the form of PREFIX-{SQL_TABLE_NAME}.csv									
- Put the column names of any fields you want updated in the top row followed by its type in the format name:type{:pk}										
	
    * "Normal" is the default type, use this for all numbers									
	* "String" is for VARCHAR, unless you choose to manually insert single quotes around the data in that case use "Normal"									
	* "Date" is for SQL Server Dates (must be stored as MM/DD/YYYY)	
	* "Default" is used if you have a default number value you want used for a given row.  In this case, the third row of the worksheet									
	  (which is also the first record) holds the default value									
	* "DefaultString" is the same as "Default", but for strings									
	* "DefaultDate" is the same as "Default", but for dates									
	* if the field is a primary key, you must put a ":pk" at the end of the datatype, this allows the script to handle updates if inserts fail									

- Insert your data in rows 2 and beyond, leave no blank rows.


## VBA code used to convert Excel sheets to csv files
``` VBA

Sub copyToCSV()
    ' before starting: remove any directions sheet at the start

    ' First, get the active sheet index 
    ' if we do this later it will always be 1
    ' we use this to create a file prefix that matches the order in the spreadsheet to make sure data is added to SQL in the correct order
    sheetIndex = ActiveSheet.Index

    ' we create a prefix of the form 0000100 - we want it to always be 7 digits so check the length in characters of the index to help make sure we keep it consistent 
    sheetIndexLen = Len(sheetIndex)
    indexPrefix = ""
    If sheetIndexLen = 1 Then
        indexPrefix = "0000"
    ElseIf sheetIndexLen = 2 Then
        indexPrefix = "000"
    Else
        indexPrefix = "00"
    End If
    
    ' now set the prefix string
    Prefix = indexPrefix & sheetIndex & "00" & "-"


    ' copy the current sheet - this copies the entire sheet to a new workbook
    ActiveSheet.Copy

    ' get the name of the table - always in A1 (put it to lowercase), then delete the first row    
    sqlTable = LCase(Range("A1").Value)
    Rows("1:1").Select
    Selection.Delete Shift:=xlUp

    ' save the worksheet as a CSV with the correct filename
    ActiveWorkbook.SaveAs Filename:= _
        "C:\git\SpiraTeam\Database\Data\" & Prefix & sqlTable & ".csv", FileFormat:=xlCSV, _
        CreateBackup:=False

    ' close the CSV window, which reverts us back to the original XLS file
    ' then move to the next sheet - this will error out on the last sheet, but there seemed little need to code nicely around that as it has no effect on the outputted CSV
    ActiveWindow.Close
    ActiveSheet.Next.Activate
End Sub

```