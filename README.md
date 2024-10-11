# chessly_wec_rec
A two-player, offline, chess site that's also a PWA.
Made for the NITK Web Enthusiast's Club GDG SIG

## how to test in-editor
1. Download and install the Unity game engine (make sure that the version is greater than 2022.3.5f1 as only then the PWA template will be supported).
2. Make sure you have the WebGL build functionality for the unity version you are using.
3. On Unity Hub, click Add and open this repo as a project.
4. Open 'SampleScene' and click Play to start.

**To delete the saved game state-**
a. Open the Unity editor
b. Click Edit > Clear All PlayerPrefs

## how to build
1. Follow all steps from the 'How to test' section.
2. Create a new folder anywhere on your computer.
3. File > Build Settings. And then click on Build.
4. Once its done, start a local http server to run the build. The build folder will contain an index.html file once the build process is over.
5. Alternatively, you can upload this folder to hosting sites like github pages or even itch.io.
6. When you are runnning the build locally, make sure that all compressions are disabled in Player Settings and in Build Settings. This is because to decompmress your game is a task for the server thats serving the website. And unless you have added that functionality to your server, don't compress your build. If you do want to test compression/build for production with compression, make sure to do it on sites like itch.io which support it.
7. Make sure to also select the PWA template. This brings ServiceWorker.js and anifest.webmanifest files to your build that defines its PWA behaviour.

# credits and references
1. [Board Pieces](https://opengameart.org/content/chess-pieces-and-board-squares)
2. [Chess.js port to C#](https://github.com/dayjur/Chess.cs)
