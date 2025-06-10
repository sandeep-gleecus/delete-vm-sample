//Override the ASP.NET validation display
function WebForm_OnSubmit()
{
    if (typeof (ValidatorOnSubmit) == "function" && ValidatorOnSubmit() == false)
    {
        for (var i in Page_Validators)
        {
            try
            {
                var el = $('#' + Page_Validators[i].controltovalidate);
                if (!Page_Validators[i].isvalid)
                {
                    el.addClass('validation-invalid')
                } else
                {
                    el.addClass('validation-valid');
                }
            } catch (e) { }
        }
        return false;
    }
    return true;
}
