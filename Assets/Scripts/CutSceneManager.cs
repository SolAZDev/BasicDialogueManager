using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

/* Dialogue Scene Manager v0.4b
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
	[Header ("Scene Details")]
	public DialogueFile file;
	public string SceneID;
	public bool FadeText = false;
	public AudioSource Voices;
	public AudioSource Sounds;
	public AudioSource BGMPlayer;
	public List<Animator> Actors;
	public string ReturnScene;

	[Space]
	[Header ("UI Settings")]
	public Text TextBox;
	public Button TBBtn;
	public Animator TextAnim;
	public Button YesBtn;
	public Button NoBtn;
	public Text YesBtnText;
	public Text NoBtnText;
	public Image FlashFade;

	[Space]
	public UnityEvent ExitEvent;

	string Line;
	DialogueManager manager;
	Dialogue ActiveLine;
	Dialogue.Choice ActiveChoice;

	Vector3[] MoveList;
	Vector3[] RotateList;
	float[] MoveRotSpeed;

	bool UpGUI = false;
	bool fade, SceneIn, flash = false;
	// Use this for initialization
	void Start () {
		manager = DialogueManager.LoadDialogueFile (file);
		ActiveLine = manager.GetDialogue (SceneID);

		MoveList = new Vector3[Actors.Count];
		RotateList = new Vector3[Actors.Count];
		MoveRotSpeed = new float[Actors.Count];

		YesBtn.gameObject.SetActive (false);
		NoBtn.gameObject.SetActive (false);
		TBBtn.interactable = true;
		SceneIn = true;
		fade = true;

	}

	// Update is called once per frame
	void Update () {
		//if(!UpGUI){StartCoroutine(UpdateGUI());}
		TextBox.text = Line;
		for (int i = 0; i < Actors.Count; i++) {
			if (Actors[i].transform.position != MoveList[i]) {
				Actors[i].transform.position = Vector3.Lerp (Actors[i].transform.position, MoveList[i], Time.deltaTime * MoveRotSpeed[i]);
			}
			if (Actors[i].transform.rotation.eulerAngles != RotateList[i]) {
				Actors[i].transform.rotation = Quaternion.Lerp (Actors[i].transform.rotation, Quaternion.Euler (RotateList[i]), Time.deltaTime * MoveRotSpeed[i]);
			}
		}
		if (fade) {
			if (SceneIn) {
				FlashFade.gameObject.SetActive (true);
				if (FlashFade.color != Color.clear) {
					FlashFade.color = Color.Lerp (FlashFade.color, Color.clear, Time.deltaTime * 8f);
				} else { FlashFade.gameObject.SetActive (false); fade = false; }
			} else {
				FlashFade.gameObject.SetActive (true);
				if (FlashFade.color != Color.black) {
					FlashFade.color = Color.Lerp (FlashFade.color, Color.black, Time.deltaTime * 8f);
				} else { FlashFade.gameObject.SetActive (false); fade = false; }
			}
		}
	}

	public void Initialize () {
		Start ();
		Continue ();

	}

	public void SetScene (string scene) {
		SceneID = scene;
	}

	public void Continue () {
		if (ActiveLine.GetChoices ().Length == 1) {
			TBBtn.interactable = false;
			StopCoroutine (UpdateLine ());
			ActiveChoice = ActiveLine.GetChoices () [0];
			ActiveLine.PickChoice (ActiveChoice);
			StartCoroutine (UpdateLine ());
			ParseArguments ();
		}
		if (ActiveLine.GetChoices ().Length == 0) {
			SceneIn = false;
			fade = true;
			//Exit Command
			ExitEvent.Invoke ();
		}
	}

	public void AnswerQuestion (int yesno) {
		Line = System.String.Empty;
		StopCoroutine (UpdateLine ());
		Line = System.String.Empty;
		YesBtn.gameObject.SetActive (false);
		NoBtn.gameObject.SetActive (false);
		TBBtn.interactable = true;
		ActiveChoice = ActiveLine.GetChoices () [yesno];
		ActiveLine.PickChoice (ActiveChoice);
		StartCoroutine (UpdateLine ());
		ParseArguments ();
	}

	public void ExitScene () {
		UnityEngine.SceneManagement.SceneManager.LoadScene (ReturnScene);
	}

	void ParseArguments () {
		if (ActiveChoice.userData == "") { return; }
		string[] arguements;
		if (ActiveChoice.userData.Contains (" ")) {
			arguements = ActiveChoice.userData.Split (new [] { ' ' });
		} else {
			arguements = new string[1];
			arguements[0] = ActiveChoice.userData;
		}
		string[] argArgs;

		//The Parsing! Check the Usage

		//This is done originally in a specific order but I've expanded it to add more
		//arguments, such as 2+ animations per lines, sounds and voices played together
		//and other things.
		for (int i = 0; i < arguements.Length; i++) {

			//Animation
			if (arguements[i].Contains ("a_")) {
				argArgs = arguements[i].Remove (0, 2).Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[0])) { //check if such an actor even exists
					switch (int.Parse (argArgs[1])) {
						case 0: //Set Trigger
							Actors[int.Parse (argArgs[0])].SetTrigger (argArgs[2]);
							break;
						case 1: //Set Bool true.
							Actors[int.Parse (argArgs[0])].SetBool (argArgs[2], true);
							break;
						case 2: //Set Bool false.
							Actors[int.Parse (argArgs[0])].SetBool (argArgs[2], false);
							break;
						case 3: //Hide!
							Actors[int.Parse (argArgs[0])].gameObject.SetActive (false);
							break;
						case 4: //Show!
							Actors[int.Parse (argArgs[0])].gameObject.SetActive (true);
							break;
					}
				}
			}

			//Moving an Actor
			if (arguements[i].Contains ("m_")) {
				argArgs = arguements[i].Remove (0, 2).Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[0])) {
					MoveList[int.Parse (argArgs[0])] = new Vector3 (
						(argArgs[1] != "N") ? float.Parse (argArgs[1]) : Actors[0].transform.position.x,
						(argArgs[2] != "N") ? float.Parse (argArgs[2]) : Actors[0].transform.position.y,
						(argArgs[3] != "N") ? float.Parse (argArgs[3]) : Actors[0].transform.position.z
					);
					MoveRotSpeed[int.Parse (argArgs[0])] = (argArgs.Length > 4) ? float.Parse (argArgs[4]) : 8f;
				}

			}

			//Rotate an Actor
			if (arguements[i].Contains ("r_")) {
				argArgs = arguements[i].Remove (0, 2).Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[0])) {
					RotateList[int.Parse (argArgs[0])] = new Vector3 (
						(argArgs[1] != "N") ? float.Parse (argArgs[1]) : Actors[0].transform.rotation.eulerAngles.x,
						(argArgs[2] != "N") ? float.Parse (argArgs[2]) : Actors[0].transform.rotation.eulerAngles.y,
						(argArgs[3] != "N") ? float.Parse (argArgs[3]) : Actors[0].transform.rotation.eulerAngles.z
					);
					MoveRotSpeed[int.Parse (argArgs[0])] = (argArgs.Length > 4) ? float.Parse (argArgs[4]) : 8f;
				}

			}

			//Look at (ironically)
			if (arguements[i].Contains ("l_")) {
				argArgs = arguements[i].Remove (0, 2).Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[0]) && Actors.Count > int.Parse (argArgs[1])) {
					//Actors [int.Parse(argArgs[0])].transform.LookAt (Actors [int.Parse(argArgs[1])].transform.position);
					Actors[int.Parse (argArgs[0])].transform.LookAt (MoveList[int.Parse (argArgs[1])]);
				}
			}

			//UNIMPLEMENTED BUT STILl CODED

			//Sounds
			if (arguements[i].Contains ("s_")) {
				argArgs = arguements[i].Remove (0, 2).Split (new [] { '_' });
				//if()
				Sounds.clip = Resources.Load ("Audio/Sounds/" + argArgs[0]) as AudioClip;
				if (argArgs.Length > 1) { Sounds.volume = float.Parse (argArgs[1]); } else { Sounds.volume = 1; }
				if (argArgs.Length > 2) { Sounds.pitch = float.Parse (argArgs[2]); } else { Sounds.pitch = 1; }
				if (argArgs.Length > 3) { Sounds.panStereo = float.Parse (argArgs[3]); } else { Sounds.panStereo = 0; }
				Sounds.Play ();
			}

			//Voice Acting (Simpler version of sound tbh)
			if (arguements[i].Contains ("v_")) {
				string voice = arguements[i].Remove (0, 2);
				Voices.clip = Resources.Load ("Audio/Voices/" + ActiveChoice.speaker + "/" + voice) as AudioClip;
				Sounds.Play ();
			}

			//Songs
			if (arguements[i].Contains ("b_")) {
				print ("Attepting to play music");
				string song = arguements[i].Remove (0, 2);
				BGMPlayer.Stop ();
				BGMPlayer.clip = Resources.Load ("Audio/BGM/" + song) as AudioClip;
				BGMPlayer.Play ();
			}

			//STAHP PLAYING
			if (arguements[i] == "bs") { BGMPlayer.Stop (); }
			if (arguements[i] == "ls") { Voices.Stop (); }
			if (arguements[i] == "ss") { Sounds.Stop (); }

			//Panel Effects
			if (arguements[i] == "fadein") { fade = true; SceneIn = true; }
			if (arguements[i] == "fadein") { fade = true; SceneIn = false; }
			if (arguements[i] == "flash") { StartCoroutine (Flash ()); }

		}
	}

	IEnumerator UpdateLine () {
		if (FadeText) {
			TextAnim.SetTrigger ("Fade");
			yield return new WaitForSeconds (.5f);
			Line = ActiveChoice.dialogue;
			yield return null;
		} else {
			Line = System.String.Empty;
			foreach (char letter in ActiveChoice.dialogue.ToCharArray ()) {
				Line += letter;
				yield return null;
			}
		}
		yield return new WaitForSecondsRealtime (.1f);
		TBBtn.interactable = true;

		if (ActiveLine.GetChoices ().Length > 1) {
			TBBtn.interactable = false;
			YesBtn.gameObject.SetActive (true);
			YesBtnText.text = ActiveLine.GetChoices () [0].dialogue;
			NoBtn.gameObject.SetActive (true);
			NoBtnText.text = ActiveLine.GetChoices () [0].dialogue;
		}
		yield return null;
	}

	IEnumerator UpdateGUI () {
		UpGUI = true;
		yield return null;
		//yield return new WaitForSecondsRealtime (0.01f);
		TextBox.text = Line;
		UpGUI = false;
	}

	IEnumerator Flash () {
		FlashFade.gameObject.SetActive (true);
		FlashFade.color = Color.white;
		yield return new WaitForSecondsRealtime (.01f);
		FlashFade.color = Color.clear;
		FlashFade.gameObject.SetActive (false);
		yield return null;
	}

}