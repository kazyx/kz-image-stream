kz-image-stream
==========
- Image stream processor for Sony camera devices.
- Analyze [Image stream from Sony camera devices](https://developer.sony.com/develop/cameras/) and provide picture frames as Events.

##Build
1. Clone repository.
 ``` bash
 git clone git@github.com:kazyx/kz-image-stream.git
 ```

2. Open csproj file by Visual Studio.
 - /Project/KzImageStreamPhone8.csproj for Windows Phone 8.
 - /Project/KzImageStreamUniversal.csproj for Universal Windows application.
 - /Project/KzImageStreamDesktop.csproj for Desktop Windows application. (.Net framework 4.5)

##Get JPEG frame data from Liveview stream.
1. Obtain URL of liveview image stream by calling startLiveview API.
See [kz-remote-api](https://github.com/kazyx/kz-remote-api).
 ``` cs
 var url = await camera.StartLiveviewAsync();
 ```

2. Create StreamProcessor instance and set Event handler.
 ``` cs
 var processor = new StreamProcessor();

 processor.JpegRetrieved += (sender, e) => {
    var data = e.Packet.ImageData; // Byte array of a single JPEG image.
 };
 ```

3. Open connection.
 ``` cs
 processor.OpenConnection(new Uri(url));
 ```

##License
This software is published under the [MIT License](http://opensource.org/licenses/mit-license.php).
