﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.ViewEngines.Razor;

namespace DinnerParty.HtmlExtensions
{
    public static class HtmlExtensions
    {
        public static Nancy.ViewEngines.Razor.IHtmlString CheckBox<T>(this HtmlHelpers<T> helper, string Name, dynamic ModelProperty)
        {
            string input = String.Empty;
            bool checkedState = false;

            if (!bool.TryParse(ModelProperty.ToString(), out checkedState))
            {
                input = "<input name=\"" + Name + "\" type=\"checkbox\" value=\"true\" />";
            }
            else
            {
                if (checkedState)
                    input = "<input name=\"" + Name + "\" type=\"checkbox\" value=\"true\" checked />";
                else
                    input = "<input name=\"" + Name + "\" type=\"checkbox\" value=\"true\" />";
            }


            return new Nancy.ViewEngines.Razor.NonEncodedHtmlString(input);
        }

        public static Nancy.ViewEngines.Razor.IHtmlString ValidationSummary<T>(this HtmlHelpers<T> helper, List<DinnerParty.Models.ErrorModel> Errors)
        {

            if (!Errors.Any())
                return new NonEncodedHtmlString("");

            string div = "<div class=\"validation-summary-errors\"><span>Account creation was unsuccessful. Please correct the errors and try again.</span><ul>";

            foreach (var item in Errors)
            {

                div += "<li>" + item.ErrorMessage + "</li>";

            }

            div += "</ul></div>";

            return new NonEncodedHtmlString(div);
        }

        public static Nancy.ViewEngines.Razor.IHtmlString ValidationMessageFor<T>(this HtmlHelpers<T> helper, List<DinnerParty.Models.ErrorModel> Errors, string PropertyName)
        {
            if (!Errors.Any())
                return new NonEncodedHtmlString("");

            string span = String.Empty;

            foreach (var item in Errors)
            {
                if (item.Name == PropertyName)
                {
                    span += "<span class=\"field-validation-error\">" + item.ErrorMessage+ "</span>";
                    break;
                }

            }

            return new NonEncodedHtmlString(span);
        }
    }
}