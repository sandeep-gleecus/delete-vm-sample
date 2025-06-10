<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

    <!--Table Elements-->
    <xsl:template match="table">
      <xsl:if test="ancestor::table">
        <xsl:apply-templates/>
      </xsl:if>
      <xsl:if test="not(ancestor::table)">
        <xsl:copy>
          <xsl:copy-of select="@*" />
          <xsl:apply-templates/>
        </xsl:copy>
      </xsl:if>
    </xsl:template>
  <xsl:template match="tr">
    <xsl:if test="ancestor::tr">
      <xsl:apply-templates/>
    </xsl:if>
    <xsl:if test="not(ancestor::tr)">
      <xsl:copy>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>
  <xsl:template match="td|th">
    <xsl:if test="ancestor::td or ancestor::th">
      <xsl:apply-templates/>
    </xsl:if>
    <xsl:if test="not(ancestor::td or ancestor::th)">
      <xsl:copy>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>

  <!--Other Block Elements-->
  <xsl:template match="html|body|ul|li|br">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      &#160;<xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

  <!--Inline elements that we don't want-->
  <xsl:template match="*[not(namespace-uri()='')]">
    <xsl:if test="parent::body or parent::div[not(@class)] or parent::td or parent::th or parent::html or parent::h1 or parent::h2 or parent::h3">
      <p>
        <xsl:apply-templates/>
      </p>
    </xsl:if>
    <xsl:if test="not(parent::body or parent::div[not(@class)] or parent::td or parent::th or parent::html or parent::h1 or parent::h2 or parent::h3)">
      <xsl:apply-templates/>
    </xsl:if>
  </xsl:template>

  <!--Inline Elements-->
  <xsl:template match="b|i|u|strong|a|img|span|font|text()">
    <xsl:if test="parent::body or parent::div[not(@class)] or parent::td or parent::th or parent::html or parent::h1 or parent::h2 or parent::h3">
      <p>
        <xsl:copy>
          <xsl:copy-of select="@*" />
          <xsl:apply-templates/>
        </xsl:copy>
      </p>
    </xsl:if>
    <xsl:if test="not(parent::body or parent::div[not(@class)] or parent::td or parent::th or parent::html or parent::h1 or parent::h2 or parent::h3)">
      <xsl:copy>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>

  <xsl:template match="*">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="div">
    <xsl:if test="@class">
      <xsl:if test="ancestor::div[@class]">
        <xsl:apply-templates />
      </xsl:if>
      <xsl:if test="ancestor::li">
        <span>
          <xsl:copy-of select="@*" />
          <xsl:apply-templates />
        </span>
      </xsl:if>
      <xsl:if test="not(ancestor::div[@class]) and not(ancestor::li)">
        <div>
          <xsl:copy-of select="@*" />
          <xsl:apply-templates />
        </div>
      </xsl:if>
    </xsl:if>
    <xsl:if test="not(@class)">
      <xsl:if test="parent::td or parent::th">
        <div>
          <xsl:copy-of select="@*" />
          <xsl:apply-templates />
        </div>
      </xsl:if>
      <xsl:if test="not(parent::td or parent::th)">
        <xsl:apply-templates />
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="p">
    <xsl:if test="(ancestor::p or ancestor::li or ancestor::table or ancestor::div[@class]) and not(parent::td or parent::th or parent::html)">
      <span>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates />
      </span>
    </xsl:if>
    <xsl:if test="not((ancestor::p or ancestor::li or ancestor::table or ancestor::div[@class]) and not(parent::td or parent::th or parent::html))">
      <p>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates />
      </p>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ol|ul">
    <xsl:if test="parent::p">
      <xsl:text disable-output-escaping="yes">&#60;/p&#62;</xsl:text>
      <xsl:copy>
        <xsl:apply-templates/>
      </xsl:copy>
      <xsl:text disable-output-escaping="yes">&#60;p&#62;</xsl:text>
    </xsl:if>
    <xsl:if test="parent::div[@class]">
      <xsl:text disable-output-escaping="yes">&#60;/div&#62;</xsl:text>
      <xsl:copy>
        <xsl:apply-templates/>
      </xsl:copy>
      <xsl:text disable-output-escaping="yes">&#60;div&#62;</xsl:text>
    </xsl:if>
    <xsl:if test="not(parent::p or parent::div[@class])">
      <xsl:copy>
        <xsl:apply-templates/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
