# PayWithFace
with Html5 camera(jpeg_camera), aws rekognition and .net core. to show a demo including register face with card information and payment process

liveness detection is not available.


# installation step:
1. set up your aws credential, save then in the .aws/config file or simply type aws config in CMD.
2. deploy the webapi project or run it in iis express
3. modify the profile.js and paywithface.js file to apply the endpoint of your service.

# test case:
1. open login page, input a useranme.
2. go to profile page, save face information and your bank card number.
3. go to paywithface page, pay with your face by clicking the button.
