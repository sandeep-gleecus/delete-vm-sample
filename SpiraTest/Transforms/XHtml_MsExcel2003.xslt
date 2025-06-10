<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:html="http://www.w3.org/TR/REC-html40"
>
  <xsl:strip-space elements="*"/>
  <xsl:preserve-space elements="listing plaintext pre samp"/>
  <xsl:output method="xml" indent="no"/>

  <xsl:template match="p|div">
    <xsl:if test="@class='Title1' or @class='Title2' or @class='Title3' or @class='Title4'">
      <Row/>
    </xsl:if>
    <Row>
      <Cell>
        <xsl:if test="@class='Title1' or @class='Title2' or @class='Title3' or @class='Title4'">
          <xsl:attribute name="ss:StyleID">sSubTitle</xsl:attribute>
        </xsl:if>
        <Data ss:Type="String">
          <xsl:apply-templates/>
        </Data>
      </Cell>
    </Row>
  </xsl:template>

  <xsl:template match="table/tr|table/tbody/tr">
    <Row>
      <xsl:apply-templates/>
    </Row>
  </xsl:template>

  <xsl:template match="td">
    <xsl:if test="@class='Date'">
      <Cell ss:StyleID="sDateTime">
        <Data ss:Type="String">
          <xsl:apply-templates/>
        </Data>
      </Cell>
    </xsl:if>
    <xsl:if test="@class='Timespan'">
      <xsl:if test=". = ''">
        <Cell ss:StyleID="sNormal">
          <Data ss:Type="String" />
        </Cell>
      </xsl:if>
      <xsl:if test="not(. = '')">
        <Cell ss:StyleID="sDecimal">
          <Data ss:Type="Number">
            <xsl:apply-templates/>
          </Data>
        </Cell>
      </xsl:if>
    </xsl:if>
    <xsl:if test="not(@class='Date') and not(@class='Timespan')">
      <Cell ss:StyleID="sNormal">
        <Data ss:Type="String">
          <xsl:apply-templates/>
        </Data>
      </Cell>
    </xsl:if>
  </xsl:template>

  <xsl:template match="th">
    <Cell ss:StyleID="sHeader">
      <Data ss:Type="String">
        <xsl:apply-templates/>
      </Data>
    </Cell>
  </xsl:template>

  <xsl:template match="br">
    <xsl:text disable-output-escaping="yes"> <![CDATA[&#10;]]></xsl:text>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="b | strong">
    <html:B>
      <xsl:apply-templates/>
    </html:B>
  </xsl:template>
  <xsl:template match="i | em">
    <html:I>
      <xsl:apply-templates/>
    </html:I>
  </xsl:template>
  <xsl:template match="u">
    <html:U>
      <xsl:apply-templates/>
    </html:U>
  </xsl:template>
  <xsl:template match="p">
    <xsl:text disable-output-escaping="yes"> <![CDATA[&#10;]]></xsl:text>
    <xsl:apply-templates/>
  </xsl:template>
  <xsl:template match="span[contains(@style,'font-weight: bold')]">
    <html:B>
      <xsl:apply-templates/>
    </html:B>
  </xsl:template>
  <xsl:template match="span[contains(@style,'font-style: italic')]">
    <html:I>
      <xsl:apply-templates/>
    </html:I>
  </xsl:template>
  <xsl:template match="span[contains(@style,'text-decoration: underline')]">
    <html:U>
      <xsl:apply-templates/>
    </html:U>
  </xsl:template>

</xsl:stylesheet>
