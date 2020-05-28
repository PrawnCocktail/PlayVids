## Boruto-Junkies Downloader

**How to use**   
Double click the exe and enter the url of the video you want to download.   

**What it does**  
- Downloads at the highest resolution available.   

**What it doesn't do**  
- Doesn't download subtitles (if any).  

The ".ts" videos that this outputs will probably need to be run though ffmpeg for a better viewing experience.  
I Personally use this command:  
	ffmpeg -i "episode-name.ts" -acodec copy -vcodec copy "episode-name.mp4"  

"Hey, your code sucks!" Yup, thats why I do this for fun and not for a living.  