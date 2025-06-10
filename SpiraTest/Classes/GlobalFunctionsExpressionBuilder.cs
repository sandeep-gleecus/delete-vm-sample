using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.CodeDom;
using System.ComponentModel;
using System.Reflection;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Allows ASP.NET pages to more easily access the GlobalFunctions constants using the GlobalFunctions prefix
    /// </summary>
    public class GlobalFunctionsExpressionBuilder : ExpressionBuilder 
    {
        public override CodeExpression GetCodeExpression(System.Web.UI.BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            CodeExpression[] inputParams = new CodeExpression[] { new CodePrimitiveExpression(entry.Expression.Trim()), 
                                                       new CodeTypeOfExpression(entry.DeclaringType), 
                                                       new CodePrimitiveExpression(entry.PropertyInfo.Name) };

            // Return a CodeMethodInvokeExpression that will invoke the GetRequestedValue method using the specified input parameters 
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetType()),
                                        "GetRequestedValue",
                                        inputParams); 

        }

        /// <summary>
        /// Gets the requested value from the appropriate constant in the GlobalFunctions class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="targetType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetRequestedValue(string key, Type targetType, string propertyName)
        {
            //Get all public static properties of GlobalFunctions type
            FieldInfo[] propertyInfos;
            propertyInfos = typeof(GlobalFunctions).GetFields(BindingFlags.Public | BindingFlags.Static);

            //Get the appropriate value
            object value = null;
            foreach (FieldInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name == key && propertyInfo.IsLiteral)
                {
                    value = propertyInfo.GetRawConstantValue();
                }
            }

            // Make sure that the specified Session variable exists 
            if (value == null)
            {
                throw new InvalidOperationException(string.Format("GlobalFunctions constant '{0}' not found.", key));
            }

            // Convert the variable if its type does not match up with the Web control property type 
            if (targetType != null)
            {
                PropertyDescriptor propDesc = TypeDescriptor.GetProperties(targetType)[propertyName];
                if (propDesc != null && propDesc.PropertyType != value.GetType())
                {
                    // Type mismatch - make sure that the variable value can be converted to the Web control property type 
                    if (propDesc.Converter.CanConvertFrom(value.GetType()) == false)
                        throw new InvalidOperationException(string.Format("GlobalFunctions constant '{0}' cannot be converted to type {1}.", key, propDesc.PropertyType.ToString()));
                    else
                        return propDesc.Converter.ConvertFrom(value);
                }
            }

            // If we reach here, no type mismatch - return the value 
            return value;
        } 
    }
}