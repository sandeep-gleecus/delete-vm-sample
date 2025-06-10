<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl xhtml"
    xmlns:xhtml="http://www.w3.org/1999/xhtml"
    xmlns:v="urn:schemas-microsoft-com:vml"
    xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
    xmlns:xd="http://schemas.microsoft.com/office/infopath/2003"
    xmlns:wx="http://schemas.microsoft.com/office/word/2003/auxHint">
  
  <xsl:output method="xml" version="1.0" standalone="yes" omit-xml-declaration="yes"
    encoding="utf-8" media-type="text/xml" indent="no"/>

  <xsl:template match="wordml">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="w:p|w:tbl">
    <xsl:if test="ancestor::w:t">
      <xsl:apply-templates select="text()"/>
    </xsl:if>
    <xsl:if test="not(ancestor::w:t)">
      <xsl:copy>
        <xsl:apply-templates/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="* | text()">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <xsl:apply-templates />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
