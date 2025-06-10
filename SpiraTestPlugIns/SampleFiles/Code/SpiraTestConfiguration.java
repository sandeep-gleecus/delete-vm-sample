package com.inflectra.spiratest.addons.junitextension;

import java.lang.annotation.*;

/**
 * This defines the 'SpiraTestConfiguration' annotation used to specify the authentication,
 * project and release information for the test being executed
 * 
 * @author		Inflectra Corporation
 * @version		3.0.2
 *
 */
@Retention(value=java.lang.annotation.RetentionPolicy.RUNTIME)
@Target(value=java.lang.annotation.ElementType.TYPE)
public @interface SpiraTestConfiguration
{
	String url ();
	String login () default "";
	Integer projectId () default null;
	Integer releaseId () default null;
	Integer testSetId () default null;
	Integer buildId () default null;
	
	String apiKey () default "";
}
