using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// V0.1.1 New addition
/// Used for OW with animators to execute code/play animations when players enter/exit.
/// Can be adjusted in multiple ways, I guess
/// </summary>
public class TriggerPlayerOut : MonoBehaviour
{
	public float volume = 1;
    public float pitch = 1;
    [Header("Change the Boolean value of the animation:")]
    public string changeBool;
	[Header("Remove the execution animation when entering the detection range")]
	public bool banTrigger;
    [Header("Remove invalidation when the previous scene is the specified scene")]
    public string notBanSceneSp;
	[Header("Set to execute when leaving/entering range")]
	public bool isEnter;
    Animator animator;
	bool triggered = false;

    [Header("Set the animator's sceneBool to true when the previous scene is the specified scene")]
    public string sceneSp;
	public string sceneBool = "SceneSp";
    [Header("When in ElectricOpen, ElectricOpen")]
    public bool electricOpen;
    
    void Start()
    {
        animator = GetComponent<Animator>();

        if (sceneSp == MainControl.instance.PlayerControl.lastScene)
        {
            animator.SetBool(sceneBool, true);
        }

    }
	private void OnTriggerEnter2D(Collider2D collision)
    {
		if (banTrigger && notBanSceneSp != MainControl.instance.PlayerControl.lastScene)
			return;
        if (!triggered && isEnter && collision.CompareTag("Player"))
        {
            animator.SetBool(changeBool, true);
			triggered = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (banTrigger && notBanSceneSp != MainControl.instance.PlayerControl.lastScene)
			return;
        if (!triggered && !isEnter && collision.CompareTag("Player"))
        {
            animator.SetBool(changeBool, true);
			triggered = true;
        }
    }

    /*
    //Called for animation

    void SetNoActive(string name)
    {
        transform.Find(name).gameObject.SetActive(false);
    }
	void SetActive(string name)
    {
        transform.Find(name).gameObject.SetActive(true);
    }
	void ChangePitch(float newPitch)
	{
		pitch = newPitch;
	}
    void ChangeVolume(float newVolume)
    {
        volume = newVolume;
    }
    void UseMixer(int boolean)
    {
        if (Convert.ToBoolean(boolean))
            mixer = Resources.Load<AudioMixerGroup>("AudioMixer/TrueLabAudioMixer");
        else mixer = null;
    }
    void PlayFx(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipUI, volume, pitch, mixer);
    }
    void PlayFxBattle(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipBattle, volume, pitch, mixer);
    }
    */
}
