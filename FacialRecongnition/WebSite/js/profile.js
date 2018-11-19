$(document).ready(function () {
    var camera; // placeholder

    var url = new URL(window.location.href);
    var collectionName = url.searchParams.get("user");
    $('#userId').html(collectionName);

    var delete_collection = function () {
        $.get("https://localhost:44320/api/delete/" + collectionName, function (response) {
            $("#profilePanel").hide();
        });
    }

    var create_collection = function () {

        $.get("https://localhost:44320/api/create/" + collectionName, function (response) {
            $("#upload_result").html(response);
        });
    }

    var enable_profile = function () {
        $("#profilePanel").show();
        create_collection();

        if (window.JpegCamera) {

            var options = {
                shutter_ogg_url: "js/jpeg_camera/shutter.ogg",
                shutter_mp3_url: "js/jpeg_camera/shutter.mp3",
                swf_url: "js/jpeg_camera/jpeg_camera.swf"
            }
            camera = new JpegCamera("#camera", options).ready(function (info) {

            });
        }
    }

    // Add the photo taken to the current Rekognition collection for later comparison
    var add_to_collection = function () {
        if (!collectionName) {
            $("#upload_status").html("User doesn't login.");
            return;
        }

        var cardNumber = $("#txtCardNumer").val();
        var snapshot = camera.capture();
        var api_url = "https://localhost:44320/api/addfaces/" + collectionName + "-" + cardNumber;

        snapshot.upload({ api_url: api_url }).done(function (response) {
            $("#processsuccess_img").show();
            $("#processfailure_img").hide();
            $("#profilePanel").hide();
            //this.discard();
        }).fail(function (status_code, error_message, response) {
            $("#processsuccess_img").hide();
            $("#processfailure_img").show();
            console.log(response);
        });

    }

    var AddPhotoToProfile = function () {
        add_to_collection();
    }

    $("#create_collection").click(enable_profile);
    $("#delete_collection").click(delete_collection);
    $("#savebtn").click(AddPhotoToProfile);
});