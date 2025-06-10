<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:xhtml="http://www.w3.org/1999/xhtml"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl xhtml"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="flow">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="fo:inline">
    <xsl:if test="parent::flow">
      <fo:block>
        <xsl:copy>
          <xsl:apply-templates />
        </xsl:copy>
      </fo:block>
    </xsl:if>
    <xsl:if test="not(parent::flow)">
      <xsl:copy>
        <xsl:copy-of select="@*[local-name() != 'id']" />
        <xsl:apply-templates />
      </xsl:copy>
    </xsl:if>
  </xsl:template>

  <xsl:template match="fo:list-block">
    <xsl:if test="count(fo:list-item) = count(*)">
      <xsl:copy>
        <xsl:copy-of select="@*[local-name() != 'id']" />
        <xsl:apply-templates />
      </xsl:copy>
    </xsl:if>
  </xsl:template>

  <xsl:template match="fo:basic-link">
    <xsl:if test="(@external-destination and @external-destination != '') or (@internal-destination and @internal-destination != '')">
      <xsl:copy>
        <xsl:copy-of select="@*[local-name() != 'id']" />
        <xsl:apply-templates />
      </xsl:copy>
    </xsl:if>
    <xsl:if test="not((@external-destination and @external-destination != '') or (@internal-destination and @internal-destination != ''))">
      <xsl:apply-templates />
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="fo:table-row">
    <xsl:if test="parent::fo:table-body or parent::fo:table-header or parent::fo:table-footer">
        <xsl:copy>
          <xsl:copy-of select="@*[local-name() != 'id']" />
          <xsl:apply-templates />
        </xsl:copy>
    </xsl:if>
    <xsl:if test="not(parent::fo:table-body or parent::fo:table-header or parent::fo:table-footer)">
      <xsl:apply-templates />
    </xsl:if>
  </xsl:template>

  <xsl:template match="@*[local-name() != 'id'] | node()">
    <xsl:copy>
      <xsl:apply-templates select="@*[local-name() != 'id'] | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
