using UnityEngine;

/// <summary>
/// V0.1.1新加
/// 用于带动画器的OW，在玩家进入/离开时，执行代码/播放动画。
/// 可多方面调整，我猜
/// </summary>
public class TriggerPlayerOut : MonoBehaviour
{
    public float volume = 1;
    public float pitch = 1;

    [Header("更改动画的布尔值：")]
    public string changeBool;

    [Header("去除进入检测范围执行动画")]
    public bool banTrigger;

    [Header("在上个场景为指定场景时去除失效")]
    public string notBanSceneSp;

    [Header("设置在离开范围/进入范围时执行")]
    public bool isEnter;

    private Animator animator;
    private bool triggered = false;

    [Header("在上个场景为指定场景时动画器的sceneBool设true")]
    public string sceneSp;

    public string sceneBool = "SceneSp";

    [Header("在electricOpen的时候electricOpen")]
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
    //给动画调用的

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