(function($) {

    var form = $("#signup-form");
    var container = form.closest('.container')
    var initHeight = container.height();
    function disUnSltedType() {
        form.find("fieldset[name=step-2]").find(".form-control").attr("disabled", true);
        form.find(".dynamic-type").attr("hidden", true);
    }
   
    form.validate({
        errorPlacement: function errorPlacement(error, element) {
            error.css({ 'color': 'rgb(255, 48, 46)', 'font-weight': '500', 'font-size': '13x' });
            element.after(error);
        },
        rules: {
            FirstName: {
                required: true
            },
            LastName: {
                required: true
            },
            Email: {
                required: true,
                email: true
            },
            Password: {
                required: true,
                minlength: 6
            },
            ConfirmPassword: {
                required: true,
                minlength: 6,
                maxlength: 100,
                equalTo: "#password"
            },
            UserType: {
                required: true
            }
        },
        messages: {
            FirstName: "The First Name field is required.",
            LastName: "The Last Name field is required.",
            Email: {
                required: "The Email field is required.",
                email: 'Not a valid email address.'
            },
            Password: {
                required: "The Password field is required.",
                minlength: 'The Password must be at least 6 and at max 100 characters long.',
                maxlength: 'The Password must be at least 6 and at max 100 characters long.'
            },
            ConfirmPassword: {
                required: "The Confirm Password field is required.",
                equalTo: 'The password and confirmation password do not match.'
            }
        }
    });


    form.steps({
        headerTag: "h3",
        bodyTag: "fieldset",
        transitionEffect: "fade",
        labels: {
            previous : 'Previous',
            next : 'Next',
            finish : 'Submit',
            current : ''
        },
        titleTemplate : '<div class="title"><span class="title-text">#title#</span><span class="title-number">0#index#</span></div>',
        onStepChanging: function (event, currentIndex, newIndex)
        {
            if (currentIndex === 0 && form.valid()) {
                disUnSltedType();              
                let accType = form.find("#accType").val();
                let selectedType = form.find("div.dynamic-type[name=type-" + accType + "]");
                selectedType.removeAttr("hidden");
                selectedType.find(".form-control").removeAttr("disabled");
                //fix content height
                let countRow = selectedType.find(".form-row").length;
                container.height(initHeight + 102 * (countRow - 3));
              
            } else if (currentIndex === 0 && !form.valid() ){
                container.height(initHeight + 104);
            }
            if (currentIndex === 1) {
                container.height(initHeight);
            }
            
            return form.valid();
        },
        onFinished: function (event, currentIndex)
        {
            form.submit();
        }
    });
    
    
})(jQuery);
