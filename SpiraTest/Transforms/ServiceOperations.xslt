<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/"
                xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
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
    <body class="px0 pt0 pb5 ma0">
      <div id="content">
        <p class="pa4 mt0 mb6 bg-tiber fs-h3 ff-josefin white">
          <xsl:value-of select="$productName"/>:
          <xsl:value-of select="wsdl:service/@name"/> SOAP Web Service (<xsl:value-of select="wsdl:portType/wsdl:documentation/comments/version"/>)
        </p>
        <div class="vw-50-xl vw-66-lg vw-75-md vw-75_sm vw-90-xs vw-90-xxs mx-auto">
          <span>
            <p class="intro">
              <xsl:value-of select="wsdl:portType/wsdl:documentation/comments/summary"/>
            </p>
            <p class="intro">
              <xsl:value-of select="wsdl:portType/wsdl:documentation/comments/remarks"/>
            </p>
          </span>
          <span>
            <p class="intro">
              The following operations are supported.  For a formal definition, please review the
              <a target="_blank">
                <xsl:attribute name="href">
                  <xsl:value-of select="wsdl:service/wsdl:port/soap:address/@location"/>?wsdl
                </xsl:attribute>
                Service Description
              </a>.
            </p>
            <table class="bn bb bw2">
              <tr class="bg-tiber white">
                <th class="pa3 tl fw-normal bn">
                  Return Value
                </th>
                <th class="pa3 tl fw-normal bn">
                  Operation
                </th>
              </tr>
              <xsl:for-each select="wsdl:portType/wsdl:operation">
                <xsl:sort select="@name"/>
                <xsl:variable name="operationName" select="@name" />
                <tr>
                  <td>
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
                  </td>
                  <td>
                    <a>
                      <xsl:attribute name="href">
                        ?operation=<xsl:value-of select="$operationName"/>
                      </xsl:attribute>
                      <xsl:value-of select="$operationName"/>
                    </a>
                    <xsl:if test="/wsdl:definitions/wsdl:types//xsd:element[@name=$operationName]//xsd:element"> ( </xsl:if>
                    <xsl:for-each select="/wsdl:definitions/wsdl:types//xsd:element[@name=$operationName]//xsd:element">
                      <xsl:variable name="requestType" select="@type" />
                      <xsl:variable name="requestTypeDisplay" select="substring-after($requestType, ':')" />
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
                      </xsl:if>&#160;<xsl:value-of select="@name" />, 
                    </xsl:for-each>                  
                    <xsl:if test="/wsdl:definitions/wsdl:types//xsd:element[@name=$operationName]//xsd:element"> ) </xsl:if>
                    <br />
                    <xsl:value-of select="wsdl:documentation/comments/summary"/>
                  </td>
                 </tr>
              </xsl:for-each>
            </table>
          </span>
        </div>
      </div>
    </body>
  </html>
  </xsl:template>

</xsl:stylesheet>
