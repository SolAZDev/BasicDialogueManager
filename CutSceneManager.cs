using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Dialogue Scene Manager v0.2b
 * 		By SolAZDev
 * 
 * 		ABOUT
 *	This is based on EasyDialogue (http://u3d.as/2NH) 
 * which is based on a choice-driven dialogue, see this 
 * the example to see what I mean. However this bare-bone
 * dialuge system doesn't come with a UI implementation,
 * you need to make it yourself. BUT it has an extra string
 * we can use to our advantage. choice.userData. You can add
 * a LOT of things, such as an argument system! Just like
 * you would with a command line. (PlayVideo file.mp4 --subs=file)
 * 
 * 		USAGE
 * Here's how it works.
 * You have your speakers (used as characters in this script),
 * your dialogues, and then the user data with all the arguments.
 * You can use as many arguments as you like but you need to set 
 * them in order... I'll explain in the argument section. You also
 * need to set up the object variables in the inspector. 
 * Yes/No text changes if there's 2 choices (simple dialogue).
 * Please do your set up according to the scrippt. you know what I mean.
 * 
 * 		ARUGMENTS (note they're case sensitive)
 * 	a_ = Animation. 	a_(character in the Actors Array)_(Trigger/BoolTrue/BoolFalse/Disable/Enable)_(Animator variable)
 * 	s_ = Sound Effect. 	s_(Clip Name)_(Volume)_(Pitch)_(Pan Stere).
 *  v_ = Voiced Line. 	v_(Clip Name). This uses the speaker string to find which character does the line
 * 
 * 		Note that these are used with Resource.Load. 
 * 		And require the placement as the following.
 * 	
 * 	Sounds go in		 "Audio/Sounds/"
 * 	Voice Lines go in 	 "Audio/Voices/(Character names)/"
 * 
 * 
 * 		EXAMPLES~
 * 
 * Dialogue 1:	s_thud_120 a_0_1_Shock a_1_1_Shock 
 * Dialogue 2:  a_2_5 a_2_0_StandUp v_groan (speaker is named Larry)
 * Dialogue 3:  a_2_1_Shock v_WhatAreYouDoingHere
 * 
 * Result.
 * D1: an audio clip ("Audio/Sounds/thud") is played at 120 volume, actors 0 & 1 in the array set the animator bool "Shock" to true
 * D2: Actor 2 in the array Sets the trigger "StandUp", and then his voiced groaing plays.
 * D3: Actor 2 then activates the "Shock" bool, thus the animation plays. then his voiced line also plays.
 * 
 * You can set them up however you like but should be kept in order of succession.
 * Read the ParseArguments() function for detailed progress.
 * 
*/


public class CutSceneManager : MonoBehaviour {
	public DialogueFile file;
	public string SceneID;
	public Animator[] Actors;
	public Text TextBox;
	public Button TBBtn;
	public Button YesBtn;
	public Button NoBtn;
	public Text YesBtnText;
	public Text NoBtnText;
	public AudioSource Voices;
	public AudioSource Sounds;

	public string Line;
	DialogueManager manager;
	Dialogue ActiveLine;
	Dialogue.Choice ActiveChoice;

	bool UpGUI = false;
	// Use this for initialization
	void Start () {
		manager = DialogueManager.LoadDialogueFile (file);
		ActiveLine = manager.GetDialogue (SceneID);
		ActiveChoice = ActiveLine.GetChoices () [0];
		ActiveLine.PickChoice (ActiveChoice);

		YesBtn.gameObject.SetActive (false);
		NoBtn.gameObject.SetActive (false);
		TBBtn.interactable = true;
	}
	
	// Update is called once per frame
	void Update () {
		if(!UpGUI){StartCoroutine(UpdateGUI());}
	}

	public void Continue(){
		if(ActiveLine.GetChoices().Length==1){
			StopCoroutine (UpdateLine ());
			Line = System.String.Empty;
			ActiveChoice = ActiveLine.GetChoices () [0];
			ActiveLine.PickChoice (ActiveChoice);
			StartCoroutine (UpdateLine ());
			ParseArguments ();
		}
	}

	public void AnswerQuestion(int yesno){
		StopCoroutine (UpdateLine ());

		YesBtn.gameObject.SetActive (false);
		NoBtn.gameObject.SetActive (false);
		TBBtn.interactable = true;

		ActiveChoice = ActiveLine.GetChoices () [yesno];
		ActiveLine.PickChoice (ActiveChoice);
		StartCoroutine (UpdateLine ());
		ParseArguments ();
	}

	void ParseArguments(){
		if(ActiveChoice.userData==""){return;}
		char[] splitchar = { ' ' };
		char[] argsplit  = { '_' };
		string test="";
		string[] arguements = ActiveChoice.userData.Split (splitchar);
		string[] argArgs;

		//The Parsing! Check the Usage

		//This is done originally in a specific order but I've expanded it to add more
		//arguments, such as 2+ animations per lines, sounds and voices played together
		//and other things.
		for(int i=0;i<arguements.Length;i++){
			
			//Animation
			if(arguements[i].Contains("a_")){
				argArgs = arguements [i].Remove (0, 2).Split (argsplit);
				if(Actors.Length>int.Parse(argArgs[0])){ //check if such an actor even exists
					switch(int.Parse(argArgs[1])){
					case 0: //Set Trigger
						Actors [int.Parse (argArgs [0])].SetTrigger (argArgs [2]);
						break;
					case 1: //Set Bool true.
						Actors [int.Parse (argArgs [0])].SetBool (argArgs [2], true);
						break;
					case 2: //Set Bool false.
						Actors [int.Parse (argArgs [0])].SetBool (argArgs [2], false);
						break;
					case 3: //Hide!
						Actors [int.Parse (argArgs [0])].gameObject.SetActive (false);
						break;
					case 4: //Show!
						Actors [int.Parse (argArgs [0])].gameObject.SetActive (true);
						break;
					}
				}
			}

			//UNIMPLEMENTED BUT STILl CODED

			//Sounds
			if(arguements[i].Contains("s_")){
				argArgs = arguements [i].Remove (0, 2).Split (argsplit);
				Sounds.clip = Resources.Load ("Audio/Sounds/" + argArgs [0]) as AudioClip;
				if (argArgs.Length > 1) { Sounds.volume		 = float.Parse (argArgs [1]);   }else { Sounds.volume = 1;	}
				if (argArgs.Length > 2) { Sounds.pitch 		 = float.Parse (argArgs [2]);   }else { Sounds.pitch = 1;	}
				if (argArgs.Length > 3) { Sounds.panStereo 	 = float.Parse (argArgs [3]);	}else { Sounds.panStereo = 0;}
				Sounds.Play ();
			}

			//Voice Acting (Simpler version of sound tbh)
			if(arguements[i].Contains("v_")){
				argArgs = arguements [i].Remove (0, 2).Split (argsplit);
				Voices.clip = Resources.Load ("Audio/Voices/" + ActiveChoice.speaker + "/" + argArgs [0]) as AudioClip;
				Sounds.Play ();
			}


		}
	}

	IEnumerator UpdateLine(){
		Line = System.String.Empty;
		foreach(char letter in ActiveChoice.dialogue.ToCharArray()){
			Line += letter;
			yield return null;
		}

		if(ActiveLine.GetChoices().Length>1){
			TBBtn.interactable = false;
			YesBtn.gameObject.SetActive (true);
			YesBtnText.text = ActiveLine.GetChoices () [0].dialogue;
			NoBtn.gameObject.SetActive (true);
			NoBtnText.text = ActiveLine.GetChoices () [0].dialogue;
		}
		yield return null;
	}

	IEnumerator UpdateGUI(){
		UpGUI = true;
		yield return new WaitForSecondsRealtime (0.05f);
		if(ActiveChoice.speaker!=""){
			TextBox.text = "\t" + ActiveChoice.speaker + "\n" + Line;
		}else{
			TextBox.text = "\n" + Line;
		}
		UpGUI = false;
	}
}

