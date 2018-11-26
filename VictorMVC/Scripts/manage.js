var alertEl = "#alert";
var loader = "#loader";
var rowToDelete = rowToDelete = $(this).parent().siblings(deletionClass).text();

function deleteSuccess(rowToDelete, rowId)
{
    $(alertEl).attr("class", "alert alert-success");
    $(alertEl).text("\"" + rowToDelete.text() + "\" was successfully deleted from the server");
    $("#" + rowId).remove();
    $(alertEl).show();
}

function deleteFailure(data)
{
    $(alertEl).attr("class", "alert alert-danger");
    $(alertEl).text(data.responseJSON.Message);
    $(alertEl).show();
}
$(".delete-button").click(function ()
{
    var rowToDelete = $(this).parent().siblings(deletionClass);
    var rowId = $(this).parent().parent().attr("id");
    var confirmation = confirm("Are you sure you want to delete \"" + rowToDelete.text() + "\"? You cannot undo this action.");

    if (confirmation) {
        $.ajax({
            url: providedUrl + rowId,
            type: "POST",
            dataType: 'json'
        }).done(function () { deleteSuccess(rowToDelete, rowId); })
            .fail(function (data) { deleteFailure(data); });
    }
});