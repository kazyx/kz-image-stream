KzLiveview
==========
- Image stream processor for Sony camera devices.
- Analyze [JPEG image stream from Sony camera devices](https://developer.sony.com/develop/cameras/) and provide each picture frame as a retrieved Event.

##Build
1. Clone repository.
 ``` bash
 git clone git@github.com:kazyx/KzLiveview.git
 ```

2. Open /Project/KzLiveview.sln by Visual Studio 2012 for WP.

##Get JPEG frame data from Liveview stream.
1. Obtain URL of liveview image stream by calling startLiveview API.
See [KzRemoteApi](https://github.com/kazyx/KzRemoteApi).
 ``` cs
 var url = await camera.StartLiveviewAsync();
 ```

2. Create LvStreamProcessor instance and set Event handler.
 ``` cs
 LvStreamProcessor processor = new LvStreamProcessor();
 processor.JpegRetrieved += (sender, e) => {
    var data = e.JpegData; // Byte array of a single JPEG image.
 };
 ```

3. Open connection.
 ``` cs
 processor.OpenConnection(url);
 ```

##License
This software is published under the [MIT License](http://opensource.org/licenses/mit-license.php).
