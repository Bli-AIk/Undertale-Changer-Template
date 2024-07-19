using UnityEngine;

/// <summary>
/// New in V0.1.1.
/// Used to drive the OW of the animator, executing code/playing animations when the player enters/leaves.
/// Adjustable in many ways, I guess.
/// </summary>
public class TriggerPlayerOut : MonoBehaviour
{
    public float volume = 1;
    public float pitch = 1;

    [Header("Change animation boolean:")]
    public string changeBool;

    [Header("Remove entry detection range to perform animation")]
    public bool banTrigger;

    [Header("Remove disabled when last scene is specified")]
    public string notBanSceneSp;

    [Header("Set to execute on out-of-scope/in-scope")]
    public bool isEnter;

    private Animator animator;
    private bool triggered = false;

    [Header("Set the animator's sceneBool to true if the last scene was the specified scene")]
    public string sceneSp;

    public string sceneBool = "SceneSp";

    [Header("electricOpen at electricOpen")]
    public bool electricOpen;

    private void Start()
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
    //called for animation

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
