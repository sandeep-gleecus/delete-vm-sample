<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:html="http://www.w3.org/TR/REC-html40"
>
    <xsl:output method="xml" indent="yes"/>

  <xsl:template match="Worksheet">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="* | text()">
    <xsl:if test="not(parent::Worksheet and not(local-name(.) = 'Row'))">
      <xsl:copy>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates />
      </xsl:copy>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
