<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" indent="yes" encoding="utf-8"/>

	<!-- Group items by person (name + surname) no matter where they are in the document -->
	<xsl:key name="itemsByPerson" match="//item" use="concat(@name,'|',@surname)"/>

	<xsl:template match="/">
		<Employees>
			<!-- Select one representative <item> per person -->
			<xsl:for-each select="//item[generate-id() = generate-id(key('itemsByPerson', concat(@name,'|',@surname))[1])]">
				<Employee name="{@name}" surname="{@surname}">
					<!-- Emit all salaries for this person -->
					<xsl:for-each select="key('itemsByPerson', concat(@name,'|',@surname))">
						<salary amount="{@amount}">
							<xsl:attribute name="mount">
								<xsl:choose>
									<!-- Prefer month from ancestor section (for Data2.xml) if present -->
									<xsl:when test="ancestor::*[self::january or self::february or self::march]">
										<xsl:value-of select="name(ancestor::*[self::january or self::february or self::march][1])"/>
									</xsl:when>
									<!-- Fallback to item's own @mount attribute (for Data1.xml) -->
									<xsl:otherwise>
										<xsl:value-of select="@mount"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:attribute>
						</salary>
					</xsl:for-each>
				</Employee>
			</xsl:for-each>
		</Employees>
	</xsl:template>
</xsl:stylesheet>