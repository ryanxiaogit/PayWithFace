$(document).ready(function () {
    var camera; // placeholder


    var enable_camera = function () {
        $("#cameraPannel").show();
        $("#paymentsuccess_img").hide();
        $("#paymentfailure_img").hide();
        if (window.JpegCamera) {
            var options = {
                shutter_ogg_url: "js/jpeg_camera/shutter.ogg",
                shutter_mp3_url: "js/jpeg_camera/shutter.mp3",
                swf_url: "js/jpeg_camera/jpeg_camera.swf"
            }
            camera = new JpegCamera("#camera", options).ready(function (info) {
                compare_image();
            });
        }
    }

    // Compare the photographed image to the current Rekognition collection
    var compare_image = function () {
        var snapshot = camera.capture();
        var url = new URL(window.location.href);
        var collectionName = url.searchParams.get("user");
        var api_url = "https://localhost:44320/api/compareface/" + collectionName;
        $("#loading_img").show();
        snapshot.upload({ api_url: api_url }).done(function (response) {
            var data = JSON.parse(response);
            if (data !== undefined && data != "") {
                if (data.isMatched) {
                    $("#paymentsuccess_img").show();
                    $("#paymentfailure_img").hide();
                    $("#cardText").html("<b> Card Number:</b>" + data.cardNumber );
                    console.log("is here");
                }
                else {
                    $("#paymentfailure_img").show();
                    $("#paymentsuccess_img").hide();
                    console.log("is there");
                }
                console.log(data.message + ": " + ", Confidence: " + data.confidence)
            } else {
                $("#upload_result").html(data.message);
                $("#paymentfailure_img").show();
                $("#paymentsuccess_img").hide();
                consol.log("is that");
                console.log(data.message);
            }
            this.discard();
            $("#cameraPannel").hide();
            $("#loading_img").hide();
        }).fail(function (status_code, error_message, response) {
            $("#upload_status").html("Upload failed with status " + status_code + " (" + error_message + ")");
            $("#upload_result").html(response);
            $("#loading_img").hide();
        });
    };

    $('#btnToPay').click(enable_camera);

    //$('#btnCheckin').click(showpaymentSuccess);
    //$('#btnCheckout').click(showpaymentFailure);
});