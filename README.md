# BasicDialogueManager
Yet Another Open Source Dialogue Manager, depends on [EasyDialogue](http://u3d.as/2NH).
This expands on the capabilities of said project, originally to create Persona Q-esque
dialogue scenes. This takes advantage of EasyDialogue's userData string which allows you
to add your own extras to your dialogue nodes. This script adds support for sounds, multiple
animations and voice acting (or another sound).

###Usage
The userData string is originally left for you to use as you desire.
This can be used to compare values or virtually anything else. The script uses
this variable as an array of arguments, processing them in order. So each node
can play sounds, and  as many animations as desired, as of this writing.

For animation :    a_(actor ID in the array)_(trigger/bool true/bool false/disable/enable)_(animator variable name)
For Sounds    :    s_(clip name)(_(volume)_(pitch)_(stereo panning)) (everything besides the clip name is optional)
For Voices    :    v_(clip name) (It uses the speaker's name to look for the audio clip)
For Moving    :    m_(actor ID in array)_(X/N)_(Y/N)_(Z/N) (use N to leave an axis as is)
For Rotation  :    r_(actor ID in array)_(X/N)_(Y/N)_(Z/N) (same as movement)


```
//Animation
a_(actor ID in array)_(trigger/bool true/bool false/disable/enable)_(animator variable name)

//Sounds,everything but clip name is optional
s_(clip name)(_(volume)_(pitch)_(stereo panning))

//voiced lines, it uses the Speaker/Talker's name as refference
v_(clip name)

//Moving. Use N to leave the axis as is.
m_(actor ID in array)_(X/N)_(Y/N)_(Z/N)

//Rotation (Set), use N to leave axis as is
r_(actor ID in array)_(X/N)_(Y/N)_(Z/N)

//Look at
l_(actor ID in array)_(actor ID to look at)

//play a song
b_(song name)

//stop bgm
sb

//stop sound
ss

//stop a voiced line
ls

//Fading, Flashing only if using a panel to fade
fadein
fadeout
flash

```

Note that this uses Resource.Load(path+name) to load any files.

Example:
Speaker are "Jack" and "Link", and their animators are 

Dialogue 1: Link: Yes!!
```a_1_0_victory1 v_victory1 s_win```

Dialogue 2: Jack: Now we need to get it working...
```a_0_0_nod v_getworking```


Result:
Link's animator component sets the triger "Victory1", plays the voiced line (found in "Audio/Voice/Link/victory1") and plays a victory sound (found in "Audio/Sound/win"), followed by Jack's animator triggering "nod" and playing his voiced line.

##TODO
* Fix bugs, reduce errors (if file's not found, then ignore it all)
* Add applied rotaion
* Add Cinemachine/Timeline arguemnt
* Possibly a function finder


