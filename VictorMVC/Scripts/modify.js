var form = "#" + $("form").first().attr("id");  //assume one form.
const alertEl = "#alert";
const loader = "#loader";

function formOnBegin() {
    $(form).hide();
    $(loader).show();
}

function formOnFail(data) {
    $(loader).hide();
    $(form).show();
    $(alertEl).text(data.responseJSON.Message);
    $(alertEl).show();
}

function formOnSuccess() {
    var directory = window.location.pathname.split("/");
    if (isNaN(directory[directory.length - 1]))
    {
        window.location.replace(".");
    }
    else
    {
        window.location.replace("..");
    }
}