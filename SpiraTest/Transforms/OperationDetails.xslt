<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/"
                xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <xsl:param name="operationName" />
  <xsl:param name="productName" />
  <xsl:template match="/wsdl:definitions">
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head>
      <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
      <link rel="stylesheet" type="text/css" href="Documentation.css" />
      <link rel="stylesheet" type="text/css" href="../../App_Themes/InflectraTheme/InflectraTheme_Unity.css" />
      <title>
        <xsl:value-of select="$productName"/>:
        <xsl:value-of select="wsdl:service/@name"/> SOAP Web Service
      </title>
    </head>
    <body class="pa0 ma0">
      <div id="content">
        <p class="pa4 mt0 mb4 bg-tiber fs-h3 ff-josefin white">
          <xsl:value-of select="$productName"/>:
          <xsl:value-of select="wsdl:service/@name"/> SOAP Web Service
        </p>
        <div class="mb6 vw-50-xl vw-66-lg vw-75-md vw-75_sm vw-90-xs vw-90-xxs mx-auto">
          <a class="mb4 br2 px4 py1 bg-near-white bg-vlight-gray-hover transition-all" href="?">See all operations</a>
          <h1 class="fs-h2">
            <xsl:value-of select="$operationName"/>
          </h1>
          <h2 class="silver fs-h3 mb2 mt5">Description</h2>
          <p>
            <xsl:value-of select="wsdl:portType/wsdl:operation[@name=$operationName]/wsdl:documentation/comments/summary"/>
          </p>
          <p>
            <xsl:value-of select="wsdl:portType/wsdl:operation[@name=$operationName]/wsdl:documentation/comments/remarks"/>
          </p>
          <xsl:if test="wsdl:portType/wsdl:operation[@name=$operationName]/wsdl:documentation/comments/example">
            <h3>Example(s)</h3>
            <pre>
              <xsl:value-of select="wsdl:portType/wsdl:operation[@name=$operationName]/wsdl:documentation/comments/example"/>
            </pre>
          </xsl:if>
          
          <span>
            <h2 class="silver fs-h3 mb2 mt5">Parameters</h2>
            <table>
              <tr>
                <th class="px3 tl">
                  Type
                </th>
                <th class="px3 tl">
                  Name
                </th>
                <th class="px3 tl">
                  Required
                </th>
              </tr>
              <xsl:for-each select="/wsdl:definitions/wsdl:types//xsd:element[@name=$operationName]//xsd:element">
                <tr>
                  <xsl:variable name="requestType" select="@type" />
                  <xsl:variable name="paramName" select="@name" />
                  <xsl:variable name="requestTypeDisplay" select="substring-after($requestType, ':')" />
                  <td>
                    <xsl:if test="substring-before($requestType, ':') = 'xsd'">
                      <xsl:if test="@nillable">
                        <xsl:value-of select="$requestTypeDisplay" />?
                      </xsl:if>
                      <xsl:if test="not(@nillable)">
                        <xsl:value-of select="$requestTypeDisplay" />
                      </xsl:if>
                    </xsl:if>
                    <xsl:if test="substring-before($requestType, ':') != 'xsd'">
                      <a>
                        <xsl:attribute name="href">
                          ?datacontract=<xsl:value-of select="$requestType"/>
                        </xsl:attribute>
                        <xsl:if test="@nillable">
                          <xsl:value-of select="$requestTypeDisplay" />?
                        </xsl:if>
                        <xsl:if test="not(@nillable)">
                          <xsl:value-of select="$requestTypeDisplay" />
                        </xsl:if>
                      </a>
                    </xsl:if>
                  </td>
                  <td>
                    <xsl:value-of select="$paramName" />
                    -
                    <i>
                      <xsl:value-of select="/wsdl:definitions/wsdl:portType/wsdl:operation[@name=$operationName]/wsdl:documentation/comments/param[@name=$paramName]"/>
                    </i>
                  </td>
                  <td>
                    <xsl:if test="not(@nillable)">
                      Yes
                    </xsl:if>
                  </td>
                </tr>
              </xsl:for-each>
            </table>

            <h2 class="silver fs-h3 mb2 mt6">Return Value</h2>
            <p>
              <xsl:variable name="operationResponse" select="concat($operationName,'Response')" />
              <xsl:variable name="responseType" select="/wsdl:definitions/wsdl:types//xsd:element[@name=$operationResponse]//xsd:element/@type" />
              <xsl:variable name="responseTypeDisplay" select="substring-after($responseType, ':')" />
              <xsl:if test="substring-before($responseType, ':') = 'xsd'">
                <xsl:value-of select="$responseTypeDisplay" />
              </xsl:if>
              <xsl:if test="substring-before($responseType, ':') != 'xsd'">
                <a>
                  <xsl:attribute name="href">
                    ?datacontract=<xsl:value-of select="$responseType"/>
                  </xsl:attribute>
                  <xsl:value-of select="$responseTypeDisplay" />
                </a>
              </xsl:if>
              -
              <i>
                <xsl:value-of select="wsdl:portType/wsdl:operation[@name=$operationName]/wsdl:documentation/comments/returns"/>
              </i>
            </p>
          </span>
        </div>
      </div>
    </body>
  </html>
  </xsl:template>

</xsl:stylesheet>
