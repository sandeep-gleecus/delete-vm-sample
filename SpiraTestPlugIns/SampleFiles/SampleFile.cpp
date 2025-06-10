// =============================================================================================
// SpiraTest Plug-In Main Class - Copyright (c) 2006-2008 Inflectra Corporation
//
// This file is part of the SpiraTest TestComplete Plug-In
//
// =============================================================================================

#include "stdafx.h"
#include "resource.h"
#include "SpiraTestCompletePlugIn_i.h"
#include "dllmain.h"
#include "PlugInRegistrator.h"
#include "SpiraTestCompletePlugIn.h"

// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    return _AtlModule.DllCanUnloadNow();
}


// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}


// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _AtlModule.DllRegisterServer();
	return hr;
}


// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	return hr;
}

// DllInstall - Adds/Removes entries to the system registry per user
//              per machine.	
STDAPI DllInstall(BOOL bInstall, LPCWSTR pszCmdLine)
{
    HRESULT hr = E_FAIL;
    static const wchar_t szUserSwitch[] = _T("user");

    if (pszCmdLine != NULL)
    {
    	if (_wcsnicmp(pszCmdLine, szUserSwitch, _countof(szUserSwitch)) == 0)
    	{
    		AtlSetPerUserRegistration(true);
    	}
    }

    if (bInstall)
    {	
    	hr = DllRegisterServer();
    	if (FAILED(hr))
    	{	
    		DllUnregisterServer();
    	}
    }
    else
    {
    	hr = DllUnregisterServer();
    }

    return hr;
}

void STDMETHODCALLTYPE CSpiraTestPlugIn::GetKey(GUID * Value)
{
    _ASSERT( Value != NULL );
    *Value = GetPluginKey();
}
  
CONST GUID CSpiraTestPlugIn::GetPluginKey()
{
    CONST GUID key = {0x8602b65d, 0x3182, 0x4ee9, {0xb4, 0xf8, 0xeb, 0xfa, 0x68, 0x50, 0x8d, 0xdd}};
    return key;
}

HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::Initialize(IaqBaseManager * Manager)
{
    HRESULT hr;

	//Get a local reference to the base Manager object
	m_base_manager = Manager;
  
	//Get the plug-in wide configuration information from the .config file
	//Instantiate the XML document object
	hr = ::CoCreateInstance(__uuidof(DOMDocument), NULL, CLSCTX_ALL, __uuidof(IXMLDOMDocument), (void**)&m_pXmlDocument);
	if (FAILED(hr))
	{
		DisplayMessage(CComBSTR(L"SpiraTest Plug-In Initialization Failure: Unable to get XML DOM Object"));
		return hr;
	}

	//Need to append the filename onto the product data folder path
	wchar_t cConfigFilePath[1000];
	VARIANT_BOOL bSuccess;
	IaqProductInfo *pProductInfo;
	m_base_manager->get_ProductInfo(&pProductInfo);
	BSTR strProductDataFolder;
	pProductInfo->get_ProductDataFolder(&strProductDataFolder);
	wcscpy_s(cConfigFilePath, 1000, OLE2W(strProductDataFolder));
	wcscat_s(cConfigFilePath, 1000, L"\\TestCompleteMappings.config");

	//Now load the XML document into the DOM object
	hr = m_pXmlDocument->load(CComVariant(cConfigFilePath), &bSuccess);
	if (FAILED(hr) || !bSuccess)
	{
		DisplayMessage(CComBSTR(L"SpiraTest Plug-In Initialization Failure: Unable to load config file"));
		DisplayMessage(CComBSTR(cConfigFilePath));
		return hr;
	}

	//Get the URL, login and password used by the web service
	//URL
	IXMLDOMNodePtr pXmlUrlNode;
	hr = m_pXmlDocument->selectSingleNode(CComBSTR(L"/configuration/url"), &pXmlUrlNode);
	if (FAILED(hr) || pXmlUrlNode == NULL)
	{
		DisplayMessage(CComBSTR(L"SpiraTest Plug-In Initialization Failure: Unable to read URL value"));
		return E_FAIL;
	}
	BSTR strUrl;
	hr = pXmlUrlNode->get_text(&strUrl);
	if (FAILED(hr)) return hr;
	
	//Now add the standard part of the URL
	wchar_t cFullUrl[255];
	wcscpy_s(cFullUrl, 255, OLE2W(strUrl));
	wcscat_s(cFullUrl, 255, L"/Services/TestExecute.asmx");
	m_spiraTestExecuteService.SetUrl(cFullUrl);

	//Login
	IXMLDOMNodePtr pXmlUsernameNode;
	hr = m_pXmlDocument->selectSingleNode(CComBSTR(L"/configuration/username"), &pXmlUsernameNode);
	if (FAILED(hr) || pXmlUrlNode == NULL)
	{
		DisplayMessage(CComBSTR(L"SpiraTest Plug-In Initialization Failure: Unable to read username value"));
		return E_FAIL;
	}
	hr = pXmlUsernameNode->get_text(&m_strLogin);
	if (FAILED(hr)) return hr;

	//Password
	IXMLDOMNodePtr pXmlPasswordNode;
	hr = m_pXmlDocument->selectSingleNode(CComBSTR(L"/configuration/password"), &pXmlPasswordNode);
	if (FAILED(hr) || pXmlUrlNode == NULL)
	{
		DisplayMessage(CComBSTR(L"SpiraTest Plug-In Initialization Failure: Unable to read password value"));
		return E_FAIL;
	}
	hr = pXmlPasswordNode->get_text(&m_strPassword);
	if (FAILED(hr)) return hr;

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::Update()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::Finalize()
{
	//Clean Up
	m_pXmlDocument.Release();
	m_spiraTestExecuteService.Release();
	m_pXmlDocument = NULL;
    m_base_manager = NULL;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::BaseManagerStateChanged(AQ_MAINMANAGER_STATE /*State*/)
{
	return S_OK;
}

EXTERN_C HRESULT STDAPICALLTYPE GetPluginRegistrator(IaqExtensionRegistrator** lpRetVal)
{
    CComObject<CPlugInRegistrator>* p;
    HRESULT hr = CComObject<CPlugInRegistrator>::CreateInstance(&p);
    if (FAILED(hr)) return hr;
    
    return p->QueryInterface(lpRetVal);
}

// IaqActionNotifier Methods

AQ_ACTION_EXEC_RESULT STDMETHODCALLTYPE CSpiraTestPlugIn::OnExecute(IaqActionItem * Sender, VARIANT /*InValue*/, VARIANT * /*OutValue*/)
{
	GUID key;
	Sender->GetKey(&key);

	return AQ_AER_OK;
}

void STDMETHODCALLTYPE CSpiraTestPlugIn::OnUpdate(IaqActionItem * Sender)
{
	GUID key;
	Sender->GetKey(&key);

}

// IaqBaseEvent Methods

HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::get_EventInfo(BSTR * Value)
{
	return S_OK;
}


// IaqEventsListenerProvider Methods

long STDMETHODCALLTYPE CSpiraTestPlugIn::GetCountListenedEvent()
{
	return 1;
}

void STDMETHODCALLTYPE CSpiraTestPlugIn::GetListenedEventInfo(long Index, AQ_EVENT_INFO * Value)
{
	_ASSERT( Value != NULL );
	_ASSERT( Index == 0 );
	if (Index != 0) return;

	Value->EventID            = IID_ItcTestEngineEvent;
	Value->NotifyInMainThread = VARIANT_FALSE;
}


// ItcTestEngineEvent Methods

//This method is called for events that concern individual tests.
//The Event parameter specifies the type of event that occurred (see below).
//The Test parameter holds a reference to the test object (ItcTest).
HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::OnTestEvent(TC_TESTENGINE_EVENT Event, ItcTest *Test)
{
	switch (Event)
	{
		case TC_PLAYING_START_FAILED:
		{
			//Test failure
			RecordTestResult(ExecutionStatus_Failed, Test);
			break;
		}
		case TC_PLAYING_BEFORE_FINALIZE:
		{
			//Test finished/success
			RecordTestResult(ExecutionStatus_Passed, Test);
			break;
		}
	}

	return S_OK;
}

HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::RecordTestResult(int iExecutionStatus, ItcTest *Test)
{
	HRESULT hr;

	//Get the test name and moniker
	BSTR strTestName;
	Test->GetName(&strTestName);

	//See if this test has an entry in the configuration file
	wchar_t cTestQuery[255];
	wcscpy_s(cTestQuery, 255, L"/configuration/projects/project/tests/test[@name='");
	wcscat_s(cTestQuery, 255, OLE2W(strTestName));
	wcscat_s(cTestQuery, 255, L"']");

	IXMLDOMNodePtr pXmlTestNode;
	hr = m_pXmlDocument->selectSingleNode(CComBSTR(cTestQuery), &pXmlTestNode);
	if (FAILED(hr) || pXmlTestNode == NULL)
	{
		//Fail quietly - since we just don't need to record a result for this test
		//Comment-out for production code
		//DisplayMessage(CComBSTR(L"Warning: One test case was executed that doesn't have a mapping entry"));
		//DisplayMessage(sTestResult.strTestName);
		return S_OK;
	}

	//Now we need to see how many child test items (<item>) exist under this test node (<test>)
	IXMLDOMNodeListPtr pXmlTestItemNodeList;
	hr = pXmlTestNode->get_childNodes(&pXmlTestItemNodeList);
	if (FAILED(hr))
	{
		DisplayMessage(CComBSTR(L"Unable to get child test items for test entry in the mapping file"));
		return hr;
	}

	//Now we need to iterate through the child items
	long lTestItemCount;
	pXmlTestItemNodeList->get_length(&lTestItemCount);
	for (long j = 0; j < lTestItemCount; j++)
	{
		//Get the test item node
		IXMLDOMNodePtr pXmlTestItemNode;
		hr = pXmlTestItemNodeList->get_item(j, &pXmlTestItemNode);
		if (FAILED(hr))
		{
			DisplayMessage(CComBSTR(L"Unable to access a child test item node for test entry in the mapping file"));
			return hr;
		}

		//Record an individual result
		STestResult sTestResult;
		sTestResult.iExecutionStatus = ExecutionStatus_Passed;
		Test->GetMoniker(&sTestResult.strMoniker);
		Test->GetName(&sTestResult.strTestName);

		//Now get the TestComplete test item name and the corresponding SpiraTest test case id

		//First the SpiraTest test case id since it's just the node's text value
		BSTR strTestCaseId;
		hr = pXmlTestItemNode->get_text(&strTestCaseId);
		if (FAILED(hr))
		{
			DisplayMessage(CComBSTR(L"Unable to get test case id for a mapped test case"));
			return hr;
		}
		//Convert to long
		long lTestCaseId = _wtol(OLE2W(strTestCaseId));
		if (lTestCaseId == 0L)
		{
			DisplayMessage(CComBSTR(L"The mapped test case id needs to be numeric"));
			return E_FAIL;
		}
		sTestResult.lTestCaseId = lTestCaseId;

		//Now get the TestComplete project item name, which is the 'name' attribute of the <item> node
		IXMLDOMNamedNodeMapPtr pXmlTestItemNodeAttributeMap;
		hr = pXmlTestItemNode->get_attributes(&pXmlTestItemNodeAttributeMap);
		if (FAILED(hr) || pXmlTestItemNodeAttributeMap == NULL)
		{
			DisplayMessage(CComBSTR(L"Cannot access the test item attributes for this mapped test"));
			return E_FAIL;
		}

		//Get the 'name' attribute of <item>
		IXMLDOMNodePtr pXmlTestItemNameAttribute;
		hr = pXmlTestItemNodeAttributeMap->getNamedItem(CComBSTR(L"name"), &pXmlTestItemNameAttribute);
		if (FAILED(hr) || pXmlTestItemNameAttribute == NULL)
		{
			DisplayMessage(CComBSTR(L"Cannot access the name attribute for this mapped test item"));
			return E_FAIL;
		}
		//Now get the name value
		hr = pXmlTestItemNameAttribute->get_text(&sTestResult.strTestItemName);
		if (FAILED(hr))
		{
			DisplayMessage(CComBSTR(L"Unable to get name value for a mapped test item"));
			return hr;
		}

		//Now get the project node to extract the project id and release id
		IXMLDOMNodePtr pXmlProjectNode;
		hr = pXmlTestNode->selectSingleNode(CComBSTR(L"../.."), &pXmlProjectNode);
		if (FAILED(hr) || pXmlProjectNode == NULL)
		{
			DisplayMessage(CComBSTR(L"Cannot access the project configuration node for this mapped test"));
			return E_FAIL;
		}
		//Now get the attributes
		IXMLDOMNamedNodeMapPtr pXmlProjectNodeAttributeMap;
		hr = pXmlProjectNode->get_attributes(&pXmlProjectNodeAttributeMap);
		if (FAILED(hr) || pXmlProjectNodeAttributeMap == NULL)
		{
			DisplayMessage(CComBSTR(L"Cannot access the project configuration node attributes for this mapped test"));
			return E_FAIL;
		}

		//Get the project id attribute
		IXMLDOMNodePtr pXmlProjectIdAttribute;
		hr = pXmlProjectNodeAttributeMap->getNamedItem(CComBSTR(L"id"), &pXmlProjectIdAttribute);
		if (FAILED(hr) || pXmlProjectIdAttribute == NULL)
		{
			DisplayMessage(CComBSTR(L"Cannot access the project id configuration attribute for this mapped test"));
			return E_FAIL;
		}
		//Now get the project id value
		BSTR strProjectId;
		hr = pXmlProjectIdAttribute->get_text(&strProjectId);
		if (FAILED(hr))
		{
			DisplayMessage(CComBSTR(L"Unable to get project id for a mapped test case"));
			return hr;
		}
		//Convert to long
		long lProjectId = _wtol(OLE2W(strProjectId));
		if (lProjectId == 0L)
		{
			DisplayMessage(CComBSTR(L"The mapped project id needs to be numeric"));
			return E_FAIL;
		}
		sTestResult.lProjectId = lProjectId;

		//Get the release id attribute (optional so fail quietly and just set releaseId to -1)
		IXMLDOMNodePtr pXmlReleaseIdAttribute;
		hr = pXmlProjectNodeAttributeMap->getNamedItem(CComBSTR(L"releaseId"), &pXmlReleaseIdAttribute);
		if (FAILED(hr) || pXmlReleaseIdAttribute == NULL)
		{
			sTestResult.lReleaseId = -1L;
		}
		else
		{
			//Now get the release id value
			BSTR strReleaseId;
			hr = pXmlReleaseIdAttribute->get_text(&strReleaseId);
			if (FAILED(hr))
			{
				sTestResult.lReleaseId = -1L;
			}
			else
			{
				//Convert to long
				long lReleaseId = _wtol(OLE2W(strReleaseId));
				if (lReleaseId == 0L)
				{
					DisplayMessage(CComBSTR(L"The mapped release id needs to be numeric"));
					return E_FAIL;
				}
				sTestResult.lReleaseId = lReleaseId;
			}
		}

		//Get the log-file manager
		CComPtr<IaqSubsystemManager> subsystem_manager;
		hr = m_base_manager->get_Managers(IID_ItcLogManager, &subsystem_manager);
		if (FAILED(hr))
		{
			DisplayMessage(CComBSTR(L"Unable to get log manager"));
			return hr;
		}

		CComPtr<ItcLogManager> logManager;
		hr = subsystem_manager->QueryInterface(&logManager);
		if (FAILED(hr))
		{
			DisplayMessage(CComBSTR(L"Unable to activate log manager"));
			return hr;
		}

		//Get the test run log we will use this for the more detailed result information
		BSTR strLogFileName;
		TC_LOG_DESCRIPTION sLogFileDesc;
		logManager->GetCurrentLogFileName(&strLogFileName);
		logManager->GetLogFileDescription(strLogFileName, &sLogFileDesc);
		
		//Calculate the number of assertions
		sTestResult.lAssertCount = sLogFileDesc.WarningCount + sLogFileDesc.ErrorCount;

		//Store the full path to the root log file
		CEnBSTR cRootLogFileName;
		cRootLogFileName = CEnBSTR(strLogFileName).copy();
		cRootLogFileName.replace(CEnBSTR("Description.tcLog"), "RootLogData.dat");
		sTestResult.strRootLogFileName.Assign(cRootLogFileName);

		//Make sure that we don't already have the same test in the vector and then add
		//(this is because we sometimes can get duplicate events posted by TestComplete)
		TESTRESULTS::iterator theIterator;
		bool bMatchFound = false;
		for (theIterator = m_test_results.begin(); theIterator != m_test_results.end(); theIterator++)
		{
			if (theIterator->lTestCaseId == sTestResult.lTestCaseId && theIterator->lProjectId == sTestResult.lProjectId && theIterator->lReleaseId == sTestResult.lReleaseId)
			{
				bMatchFound = true;
			}
		}
		if (!bMatchFound)
		{
			m_test_results.push_back(sTestResult);
		}
	}

	return S_OK;
}

//Records the actual test results for the entire run
HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::RecordTestResults(TESTRESULTS::iterator itTestResult)
{
	//Comment out for production code
	//DisplayMessage(itTestResult->strMoniker);

	HRESULT hr;

	//Instantiate the XML document object
	IXMLDOMDocumentPtr pXmlDocument;
	hr = ::CoCreateInstance(__uuidof(DOMDocument), NULL, CLSCTX_ALL, __uuidof(IXMLDOMDocument), (void**)&pXmlDocument);
	if (FAILED(hr)) return hr;

	//Open up the XML root log file for reading
	FILE *pFile;
	_wfopen_s(&pFile, OLE2W(itTestResult->strRootLogFileName), L"r");
	if (pFile == NULL)
	{
		DisplayMessage(CComBSTR(L"Unable to open test result root log file"));
		return E_FAIL;
	}

	//Read in all the xml nodes into a BSTR
	long lFileSize = 9999999;
	wchar_t *cLogFileText = (wchar_t *)malloc(lFileSize * sizeof(wchar_t));
	wchar_t cBuffer[255];
	while (!feof(pFile))
	{
		fgetws(cBuffer, 255, pFile);
		//Exclude the embedded DTD elements since they confuse the MSXML parser
		if (wcsstr(cBuffer, L"<!") == NULL && wcsstr(cBuffer, L"]>") == NULL)
		{
			wcscat_s(cLogFileText, lFileSize, cBuffer);
		}
	}
	fclose(pFile);
	
	//Now load the XML into the parser
	VARIANT_BOOL bSuccess;
	hr = pXmlDocument->loadXML(CComBSTR(cLogFileText), &bSuccess);
	free(cLogFileText);
	if (FAILED(hr) || !bSuccess)
	{
		DisplayMessage(CComBSTR(L"Unable to load test result root log file"));
		return hr;
	}

	//Extract the list of individual test files from this top-level logfile (RootLogData.dat)
	IXMLDOMElementPtr pXmlDOMElement;
	hr = pXmlDocument->get_documentElement(&pXmlDOMElement);
	if (FAILED(hr)) return hr;
	IXMLDOMNodeListPtr pXmlResultNodeList;
	hr = pXmlDOMElement->selectNodes(CComBSTR(L"/Nodes/Node/Node"), &pXmlResultNodeList);
	if (FAILED(hr)) return hr;
	if (pXmlResultNodeList == NULL)
	{
		DisplayMessage(CComBSTR(L"Unable to retrieve node list"));
		return E_FAIL;
	}

	//Instantiate the character buffer used to hold the log file messages
	wchar_t *cMessageText = (wchar_t *)malloc(9999999 * sizeof(wchar_t));
	wcscpy_s (cMessageText, 9999999, L"");

	//Iterate through each of the nodes
	long lCount;
	hr = pXmlResultNodeList->get_length(&lCount);
	if (FAILED(hr)) return hr;
	BSTR strFileName;
	bool bHasFileName = false;
	for (long i = 0; i < lCount; i++)
	{
		//Access each node in turn
		IXMLDOMNodePtr pXmlNode;
		hr = pXmlResultNodeList->get_item(i, &pXmlNode);
		if (FAILED(hr)) return hr;
		
		//Now select the Node/Prp element that has the matching test moniker
		IXMLDOMNodePtr pXmlPrpNode;
		wchar_t cTestXPath [255];
		wcscpy_s (cTestXPath, 255, L"./Prp[(@name='test' and @value='");
		wcscat_s (cTestXPath, 255, OLE2W(itTestResult->strMoniker));
		wcscat_s (cTestXPath, 255, L"')]");
		hr = pXmlNode->selectSingleNode(CComBSTR(cTestXPath), &pXmlPrpNode);
		if (FAILED(hr)) return hr;

		//If we have a match, need to get the child node that contains the log filename for the matched test item
		if (pXmlPrpNode != NULL)
		{
			//Since we might have multiple matching test items under this test, need to make sure that the test item name matches
			IXMLDOMNodePtr pXmlPrpTestItemNode;
			wchar_t cTestXPath [255];
			wcscpy_s (cTestXPath, 255, L"../Prp[(@name='name' and @value='");
			wcscat_s (cTestXPath, 255, OLE2W(itTestResult->strTestItemName));
			wcscat_s (cTestXPath, 255, L"')]");
			hr = pXmlPrpNode->selectSingleNode(CComBSTR(cTestXPath), &pXmlPrpTestItemNode);
			if (SUCCEEDED(hr) && pXmlPrpTestItemNode != NULL)
			{
				//Now get the actual child node that points to the log file
				IXMLDOMNodePtr pXmlPrpNode2;
				hr = pXmlPrpNode->selectSingleNode(CComBSTR(L"../Node[@name='children']/Prp[@name='child 0']"), &pXmlPrpNode2);
				if (FAILED(hr)) return hr;
				if (pXmlPrpNode2 != NULL)
				{
					//Extract the filename attribute from the child-node
					IXMLDOMNamedNodeMapPtr pXmlAttributeMap;
					hr = pXmlPrpNode2->get_attributes(&pXmlAttributeMap);
					if (FAILED(hr)) return hr;
					IXMLDOMNodePtr pXmlAttribute;
					hr = pXmlAttributeMap->getNamedItem(CComBSTR(L"value"), &pXmlAttribute);
					if (FAILED(hr)) return hr;
					if (pXmlAttribute != NULL)
					{
						//Get the filename value
						bHasFileName = true;
						pXmlAttribute->get_text(&strFileName);
						
						//Since this filename does not include the path, need to get that from the root path.
						CEnBSTR strPathName;
						strPathName = CEnBSTR(itTestResult->strRootLogFileName).copy();
						strPathName.replace(CEnBSTR("RootLogData.dat"), strFileName);

						//Now we need to load this document and extract the detailed test log messages
						hr = ExtractLogMessages(strPathName.GetBSTR(), cMessageText, &itTestResult->iExecutionStatus);
						if (FAILED(hr)) return hr;
					}
				}
			}
		}
	}
	
	//Close the file and release the COM objects
	pXmlDocument.Release();

	if (bHasFileName && itTestResult->strTestName != NULL)
	{
		//Need to get the dates in YYYY-MM-DDTHH:MM:SS SOAP format - just use system time rather than converting log time
		SYSTEMTIME localTime;
		GetLocalTime(&localTime);
		wchar_t cFormattedDateTime[128];
		swprintf_s(cFormattedDateTime, 128, L"%04u-%02u-%02uT%02u:%02u:%02u", localTime.wYear, localTime.wMonth, localTime.wDay, localTime.wHour, localTime.wMinute, localTime.wSecond);
		
		//Specify the runner name
		CComBSTR strRunnerName (L"TestComplete");

		//Log the result using the web service
		int i_result;
		hr = m_spiraTestExecuteService.RecordTestRun2 (m_strLogin, m_strPassword, itTestResult->lProjectId, -1, itTestResult->lTestCaseId, itTestResult->lReleaseId, CComBSTR(cFormattedDateTime), CComBSTR(cFormattedDateTime), itTestResult->iExecutionStatus, strRunnerName, itTestResult->strTestName, itTestResult->lAssertCount, itTestResult->strTestItemName, CComBSTR(cMessageText), &i_result);
		if (FAILED(hr))
		{
			SOAPCLIENT_ERROR iClientError = m_spiraTestExecuteService.GetClientError();
			if (iClientError == SOAPCLIENT_SEND_ERROR)
			{
				//Trying to send again
  				hr = m_spiraTestExecuteService.RecordTestRun2 (m_strLogin, m_strPassword, itTestResult->lProjectId, -1, itTestResult->lTestCaseId, itTestResult->lReleaseId, CComBSTR(cFormattedDateTime), CComBSTR(cFormattedDateTime), itTestResult->iExecutionStatus, strRunnerName, itTestResult->strTestName, itTestResult->lAssertCount, itTestResult->strTestItemName, CComBSTR(cMessageText), &i_result);
			}
			if (FAILED(hr))
			{
				wchar_t cErrorMessage[255];
				swprintf_s(cErrorMessage, 255, L"Failed to send test results of '%s' to SpiraTest due to system error %u.", OLE2W(itTestResult->strTestName), iClientError);
				DisplayMessage(CComBSTR(cErrorMessage));

				//Now display the detailed SOAP error message if applicable
				if (iClientError == SOAPCLIENT_SOAPFAULT)
				{
					swprintf_s(cErrorMessage, 255, L"SOAP Error %d - %s", m_spiraTestExecuteService.m_fault.m_soapErrCode, m_spiraTestExecuteService.m_fault.m_strDetail);
					DisplayMessage(CComBSTR(cErrorMessage));
				}
			}

			return hr;
		}
		free (cMessageText);
	}

	//Comment out for production code
	//DisplayMessage(CComBSTR(L"Exiting RecordTestResults"));
	return S_OK;
}

//Extracts the log messages
HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::ExtractLogMessages (BSTR strFileName, wchar_t *cMessageText, int *iExecutionStatus)
{
	//Comment out for production code
	//DisplayMessage(strFileName);

	HRESULT hr;

	//Instantiate the XML document object
	IXMLDOMDocumentPtr pXmlDocument;
	hr = ::CoCreateInstance(__uuidof(DOMDocument), NULL, CLSCTX_ALL, __uuidof(IXMLDOMDocument), (void**)&pXmlDocument);
	if (FAILED(hr)) return hr;

	//Open up the XML test log file for reading
	FILE *pFile;
	_wfopen_s(&pFile, OLE2W(strFileName), L"r");
	if (pFile == NULL)
	{
		DisplayMessage(CComBSTR(L"Unable to open test result log file"));
		return E_FAIL;
	}

	//Read in all the xml nodes into a wide string
	long lFileSize = 9999999;
	wchar_t *cLogFileText = (wchar_t *)malloc(lFileSize * sizeof(wchar_t));
	wchar_t cBuffer[255];
	while (!feof(pFile))
	{
		fgetws(cBuffer, 255, pFile);
		//Exclude the embedded DTD elements since they confuse the MSXML parser
		if (wcsstr(cBuffer, L"<!") == NULL && wcsstr(cBuffer, L"]>") == NULL)
		{
			wcscat_s(cLogFileText, lFileSize, cBuffer);
		}
	}
	fclose(pFile);
	
	//Now load the XML into the parser
	VARIANT_BOOL bSuccess;
	hr = pXmlDocument->loadXML(CComBSTR(cLogFileText), &bSuccess);
	free(cLogFileText);
	if (FAILED(hr) || !bSuccess)
	{
		DisplayMessage(CComBSTR(L"Unable to load test result log file"));
		return hr;
	}

	//Extract the list of individual test test messages from this logfile
	IXMLDOMElementPtr pXmlDOMElement;
	hr = pXmlDocument->get_documentElement(&pXmlDOMElement);
	if (FAILED(hr)) return hr;
	IXMLDOMNodeListPtr pXmlResultNodeList;
	hr = pXmlDOMElement->selectNodes(CComBSTR(L"/Nodes/Node/Node"), &pXmlResultNodeList);
	if (FAILED(hr)) return hr;
	if (pXmlResultNodeList == NULL)
	{
		DisplayMessage(CComBSTR(L"Unable to retrieve node list"));
		return E_FAIL;
	}

	//Iterate through each of the nodes and get the message and results. Then add to string
	//(need to iterate in reverse order so that they appear in chronological order)
	long lCount;
	hr = pXmlResultNodeList->get_length(&lCount);
	if (FAILED(hr)) return hr;
	for (long i = lCount-1; i >= 0; i--)
	{
		//Access each node in turn
		IXMLDOMNodePtr pXmlNode;
		hr = pXmlResultNodeList->get_item(i, &pXmlNode);
		if (FAILED(hr)) return hr;
		
		//Now select the Node/Prp element that has the message and add to the string
		IXMLDOMNodePtr pXmlPrpNode;
		hr = pXmlNode->selectSingleNode(CComBSTR(L"./Prp[@name='message']"), &pXmlPrpNode);
		if (FAILED(hr)) return hr;

		if (pXmlPrpNode != NULL)
		{
			//Extract the value attribute from this node
			IXMLDOMNamedNodeMapPtr pXmlAttributeMap;
			hr = pXmlPrpNode->get_attributes(&pXmlAttributeMap);
			if (FAILED(hr)) return hr;
			IXMLDOMNodePtr pXmlAttribute;
			hr = pXmlAttributeMap->getNamedItem(CComBSTR(L"value"), &pXmlAttribute);
			if (FAILED(hr)) return hr;
			BSTR strMessage;
			pXmlAttribute->get_text(&strMessage);

			//Now select the Node/Prp element that has the type and add the message to the string
			IXMLDOMNodePtr pXmlPrpNode2;
			hr = pXmlNode->selectSingleNode(CComBSTR(L"./Prp[@name='type']"), &pXmlPrpNode2);
			if (FAILED(hr)) return hr;
			if (pXmlPrpNode2 != NULL)
			{
				//Now get the type and add the appropriate message prefix
				IXMLDOMNamedNodeMapPtr pXmlAttributeMap2;
				hr = pXmlPrpNode2->get_attributes(&pXmlAttributeMap2);
				if (FAILED(hr)) return hr;
				IXMLDOMNodePtr pXmlAttribute2;
				hr = pXmlAttributeMap2->getNamedItem(CComBSTR(L"value"), &pXmlAttribute2);
				if (FAILED(hr)) return hr;
				BSTR strType;
				pXmlAttribute2->get_text(&strType);
				long lType = _wtol(OLE2W(strType));
				if (lType == 3)
				{
					//Error
					wcscat_s(cMessageText, 9999999, L"Error: ");
				}
				else if (lType == 2)
				{
					//Warning
					wcscat_s(cMessageText, 9999999, L"Warning: ");
				}
				else if (lType == 1)
				{
					//Message
					wcscat_s(cMessageText, 9999999, L"Message: ");
				}
				else
				{
					//Message
					wcscat_s(cMessageText, 9999999, L"Other: ");
				}

				//Now add to the text message
				wcscat_s (cMessageText, 9999999, OLE2W(strMessage));
				wcscat_s (cMessageText, 9999999, L"\n");
			}
		}
	}

	//Finally we need to get the overall execution status from the file
	IXMLDOMNodePtr pXmlPrpNode;
	hr = pXmlDOMElement->selectSingleNode(CComBSTR(L"/Nodes/Node/Prp[@name='status']"), &pXmlPrpNode);
	if (FAILED(hr)) return hr;
	if (pXmlPrpNode != NULL)
	{
		//Now get the type and convert to the equivalent SpiraTest status
		IXMLDOMNamedNodeMapPtr pXmlAttributeMap;
		hr = pXmlPrpNode->get_attributes(&pXmlAttributeMap);
		if (FAILED(hr)) return hr;
		IXMLDOMNodePtr pXmlAttribute;
		hr = pXmlAttributeMap->getNamedItem(CComBSTR(L"value"), &pXmlAttribute);
		if (FAILED(hr)) return hr;
		BSTR strStatus;
		pXmlAttribute->get_text(&strStatus);
		long lStatusId = _wtol(OLE2W(strStatus));
		if (lStatusId == 0)
		{
			//OK
			*iExecutionStatus = 2;	//Passed
		}
		if (lStatusId == 1)
		{
			//Warning
			*iExecutionStatus = 6;	//Caution
		}
		if (lStatusId == 2)
		{
			//Error
			*iExecutionStatus = 1;	//Failed
		}		
	}
	
	//Release the COM objects
	pXmlDocument.Release();

	//Comment out for production code
	//DisplayMessage(CComBSTR(L"Exiting ExtractLogMessages"));
	return S_OK;
}

//Displays a popup message box (used for debug purposes)
HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::DisplayMessage(BSTR message)
{
	HRESULT hr;

    CComPtr<IaqSubsystemManager> subsystem_manager;
    hr = m_base_manager ->get_Managers(IID_IaqMessenger, &subsystem_manager);
    if (FAILED(hr)) return hr;
  
    CComPtr<IaqMessenger> messenger;
    hr = subsystem_manager->QueryInterface(&messenger);
    if (FAILED(hr)) return hr;
  
    hr = messenger->ShowInformation(message, 0, CComBSTR(_T("")));
    if (FAILED(hr)) return hr;
	return S_OK;
}

//This method is called for events that concern the whole test run.
//The Event parameter specifies the type of event that occurred.
HRESULT STDMETHODCALLTYPE CSpiraTestPlugIn::OnEngineEvent(TC_TESTENGINE_EVENT Event)
{
	HRESULT hr;

	switch (Event)
	{
		case TC_PLAYING_START:
		{
			//Do nothing, the list of test run events is already initialized
		}
		case TC_PLAYING_STOP:
		{
			//The log files will have been written, so now we can use them to upload the information to SpiraTest
			//Loop through all of the test results
			if (!m_test_results.empty())
			{
				TESTRESULTS::iterator theIterator;
				for (theIterator = m_test_results.begin(); theIterator != m_test_results.end(); theIterator++)
				{
					hr = RecordTestResults(theIterator);
					if (FAILED(hr)) return hr;
				}
			}

			//Finally empty the vector
			m_test_results.clear();
			break;
		}
	}

	return S_OK;
}

