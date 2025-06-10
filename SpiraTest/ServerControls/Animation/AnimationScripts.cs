

using System;
using System.Web.UI;
using Inflectra.SpiraTest.Web.ServerControls.ExtenderBase;
using Inflectra.SpiraTest.Web.ServerControls.CommonScripts;


namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// The AnimationScripts class is used to load all of the animation support for the AJAX
    /// Control Toolkit.  To use any of the animations you find in Animations.js, simply include
    /// the attribute [RequiredScript(typeof(AnimationScripts))] on your extender.
    /// </summary>
    [ClientScriptResource(null, "Inflectra.SpiraTest.Web.ClientScripts.Animations.js")]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [RequiredScript(typeof(TimerScript))]
    public static class AnimationScripts
    {
    }
}
