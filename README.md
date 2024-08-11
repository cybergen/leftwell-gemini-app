# Teleportation Turmoil
Teleportation Turmoil is a storytelling app for younger players. Help Sizzlewhisker the junior wizard (and sarcastic, Brooklyn-accent wielding dragon), prepare for his teleportation exam at Thromwell Magical Correspondence School by creating an exciting or hilarious tale together. Pick a story prompt and capture items throughout your house that will get transformed into powerful artifacts for use in an adventure that Sizzlewhisker makes up and narrates. Generate enough creative energy, and you can open up a magic portal to another world!

[Demo Video](https://www.youtube.com/watch?v=ADICV-VjrJI)

![Flying Dragon Wizard in a purple conical hat](https://github.com/cybergen/leftwell-gemini-app/blob/main/UnityClient/Assets/Images/Textures/Zoomed-BG.png?raw=true)

## Platforms
Works on ARCore-capable Android devices and ARKit-capable iOS devices

## Key Features
* Talk back and forth with Sizzlewhisker, who is fully voiced and can dynamically converse
* Take pictures of items in your environment and see them transformed via Imagen 3 using a set of pre-defined edit styles
* Generate and listen to narration of a fully custom story based on your story prompt, speech responses, and captured items
* Get a custom image generated from a pivotal moment in your adventure, using a combination of LLM prompt generation and Imagen 3 image generation
* Enter free conversation mode with Sizzlewhisker at the end of your adventure and just talk back and forth to your heart's content (if you don't elect to go on another adventure)
* Social sharing of transformed item images, final story image, and final story text
* Lore-friendly permissions request flow
* Lore-friendly tutorial

## How to Test with Minimal Effort
### Android
* Install [release APK 0.1.4](https://drive.google.com/file/d/1MY1rXkWPfDktPgspPZ23o-jDdPyhkRBm/view?usp=drive_link) on an ARCore-compatible Android device (Pixel 4-onwards works)
* Ensure you have a stable internet connection or wifi
* Launch the game

## Technology Used
* Unity 2022.3.36f1
* Gemini 1.5 Pro
* Imagen 3
* ElevenLabs speech synth
* ARCore
* ARKit
* NodeJS proxy server for managing Gemini, Imagen, and ElevenLabs interactions
* GCP VM instance for hosting proxy server
* BriLib - personal library for Unity games that I open-sourced back in 2016 or so

![Flying Dragon Wizard on the ground in front of a dark forest with a looming figure](https://github.com/cybergen/leftwell-gemini-app/blob/main/UnityClient/Assets/Images/Textures/FailedHeroImage.png?raw=true)

## Known Issues
* If you reject camera permissions on iOS, you will become unable to progress due to inability to show request dialog again, resulting in need to reinstall app
* The LLM rarely starts describing Sizzlewhisker's actions in third person, but it does not disrupt progression
* AR tracking sometimes gets lost while the app is backgrounded during use of social share
* Proxy server does not currently have an auth flow - this is a mission critical feature I'll be building very soon, and the reason why my repository isn't public

## About the Dev Process
I didn't find out about the competition until early July, so I was a bit behind, especially working as a solo dev. Luckily, I have been building on top of various LLM's and generative art tools for the last year or so, and previously built a lot of AR experiences, so I was able to make rapid progress.

### What Went Well
* Constrained scope effectively to ensure I could execute on the concept in a month
* Initial breakout of features and estimates were quite accurate
* AI tools helped speed some of the coding process (albeit with a lot of code review needed)

### What Went Poorly
* Needed to roll my own managers for interacting with REST API's, since there weren't great Unity-compatible options available
* Should've applied systemic thinking to how to handle game flow effectively - AdventureSequence needs the world's most aggressive refactor
* Underestimated need for tweaking and polish for procedural animations for dragon
* Should've applied systemic thinking to speech and animation, especially as regards animating while speaking and doing other things

### High Priority Clean Up Work to Do Next
* Server DESPARATELY needs an auth flow to prevent abuse
* Factor HTTP request stuff on client into a single core with retries managed there, rather than different implementations in SpeechManager, LLMInteractionManager, ImageGenerator, etc
* Come up with a data structure for handling complex sequences where many processes such as requesting info from the LLM, requesting speech, requesting image generation, should be handlable in the background but also many of the tasks have complex series of dependencies before they can proceed, and they all need to place results in an accessible location for app flow to make use of
* Clean up dragon behavior controller SIGNIFICANTLY

### Upcoming Features
* Allow redoing tutorial
* Persistent story history (via compact summarization of prior sessions)
* Previous story gallery
* New story structures that can focus on specific lessons or concepts Sizzlewhisker can make games around to help learning

## How to Build for Yourself
### Server Build
In order to interact with Vertex AI services and ElevenLabs, I built a minimal proxy server that is responsible for owning app secrets and critical service access. If you wish to try out the app utterly independent from my server (which is currently live on a GCP VM instance with domain leftwell.com), you will need to stand up your own server and configure it.
* Create a project via the Google Cloud dash
* Enable the Generative Language API and Vertex AI API
* Generate and save an API key with access to those API's
* Create an account at ElevenLabs.io and generate an API key
* Stand up a server instance via a service of your choosing
* Set up gcloud CLI on your server instance and auth on it, such that you can confirm the command `gcloud auth print-access-token` returns a valid token
* Point valid DNS entries at your server's external IP
* Generate a valid certificate for https support via letsencrypt and remember your certificate locations
* Pull this repo and push the Server folder up to your server
* Create a .env file in the Server folder root and populate it with your API keys, port, etc:
```
PORT=LOCAL_PORT
ELEVEN_LABS_API_KEY=GET_FROM_ELEVEN_LABS
API_KEY=GOOGLE_CLOUD_API_KEY
CERT_PRIVATE_KEY=PATH_TO_PRIVKEY_PEM
CERT=PATH_TO_CERT_PEM
CERT_AUTHORITY=PATH_TO_FULLCHAIN_PEM
VOICE_ID=2ovNLFOsfyKPEWV5kqQi
```
* Run the server via `npm run prod`

### Android Client Build
* Pull the repo
* Load UnityClient folder in the indicated version of Unity
* You may get an issue about a missing package for AR Remoting - click continue to move forward without this package, which I used for testing but only had one license for
* Switch platform to Android via build dialog
* Edit the settings file at UnityClient/Assets/Scripts/Network/NetworkSettings.cs to point at the path where your proxy server lives (or use mine at leftwell.com)
* Connect an ARCore-compatible Android device (I've confirmed that Pixel 4-onward works)
* Set phone to dev mode
* Build and run

### iOS Client Build
* Pull the repo
* Load UnityClient folder in the indicated version of Unity
* You may get an issue about a missing package for AR Remoting - click continue to move forward without this package, which I used for testing but only had one license for
* Switch platform to iOS via build dialog
* Edit the settings file at UnityClient/Assets/Scripts/Network/NetworkSettings.cs to point at the path where your proxy server lives (or use mine at leftwell.com)
* Connect an ARKit-compatible iPhone (I've confirmed that iPhoneXR-onward works)
* Ensure device is trusted for development
* Build and run
* Switch to XCode
* Go to project>signing and teams
* Set up automatic signing with your Apple dev team
* Hit play to install and launch on device
