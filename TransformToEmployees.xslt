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
						<salary amount="{@amount}" mount="{@mount}"/>
					</xsl:for-each>
				</Employee>
			</xsl:for-each>
		</Employees>
	</xsl:template>
</xsl:stylesheet>